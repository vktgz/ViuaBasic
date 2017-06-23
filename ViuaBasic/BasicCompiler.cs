using System;
using System.Collections.Generic;

namespace ViuaBasic
{
  public class BasicCompiler
  {
    private struct ForLoop
    {
      public long line_num;
      public int register, from, to, step;
      public bool integer;
    }

    private Dictionary<long, List<string>> listing;
    private List<string> labels, assembly;
    private List<long> goto_lines;
    private Variables vars;
    private Dictionary<string, ForLoop> for_loops;
    private int register;
    private bool math_modulo, math_power, math_round;

    public BasicCompiler()
    {
      listing = new Dictionary<long, List<string>>();
      labels = new List<string>();
      goto_lines = new List<long>();
      assembly = new List<string>();
      vars = new Variables();
      for_loops = new Dictionary<string, ForLoop>();
      register = 1;
      math_modulo = false;
      math_power = false;
      math_round = false;
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
        else if (instr.Equals("LET"))
        {
          if (!parse_let(line.Key, line.Value))
          {
            return false;
          }
        }
        else if (instr.Equals("FOR"))
        {
          if (!parse_for(line.Key, line.Value))
          {
            return false;
          }
        }
        else if (instr.Equals("NEXT"))
        {
          if (!parse_next(line.Key, line.Value))
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
      if (math_modulo)
      {
        // TODO: modulo function
      }
      if (math_power)
      {
        // TODO: power function
      }
      if (math_round)
      {
        // TODO: round function
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
        if (args.Count.Equals(1))
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
        if (args.Count.Equals(2))
        {
          if (args[0].Equals(","))
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
        if (args.Count.Equals(3))
        {
          if (args[1].Equals(","))
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
      if (!parts.Count.Equals(2))
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
      if (!parts.Count.Equals(2))
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
      if (!(plist.Count % 2).Equals(0))
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

    private bool parse_integer_exp(int int_reg, List<string> exp, bool show_err)
    {
      if (parse_float_exp(int_reg + 1, exp, show_err))
      {
        assembly.Add("frame ^[(param %0 %" + (int_reg + 1) + " local)]");
        assembly.Add("call %" + (int_reg + 1) + " local round/1");
        assembly.Add("ftoi %" + int_reg + " local %" + (int_reg + 1) + " local");
        math_round = true;
        return true;
      }
      else
      {
        return false;
      }
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
                stack++;
              }
              if (arg.Equals("-"))
              {
                assembly.Add("sub %" + (float_reg + 2) + " local %" + (float_reg + 3) + " local %" + (float_reg + 2) + " local");
                stack++;
              }
              if (arg.Equals("*"))
              {
                assembly.Add("mul %" + (float_reg + 2) + " local %" + (float_reg + 3) + " local %" + (float_reg + 2) + " local");
                stack++;
              }
              if (arg.Equals("/"))
              {
                assembly.Add("div %" + (float_reg + 2) + " local %" + (float_reg + 3) + " local %" + (float_reg + 2) + " local");
                stack++;
              }
              if (arg.Equals("%"))
              {
                assembly.Add("frame ^[(param %0 %" + (float_reg + 3) + " local) (param %1 %" + (float_reg + 2) + " local)]");
                assembly.Add("call %" + (float_reg + 2) + " local mod/2");
                stack++;
                math_modulo = true;
              }
              if (arg.Equals("^"))
              {
                assembly.Add("frame ^[(param %0 %" + (float_reg + 3) + " local) (param %1 %" + (float_reg + 2) + " local)]");
                assembly.Add("call %" + (float_reg + 2) + " local pow/2");
                stack++;
                math_power = true;
              }
              assembly.Add("vpush %" + (float_reg + 1) + " local %" + (float_reg + 2) + " local");
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

    private bool parse_let(long line_num, List<string> parts)
    {
      bool res = false;
      if (parts.Count > 3)
      {
        string var_name = parts[1].ToUpper();
        Variables.Type var_type = Variables.Type.UNKNOWN;
        int idx = 0;
        if (parts[2].Equals(":"))
        {
          if (parts.Count > 5)
          {
            if (parts[4].Equals("="))
            {
              idx = 5;
              if (vars.exists(var_name))
              {
                var_type = vars.get_type(var_name);
                if (!var_type.ToString().Equals(parts[3].ToUpper()))
                {
                  Console.WriteLine("?RUNTIME ERROR: VARIABLE " + var_name + " IS OF TYPE " + var_type.ToString());
                }
              }
              else
              {
                if (parts[3].ToUpper().Equals("INTEGER"))
                {
                  var_type = Variables.Type.INTEGER;
                  res = true;
                }
                if (parts[3].ToUpper().Equals("FLOAT"))
                {
                  var_type = Variables.Type.FLOAT;
                  res = true;
                }
                if (parts[3].ToUpper().Equals("STRING"))
                {
                  var_type = Variables.Type.STRING;
                  res = true;
                }
              }
            }
          }
        }
        else
        {
          if (parts[2].Equals("="))
          {
            idx = 3;
            if (vars.exists(var_name))
            {
              var_type = vars.get_type(var_name);
              res = true;
            }
            else
            {
              Console.WriteLine("?RUNTIME ERROR: EXPECTING TYPE FOR NEW VARIABLE " + var_name);
            }
          }
        }
        if (res)
        {
          res = false;
          if (var_type.Equals(Variables.Type.INTEGER))
          {
            List<string> exp = parts.GetRange(idx, parts.Count - idx);
            if (parse_integer_exp(register, exp, true))
            {
              vars.set_var(var_name, var_type, register++);
              res = true;
            }
          }
          if (var_type.Equals(Variables.Type.FLOAT))
          {
            List<string> exp = parts.GetRange(idx, parts.Count - idx);
            if (parse_float_exp(register, exp, true))
            {
              vars.set_var(var_name, var_type, register++);
              res = true;
            }
          }
          if (var_type.Equals(Variables.Type.STRING))
          {
            List<string> exp = parts.GetRange(idx, parts.Count - idx);
            if (parse_print_list(register, exp))
            {
              vars.set_var(var_name, var_type, register++);
              res = true;
            }
          }
        }
      }
      if (!res)
      {
        Console.WriteLine("?SYNTAX ERROR: ILLEGAL ASSIGNMENT");
        list(line_num, line_num);
      }
      return res;
    }

    private bool parse_for(long line_num, List<string> parts)
    {
      bool res = false;
      ForLoop for_loop = new ForLoop();
      for_loop.line_num = line_num;
      bool stop = false;
      int idx = 0;
      if (parts.Count > 5)
      {
        if (parts[2].Equals("="))
        {
          string var_name = parts[1].ToUpper();
          if (for_loops.ContainsKey(var_name))
          {
            Console.WriteLine("?SYNTAX ERROR: NESTED LOOP FOR VARIABLE " + var_name);
            list(for_loops[var_name].line_num, line_num);
            return false;
          }
          if (vars.exists(var_name))
          {
            if (vars.get_type(var_name).Equals(Variables.Type.INTEGER))
            {
              for_loop.register = vars.get_register(var_name);
              for_loop.integer = true;
            }
            else if (vars.get_type(var_name).Equals(Variables.Type.FLOAT))
            {
              for_loop.register = vars.get_register(var_name);
              for_loop.integer = false;
            }
            else
            {
              Console.WriteLine("?SYNTAX ERROR: VARIABLE " + var_name + " IS NOT NUMERIC");
              list(line_num, line_num);
              return false;
            }
          }
          else
          {
            for_loop.register = register++;
            for_loop.integer = true;
            vars.set_var(var_name, Variables.Type.INTEGER, for_loop.register);
          }
          List<string> from_exp = Utl.take_until(3, "TO", parts);
          if ((for_loop.integer && parse_integer_exp(register, from_exp, true)) || ((!for_loop.integer) && parse_float_exp(register, from_exp, true)))
          {
            for_loop.from = register++;
            idx = 3 + from_exp.Count;
          }
          else
          {
            Console.WriteLine("?SYNTAX ERROR: EXPECTING NUMERIC FROM EXPRESSION");
            list(line_num, line_num);
            return false;
          }
          stop = true;
          if (parts.Count > (idx + 1))
          {
            if (parts[idx].ToUpper().Equals("TO"))
            {
              List<string> to_exp = Utl.take_until(idx + 1, "STEP", parts);
              if ((for_loop.integer && parse_integer_exp(register, to_exp, true)) || ((!for_loop.integer) && parse_float_exp(register, to_exp, true)))
              {
                for_loop.to = register++;
                idx = idx + 1 + to_exp.Count;
                stop = false;
              }
            }
          }
          if (stop)
          {
            Console.WriteLine("?SYNTAX ERROR: EXPECTING NUMERIC TO EXPRESSION");
            list(line_num, line_num);
            return false;
          }
          for_loop.step = 0;
          if (parts.Count > idx)
          {
            stop = true;
            if (parts.Count > (idx + 1))
            {
              if (parts[idx].ToUpper().Equals("STEP"))
              {
                List<string> step_exp = parts.GetRange(idx + 1, parts.Count - (idx + 1));
                if ((for_loop.integer && parse_integer_exp(register, step_exp, true)) || ((!for_loop.integer) && parse_float_exp(register, step_exp, true)))
                {
                  for_loop.step = register++;
                  stop = false;
                }
              }
            }
            if (stop)
            {
              Console.WriteLine("?SYNTAX ERROR: EXPECTING NUMERIC STEP EXPRESSION");
              list(line_num, line_num);
              return false;
            }
          }
          if (for_loop.step.Equals(0))
          {
            for_loop.step = register++;
            if (for_loop.integer)
            {
              assembly.Add("istore %" + for_loop.step + " local 1");
            }
            else
            {
              assembly.Add("fstore %" + for_loop.step + " local 1");
            }
          }
          assembly.Add("copy %" + for_loop.register + " local %" + for_loop.from + " local");
          assembly.Add(".mark: for_" + for_loop.line_num + "_begin");
          assembly.Add("if (lt %" + register + " local %" + for_loop.step + " local (fstore %" + (register + 1) + " local 0)) for_" + for_loop.line_num + "_descend");
          assembly.Add("if (gt %" + register + " local %" + for_loop.register + " local %" + for_loop.to + " local) for_" + for_loop.line_num + "_end");
          assembly.Add("jump for_" + for_loop.line_num + "_step");
          assembly.Add(".mark: for_" + for_loop.line_num + "_descend");
          assembly.Add("if (lt %" + register + " local %" + for_loop.register + " local %" + for_loop.to + " local) for_" + for_loop.line_num + "_end");
          assembly.Add(".mark: for_" + for_loop.line_num + "_step");
          for_loops[var_name] = for_loop;
          res = true;
        }
      }
      if (!res)
      {
        Console.WriteLine("?SYNTAX ERROR: ILLEGAL LOOP ARGUMENT");
        list(line_num, line_num);
      }
      return res;
    }

    private bool parse_next(long line_num, List<string> parts)
    {
      if (parts.Count.Equals(2))
      {
        string var_name = parts[1].ToUpper();
        if (for_loops.ContainsKey(var_name))
        {
          ForLoop for_loop = for_loops[var_name];
          assembly.Add("add %" + for_loop.register + " local %" + for_loop.register + " local %" + for_loop.step + " local");
          assembly.Add("jump for_" + for_loop.line_num + "_begin");
          assembly.Add(".mark: for_" + for_loop.line_num + "_end");
          for_loops.Remove(var_name);
          return true;
        }
        else
        {
          Console.WriteLine("?SYNTAX ERROR: UNKNOWN LOOP VARIABLE " + var_name);
          list(line_num, line_num);
          return false;
        }
      }
      else
      {
        Console.WriteLine("?SYNTAX ERROR: EXPECTING FOR LOOP VARIABLE");
        list(line_num, line_num);
        return false;
      }
    }
  }
}
