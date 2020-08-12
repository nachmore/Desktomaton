using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Desktomaton.PluginBase
{
  [Serializable]
  public abstract class DesktomatonPluginBase
  {
    public abstract string Name { get; }

    public abstract List<IPluginProperty> Properties { get; }

    public int GetSetPropertyCount()
    {
      var count = 0;

      foreach (var property in Properties)
        count += (property.IsSet ? 1 : 0);

      return count;
    }

    /// <summary>
    /// Utilty function, generally useful when coding directly against a plugin
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void SetProperty(string name, object value)
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
    public object GetProperty(string name)
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

    public bool IsSet { get; }

    void SetValue(object value);
    object GetValue();
  }

  [Serializable]
  public class PluginProperty<T> : IPluginProperty
  {
    public string Name { get; set; }

    private bool _isSet = false;
    public bool IsSet { get
      {
        return _isSet || _value != null;
      }
    }

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
          _isSet = true;
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

      if (value == null)
      {
        Value = default(T);
        return;
      }

      // if the value is a nullable enum then we need to cast to the enum, not to the
      // nullable type
      var underlyingType = Nullable.GetUnderlyingType(typeof(T));

      if (underlyingType != null)
      {
        Value = (T)Enum.ToObject(underlyingType, value);
      }
      else
      {

        // First try to Convert, this catches things like int -> uint which doesn't
        // work with plain cast of (T)
        try
        {
          Value = (T)Convert.ChangeType(value, typeof(T));
        } 
        catch (InvalidCastException)
        {
          // nope that failed (for example, from int to enum), try a standard cast
          Value = (T)value;
        }
      }
    }

    public object GetValue()
    {
      return Value;
    }
  }
}
