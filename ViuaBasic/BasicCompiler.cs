using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

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

    private SortedDictionary<long, List<string>> listing;
    private List<string> labels, assembly;
    private List<long> goto_lines;
    private Variables vars;
    private Dictionary<string, ForLoop> for_loops;
    private Stack<int> nested_ifs;
    private int register, if_idx;
    private bool math_modulo, math_power, math_round, math_exponent, math_logarithm, math_absolute, use_array;

    public BasicCompiler()
    {
      listing = new SortedDictionary<long, List<string>>();
      labels = new List<string>();
      goto_lines = new List<long>();
      assembly = new List<string>();
      vars = new Variables();
      for_loops = new Dictionary<string, ForLoop>();
      nested_ifs = new Stack<int>();
      register = 1;
      if_idx = 0;
      math_modulo = false;
      math_power = false;
      math_round = false;
      math_exponent = false;
      math_logarithm = false;
      math_absolute = false;
      use_array = false;
      CultureInfo format = CultureInfo.CreateSpecificCulture("en-US");
      format.NumberFormat.NumberDecimalSeparator = ".";
      Thread.CurrentThread.CurrentCulture = format;
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
          if (instr.Equals("IF"))
          {
            if (!parse_if_label(line_num, parts))
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
      assembly.Add(".function: main/0");
      foreach (KeyValuePair<long, List<string>> line in listing)
      {
        if (goto_lines.Contains(line.Key))
        {
          assembly.Add(".mark: goto_line_" + line.Key.ToString());
        }
        string instr = line.Value[0].ToUpper();
        if (instr.Equals("LABEL"))
        {
          assembly.Add(".mark: label_" + line.Value[1].ToLower());
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
        else if (instr.Equals("IF"))
        {
          if (!parse_if(line.Key, line.Value))
          {
            return false;
          }
        }
        else if (instr.Equals("ELSE"))
        {
          if (!parse_else(line.Key, line.Value))
          {
            return false;
          }
        }
        else if (instr.Equals("ENDIF"))
        {
          if (!parse_endif(line.Key, line.Value))
          {
            return false;
          }
        }
        else if (instr.Equals("DIM"))
        {
          if (!parse_dim(line.Key, line.Value))
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
      assembly.Add("izero %0 local");
      assembly.Add("return");
      assembly.Add(".end");
      if (math_modulo || math_power)
      {
        assembly.Add(".function: mod/2");
        assembly.Add("arg %2 local %1");
        assembly.Add("arg %3 local %0");
        assembly.Add("if (not (eq %5 local %2 local (fstore %4 local 0))) mod_not_zero");
        assembly.Add("throw (strstore %1 local \"modulo by zero\")");
        assembly.Add(".mark: mod_not_zero");
        assembly.Add("if (lt %5 local %2 local (fstore %4 local 0)) mod_negative");
        assembly.Add("fstore %6 local 0");
        assembly.Add("copy %7 local %2 local");
        assembly.Add("jump mod_prepare");
        assembly.Add(".mark: mod_negative");
        assembly.Add("copy %6 local %2 local");
        assembly.Add("fstore %7 local 0");
        assembly.Add(".mark: mod_prepare");
        assembly.Add("copy %8 local %2 local");
        assembly.Add("if (lt %5 local %3 local (fstore %4 local 0)) mod_check_step");
        assembly.Add("if (gt %5 local %8 local (fstore %4 local 0)) mod_negate_step mod_check");
        assembly.Add(".mark: mod_check_step");
        assembly.Add("if (gt %5 local %8 local (fstore %4 local 0)) mod_check");
        assembly.Add(".mark: mod_negate_step");
        assembly.Add("mul %8 local %8 local (fstore %4 local -1)");
        assembly.Add(".mark: mod_check");
        assembly.Add("if (not (gte %5 local %3 local %6 local)) mod_add");
        assembly.Add("if (lte %5 local %3 local %7 local) mod_done");
        assembly.Add(".mark: mod_add");
        assembly.Add("add %3 local %3 local %8 local");
        assembly.Add("jump mod_check");
        assembly.Add(".mark: mod_done");
        assembly.Add("copy %0 local %3 local");
        assembly.Add("return");
        assembly.Add(".end");
      }
      if (math_power)
      {
        assembly.Add(".function: pow/2");
        assembly.Add("arg %1 local %0");
        assembly.Add("arg %2 local %1");
        assembly.Add("if (not (eq %3 local %2 local (fstore %4 local 1))) pow_pow_zero");
        assembly.Add("copy %0 local %1 local");
        assembly.Add("jump pow_done");
        assembly.Add(".mark: pow_pow_zero");
        assembly.Add("if (not (eq %3 local %2 local (fstore %4 local 0))) pow_pow_minus1");
        assembly.Add("if (not (eq %3 local %1 local (fstore %4 local 0))) pow_pow_zero_base_not_zero");
        assembly.Add("throw (strstore %5 local \"0 ^ 0 is undefined\")");
        assembly.Add(".mark: pow_pow_zero_base_not_zero");
        assembly.Add("fstore %0 local 1");
        assembly.Add("jump pow_done");
        assembly.Add(".mark: pow_pow_minus1");
        assembly.Add("if (not (eq %3 local %2 local (fstore %4 local -1))) pow_base_plus1");
        assembly.Add("if (not (eq %3 local %1 local (fstore %4 local 0))) pow_pow_minus1_base_not_zero");
        assembly.Add("throw (strstore %5 local \"divide by zero\")");
        assembly.Add(".mark: pow_pow_minus1_base_not_zero");
        assembly.Add("div %0 local (fstore %4 local 1) %1 local");
        assembly.Add("jump pow_done");
        assembly.Add(".mark: pow_base_plus1");
        assembly.Add("if (not (eq %3 local %1 local (fstore %4 local 1))) pow_base_minus1");
        assembly.Add("fstore %0 local 1");
        assembly.Add("jump pow_done");
        assembly.Add(".mark: pow_base_minus1");
        assembly.Add("if (not (eq %3 local %1 local (fstore %4 local -1))) pow_pow_int");
        assembly.Add("frame ^[(param %0 %2 local)]");
        assembly.Add("call %6 local abs/1");
        assembly.Add("frame ^[(param %0 %6 local) (param %1 (fstore %4 local 2))]");
        assembly.Add("call %6 local mod/2");
        assembly.Add("if (eq %3 local %6 local (fstore %4 local 0)) pow_base_minus1_positive");
        assembly.Add("fstore %0 local -1");
        assembly.Add("jump pow_done");
        assembly.Add(".mark: pow_base_minus1_positive");
        assembly.Add("fstore %0 local 1");
        assembly.Add("jump pow_done");
        assembly.Add(".mark: pow_pow_int");
        assembly.Add("if (not (eq %3 local %2 local (ftoi %4 local %2 local))) pow_other");
        assembly.Add("if (lte %3 local %2 local (fstore %4 local 0)) pow_pow_int_negative");
        assembly.Add("frame ^[(param %0 %1 local) (param %1 %2 local)]");
        assembly.Add("call %0 local simple_pow/2");
        assembly.Add("jump pow_done");
        assembly.Add(".mark: pow_pow_int_negative");
        assembly.Add("if (not (eq %3 local %1 local (fstore %4 local 0))) pow_pow_int_negative_base_not_zero");
        assembly.Add("throw (strstore %5 local \"divide by zero\")");
        assembly.Add(".mark: pow_pow_int_negative_base_not_zero");
        assembly.Add("mul %6 local %2 local (fstore %4 local -1)");
        assembly.Add("frame ^[(param %0 %1 local) (param %1 %6 local)]");
        assembly.Add("call %6 local simple_pow/2");
        assembly.Add("div %0 local (fstore %4 local 1) %6 local");
        assembly.Add("jump pow_done");
        assembly.Add(".mark: pow_other");
        assembly.Add("if (lt %3 local %1 local (fstore %4 local 0)) pow_other_base_negative");
        assembly.Add("frame ^[(param %0 %1 local) (param %1 %2 local)]");
        assembly.Add("call %0 local complicated_pow/2");
        assembly.Add("jump pow_done");
        assembly.Add(".mark: pow_other_base_negative");
        assembly.Add("throw (strstore %5 local \"result is complex number\")");
        assembly.Add(".mark: pow_done");
        assembly.Add("return");
        assembly.Add(".end");
        assembly.Add(".function: simple_pow/2");
        assembly.Add("arg %1 local %0");
        assembly.Add("arg %2 local %1");
        assembly.Add("fstore %0 local 1");
        assembly.Add("istore %4 local 1");
        assembly.Add(".mark: spow_loop");
        assembly.Add("mul %0 local %0 local %1 local");
        assembly.Add("iinc %4 local");
        assembly.Add("if (lte %3 local %4 local %2 local) spow_loop");
        assembly.Add("return");
        assembly.Add(".end");
        assembly.Add(".function: complicated_pow/2");
        assembly.Add("arg %1 local %0");
        assembly.Add("arg %2 local %1");
        assembly.Add("frame ^[(param %0 %2 local)]");
        assembly.Add("call %3 local abs/1");
        assembly.Add("ftoi %4 local %3 local");
        assembly.Add("sub %5 local %3 local %4 local");
        assembly.Add("frame ^[(param %0 %1 local) (param %1 %4 local)]");
        assembly.Add("call %0 local simple_pow/2");
        assembly.Add("frame ^[(param %0 %1 local)]");
        assembly.Add("call %6 local log/1");
        assembly.Add("mul %6 local %5 local %6 local");
        assembly.Add("frame ^[(param %0 %6 local)]");
        assembly.Add("call %6 local exp/1");
        assembly.Add("mul %0 local %0 local %6 local");
        assembly.Add("if (gt %7 local %2 local (fstore %8 local 0)) cpow_done");
        assembly.Add("div %0 local (fstore %8 local 1) %0 local");
        assembly.Add(".mark: cpow_done");
        assembly.Add("return");
        assembly.Add(".end");
      }
      if (math_round)
      {
        assembly.Add(".function: round/1");
        assembly.Add("arg %1 local %0");
        assembly.Add("if (lt %3 local %1 local (fstore %2 local 0)) round_negative");
        assembly.Add("fstore %2 local 0.5");
        assembly.Add("jump math_round");
        assembly.Add(".mark: round_negative");
        assembly.Add("fstore %2 local -0.5");
        assembly.Add(".mark: math_round");
        assembly.Add("add %1 local %1 local %2 local");
        assembly.Add("ftoi %0 local %1 local");
        assembly.Add("return");
        assembly.Add(".end");
      }
      if (math_exponent || math_power)
      {
        assembly.Add(".function: exp/1");
        assembly.Add("arg %1 local %0");
        assembly.Add("fstore %0 local 1");
        assembly.Add("copy %2 local %1 local");
        assembly.Add("fstore %3 local 1");
        assembly.Add("istore %4 local 1");
        assembly.Add("fstore %7 local 0.00000000000000000000000000000001");
        assembly.Add("frame ^[(param %0 %2 local)]");
        assembly.Add("call %8 local abs/1");
        assembly.Add(".mark: exp_loop");
        assembly.Add("div %5 local %2 local %3 local");
        assembly.Add("div %9 local %8 local %3 local");
        assembly.Add("add %0 local %0 local %5 local");
        assembly.Add("mul %2 local %2 local %1 local");
        assembly.Add("iinc %4 local");
        assembly.Add("mul %3 local %3 local %4 local");
        assembly.Add("if (gte %6 local %9 local %7 local) exp_loop");
        assembly.Add("return");
        assembly.Add(".end");
      }
      if (math_logarithm || math_power)
      {
        assembly.Add(".function: log/1");
        assembly.Add("arg %1 local %0");
        assembly.Add("if (gt %3 local %1 local (fstore %2 local 0)) log_positive");
        assembly.Add("throw (strstore %4 local \"logarithm argument must be greater than zero\")");
        assembly.Add(".mark: log_positive");
        assembly.Add("if (not (isnull %3 local %1 global)) log_begin");
        assembly.Add("frame ^[(param %0 (fstore %2 local 1.9))]");
        assembly.Add("call %1 global series_log/1");
        assembly.Add(".mark: log_begin");
        assembly.Add("fstore %0 local 0");
        assembly.Add("if (lt %3 local %1 local (fstore %2 local 2)) log_rest");
        assembly.Add("istore %4 local 0");
        assembly.Add("fstore %5 local 1.9");
        assembly.Add("fstore %6 local 2");
        assembly.Add(".mark: log_divide");
        assembly.Add("div %1 local %1 local %5 local");
        assembly.Add("iinc %4 local");
        assembly.Add("if (gte %3 local %1 local %6 local) log_divide");
        assembly.Add("mul %0 local %1 global %4 local");
        assembly.Add(".mark: log_rest");
        assembly.Add("frame ^[(param %0 %1 local)]");
        assembly.Add("call %2 local series_log/1");
        assembly.Add("add %0 local %0 local %2 local");
        assembly.Add("return");
        assembly.Add(".end");
        assembly.Add(".function: series_log/1");
        assembly.Add("arg %1 local %0");
        assembly.Add("sub %1 local %1 local (fstore %2 local 1)");
        assembly.Add("copy %2 local %1 local");
        assembly.Add("fstore %3 local 1");
        assembly.Add("fstore %0 local 0");
        assembly.Add("istore %4 local 1");
        assembly.Add("fstore %5 local 0.00000000000000000000000000000001");
        assembly.Add(".mark: series_log_loop");
        assembly.Add("div %6 local %2 local %4 local");
        assembly.Add("copy %8 local %6 local");
        assembly.Add("mul %6 local %3 local %6 local");
        assembly.Add("add %0 local %0 local %6 local");
        assembly.Add("mul %2 local %2 local %1 local");
        assembly.Add("mul %3 local %3 local (fstore %6 local -1)");
        assembly.Add("iinc %4 local");
        assembly.Add("if (gte %7 local %8 local %5 local) series_log_loop");
        assembly.Add("return");
        assembly.Add(".end");
      }
      if (math_absolute || math_power || math_exponent)
      {
        assembly.Add(".function: abs/1");
        assembly.Add("arg %0 local %0");
        assembly.Add("if (gte %1 local %0 local (fstore %2 local 0)) abs_done");
        assembly.Add("mul %0 local %0 local (fstore %2 local -1)");
        assembly.Add(".mark: abs_done");
        assembly.Add("return");
        assembly.Add(".end");
      }
      if (use_array)
      {
        assembly.Add(".function: array_create/2");
        assembly.Add("arg %1 local %0");
        assembly.Add("arg %2 local %1");
        assembly.Add("vec %0 local");
        assembly.Add("istore %5 local 0");
        assembly.Add("if (gt %3 local (vlen %4 local %1 local) %5 local) ar_dims");
        assembly.Add("throw (strstore %6 local \"array dimension must be greater than zero\")");
        assembly.Add(".mark: ar_dims");
        assembly.Add("vpop %4 local %1 local %5 local");
        assembly.Add("if (gt %3 local %4 local %5 local) ar_dim");
        assembly.Add("throw (strstore %6 local \"array dimension must be greater than zero\")");
        assembly.Add(".mark: ar_dim");
        assembly.Add("if (eq %3 local (vlen %7 local %1 local) %5 local) ar_fill_val");
        assembly.Add(".mark: ar_fill_arr");
        assembly.Add("if (eq %3 local %4 local %5 local) ar_done");
        assembly.Add("frame ^[(param %0 %1 local) (param %1 %2 local)]");
        assembly.Add("call %7 local array_create/2");
        assembly.Add("vpush %0 local %7 local");
        assembly.Add("idec %4 local");
        assembly.Add("jump ar_fill_arr");
        assembly.Add(".mark: ar_fill_val");
        assembly.Add("if (eq %3 local %4 local %5 local) ar_done");
        assembly.Add("copy %7 local %2 local");
        assembly.Add("vpush %0 local %7 local");
        assembly.Add("idec %4 local");
        assembly.Add("jump ar_fill_val");
        assembly.Add(".mark: ar_done");
        assembly.Add("return");
        assembly.Add(".end");
        assembly.Add(".function: array_get/2");
        assembly.Add("arg %1 local %0");
        assembly.Add("arg %2 local %1");
        assembly.Add("istore %5 local 0");
        assembly.Add("if (gt %3 local (vlen %4 local %2 local) %5 local) ar_dims");
        assembly.Add("throw (strstore %6 local \"array dimension do not match\")");
        assembly.Add(".mark: ar_dims");
        assembly.Add("vpop %4 local %2 local %5 local");
        assembly.Add("if (gte %3 local %4 local %5 local) ar_bound");
        assembly.Add("if (lt %3 local %4 local (vlen %7 local %1 local)) ar_bound");
        assembly.Add("throw (strstore %6 local \"array index out of bounds\")");
        assembly.Add(".mark: ar_bound");
        assembly.Add("vpop %0 local %1 local %4 local");
        assembly.Add("if (eq %3 local (vlen %7 local %2 local) %5 local) ar_done");
        assembly.Add("frame ^[(param %0 %0 local) (param %1 %2 local)]");
        assembly.Add("call %0 local array_get/2");
        assembly.Add(".mark: ar_done");
        assembly.Add("return");
        assembly.Add(".end");
        assembly.Add(".function: array_set/3");
        assembly.Add("arg %1 local %0");
        assembly.Add("arg %2 local %1");
        assembly.Add("arg %8 local %2");
        assembly.Add("istore %5 local 0");
        assembly.Add("if (gt %3 local (vlen %4 local %2 local) %5 local) ar_dims");
        assembly.Add("throw (strstore %6 local \"array dimension do not match\")");
        assembly.Add(".mark: ar_dims");
        assembly.Add("vpop %4 local %2 local %5 local");
        assembly.Add("if (gte %3 local %4 local %5 local) ar_bound");
        assembly.Add("if (lt %3 local %4 local (vlen %7 local *1 local)) ar_bound");
        assembly.Add("throw (strstore %6 local \"array index out of bounds\")");
        assembly.Add(".mark: ar_bound");
        assembly.Add("if (eq %3 local (vlen %7 local %2 local) %5 local) ar_set");
        assembly.Add("vat %0 local *1 local %4 local");
        assembly.Add("frame ^[(param %0 %0 local) (param %1 %2 local) (param %2 %8 local)]");
        assembly.Add("call %0 local array_set/3");
        assembly.Add("return");
        assembly.Add(".mark: ar_set");
        assembly.Add("vpop %0 local *1 local %4 local");
        assembly.Add("vinsert *1 local %8 local %4 local");
        assembly.Add("return");
        assembly.Add(".end");
      }
      return true;
    }

    public List<string> output()
    {
      return assembly;
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
      if (labels.Contains(parts[1].ToLower()))
      {
        Console.WriteLine("?SYNTAX ERROR: DUPLICATE LABEL");
        list(line_num, line_num);
        return false;
      }
      labels.Add(parts[1].ToLower());
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

    private bool parse_if_label(long line_num, List<string> parts)
    {
      if (parts.Count > 2)
      {
        List<string> cond_exp = Utl.take_until(1, "THEN", parts);
        int idx = 1 + cond_exp.Count;
        if (parts[idx].ToUpper().Equals("THEN"))
        {
          if (parts.Count > (idx + 1))
          {
            try
            {
              goto_lines.Add(Convert.ToInt64(parts[idx + 1]));
            }
            catch
            {
            }
            if (parts.Count > (idx + 2))
            {
              if (parts[idx + 2].ToUpper().Equals("ELSE"))
              {
                if (parts.Count.Equals(idx + 4))
                {
                  try
                  {
                    goto_lines.Add(Convert.ToInt64(parts[idx + 3]));
                  }
                  catch
                  {
                  }
                }
              }
            }
          }
        }
      }
      return true;
    }

    private bool parse_goto(long line_num, List<string> parts)
    {
      string label = parts[1].ToLower();
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
        assembly.Add("jump label_" + label);
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
      bool empty = true;
      if (!(plist.Count % 2).Equals(0))
      {
        result = true;
        int idx = 0;
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
            if (empty)
            {
              assembly.Add("text %" + plist_reg + " local %" + vars.get_register(plist[idx].ToUpper()) + " local");
              empty = false;
            }
            else
            {
              assembly.Add("text %" + (plist_reg + 1) + " local %" + vars.get_register(plist[idx].ToUpper()) + " local");
              assembly.Add("textconcat %" + plist_reg + " local %" + plist_reg + " local %" + (plist_reg + 1) + " local");
            }
          }
          else
          {
            int math_reg = plist_reg + 1;
            List<string> math_exp = new List<string>();
            math_exp.Add(plist[idx]);
            if (Utl.is_quoted(plist[idx]))
            {
              string esc = Utl.escape_quotes(plist[idx]);
              if (empty)
              {
                assembly.Add("text %" + plist_reg + " local " + esc);
                empty = false;
              }
              else
              {
                assembly.Add("text %" + (plist_reg + 1) + " local " + esc);
                assembly.Add("textconcat %" + plist_reg + " local %" + plist_reg + " local %" + (plist_reg + 1) + " local");
              }
            }
            else if (parse_float_exp(math_reg, math_exp, false))
            {
              if (empty)
              {
                assembly.Add("text %" + plist_reg + " local %" + math_reg + " local");
                empty = false;
              }
              else
              {
                assembly.Add("text %" + math_reg + " local %" + math_reg + " local");
                assembly.Add("textconcat %" + plist_reg + " local %" + plist_reg + " local %" + math_reg + " local");
              }
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
        assembly.Add("call %" + int_reg + " local round/1");
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
      List<string> rpn = Utl.exp_to_math_rpn(exp);
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
              assembly.Add("copy %" + (float_reg + 2) + " local %" + vars.get_register(arg.ToUpper()) + " local");
              assembly.Add("vpush %" + (float_reg + 1) + " local %" + (float_reg + 2) + " local");
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
              }
              if (arg.Equals("-"))
              {
                assembly.Add("sub %" + (float_reg + 2) + " local %" + (float_reg + 3) + " local %" + (float_reg + 2) + " local");
              }
              if (arg.Equals("*"))
              {
                assembly.Add("mul %" + (float_reg + 2) + " local %" + (float_reg + 3) + " local %" + (float_reg + 2) + " local");
              }
              if (arg.Equals("/"))
              {
                assembly.Add("div %" + (float_reg + 2) + " local %" + (float_reg + 3) + " local %" + (float_reg + 2) + " local");
              }
              if (arg.Equals("%"))
              {
                assembly.Add("frame ^[(param %0 %" + (float_reg + 3) + " local) (param %1 %" + (float_reg + 2) + " local)]");
                assembly.Add("call %" + (float_reg + 2) + " local mod/2");
                math_modulo = true;
              }
              if (arg.Equals("^"))
              {
                assembly.Add("frame ^[(param %0 %" + (float_reg + 3) + " local) (param %1 %" + (float_reg + 2) + " local)]");
                assembly.Add("call %" + (float_reg + 2) + " local pow/2");
                math_power = true;
              }
              assembly.Add("vpush %" + (float_reg + 1) + " local %" + (float_reg + 2) + " local");
              stack++;
            }
          }
          else if (arg.Equals("ABS") || arg.Equals("EXP") || arg.Equals("LOG"))
          {
            if (stack < 1)
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
              if (arg.Equals("ABS"))
              {
                assembly.Add("frame ^[(param %0 %" + (float_reg + 2) + " local)]");
                assembly.Add("call %" + (float_reg + 2) + " local abs/1");
                math_absolute = true;
              }
              if (arg.Equals("EXP"))
              {
                assembly.Add("frame ^[(param %0 %" + (float_reg + 2) + " local)]");
                assembly.Add("call %" + (float_reg + 2) + " local exp/1");
                math_exponent = true;
              }
              if (arg.Equals("LOG"))
              {
                assembly.Add("frame ^[(param %0 %" + (float_reg + 2) + " local)]");
                assembly.Add("call %" + (float_reg + 2) + " local log/1");
                math_logarithm = true;
              }
              assembly.Add("vpush %" + (float_reg + 1) + " local %" + (float_reg + 2) + " local");
              stack++;
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
      bool result = false;
      parts = Utl.list_split_separator(parts, ':', true, true);
      parts = Utl.list_split_separator(parts, '=', true, true);
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
                  Console.WriteLine("?SYNTAX ERROR: VARIABLE " + var_name + " IS OF TYPE " + var_type.ToString());
                }
              }
              else
              {
                if (parts[3].ToUpper().Equals("INTEGER"))
                {
                  var_type = Variables.Type.INTEGER;
                  result = true;
                }
                if (parts[3].ToUpper().Equals("FLOAT"))
                {
                  var_type = Variables.Type.FLOAT;
                  result = true;
                }
                if (parts[3].ToUpper().Equals("STRING"))
                {
                  var_type = Variables.Type.STRING;
                  result = true;
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
              result = true;
            }
            else
            {
              Console.WriteLine("?SYNTAX ERROR: EXPECTING TYPE FOR NEW VARIABLE " + var_name);
            }
          }
        }
        if (result)
        {
          result = false;
          if (var_type.Equals(Variables.Type.INTEGER))
          {
            List<string> exp = parts.GetRange(idx, parts.Count - idx);
            if (parse_integer_exp(register, exp, true))
            {
              if (vars.exists(var_name))
              {
                assembly.Add("move %" + vars.get_register(var_name) + " local %" + register + " local");
              }
              else
              {
                vars.set_var(var_name, var_type, register++);
              }
              result = true;
            }
          }
          if (var_type.Equals(Variables.Type.FLOAT))
          {
            List<string> exp = parts.GetRange(idx, parts.Count - idx);
            if (parse_float_exp(register, exp, true))
            {
              if (vars.exists(var_name))
              {
                assembly.Add("move %" + vars.get_register(var_name) + " local %" + register + " local");
              }
              else
              {
                vars.set_var(var_name, var_type, register++);
              }
              result = true;
            }
          }
          if (var_type.Equals(Variables.Type.STRING))
          {
            List<string> exp = parts.GetRange(idx, parts.Count - idx);
            if (parse_print_list(register, exp))
            {
              if (vars.exists(var_name))
              {
                assembly.Add("move %" + vars.get_register(var_name) + " local %" + register + " local");
              }
              else
              {
                vars.set_var(var_name, var_type, register++);
              }
              result = true;
            }
          }
        }
      }
      if (!result)
      {
        Console.WriteLine("?SYNTAX ERROR: ILLEGAL ASSIGNMENT");
        list(line_num, line_num);
      }
      return result;
    }

    private bool parse_for(long line_num, List<string> parts)
    {
      bool result = false;
      parts = Utl.list_split_separator(parts, '=', true, true);
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
          result = true;
        }
      }
      if (!result)
      {
        Console.WriteLine("?SYNTAX ERROR: ILLEGAL LOOP ARGUMENT");
        list(line_num, line_num);
      }
      return result;
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

    private bool parse_if(long line_num, List<string> parts)
    {
      bool res = false;
      int idx = 0;
      string label_if = "";
      string label_else = "";
      if (parts.Count > 2)
      {
        List<string> cond_exp = Utl.take_until(1, "THEN", parts);
        idx = 1 + cond_exp.Count;
        if (parts[idx].ToUpper().Equals("THEN"))
        {
          if_idx++;
          if (parts.Count > (idx + 1))
          {
            label_if = parts[idx + 1].ToLower();
            long goto_line = 0;
            try
            {
              goto_line = Convert.ToInt64(label_if);
              res = goto_lines.Contains(goto_line);
              label_if = "goto_line_" + goto_line;
            }
            catch
            {
              res = labels.Contains(label_if);
              label_if = "label_" + label_if;
            }
            if (res)
            {
              if (parts.Count > (idx + 2))
              {
                res = false;
                if (parts[idx + 2].ToUpper().Equals("ELSE"))
                {
                  if (parts.Count.Equals(idx + 4))
                  {
                    label_else = parts[idx + 3].ToLower();
                    try
                    {
                      goto_line = Convert.ToInt64(label_else);
                      res = goto_lines.Contains(goto_line);
                      label_else = "goto_line_" + goto_line;
                    }
                    catch
                    {
                      res = labels.Contains(label_else);
                      label_else = "label_" + label_else;
                    }
                  }
                }
              }
            }
            if (!res)
            {
              Console.WriteLine("?SYNTAX ERROR: EXPECTING LINE NUMBER OR LABEL");
              list(line_num, line_num);
              return false;
            }
          }
          else
          {
            nested_ifs.Push(if_idx);
            label_if = "if_" + if_idx;
            label_else = "else_" + if_idx;
            res = true;
          }
          if (res)
          {
            res = false;
            if (parse_logic_exp(register, cond_exp, true))
            {
              string cond = "if %" + register + " local " + label_if;
              if (label_else.Length > 0)
              {
                cond = cond + " " + label_else;
              }
              assembly.Add(cond);
              if (nested_ifs.Count > 0)
              {
                if (nested_ifs.Peek().Equals(if_idx))
                {
                  assembly.Add(".mark: " + label_if);
                }
              }
              res = true;
            }
          }
        }
      }
      if (!res)
      {
        Console.WriteLine("?SYNTAX ERROR: ILLEGAL IF ARGUMENT");
        list(line_num, line_num);
      }
      return res;
    }

    private bool parse_logic_exp(int cond_reg, List<string> exp, bool show_err)
    {
      List<string> rpn = Utl.exp_to_logic_rpn(exp);
      assembly.Add("vec %" + (cond_reg + 1) + " local");
      int stack = 0;
      foreach (string arg in rpn)
      {
        if (arg.Equals("=") || arg.Equals(">") || arg.Equals("<") || arg.Equals(">=") || arg.Equals("<=") || arg.Equals("<>") || arg.Equals("OR") || arg.Equals("AND"))
        {
          if (stack < 2)
          {
            if (show_err)
            {
              Console.WriteLine("?SYNTAX ERROR: ILLEGAL LOGICAL EXPRESSION " + Utl.exp_to_str(exp));
            }
            return false;
          }
          else
          {
            assembly.Add("vpop %" + (cond_reg + 2) + " local %" + (cond_reg + 1) + " local");
            stack--;
            assembly.Add("vpop %" + (cond_reg + 3) + " local %" + (cond_reg + 1) + " local");
            stack--;
            if (arg.Equals("="))
            {
              assembly.Add("eq %" + (cond_reg + 2) + " local %" + (cond_reg + 3) + " local %" + (cond_reg + 2) + " local");
            }
            if (arg.Equals(">"))
            {
              assembly.Add("gt %" + (cond_reg + 2) + " local %" + (cond_reg + 3) + " local %" + (cond_reg + 2) + " local");
            }
            if (arg.Equals("<"))
            {
              assembly.Add("lt %" + (cond_reg + 2) + " local %" + (cond_reg + 3) + " local %" + (cond_reg + 2) + " local");
            }
            if (arg.Equals(">="))
            {
              assembly.Add("gte %" + (cond_reg + 2) + " local %" + (cond_reg + 3) + " local %" + (cond_reg + 2) + " local");
            }
            if (arg.Equals("<="))
            {
              assembly.Add("lte %" + (cond_reg + 2) + " local %" + (cond_reg + 3) + " local %" + (cond_reg + 2) + " local");
            }
            if (arg.Equals("<>"))
            {
              assembly.Add("eq %" + (cond_reg + 2) + " local %" + (cond_reg + 3) + " local %" + (cond_reg + 2) + " local");
              assembly.Add("not %" + (cond_reg + 2) + " local %" + (cond_reg + 2) + " local");
            }
            if (arg.Equals("OR"))
            {
              assembly.Add("or %" + (cond_reg + 2) + " local %" + (cond_reg + 3) + " local %" + (cond_reg + 2) + " local");
            }
            if (arg.Equals("AND"))
            {
              assembly.Add("and %" + (cond_reg + 2) + " local %" + (cond_reg + 3) + " local %" + (cond_reg + 2) + " local");
            }
            assembly.Add("vpush %" + (cond_reg + 1) + " local %" + (cond_reg + 2) + " local");
            stack++;
          }
        }
        else if (arg.Equals("NOT"))
        {
          if (stack < 1)
          {
            if (show_err)
            {
              Console.WriteLine("?SYNTAX ERROR: ILLEGAL LOGICAL EXPRESSION " + Utl.exp_to_str(exp));
            }
            return false;
          }
          else
          {
            assembly.Add("vpop %" + (cond_reg + 2) + " local %" + (cond_reg + 1) + " local");
            stack--;
            assembly.Add("not %" + (cond_reg + 2) + " local %" + (cond_reg + 2) + " local");
            assembly.Add("vpush %" + (cond_reg + 1) + " local %" + (cond_reg + 2) + " local");
            stack++;
          }
        }
        else
        {
          List<string> math_exp = new List<string>();
          math_exp.Add(arg);
          if (parse_float_exp(cond_reg + 2, math_exp, false))
          {
            assembly.Add("vpush %" + (cond_reg + 1) + " local %" + (cond_reg + 2) + " local");
            stack++;
          }
          else
          {
            if (show_err)
            {
              Console.WriteLine("?SYNTAX ERROR: ILLEGAL ARGUMENT " + arg + " IN LOGICAL EXPRESSION " + Utl.exp_to_str(exp));
            }
            return false;
          }
        }
      }
      if (stack.Equals(1))
      {
        assembly.Add("vpop %" + cond_reg + " local %" + (cond_reg + 1) + " local");
        return true;
      }
      else
      {
        if (show_err)
        {
          Console.WriteLine("?SYNTAX ERROR: ILLEGAL LOGICAL EXPRESSION " + Utl.exp_to_str(exp));
        }
        return false;
      }
    }

    private bool parse_else(long line_num, List<string> parts)
    {
      if (nested_ifs.Count > 0)
      {
        assembly.Add("jump endif_" + nested_ifs.Peek());
        assembly.Add(".mark: else_" + nested_ifs.Peek());
        return true;
      }
      else
      {
        Console.WriteLine("?SYNTAX ERROR: UNMATCHED ELSE");
        list(line_num, line_num);
        return false;
      }
    }

    private bool parse_endif(long line_num, List<string> parts)
    {
      if (nested_ifs.Count > 0)
      {
        assembly.Add(".mark: endif_" + nested_ifs.Pop());
        return true;
      }
      else
      {
        Console.WriteLine("?SYNTAX ERROR: UNMATCHED ENDIF");
        list(line_num, line_num);
        return false;
      }
    }

    private bool parse_dim(long line_num, List<string> parts)
    {
      bool result = false;
      parts = Utl.list_split_separator(parts, '(', true, true);
      parts = Utl.list_split_separator(parts, ',', true, true);
      parts = Utl.list_split_separator(parts, ')', true, true);
      parts = Utl.list_split_separator(parts, '=', true, true);
      if (parts.Count > 5)
      {
        string var_name = parts[1].ToUpper();
        if (parts[2].Equals("("))
        {
          if (vars.exists(var_name))
          {
            Console.WriteLine("?SYNTAX ERROR: VARIABLE " + var_name + " ALREADY EXISTS");
          }
          else
          {
            List<string> dims = Utl.take_until(3, "=", parts);
            if (dims.Count > 2)
            {
              if (dims[dims.Count - 2].Equals(")"))
              {
                Variables.Type var_type = Variables.Type.UNKNOWN;
                if (dims[dims.Count - 1].ToUpper().Equals("INTEGER"))
                {
                  var_type = Variables.Type.INTEGER;
                }
                if (dims[dims.Count - 1].ToUpper().Equals("FLOAT"))
                {
                  var_type = Variables.Type.FLOAT;
                }
                if (dims[dims.Count - 1].ToUpper().Equals("STRING"))
                {
                  var_type = Variables.Type.STRING;
                }
                if (var_type.Equals(Variables.Type.UNKNOWN))
                {
                  Console.WriteLine("?SYNTAX ERROR: EXPECTING TYPE FOR ARRAY " + var_name);
                }
                else
                {
                  int array_reg = register++;
                  int init_reg = 0;
                  result = true;
                  if (parts.Count > (3 + dims.Count))
                  {
                    result = false;
                    int idx = 3 + dims.Count;
                    if (parts[idx].Equals("="))
                    {
                      if (parts.Count > (idx + 1))
                      {
                        idx++;
                        if (var_type.Equals(Variables.Type.INTEGER))
                        {
                          List<string> init_exp = parts.GetRange(idx, parts.Count - idx);
                          if (parse_integer_exp(array_reg + 1, init_exp, true))
                          {
                            init_reg = array_reg + 1;
                            result = true;
                          }
                        }
                        if (var_type.Equals(Variables.Type.FLOAT))
                        {
                          List<string> init_exp = parts.GetRange(idx, parts.Count - idx);
                          if (parse_float_exp(array_reg + 1, init_exp, true))
                          {
                            init_reg = array_reg + 1;
                            result = true;
                          }
                        }
                        if (var_type.Equals(Variables.Type.STRING))
                        {
                          List<string> init_exp = parts.GetRange(idx, parts.Count - idx);
                          if (parse_print_list(array_reg + 1, init_exp))
                          {
                            init_reg = array_reg + 1;
                            result = true;
                          }
                        }
                      }
                    }
                  }
                  if (result)
                  {
                    result = false;
                    if (init_reg.Equals(0))
                    {
                      init_reg = array_reg + 1;
                      if (var_type.Equals(Variables.Type.INTEGER))
                      {
                        assembly.Add("istore %" + init_reg + " 0");
                      }
                      if (var_type.Equals(Variables.Type.FLOAT))
                      {
                        assembly.Add("fstore %" + init_reg + " 0");
                      }
                      if (var_type.Equals(Variables.Type.STRING))
                      {
                        assembly.Add("text %" + init_reg + " local \"\"");
                      }
                    }
                    dims.RemoveRange(dims.Count - 2, 2);
                    dims = Utl.split_separator(Utl.exp_to_str(dims), ',', true, true);
                    if (!(dims.Count % 2).Equals(0))
                    {
                      result = true;
                      int idx = 0;
                      int size_reg = init_reg + 1;
                      assembly.Add("vec %" + size_reg + " local");
                      while (idx < dims.Count)
                      {
                        if (idx > 0)
                        {
                          result = result && (dims[idx - 1].Equals(","));
                          if (!result)
                          {
                            break;
                          }
                        }
                        List<string> math_exp = new List<string>();
                        math_exp.Add(dims[idx]);
                        if (parse_integer_exp(size_reg + 1, math_exp, false))
                        {
                          assembly.Add("vpush %" + size_reg + " local %" + (size_reg + 1) + " local");
                        }
                        else
                        {
                          result = false;
                          break;
                        }
                        idx = idx + 2;
                      }
                      if (result)
                      {
                        assembly.Add("frame ^[(param %0 %" + size_reg + " local) (param %1 %" + init_reg + " local)]");
                        assembly.Add("call %" + array_reg + " local array_create/2");
                        vars.set_var(var_name, var_type, array_reg);
                        use_array = true;
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }
      if (!result)
      {
        Console.WriteLine("?SYNTAX ERROR: ILLEGAL ARRAY DECLARATION");
        list(line_num, line_num);
      }
      return result;
    }
  }
}
