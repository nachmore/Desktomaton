using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Desktomaton.PluginBase
{
  [Serializable]
  public abstract class DesktomatonPluginBase
  {
    public abstract string Name { get; }

    private IEnumerable<PropertyInfo> _properties;
    private IEnumerable<PropertyInfo> Properties
    {
      get
      {
        if (_properties == null)
        {
          _properties = this.GetType().GetProperties().Where(p => Attribute.IsDefined(p, typeof(DesktomatonPropertyAttribute)));
        }

        return _properties;
      }
    }

    /// <summary>
    /// Returns the count of properties that are not null and are not marked as being
    /// ignored for the purpose of the property count
    /// </summary>
    /// <returns></returns>
    public int GetSetPropertyCount()
    {
      return Properties.Where(p => p.GetValue(this) != null && p.GetCustomAttribute<DesktomatonPropertyAttribute>()?.CountsTowardsTrigger == true).Count();
    }

    /// <summary>
    /// Utility function, generally useful when coding directly against a plugin
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void SetProperty(string name, object value)
    {

      foreach (var prop in Properties)
      {
        if (prop.Name == name)
        {

          var valueToSet = value;

          if (valueToSet != null)
          {
            valueToSet = ConvertType(valueToSet, prop.PropertyType);
          }

          prop.SetValue(this, valueToSet);

          return;
        }
      }

      throw new KeyNotFoundException($"Property {name} not found");
    }

    private object ConvertType(object value, Type type)
    {
      // if the value is nullable then operate on the underlying type
      var underlyingType = Nullable.GetUnderlyingType(type);

      if (underlyingType != null)
      {
        return ConvertType(value, underlyingType);
      }

      if (type.IsEnum)
      {
        return Enum.ToObject(type, value);
      }

      return Convert.ChangeType(value, type);
    }

    /// <summary>
    /// Utility function, generally useful when coding directly against a plugin
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public object GetProperty(string name)
    {
      foreach (var property in Properties)
      {
        if (property.Name == name)
        {
          return property.GetValue(this);
        }
      }

      throw new KeyNotFoundException($"Property {name} not found");
    }

  }
}
