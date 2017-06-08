using System;
using System.Collections.Generic;

namespace ViuaBasic
{
  public class Variables
  {
    public enum Type
    {
      INTEGER,
      FLOAT,
      STRING
    }

    private Dictionary<string, int> names;
    private Dictionary<string, Type> types;

    public Variables()
    {
      names = new Dictionary<string, int>();
      types = new Dictionary<string, Type>();
    }

    public bool exists(string name)
    {
      return names.ContainsKey(name);
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
  }
}
