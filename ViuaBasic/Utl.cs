using System;
using System.Collections.Generic;

namespace ViuaBasic
{
  public class Utl
  {
    public static List<string> split_line(string line)
    {
      List<string> parts = new List<string>();
      string part = "";
      bool in_quote = false;
      bool out_quote = false;
      foreach (char c in line)
      {
        if (Char.IsWhiteSpace(c))
        {
          if (in_quote)
          {
            part = part + c;
          }
          else
          {
            if (out_quote)
            {
              out_quote = false;
            }
            if (part.Length > 0)
            {
              parts.Add(part);
              part = "";
            }
          }
        }
        else if (c == '"')
        {
          part = part + c;
          if (in_quote)
          {
            in_quote = false;
            out_quote = true;
          }
          else if (out_quote)
          {
            out_quote = false;
            in_quote = true;
          }
          else
          {
            in_quote = true;
          }
        }
        else
        {
          if (out_quote)
          {
            out_quote = false;
          }
          part = part + c;
        }
      }
      if (part.Length > 0)
      {
        parts.Add(part);
      }
      return parts;
    }
  }
}
