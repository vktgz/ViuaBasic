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

    public static List<string> split_separator(string line, char sep, bool trim, bool keep_sep)
    {
      List<string> parts = new List<string>();
      string part = "";
      bool in_quote = false;
      bool out_quote = false;
      foreach (char c in line)
      {
        if (c == sep)
        {
          if (in_quote)
          {
            part = part + c;
          }
          else
          {
            if (trim)
            {
              part = part.Trim();
            }
            if (part.Length > 0)
            {
              parts.Add(part);
            }
            part = "";
            if (keep_sep)
            {
              parts.Add(part + c);
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
          part = part + c;
          if (out_quote)
          {
            out_quote = false;
          }
        }
      }
      if (trim)
      {
        part = part.Trim();
      }
      if (part.Length > 0)
      {
        parts.Add(part);
      }
      return parts;
    }

    public static bool is_quoted(string arg)
    {
      return (arg.Length > 1) && (arg.StartsWith("\"")) && (arg.EndsWith("\""));
    }

    public static string flat_list(List<string> arg)
    {
      string buf = "";
      foreach (string s in arg)
      {
        buf = buf + s;
      }
      return buf;
    }
  }
}
