using System;
using System.Collections.Generic;
using System.Text;

namespace Desktomaton.PluginBase
{
  public interface IDesktomatonPluginBase
  {
    public string Name { get; }
    public List<IPluginProperty> Properties { get; }
  }

  public interface IPluginProperty
  {
    string Name { get; set; }
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

    public  delegate bool ValidationCallback(T value);

    public ValidationCallback Validator { get; }

    public PluginProperty(string Name, ValidationCallback validator = null)
    {
      Name = this.Name;
      Validator = validator;
    }

  }
}
