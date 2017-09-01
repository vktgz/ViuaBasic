using System;
using System.Collections.Generic;

namespace ViuaBasic
{
  public class Variables
  {
    public enum Type
    {
      UNKNOWN,
      INTEGER,
      FLOAT,
      STRING,
      ARRAY
    }

    private Dictionary<string, int> names;
    private Dictionary<string, Type> types;
    private Dictionary<string, Type> array_types;

    public Variables()
    {
      names = new Dictionary<string, int>();
      types = new Dictionary<string, Type>();
      array_types = new Dictionary<string, Type>();
    }

    public bool exists(string name)
    {
      return names.ContainsKey(name);
    }

    public bool is_array(string name)
    {
      bool result = exists(name);
      if (result)
      {
        result = get_type(name).Equals(Type.ARRAY);
      }
      return result;
    }

    public void set_var(string name, Type type, int register)
    {
      if (exists(name))
      {
        throw new Exception("var " + name + " exists");
      }
      names.Add(name, register);
      types.Add(name, type);
    }

    public void set_array(string name, Type type, int register)
    {
      if (exists(name))
      {
        throw new Exception("var " + name + " exists");
      }
      names.Add(name, register);
      types.Add(name, Type.ARRAY);
      array_types.Add(name, type);
    }

    public int get_register(string name)
    {
      if (!exists(name))
      {
        throw new Exception("var " + name + " not exists");
      }
      return names[name];
    }

    public Type get_type(string name)
    {
      if (!exists(name))
      {
        throw new Exception("var " + name + " not exists");
      }
      return types[name];
    }

    public Type get_array_type(string name)
    {
      if (!exists(name))
      {
        throw new Exception("var " + name + " not exists");
      }
      if (!is_array(name))
      {
        throw new Exception("var " + name + " is not an array");
      }
      return array_types[name];
    }
  }
}
