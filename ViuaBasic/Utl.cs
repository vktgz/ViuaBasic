﻿using System;
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

    public static List<string> exp_to_rpn(List<string> exp)
    {
      List<string> math = exp_to_math(exp);
      List<string> rpn = new List<string>();
      Stack<string> stack = new Stack<string>();
      bool unary = true;
      bool negate = false;
      foreach (string arg in math)
      {
        if (arg.Equals("("))
        {
          if (negate)
          {
            rpn.Add("-1");
            while (stack.Count > 0)
            {
              if (stack.Peek().Equals("*") || stack.Peek().Equals("/") || stack.Peek().Equals("%") || stack.Peek().Equals("^"))
              {
                rpn.Add(stack.Pop());
                continue;
              }
              else
              {
                break;
              }
            }
            stack.Push("*");
            negate = false;
          }
          stack.Push(arg);
          unary = true;
        }
        else if (arg.Equals(")"))
        {
          while (stack.Count > 0)
          {
            string elem = stack.Pop();
            if (elem.Equals("("))
            {
              break;
            }
            else
            {
              rpn.Add(elem);
            }
          }
          unary = false;
          negate = false;
        }
        else if (arg.Equals("+") || arg.Equals("-"))
        {
          if (unary)
          {
            if (arg.Equals("-"))
            {
              negate = !negate;
            }
          }
          else
          {
            while (stack.Count > 0)
            {
              if (stack.Peek().Equals("+") || stack.Peek().Equals("-") || stack.Peek().Equals("*") || stack.Peek().Equals("/") || stack.Peek().Equals("%") || stack.Peek().Equals("^"))
              {
                rpn.Add(stack.Pop());
                continue;
              }
              else
              {
                break;
              }
            }
            stack.Push(arg);
            unary = true;
            negate = false;
          }
        }
        else if (arg.Equals("*") || arg.Equals("/") || arg.Equals("%"))
        {
          while (stack.Count > 0)
          {
            if (stack.Peek().Equals("*") || stack.Peek().Equals("/") || stack.Peek().Equals("%") || stack.Peek().Equals("^"))
            {
              rpn.Add(stack.Pop());
              continue;
            }
            else
            {
              break;
            }
          }
          stack.Push(arg);
          unary = true;
          negate = false;
        }
        else if (arg.Equals("^"))
        {
          while (stack.Count > 0)
          {
            if (stack.Peek().Equals("^"))
            {
              rpn.Add(stack.Pop());
              continue;
            }
            else
            {
              break;
            }
          }
          stack.Push(arg);
          unary = true;
          negate = false;
        }
        else
        {
          if (negate)
          {
            rpn.Add("-" + arg);
          }
          else
          {
            rpn.Add(arg);
          }
          unary = false;
          negate = false;
        }
      }
      while (stack.Count > 0)
      {
        rpn.Add(stack.Pop());
      }
      return rpn;
    }

    public static List<string> exp_to_math(List<string> exp)
    {
      List<string> tmp1 = new List<string>();
      foreach (string elem in exp)
      {
        tmp1.AddRange(split_separator(elem, '(', true, true));
      }
      List<string> tmp2 = new List<string>();
      foreach (string elem in tmp1)
      {
        tmp2.AddRange(split_separator(elem, ')', true, true));
      }
      tmp1.Clear();
      foreach (string elem in tmp2)
      {
        tmp1.AddRange(split_separator(elem, '+', true, true));
      }
      tmp2.Clear();
      foreach (string elem in tmp1)
      {
        tmp2.AddRange(split_separator(elem, '-', true, true));
      }
      tmp1.Clear();
      foreach (string elem in tmp2)
      {
        tmp1.AddRange(split_separator(elem, '*', true, true));
      }
      tmp2.Clear();
      foreach (string elem in tmp1)
      {
        tmp2.AddRange(split_separator(elem, '/', true, true));
      }
      tmp1.Clear();
      foreach (string elem in tmp2)
      {
        tmp1.AddRange(split_separator(elem, '%', true, true));
      }
      tmp2.Clear();
      foreach (string elem in tmp1)
      {
        tmp2.AddRange(split_separator(elem, '^', true, true));
      }
      return tmp2;
    }
  }
}
