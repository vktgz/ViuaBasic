package org.viuavm.viuabasic;

import java.util.HashMap;

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

  private HashMap<String, Integer> names;
  private HashMap<String, Type> types;
  private HashMap<String, Type> array_types;

  public Variables()
  {
    names = new HashMap<String, Integer>();
    types = new HashMap<String, Type>();
    array_types = new HashMap<String, Type>();
  }

  public boolean exists(String name)
  {
    return names.containsKey(name);
  }

  public boolean is_array(String name)
  {
    boolean result = exists(name);
    if (result)
    {
      result = get_type(name).equals(Type.ARRAY);
    }
    return result;
  }

  public void set_var(String name, Type type, int register)
  {
    if (exists(name))
    {
      throw new RuntimeException("var " + name + " exists");
    }
    names.put(name, register);
    types.put(name, type);
  }

  public void set_array(String name, Type type, int register)
  {
    if (exists(name))
    {
      throw new RuntimeException("var " + name + " exists");
    }
    names.put(name, register);
    types.put(name, Type.ARRAY);
    array_types.put(name, type);
  }

  public int get_register(String name)
  {
    if (!exists(name))
    {
      throw new RuntimeException("var " + name + " not exists");
    }
    return names.get(name);
  }

  public Type get_type(String name)
  {
    if (!exists(name))
    {
      throw new RuntimeException("var " + name + " not exists");
    }
    return types.get(name);
  }

  public Type get_array_type(String name)
  {
    if (!exists(name))
    {
      throw new RuntimeException("var " + name + " not exists");
    }
    if (!is_array(name))
    {
      throw new RuntimeException("var " + name + " is not an array");
    }
    return array_types.get(name);
  }
}
