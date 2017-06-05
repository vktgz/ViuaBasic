using System;
using System.Collections.Generic;

namespace ViuaBasic
{
  public class BasicCompiler
  {
    private Dictionary<int, string> listing;

    public BasicCompiler()
    {
      listing = new Dictionary<int, string>();
    }

    public bool load(List<string> src)
    {
      for (int i = 0; i < src.Count; i++)
      {
        List<string> parts = Utl.split_line(src[i]);
      }
      return true;
    }

    public void run()
    {
    }
  }
}

