using System;
using System.Collections.Generic;

namespace ViuaBasic
{
  public class BasicCompiler
  {
    private Dictionary<long, List<string>> listing;
    private List<string> labels, assembly;
    private List<long> goto_lines;
    private Variables vars;
    private int register;
    private bool math_mod, math_pow;

    public BasicCompiler()
    {
      listing = new Dictionary<long, List<string>>();
      labels = new List<string>();
      goto_lines = new List<long>();
      assembly = new List<string>();
      vars = new Variables();
      register = 1;
      math_mod = false;
      math_pow = false;
    }

    public bool load(List<string> src)
    {
      for (int i = 0; i < src.Count; i++)
      {
        long line_num = i + 1;
        List<string> parts = Utl.split_line(src[i]);
        if (parts.Count > 0)
        {
          if (parts.Count > 1)
          {
            try
            {
              line_num = Convert.ToInt64(parts[0]);
              parts.RemoveAt(0);
            }
            catch
            {
              line_num = i + 1;
            }
          }
          string instr = parts[0].ToUpper();
          if (instr.Equals("REM"))
          {
            continue;
          }
          if (listing.ContainsKey(line_num))
          {
            Console.WriteLine("?WARNING: DUPLICATE LINE");
            list(line_num, line_num);
          }
          listing.Add(line_num, parts);
          if (instr.Equals("LABEL"))
          {
            if (!parse_label(line_num, parts))
            {
              return false;
            }
          }
          if (instr.Equals("GOTO"))
          {
            if (!parse_goto_line(line_num, parts))
            {
              return false;
            }
          }
        }
      }
      return (listing.Count > 0);
    }

    public bool compile()
    {
      foreach (KeyValuePair<long, List<string>> line in listing)
      {
        if (goto_lines.Contains(line.Key))
        {
          assembly.Add(".mark: goto_line_" + line.Key.ToString());
        }
        string instr = line.Value[0].ToUpper();
        if (instr.Equals("LABEL"))
        {
          assembly.Add(".mark: " + line.Value[1]);
        }
        else if (instr.Equals("GOTO"))
        {
          if (!parse_goto(line.Key, line.Value))
          {
            return false;
          }
        }
        else if (instr.Equals("LIST"))
        {
          if (!parse_list(line.Key, line.Value))
          {
            return false;
          }
        }
        else if (instr.Equals("PRINT"))
        {
          if (!parse_print(line.Key, line.Value))
          {
            return false;
          }
        }
        else
        {
          Console.WriteLine("?SYNTAX ERROR: UNKNOWN INSTRUCTION: " + instr);
          list(line.Key, line.Key);
          return false;
        }
      }
      if (math_mod)
      {
        // TODO: mod function
      }
      if (math_pow)
      {
        // TODO: pow function
      }
      return true;
    }

    private void list(long line_from, long line_to)
    {
      foreach (KeyValuePair<long, List<string>> line in listing)
      {
        if ((line_from > 0) && (line.Key < line_from))
        {
          continue;
        }
        if ((line_to > 0) && (line.Key > line_to))
        {
          continue;
        }
        string buf = line.Key.ToString();
        foreach (string part in line.Value)
        {
          buf = buf + " " + part;
        }
        Console.WriteLine(buf);
      }
    }

    private bool parse_list(long line_num, List<string> parts)
    {
      List<string> args = new List<string>();
      if (parts.Count > 1)
      {
        args = Utl.split_separator(Utl.exp_to_str(parts.GetRange(1, parts.Count - 1)), ',', true, true);
      }
      long line_from = 0;
      long line_to = 0;
      bool result = true;
      if (args.Count > 0)
      {
        result = false;
        if (args.Count == 1)
        {
          try
          {
            line_from = Convert.ToInt64(args[0]);
          }
          catch
          {
            line_from = 0;
          }
          result = (line_from > 0);
        }
        if (args.Count == 2)
        {
          if (args[0] == ",")
          {
            try
            {
              line_to = Convert.ToInt64(args[1]);
            }
            catch
            {
              line_to = 0;
            }
            result = (line_to > 0);
          }
        }
        if (args.Count == 3)
        {
          if (args[1] == ",")
          {
            try
            {
              line_from = Convert.ToInt64(args[0]);
              line_to = Convert.ToInt64(args[2]);
            }
            catch
            {
              line_from = 0;
              line_to = 0;
            }
            result = (line_from > 0) && (line_to > 0);
          }
        }
      }
      if (result)
      {
        list(line_from, line_to);
      }
      else
      {
        Console.WriteLine("?SYNTAX ERROR: EXPECTING LINE NUMBER OR LABEL");
        list(line_num, line_num);
      }
      return result;
    }

