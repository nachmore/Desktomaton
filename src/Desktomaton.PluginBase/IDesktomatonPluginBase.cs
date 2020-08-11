using System;
using System.Collections.Generic;
using System.Text;

namespace Desktomaton.PluginBase
{
  public interface IDesktomatonPluginBase
  {
    public string Name { get; }
    public List<IPluginProperty> Properties { get; }

    /// <summary>
    /// Utilty function, generally useful when coding directly against a plugin
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void SetValue(string name, object value)
    {

      foreach (var property in Properties)
      {
        if (property.Name == name)
        {
          property.SetValue(value);
          return;
        }
      }

      throw new KeyNotFoundException($"Property {name} not found");
    }

    /// <summary>
    /// Utilty function, generally useful when coding directly against a plugin
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public object GetValue(string name)
    {
      foreach (var property in Properties)
      {
        if (property.Name == name)
        {
          return property.GetValue();
        }
      }

      throw new KeyNotFoundException($"Property {name} not found");
    }

  }

  /// <summary>
  /// Base interface to allow mixing PluginProperty of different types in a List
  /// </summary>
  public interface IPluginProperty
  {
    string Name { get; set; }

    void SetValue(object value);
    object GetValue();
  }

  public class PluginProperty<T> : IPluginProperty
  {
    public string Name { get; set; }

    private T _value;

    public T Value
    {
      get
      {
        return _value;
      }
      set
      {
        if (Validator == null || Validator(value))
        {
          _value = value;
        }
      }
    }

    public string ValidationError { get; internal set; }

    public delegate bool ValidationCallback(T value);

    public ValidationCallback Validator { get; }

    public PluginProperty(string name, ValidationCallback validator = null)
    {
      Name = name;
      Validator = validator;
    }

    /// <summary>
    /// Since there is no good way to expose T in the base interface we
    /// use a SetValue
    /// </summary>
    /// <param name="value"></param>
    public void SetValue(object value)
    {
      // no need for fancy handling - if the cast fails an exception will be thrown
      // and bubbled up
      Value = (T)value;
    }

    public object GetValue()
    {
      // explicitly go to the property in case there is handler code we want from there
      return Value;
    }
  }
}
