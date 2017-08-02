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
        else if (c.Equals('"'))
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
        if (c.Equals(sep))
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
        else if (c.Equals('"'))
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

    public static string exp_to_str(List<string> arg)
    {
      string buf = "";
      foreach (string s in arg)
      {
        buf = buf + s;
      }
      return buf;
    }

    public static List<string> exp_to_math_rpn(List<string> exp)
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

    public static List<string> exp_to_logic_rpn(List<string> exp)
    {
      List<string> cond = exp_to_logic(exp);
      List<string> rpn = new List<string>();
      Stack<string> stack = new Stack<string>();
      foreach (string arg in cond)
      {
        if (arg.Equals("("))
        {
          stack.Push(arg);
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
        }
        else if (arg.Equals("=") || arg.Equals(">") || arg.Equals("<") || arg.Equals(">=") || arg.Equals("<=") || arg.Equals("<>"))
        {
          while (stack.Count > 0)
          {
            if (stack.Peek().Equals("=") || stack.Peek().Equals(">") || stack.Peek().Equals("<") || stack.Peek().Equals(">=") || stack.Peek().Equals("<=") || stack.Peek().Equals("<>") || stack.Peek().Equals("OR") || stack.Peek().Equals("AND") || stack.Peek().Equals("NOT"))
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
        }
        else if (arg.Equals("OR"))
        {
          while (stack.Count > 0)
          {
            if (stack.Peek().Equals("OR") || stack.Peek().Equals("AND") || stack.Peek().Equals("NOT"))
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
        }
        else if (arg.Equals("AND"))
        {
          while (stack.Count > 0)
          {
            if (stack.Peek().Equals("AND") || stack.Peek().Equals("NOT"))
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
        }
        else if (arg.Equals("NOT"))
        {
          while (stack.Count > 0)
          {
            if (stack.Peek().Equals("NOT"))
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
        }
        else
        {
          rpn.Add(arg);
        }
      }
      while (stack.Count > 0)
      {
        rpn.Add(stack.Pop());
      }
      return rpn;
    }

    public static List<string> exp_to_logic(List<string> exp)
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
        tmp1.AddRange(split_separator(elem, '<', true, true));
      }
      tmp2.Clear();
      foreach (string elem in tmp1)
      {
        tmp2.AddRange(split_separator(elem, '>', true, true));
      }
      tmp1.Clear();
      foreach (string elem in tmp2)
      {
        tmp1.AddRange(split_separator(elem, '=', true, true));
      }
      tmp2.Clear();
      int idx = 0;
      while (idx < tmp1.Count)
      {
        if ((idx + 1) < tmp1.Count)
        {
          if (tmp1[idx].Equals("<"))
          {
            if (tmp1[idx + 1].Equals("="))
            {
              tmp2.Add("<=");
              idx = idx + 2;
              continue;
            }
            if (tmp1[idx + 1].Equals(">"))
            {
              tmp2.Add("<>");
              idx = idx + 2;
              continue;
            }
          }
          if (tmp1[idx].Equals(">"))
          {
            if (tmp1[idx + 1].Equals("="))
            {
              tmp2.Add(">=");
              idx = idx + 2;
              continue;
            }
          }
        }
        if (tmp1[idx].ToUpper().Equals("OR"))
        {
          tmp2.Add("OR");
        }
        else if (tmp1[idx].ToUpper().Equals("AND"))
        {
          tmp2.Add("AND");
        }
        else if (tmp1[idx].ToUpper().Equals("NOT"))
        {
          tmp2.Add("NOT");
        }
        else
        {
          tmp2.Add(tmp1[idx]);
        }
        idx++;
      }
      idx = 0;
      tmp1.Clear();
      while (idx < tmp2.Count)
      {
        if (tmp2[idx].Equals("(") && ((idx + 1) < tmp2.Count))
        {
          int math_idx = idx + 1;
          int math_cnt = 0;
          string math = "";
          while (math_idx < tmp2.Count)
          {
            if (tmp2[math_idx].Equals("("))
            {
              math_cnt++;
              math = math + tmp2[math_idx];
              math_idx++;
              continue;
            }
            else if (tmp2[math_idx].Equals(")"))
            {
              if (math_cnt > 0)
              {
                math_cnt--;
                math = math + tmp2[math_idx];
                math_idx++;
                continue;
              }
              else
              {
                break;
              }
            }
            else if (tmp2[math_idx].Equals("=") || tmp2[math_idx].Equals(">") || tmp2[math_idx].Equals("<") || tmp2[math_idx].Equals(">=") || tmp2[math_idx].Equals("<=") || tmp2[math_idx].Equals("<>") || tmp2[math_idx].Equals("OR") || tmp2[math_idx].Equals("AND") || tmp2[math_idx].Equals("NOT"))
            {
              math = "";
              break;
            }
          }
          if (math.Length.Equals(0))
          {
            tmp1.Add(tmp2[idx]);
            idx++;
          }
          else
          {
            tmp1.Add(math);
            idx = math_idx + 1;
          }
        }
        else
        {
          tmp1.Add(tmp2[idx]);
          idx++;
        }
      }
      return tmp1;
    }

    public static List<string> take_until(int from_idx, string to_ident, List<string> parts)
    {
      List<string> res = new List<string>();
      int idx = from_idx;
      while (idx < parts.Count)
      {
        if (parts[idx].ToUpper().Equals(to_ident))
        {
          return res;
        }
        res.Add(parts[idx++]);
      }
      return res;
    }
  }
}