    private bool parse_label(long line_num, List<string> parts)
    {
      if (parts.Count != 2)
      {
        Console.WriteLine("?SYNTAX ERROR: EXPECTING LABEL");
        list(line_num, line_num);
        return false;
      }
      if (labels.Contains(parts[1]))
      {
        Console.WriteLine("?SYNTAX ERROR: DUPLICATE LABEL");
        list(line_num, line_num);
        return false;
      }
      labels.Add(parts[1]);
      return true;
    }

    private bool parse_goto_line(long line_num, List<string> parts)
    {
      if (parts.Count != 2)
      {
        Console.WriteLine("?SYNTAX ERROR: EXPECTING LINE NUMBER OR LABEL");
        list(line_num, line_num);
        return false;
      }
      try
      {
        goto_lines.Add(Convert.ToInt64(parts[1]));
      }
      catch
      {
      }
      return true;
    }

    private bool parse_goto(long line_num, List<string> parts)
    {
      string label = parts[1];
      long goto_line = 0;
      try
      {
        goto_line = Convert.ToInt64(label);
      }
      catch
      {
        goto_line = 0;
      }
      if (labels.Contains(label))
      {
        assembly.Add("jump " + label);
      }
      else if (goto_lines.Contains(goto_line))
      {
        assembly.Add("jump goto_line_" + goto_line.ToString());
      }
      else
      {
        Console.WriteLine("?SYNTAX ERROR: EXPECTING LINE NUMBER OR LABEL");
        list(line_num, line_num);
        return false;
      }
      return true;
    }

    private bool parse_print(long line_num, List<string> parts)
    {
      if (parts.Count < 2)
      {
        Console.WriteLine("?SYNTAX ERROR: EXPECTING PRINT LIST");
        list(line_num, line_num);
        return false;
      }
      List<string> exp = parts.GetRange(1, parts.Count - 1);
      if (parse_print_list(register, exp))
      {
        assembly.Add("print %" + register + " local");
        return true;
      }
      else
      {
        list(line_num, line_num);
        return false;
      }
    }

    private bool parse_print_list(int plist_reg, List<string> exp)
    {
      List<string> plist = Utl.split_separator(Utl.exp_to_str(exp), ',', true, true);
      bool result = false;
      if ((plist.Count % 2) != 0)
      {
        result = true;
        int idx = 0;
        assembly.Add("text %" + plist_reg + " local \"\"");
        while (idx < plist.Count)
        {
          if (idx > 0)
          {
            result = result && (plist[idx - 1].Equals(","));
            if (!result)
            {
              break;
            }
          }
          if (vars.exists(plist[idx].ToUpper()))
          {
            assembly.Add("text %" + (plist_reg + 1) + " local %" + vars.get_register(plist[idx].ToUpper()) + " local");
            assembly.Add("textconcat %" + plist_reg + " local %" + plist_reg + " local %" + (plist_reg + 1) + " local");
          }
          else
          {
            int math_reg = plist_reg + 1;
            List<string> math_exp = new List<string>();
            math_exp.Add(plist[idx]);
            if (parse_float_exp(math_reg, math_exp, false))
            {
              assembly.Add("text %" + math_reg + " local %" + math_reg + " local");
              assembly.Add("textconcat %" + plist_reg + " local %" + plist_reg + " local %" + math_reg + " local");
            }
            else if (Utl.is_quoted(plist[idx]))
            {
              assembly.Add("text %" + (plist_reg + 1) + " local " + plist[idx]);
              assembly.Add("textconcat %" + plist_reg + " local %" + plist_reg + " local %" + (plist_reg + 1) + " local");
            }
            else
            {
              result = false;
              break;
            }
          }
          idx = idx + 2;
        }
      }
      if (!result)
      {
        Console.WriteLine("?SYNTAX ERROR: ILLEGAL PRINT LIST");
      }
      return result;
    }

    private bool parse_float_exp(int float_reg, List<string> exp, bool show_err)
    {
      List<string> rpn = Utl.exp_to_rpn(exp);
      assembly.Add("vec %" + (float_reg + 1) + " local");
      int stack = 0;
      foreach (string arg in rpn)
      {
        double num = 0;
        bool is_num = false;
        try
        {
          num = Convert.ToDouble(arg);
          is_num = true;
        }
        catch
        {
          is_num = false;
        }
        if (is_num)
        {
          assembly.Add("fstore %" + (float_reg + 2) + " local " + num);
          assembly.Add("vpush %" + (float_reg + 1) + " local %" + (float_reg + 2) + " local");
          stack++;
        }
        else
        {
          if (vars.exists(arg.ToUpper()))
          {
            if (vars.get_type(arg.ToUpper()).Equals(Variables.Type.FLOAT))
            {
              assembly.Add("vpush %" + (float_reg + 1) + " local %" + vars.get_register(arg.ToUpper()) + " local");
              stack++;
            }
            if (vars.get_type(arg.ToUpper()).Equals(Variables.Type.INTEGER))
            {
              assembly.Add("itof %" + (float_reg + 2) + " local %" + vars.get_register(arg.ToUpper()) + " local");
              assembly.Add("vpush %" + (float_reg + 1) + " local %" + (float_reg + 2) + " local");
              stack++;
            }
            if (vars.get_type(arg.ToUpper()).Equals(Variables.Type.STRING))
            {
              assembly.Add("stof %" + (float_reg + 2) + " local %" + vars.get_register(arg.ToUpper()) + " local");
              assembly.Add("vpush %" + (float_reg + 1) + " local %" + (float_reg + 2) + " local");
              stack++;
            }
          }
          else if (arg.Equals("+") || arg.Equals("-") || arg.Equals("*") || arg.Equals("/") || arg.Equals("%") || arg.Equals("^"))
          {
            if (stack < 2)
            {
              if (show_err)
              {
                Console.WriteLine("?SYNTAX ERROR: ILLEGAL ARITHMETIC EXPRESSION " + Utl.exp_to_str(exp));
              }
              return false;
            }
            else
            {
              assembly.Add("vpop %" + (float_reg + 2) + " local %" + (float_reg + 1) + " local");
              stack--;
              assembly.Add("vpop %" + (float_reg + 3) + " local %" + (float_reg + 1) + " local");
              stack--;
              if (arg.Equals("+"))
              {
                assembly.Add("add %" + (float_reg + 2) + " local %" + (float_reg + 3) + " local %" + (float_reg + 2) + " local");
                assembly.Add("vpush %" + (float_reg + 1) + " local %" + (float_reg + 2) + " local");
                stack++;
              }
              if (arg.Equals("-"))
              {
                assembly.Add("sub %" + (float_reg + 2) + " local %" + (float_reg + 3) + " local %" + (float_reg + 2) + " local");
                assembly.Add("vpush %" + (float_reg + 1) + " local %" + (float_reg + 2) + " local");
                stack++;
              }
              if (arg.Equals("*"))
              {
                assembly.Add("mul %" + (float_reg + 2) + " local %" + (float_reg + 3) + " local %" + (float_reg + 2) + " local");
                assembly.Add("vpush %" + (float_reg + 1) + " local %" + (float_reg + 2) + " local");
                stack++;
              }
              if (arg.Equals("/"))
              {
                assembly.Add("div %" + (float_reg + 2) + " local %" + (float_reg + 3) + " local %" + (float_reg + 2) + " local");
                assembly.Add("vpush %" + (float_reg + 1) + " local %" + (float_reg + 2) + " local");
                stack++;
              }
              if (arg.Equals("%"))
              {
                assembly.Add("frame ^[(param %0 %" + (float_reg + 3) + ") (param %1 %" + (float_reg + 2) + " local)]");
                assembly.Add("call %" + (float_reg + 2) + " local mod/2");
                assembly.Add("vpush %" + (float_reg + 1) + " local %" + (float_reg + 2) + " local");
                stack++;
                math_mod = true;
              }
              if (arg.Equals("^"))
              {
                assembly.Add("frame ^[(param %0 %" + (float_reg + 3) + ") (param %1 %" + (float_reg + 2) + " local)]");
                assembly.Add("call %" + (float_reg + 2) + " local pow/2");
                assembly.Add("vpush %" + (float_reg + 1) + " local %" + (float_reg + 2) + " local");
                stack++;
                math_pow = true;
              }
            }
          }
          else
          {
            if (show_err)
            {
              Console.WriteLine("?SYNTAX ERROR: ILLEGAL ARGUMENT " + arg + " IN ARITHMETIC EXPRESSION " + Utl.exp_to_str(exp));
            }
            return false;
          }
        }
      }
      if (stack.Equals(1))
      {
        assembly.Add("vpop %" + float_reg + " local %" + (float_reg + 1) + " local");
        return true;
      }
      else
      {
        if (show_err)
        {
          Console.WriteLine("?SYNTAX ERROR: ILLEGAL ARITHMETIC EXPRESSION " + Utl.exp_to_str(exp));
        }
        return false;
      }
    }
  }
}
