package org.viuavm.viuabasic;

import java.text.DecimalFormat;
import java.text.DecimalFormatSymbols;
import java.util.ArrayDeque;
import java.util.ArrayList;
import java.util.Deque;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.TreeMap;

public class BasicCompiler
{
  private class ForLoop
  {
    public long line_num;
    public int register, from, to, step;
    public boolean integer;
  }

  private TreeMap<Long, List<String>> listing;
  private List<String> labels, assembly;
  private List<Long> goto_lines;
  private Variables vars;
  private HashMap<String, ForLoop> for_loops;
  private Deque<Integer> nested_ifs;
  private List<Integer> nested_elses;
  private DecimalFormat numStr;
  private int register, if_idx;
  private boolean math_modulo, math_power, math_round, math_exponent, math_logarithm, math_absolute, use_array;

  public BasicCompiler()
  {
    listing = new TreeMap<Long, List<String>>();
    labels = new ArrayList<String>();
    goto_lines = new ArrayList<Long>();
    assembly = new ArrayList<String>();
    vars = new Variables();
    for_loops = new HashMap<String, ForLoop>();
    nested_ifs = new ArrayDeque<Integer>();
    nested_elses = new ArrayList<Integer>();
    register = 1;
    if_idx = 0;
    math_modulo = false;
    math_power = false;
    math_round = false;
    math_exponent = false;
    math_logarithm = false;
    math_absolute = false;
    use_array = false;
    DecimalFormatSymbols decimalSymbols = DecimalFormatSymbols.getInstance();
    decimalSymbols.setDecimalSeparator('.');
    numStr = new DecimalFormat("0.0", decimalSymbols);
  }

  public boolean load(List<String> src)
  {
    for (int i = 0; i < src.size(); i++)
    {
      long line_num = i + 1;
      List<String> parts = Utl.split_line(src.get(i));
      if (parts.size() > 0)
      {
        if (parts.size() > 1)
        {
          try
          {
            line_num = Long.parseLong(parts.get(0));
            parts.remove(0);
          }
          catch (Exception ex)
          {
            line_num = i + 1;
          }
        }
        String instr = parts.get(0).toUpperCase();
        if (instr.equals("REM"))
        {
          continue;
        }
        if (listing.containsKey(line_num))
        {
          System.out.println("?WARNING: DUPLICATE LINE");
          list(line_num, line_num);
        }
        listing.put(line_num, parts);
        if (instr.equals("LABEL"))
        {
          if (!parse_label(line_num, parts))
          {
            return false;
          }
        }
        if (instr.equals("GOTO"))
        {
          if (!parse_goto_line(line_num, parts))
          {
            return false;
          }
        }
        if (instr.equals("IF"))
        {
          if (!parse_if_label(line_num, parts))
          {
            return false;
          }
        }
      }
    }
    return (!listing.isEmpty());
  }

  public boolean compile()
  {
    assembly.add(".function: basic_main/0");
    for (Map.Entry<Long, List<String>> line : listing.entrySet())
    {
      if (goto_lines.contains(line.getKey()))
      {
        assembly.add(".mark: goto_line_" + line.getKey().toString());
      }
      String instr = line.getValue().get(0).toUpperCase();
      if (instr.equals("LABEL"))
      {
        assembly.add(".mark: label_" + line.getValue().get(1).toLowerCase());
      }
      else if (instr.equals("GOTO"))
      {
        if (!parse_goto(line.getKey(), line.getValue()))
        {
          return false;
        }
      }
      else if (instr.equals("LIST"))
      {
        if (!parse_list(line.getKey(), line.getValue()))
        {
          return false;
        }
      }
      else if (instr.equals("PRINT"))
      {
        if (!parse_print(line.getKey(), line.getValue()))
        {
          return false;
        }
      }
      else if (instr.equals("LET"))
      {
        if (!parse_let(line.getKey(), line.getValue()))
        {
          return false;
        }
      }
      else if (instr.equals("FOR"))
      {
        if (!parse_for(line.getKey(), line.getValue()))
        {
          return false;
        }
      }
      else if (instr.equals("NEXT"))
      {
        if (!parse_next(line.getKey(), line.getValue()))
        {
          return false;
        }
      }
      else if (instr.equals("IF"))
      {
        if (!parse_if(line.getKey(), line.getValue()))
        {
          return false;
        }
      }
      else if (instr.equals("ELSE"))
      {
        if (!parse_else(line.getKey(), line.getValue()))
        {
          return false;
        }
      }
      else if (instr.equals("ENDIF"))
      {
        if (!parse_endif(line.getKey(), line.getValue()))
        {
          return false;
        }
      }
      else if (instr.equals("DIM"))
      {
        if (!parse_dim(line.getKey(), line.getValue()))
        {
          return false;
        }
      }
      else
      {
        System.out.println("?SYNTAX ERROR: UNKNOWN INSTRUCTION: " + instr);
        list(line.getKey(), line.getKey());
        return false;
      }
    }
    assembly.add("izero %0 local");
    assembly.add("return");
    assembly.add(".end");
    assembly.add(".function: main/0");
    assembly.add("frame %0 %" + (register + 16));
    assembly.add("call basic_main/0");
    assembly.add("izero %0 local");
    assembly.add("return");
    assembly.add(".end");
    if (math_modulo || math_power)
    {
      assembly.add(".function: mod/2");
      assembly.add("arg %2 local %1");
      assembly.add("arg %3 local %0");
      assembly.add("if (not (eq %5 local %2 local (float %4 local 0))) mod_not_zero");
      assembly.add("throw (string %1 local \"modulo by zero\")");
      assembly.add(".mark: mod_not_zero");
      assembly.add("if (lt %5 local %2 local (float %4 local 0)) mod_negative");
      assembly.add("float %6 local 0");
      assembly.add("copy %7 local %2 local");
      assembly.add("jump mod_prepare");
      assembly.add(".mark: mod_negative");
      assembly.add("copy %6 local %2 local");
      assembly.add("float %7 local 0");
      assembly.add(".mark: mod_prepare");
      assembly.add("copy %8 local %2 local");
      assembly.add("if (lt %5 local %3 local (float %4 local 0)) mod_check_step");
      assembly.add("if (gt %5 local %8 local (float %4 local 0)) mod_negate_step mod_check");
      assembly.add(".mark: mod_check_step");
      assembly.add("if (gt %5 local %8 local (float %4 local 0)) mod_check");
      assembly.add(".mark: mod_negate_step");
      assembly.add("mul %8 local %8 local (float %4 local -1)");
      assembly.add(".mark: mod_check");
      assembly.add("if (not (gte %5 local %3 local %6 local)) mod_add");
      assembly.add("if (lte %5 local %3 local %7 local) mod_done");
      assembly.add(".mark: mod_add");
      assembly.add("add %3 local %3 local %8 local");
      assembly.add("jump mod_check");
      assembly.add(".mark: mod_done");
      assembly.add("copy %0 local %3 local");
      assembly.add("return");
      assembly.add(".end");
    }
    if (math_power)
    {
      assembly.add(".function: pow/2");
      assembly.add("arg %1 local %0");
      assembly.add("arg %2 local %1");
      assembly.add("if (not (eq %3 local %2 local (float %4 local 1))) pow_pow_zero");
      assembly.add("copy %0 local %1 local");
      assembly.add("jump pow_done");
      assembly.add(".mark: pow_pow_zero");
      assembly.add("if (not (eq %3 local %2 local (float %4 local 0))) pow_pow_minus1");
      assembly.add("if (not (eq %3 local %1 local (float %4 local 0))) pow_pow_zero_base_not_zero");
      assembly.add("throw (string %5 local \"0 ^ 0 is undefined\")");
      assembly.add(".mark: pow_pow_zero_base_not_zero");
      assembly.add("float %0 local 1");
      assembly.add("jump pow_done");
      assembly.add(".mark: pow_pow_minus1");
      assembly.add("if (not (eq %3 local %2 local (float %4 local -1))) pow_base_plus1");
      assembly.add("if (not (eq %3 local %1 local (float %4 local 0))) pow_pow_minus1_base_not_zero");
      assembly.add("throw (string %5 local \"divide by zero\")");
      assembly.add(".mark: pow_pow_minus1_base_not_zero");
      assembly.add("div %0 local (float %4 local 1) %1 local");
      assembly.add("jump pow_done");
      assembly.add(".mark: pow_base_plus1");
      assembly.add("if (not (eq %3 local %1 local (float %4 local 1))) pow_base_minus1");
      assembly.add("float %0 local 1");
      assembly.add("jump pow_done");
      assembly.add(".mark: pow_base_minus1");
      assembly.add("if (not (eq %3 local %1 local (float %4 local -1))) pow_pow_int");
      assembly.add("frame ^[(param %0 %2 local)]");
      assembly.add("call %6 local abs/1");
      assembly.add("frame ^[(param %0 %6 local) (param %1 (float %4 local 2))]");
      assembly.add("call %6 local mod/2");
      assembly.add("if (eq %3 local %6 local (float %4 local 0)) pow_base_minus1_positive");
      assembly.add("float %0 local -1");
      assembly.add("jump pow_done");
      assembly.add(".mark: pow_base_minus1_positive");
      assembly.add("float %0 local 1");
      assembly.add("jump pow_done");
      assembly.add(".mark: pow_pow_int");
      assembly.add("if (not (eq %3 local %2 local (ftoi %4 local %2 local))) pow_other");
      assembly.add("if (lte %3 local %2 local (float %4 local 0)) pow_pow_int_negative");
      assembly.add("frame ^[(param %0 %1 local) (param %1 %2 local)]");
      assembly.add("call %0 local simple_pow/2");
      assembly.add("jump pow_done");
      assembly.add(".mark: pow_pow_int_negative");
      assembly.add("if (not (eq %3 local %1 local (float %4 local 0))) pow_pow_int_negative_base_not_zero");
      assembly.add("throw (string %5 local \"divide by zero\")");
      assembly.add(".mark: pow_pow_int_negative_base_not_zero");
      assembly.add("mul %6 local %2 local (float %4 local -1)");
      assembly.add("frame ^[(param %0 %1 local) (param %1 %6 local)]");
      assembly.add("call %6 local simple_pow/2");
      assembly.add("div %0 local (float %4 local 1) %6 local");
      assembly.add("jump pow_done");
      assembly.add(".mark: pow_other");
      assembly.add("if (lt %3 local %1 local (float %4 local 0)) pow_other_base_negative");
      assembly.add("frame ^[(param %0 %1 local) (param %1 %2 local)]");
      assembly.add("call %0 local complicated_pow/2");
      assembly.add("jump pow_done");
      assembly.add(".mark: pow_other_base_negative");
      assembly.add("throw (string %5 local \"result is complex number\")");
      assembly.add(".mark: pow_done");
      assembly.add("return");
      assembly.add(".end");
      assembly.add(".function: simple_pow/2");
      assembly.add("arg %1 local %0");
      assembly.add("arg %2 local %1");
      assembly.add("float %0 local 1");
      assembly.add("integer %4 local 1");
      assembly.add(".mark: spow_loop");
      assembly.add("mul %0 local %0 local %1 local");
      assembly.add("iinc %4 local");
      assembly.add("if (lte %3 local %4 local %2 local) spow_loop");
      assembly.add("return");
      assembly.add(".end");
      assembly.add(".function: complicated_pow/2");
      assembly.add("arg %1 local %0");
      assembly.add("arg %2 local %1");
      assembly.add("frame ^[(param %0 %2 local)]");
      assembly.add("call %3 local abs/1");
      assembly.add("ftoi %4 local %3 local");
      assembly.add("sub %5 local %3 local %4 local");
      assembly.add("frame ^[(param %0 %1 local) (param %1 %4 local)]");
      assembly.add("call %0 local simple_pow/2");
      assembly.add("frame ^[(param %0 %1 local)]");
      assembly.add("call %6 local log/1");
      assembly.add("mul %6 local %5 local %6 local");
      assembly.add("frame ^[(param %0 %6 local)]");
      assembly.add("call %6 local exp/1");
      assembly.add("mul %0 local %0 local %6 local");
      assembly.add("if (gt %7 local %2 local (float %8 local 0)) cpow_done");
      assembly.add("div %0 local (float %8 local 1) %0 local");
      assembly.add(".mark: cpow_done");
      assembly.add("return");
      assembly.add(".end");
    }
    if (math_round)
    {
      assembly.add(".function: round/1");
      assembly.add("arg %1 local %0");
      assembly.add("if (lt %3 local %1 local (float %2 local 0)) round_negative");
      assembly.add("float %2 local 0.5");
      assembly.add("jump math_round");
      assembly.add(".mark: round_negative");
      assembly.add("float %2 local -0.5");
      assembly.add(".mark: math_round");
      assembly.add("add %1 local %1 local %2 local");
      assembly.add("ftoi %0 local %1 local");
      assembly.add("return");
      assembly.add(".end");
    }
    if (math_exponent || math_power)
    {
      assembly.add(".function: exp/1");
      assembly.add("arg %1 local %0");
      assembly.add("float %0 local 1");
      assembly.add("copy %2 local %1 local");
      assembly.add("float %3 local 1");
      assembly.add("integer %4 local 1");
      assembly.add("float %7 local 0.00000000000000000000000000000001");
      assembly.add("frame ^[(param %0 %2 local)]");
      assembly.add("call %8 local abs/1");
      assembly.add(".mark: exp_loop");
      assembly.add("div %5 local %2 local %3 local");
      assembly.add("div %9 local %8 local %3 local");
      assembly.add("add %0 local %0 local %5 local");
      assembly.add("mul %2 local %2 local %1 local");
      assembly.add("iinc %4 local");
      assembly.add("mul %3 local %3 local %4 local");
      assembly.add("if (gte %6 local %9 local %7 local) exp_loop");
      assembly.add("return");
      assembly.add(".end");
    }
    if (math_logarithm || math_power)
    {
      assembly.add(".function: log/1");
      assembly.add("arg %1 local %0");
      assembly.add("if (gt %3 local %1 local (float %2 local 0)) log_positive");
      assembly.add("throw (string %4 local \"logarithm argument must be greater than zero\")");
      assembly.add(".mark: log_positive");
      assembly.add("if (not (isnull %3 local %1 global)) log_begin");
      assembly.add("frame ^[(param %0 (float %2 local 1.9))]");
      assembly.add("call %1 global series_log/1");
      assembly.add(".mark: log_begin");
      assembly.add("float %0 local 0");
      assembly.add("if (lt %3 local %1 local (float %2 local 2)) log_rest");
      assembly.add("integer %4 local 0");
      assembly.add("float %5 local 1.9");
      assembly.add("float %6 local 2");
      assembly.add(".mark: log_divide");
      assembly.add("div %1 local %1 local %5 local");
      assembly.add("iinc %4 local");
      assembly.add("if (gte %3 local %1 local %6 local) log_divide");
      assembly.add("mul %0 local %1 global %4 local");
      assembly.add(".mark: log_rest");
      assembly.add("frame ^[(param %0 %1 local)]");
      assembly.add("call %2 local series_log/1");
      assembly.add("add %0 local %0 local %2 local");
      assembly.add("return");
      assembly.add(".end");
      assembly.add(".function: series_log/1");
      assembly.add("arg %1 local %0");
      assembly.add("sub %1 local %1 local (float %2 local 1)");
      assembly.add("copy %2 local %1 local");
      assembly.add("float %3 local 1");
      assembly.add("float %0 local 0");
      assembly.add("integer %4 local 1");
      assembly.add("float %5 local 0.00000000000000000000000000000001");
      assembly.add(".mark: series_log_loop");
      assembly.add("div %6 local %2 local %4 local");
      assembly.add("copy %8 local %6 local");
      assembly.add("mul %6 local %3 local %6 local");
      assembly.add("add %0 local %0 local %6 local");
      assembly.add("mul %2 local %2 local %1 local");
      assembly.add("mul %3 local %3 local (float %6 local -1)");
      assembly.add("iinc %4 local");
      assembly.add("if (gte %7 local %8 local %5 local) series_log_loop");
      assembly.add("return");
      assembly.add(".end");
    }
    if (math_absolute || math_power || math_exponent)
    {
      assembly.add(".function: abs/1");
      assembly.add("arg %0 local %0");
      assembly.add("if (gte %1 local %0 local (float %2 local 0)) abs_done");
      assembly.add("mul %0 local %0 local (float %2 local -1)");
      assembly.add(".mark: abs_done");
      assembly.add("return");
      assembly.add(".end");
    }
    if (use_array)
    {
      assembly.add(".function: array_create/2");
      assembly.add("arg %1 local %0");
      assembly.add("arg %2 local %1");
      assembly.add("vector %0 local");
      assembly.add("integer %5 local 0");
      assembly.add("if (gt %3 local (vlen %4 local %1 local) %5 local) ar_dims");
      assembly.add("throw (string %6 local \"array dimension must be greater than zero\")");
      assembly.add(".mark: ar_dims");
      assembly.add("vpop %4 local %1 local %5 local");
      assembly.add("if (gt %3 local %4 local %5 local) ar_dim");
      assembly.add("throw (string %6 local \"array dimension must be greater than zero\")");
      assembly.add(".mark: ar_dim");
      assembly.add("if (eq %3 local (vlen %7 local %1 local) %5 local) ar_fill_val");
      assembly.add(".mark: ar_fill_arr");
      assembly.add("if (eq %3 local %4 local %5 local) ar_done");
      assembly.add("frame ^[(param %0 %1 local) (param %1 %2 local)]");
      assembly.add("call %7 local array_create/2");
      assembly.add("vpush %0 local %7 local");
      assembly.add("idec %4 local");
      assembly.add("jump ar_fill_arr");
      assembly.add(".mark: ar_fill_val");
      assembly.add("if (eq %3 local %4 local %5 local) ar_done");
      assembly.add("copy %7 local %2 local");
      assembly.add("vpush %0 local %7 local");
      assembly.add("idec %4 local");
      assembly.add("jump ar_fill_val");
      assembly.add(".mark: ar_done");
      assembly.add("return");
      assembly.add(".end");
      assembly.add(".function: array_get/2");
      assembly.add("arg %1 local %0");
      assembly.add("arg %2 local %1");
      assembly.add("integer %5 local 0");
      assembly.add("if (gt %3 local (vlen %4 local %2 local) %5 local) ar_dims");
      assembly.add("throw (string %6 local \"array dimension do not match\")");
      assembly.add(".mark: ar_dims");
      assembly.add("vpop %4 local %2 local %5 local");
      assembly.add("if (gte %3 local %4 local %5 local) ar_bound");
      assembly.add("if (lt %3 local %4 local (vlen %7 local %1 local)) ar_bound");
      assembly.add("throw (string %6 local \"array index out of bounds\")");
      assembly.add(".mark: ar_bound");
      assembly.add("vpop %0 local %1 local %4 local");
      assembly.add("if (eq %3 local (vlen %7 local %2 local) %5 local) ar_done");
      assembly.add("frame ^[(param %0 %0 local) (param %1 %2 local)]");
      assembly.add("call %0 local array_get/2");
      assembly.add(".mark: ar_done");
      assembly.add("return");
      assembly.add(".end");
      assembly.add(".function: array_set/3");
      assembly.add("arg %1 local %0");
      assembly.add("arg %2 local %1");
      assembly.add("arg %8 local %2");
      assembly.add("integer %5 local 0");
      assembly.add("if (gt %3 local (vlen %4 local %2 local) %5 local) ar_dims");
      assembly.add("throw (string %6 local \"array dimension do not match\")");
      assembly.add(".mark: ar_dims");
      assembly.add("vpop %4 local %2 local %5 local");
      assembly.add("if (gte %3 local %4 local %5 local) ar_bound");
      assembly.add("if (lt %3 local %4 local (vlen %7 local *1 local)) ar_bound");
      assembly.add("throw (string %6 local \"array index out of bounds\")");
      assembly.add(".mark: ar_bound");
      assembly.add("if (eq %3 local (vlen %7 local %2 local) %5 local) ar_set");
      assembly.add("vat %0 local *1 local %4 local");
      assembly.add("frame ^[(param %0 %0 local) (param %1 %2 local) (param %2 %8 local)]");
      assembly.add("call %0 local array_set/3");
      assembly.add("return");
      assembly.add(".mark: ar_set");
      assembly.add("vpop %0 local *1 local %4 local");
      assembly.add("vinsert *1 local %8 local %4 local");
      assembly.add("return");
      assembly.add(".end");
      assembly.add(".function: is_vec/1");
      assembly.add("arg %1 local %0");
      assembly.add("integer %2 local 0");
      assembly.add("integer %3 local 1");
      assembly.add("eq %0 local %2 local %3 local");
      assembly.add("try");
      assembly.add("catch \"Exception\" .block: do_nothing");
      assembly.add("leave");
      assembly.add(".end");
      assembly.add("enter .block: test_vec");
      assembly.add("vlen %4 local %1 local");
      assembly.add("integer %3 local 0");
      assembly.add("eq %0 local %2 local %3 local");
      assembly.add("leave");
      assembly.add(".end");
      assembly.add("return");
      assembly.add(".end");
      assembly.add(".function: array_comma/2");
      assembly.add("arg %1 local %0");
      assembly.add("arg %2 local %1");
      assembly.add("frame ^[(param %0 %1 local)]");
      assembly.add("call %3 local is_vec/1");
      assembly.add("if %3 local push_dim");
      assembly.add("vector %0 local");
      assembly.add("frame ^[(param %0 %1 local)]");
      assembly.add("call %3 local round/1");
      assembly.add("vpush %0 local %3 local");
      assembly.add("frame ^[(param %0 %2 local)]");
      assembly.add("call %3 local round/1");
      assembly.add("vpush %0 local %3 local");
      assembly.add("jump push_dim_done");
      assembly.add(".mark: push_dim");
      assembly.add("frame ^[(param %0 %2 local)]");
      assembly.add("call %3 local round/1");
      assembly.add("vpush %1 local %3 local");
      assembly.add("move %0 local %1 local");
      assembly.add(".mark: push_dim_done");
      assembly.add("return");
      assembly.add(".end");
    }
    return true;
  }

  public List<String> output()
  {
    return assembly;
  }

  private void list(long line_from, long line_to)
  {
    for (Map.Entry<Long, List<String>> line : listing.entrySet())
    {
      if ((line_from > 0) && (line.getKey() < line_from))
      {
        continue;
      }
      if ((line_to > 0) && (line.getKey() > line_to))
      {
        continue;
      }
      String buf = line.getKey().toString();
      for (String part : line.getValue())
      {
        buf = buf + " " + part;
      }
      System.out.println(buf);
    }
  }

  private boolean parse_list(long line_num, List<String> parts)
  {
    List<String> args = new ArrayList<String>();
    if (parts.size() > 1)
    {
      args = Utl.split_separator(Utl.exp_to_str(parts.subList(1, parts.size())), ',', true, true, false);
    }
    long line_from = 0;
    long line_to = 0;
    boolean result = true;
    if (args.size() > 0)
    {
      result = false;
      if (args.size() == 1)
      {
        try
        {
          line_from = Long.parseLong(args.get(0));
        }
        catch (Exception ex)
        {
          line_from = 0;
        }
        result = (line_from > 0);
      }
      if (args.size() == 2)
      {
        if (args.get(0).equals(","))
        {
          try
          {
            line_to = Long.parseLong(args.get(1));
          }
          catch (Exception ex)
          {
            line_to = 0;
          }
          result = (line_to > 0);
        }
      }
      if (args.size() == 3)
      {
        if (args.get(1).equals(","))
        {
          try
          {
            line_from = Long.parseLong(args.get(0));
            line_to = Long.parseLong(args.get(2));
          }
          catch (Exception ex)
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
      System.out.println("?SYNTAX ERROR: EXPECTING LINE NUMBER OR LABEL");
      list(line_num, line_num);
    }
    return result;
  }

  private boolean parse_label(long line_num, List<String> parts)
  {
    if (parts.size() != 2)
    {
      System.out.println("?SYNTAX ERROR: EXPECTING LABEL");
      list(line_num, line_num);
      return false;
    }
    if (labels.contains(parts.get(1).toLowerCase()))
    {
      System.out.println("?SYNTAX ERROR: DUPLICATE LABEL");
      list(line_num, line_num);
      return false;
    }
    labels.add(parts.get(1).toLowerCase());
    return true;
  }

  private boolean parse_goto_line(long line_num, List<String> parts)
  {
    if (parts.size() != 2)
    {
      System.out.println("?SYNTAX ERROR: EXPECTING LINE NUMBER OR LABEL");
      list(line_num, line_num);
      return false;
    }
    try
    {
      goto_lines.add(Long.parseLong(parts.get(1)));
    }
    catch (Exception ex)
    {
    }
    return true;
  }

  private boolean parse_if_label(long line_num, List<String> parts)
  {
    if (parts.size() > 2)
    {
      List<String> cond_exp = Utl.take_until(1, "THEN", parts);
      int idx = 1 + cond_exp.size();
      if (parts.get(idx).toUpperCase().equals("THEN"))
      {
        if (parts.size() > (idx + 1))
        {
          try
          {
            goto_lines.add(Long.parseLong(parts.get(idx + 1)));
          }
          catch (Exception ex)
          {
          }
          if (parts.size() > (idx + 2))
          {
            if (parts.get(idx + 2).toUpperCase().equals("ELSE"))
            {
              if (parts.size() == idx + 4)
              {
                try
                {
                  goto_lines.add(Long.parseLong(parts.get(idx + 3)));
                }
                catch (Exception ex)
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

  private boolean parse_goto(long line_num, List<String> parts)
  {
    String label = parts.get(1).toLowerCase();
    long goto_line = 0;
    try
    {
      goto_line = Long.parseLong(label);
    }
    catch (Exception ex)
    {
      goto_line = 0;
    }
    if (labels.contains(label))
    {
      assembly.add("jump label_" + label);
    }
    else if (goto_lines.contains(goto_line))
    {
      assembly.add("jump goto_line_" + Long.toString(goto_line));
    }
    else
    {
      System.out.println("?SYNTAX ERROR: EXPECTING LINE NUMBER OR LABEL");
      list(line_num, line_num);
      return false;
    }
    return true;
  }

  private boolean parse_print(long line_num, List<String> parts)
  {
    if (parts.size() < 2)
    {
      System.out.println("?SYNTAX ERROR: EXPECTING PRINT LIST");
      list(line_num, line_num);
      return false;
    }
    List<String> exp = parts.subList(1, parts.size());
    if (parse_print_list(register, exp))
    {
      assembly.add("print %" + register + " local");
      return true;
    }
    else
    {
      list(line_num, line_num);
      return false;
    }
  }

  private boolean parse_print_list(int plist_reg, List<String> exp)
  {
    List<String> plist = Utl.split_separator(Utl.exp_to_str(exp), ',', true, true, true);
    boolean result = false;
    boolean empty = true;
    if ((plist.size() % 2) != 0)
    {
      result = true;
      int idx = 0;
      while (idx < plist.size())
      {
        if (idx > 0)
        {
          result = result && (plist.get(idx - 1).equals(","));
          if (!result)
          {
            break;
          }
        }
        if (vars.exists(plist.get(idx).toUpperCase()))
        {
          if (empty)
          {
            assembly.add("text %" + plist_reg + " local %" + vars.get_register(plist.get(idx).toUpperCase()) + " local");
            empty = false;
          }
          else
          {
            assembly.add("text %" + (plist_reg + 1) + " local %" + vars.get_register(plist.get(idx).toUpperCase()) + " local");
            assembly.add("textconcat %" + plist_reg + " local %" + plist_reg + " local %" + (plist_reg + 1) + " local");
          }
        }
        else
        {
          int math_reg = plist_reg + 1;
          List<String> math_exp = new ArrayList<String>();
          math_exp.add(plist.get(idx));
          if (Utl.is_quoted(plist.get(idx)))
          {
            String esc = Utl.escape_quotes(plist.get(idx));
            if (empty)
            {
              assembly.add("text %" + plist_reg + " local " + esc);
              empty = false;
            }
            else
            {
              assembly.add("text %" + (plist_reg + 1) + " local " + esc);
              assembly.add("textconcat %" + plist_reg + " local %" + plist_reg + " local %" + (plist_reg + 1) + " local");
            }
          }
          else if (parse_float_exp(math_reg, math_exp, false))
          {
            if (empty)
            {
              assembly.add("text %" + plist_reg + " local %" + math_reg + " local");
              empty = false;
            }
            else
            {
              assembly.add("text %" + math_reg + " local %" + math_reg + " local");
              assembly.add("textconcat %" + plist_reg + " local %" + plist_reg + " local %" + math_reg + " local");
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
      System.out.println("?SYNTAX ERROR: ILLEGAL PRINT LIST");
    }
    return result;
  }

  private boolean parse_integer_exp(int int_reg, List<String> exp, boolean show_err)
  {
    if (parse_float_exp(int_reg + 1, exp, show_err))
    {
      assembly.add("frame ^[(param %0 %" + (int_reg + 1) + " local)]");
      assembly.add("call %" + int_reg + " local round/1");
      math_round = true;
      return true;
    }
    else
    {
      return false;
    }
  }

  private boolean parse_float_exp(int float_reg, List<String> exp, boolean show_err)
  {
    List<String> rpn = Utl.exp_to_math_rpn(exp, vars);
    assembly.add("vector %" + (float_reg + 1) + " local");
    int stack = 0;
    for (String arg : rpn)
    {
      double num = 0;
      boolean is_num = false;
      try
      {
        num = Double.parseDouble(arg);
        is_num = true;
      }
      catch (Exception ex)
      {
        is_num = false;
      }
      if (is_num)
      {
        assembly.add("float %" + (float_reg + 2) + " local " + num);
        assembly.add("vpush %" + (float_reg + 1) + " local %" + (float_reg + 2) + " local");
        stack++;
      }
      else
      {
        if (vars.exists(arg.toUpperCase()))
        {
          if (vars.get_type(arg.toUpperCase()).equals(Variables.Type.FLOAT))
          {
            assembly.add("copy %" + (float_reg + 2) + " local %" + vars.get_register(arg.toUpperCase()) + " local");
            assembly.add("vpush %" + (float_reg + 1) + " local %" + (float_reg + 2) + " local");
            stack++;
          }
          if (vars.get_type(arg.toUpperCase()).equals(Variables.Type.INTEGER))
          {
            assembly.add("itof %" + (float_reg + 2) + " local %" + vars.get_register(arg.toUpperCase()) + " local");
            assembly.add("vpush %" + (float_reg + 1) + " local %" + (float_reg + 2) + " local");
            stack++;
          }
          if (vars.get_type(arg.toUpperCase()).equals(Variables.Type.STRING))
          {
            assembly.add("stof %" + (float_reg + 2) + " local %" + vars.get_register(arg.toUpperCase()) + " local");
            assembly.add("vpush %" + (float_reg + 1) + " local %" + (float_reg + 2) + " local");
            stack++;
          }
          if (vars.get_type(arg.toUpperCase()).equals(Variables.Type.ARRAY))
          {
            if (stack < 1)
            {
              if (show_err)
              {
                System.out.println("?SYNTAX ERROR: ILLEGAL ARITHMETIC EXPRESSION " + Utl.exp_to_str(exp));
              }
              return false;
            }
            else
            {
              assembly.add("vpop %" + (float_reg + 2) + " local %" + (float_reg + 1) + " local");
              stack--;
              assembly.add("frame ^[(param %0 %" + vars.get_register(arg.toUpperCase()) + " local) (param %1 %" + (float_reg + 2) + " local)]");
              assembly.add("call %" + (float_reg + 3) + " local array_get/2");
              if (vars.get_array_type(arg.toUpperCase()).equals(Variables.Type.FLOAT))
              {
                assembly.add("move %" + (float_reg + 2) + " local %" + (float_reg + 3) + " local");
              }
              if (vars.get_array_type(arg.toUpperCase()).equals(Variables.Type.INTEGER))
              {
                assembly.add("itof %" + (float_reg + 2) + " local %" + (float_reg + 3) + " local");
              }
              if (vars.get_array_type(arg.toUpperCase()).equals(Variables.Type.STRING))
              {
                assembly.add("stof %" + (float_reg + 2) + " local %" + (float_reg + 3) + " local");
              }
              assembly.add("vpush %" + (float_reg + 1) + " local %" + (float_reg + 2) + " local");
              stack++;
            }
          }
        }
        else if (arg.equals("+") || arg.equals("-") || arg.equals("*") || arg.equals("/") || arg.equals("%") || arg.equals("^") || arg.equals(","))
        {
          if (stack < 2)
          {
            if (show_err)
            {
              System.out.println("?SYNTAX ERROR: ILLEGAL ARITHMETIC EXPRESSION " + Utl.exp_to_str(exp));
            }
            return false;
          }
          else
          {
            assembly.add("vpop %" + (float_reg + 2) + " local %" + (float_reg + 1) + " local");
            stack--;
            assembly.add("vpop %" + (float_reg + 3) + " local %" + (float_reg + 1) + " local");
            stack--;
            if (arg.equals("+"))
            {
              assembly.add("add %" + (float_reg + 2) + " local %" + (float_reg + 3) + " local %" + (float_reg + 2) + " local");
            }
            if (arg.equals("-"))
            {
              assembly.add("sub %" + (float_reg + 2) + " local %" + (float_reg + 3) + " local %" + (float_reg + 2) + " local");
            }
            if (arg.equals("*"))
            {
              assembly.add("mul %" + (float_reg + 2) + " local %" + (float_reg + 3) + " local %" + (float_reg + 2) + " local");
            }
            if (arg.equals("/"))
            {
              assembly.add("div %" + (float_reg + 2) + " local %" + (float_reg + 3) + " local %" + (float_reg + 2) + " local");
            }
            if (arg.equals("%"))
            {
              assembly.add("frame ^[(param %0 %" + (float_reg + 3) + " local) (param %1 %" + (float_reg + 2) + " local)]");
              assembly.add("call %" + (float_reg + 2) + " local mod/2");
              math_modulo = true;
            }
            if (arg.equals("^"))
            {
              assembly.add("frame ^[(param %0 %" + (float_reg + 3) + " local) (param %1 %" + (float_reg + 2) + " local)]");
              assembly.add("call %" + (float_reg + 2) + " local pow/2");
              math_power = true;
            }
            if (arg.equals(","))
            {
              assembly.add("frame ^[(param %0 %" + (float_reg + 3) + " local) (param %1 %" + (float_reg + 2) + " local)]");
              assembly.add("call %" + (float_reg + 2) + " local array_comma/2");
              math_round = true;
            }
            assembly.add("vpush %" + (float_reg + 1) + " local %" + (float_reg + 2) + " local");
            stack++;
          }
        }
        else if (arg.equals("ABS") || arg.equals("EXP") || arg.equals("LOG"))
        {
          if (stack < 1)
          {
            if (show_err)
            {
              System.out.println("?SYNTAX ERROR: ILLEGAL ARITHMETIC EXPRESSION " + Utl.exp_to_str(exp));
            }
            return false;
          }
          else
          {
            assembly.add("vpop %" + (float_reg + 2) + " local %" + (float_reg + 1) + " local");
            stack--;
            if (arg.equals("ABS"))
            {
              assembly.add("frame ^[(param %0 %" + (float_reg + 2) + " local)]");
              assembly.add("call %" + (float_reg + 2) + " local abs/1");
              math_absolute = true;
            }
            if (arg.equals("EXP"))
            {
              assembly.add("frame ^[(param %0 %" + (float_reg + 2) + " local)]");
              assembly.add("call %" + (float_reg + 2) + " local exp/1");
              math_exponent = true;
            }
            if (arg.equals("LOG"))
            {
              assembly.add("frame ^[(param %0 %" + (float_reg + 2) + " local)]");
              assembly.add("call %" + (float_reg + 2) + " local log/1");
              math_logarithm = true;
            }
            assembly.add("vpush %" + (float_reg + 1) + " local %" + (float_reg + 2) + " local");
            stack++;
          }
        }
        else
        {
          if (show_err)
          {
            System.out.println("?SYNTAX ERROR: ILLEGAL ARGUMENT " + arg + " IN ARITHMETIC EXPRESSION " + Utl.exp_to_str(exp));
          }
          return false;
        }
      }
    }
    if (stack == 1)
    {
      assembly.add("vpop %" + float_reg + " local %" + (float_reg + 1) + " local");
      return true;
    }
    else
    {
      if (show_err)
      {
        System.out.println("?SYNTAX ERROR: ILLEGAL ARITHMETIC EXPRESSION " + Utl.exp_to_str(exp));
      }
      return false;
    }
  }

  private boolean parse_let(long line_num, List<String> parts)
  {
    boolean result = false;
    parts = Utl.list_split_separator(parts, '=', true, true, false);
    if (parts.size() > 3)
    {
      List<String> var_exp = Utl.take_until(1, "=", parts);
      int idx = 1 + var_exp.size();
      if (parts.size() > (idx + 1))
      {
        if (parts.get(idx).toUpperCase().equals("="))
        {
          List<String> val_exp = parts.subList(idx + 1, parts.size());
          var_exp = Utl.split_separator(Utl.exp_to_str(var_exp), ':', true, true, false);
          String var_name = "";
          Variables.Type var_type = Variables.Type.UNKNOWN;
          if (var_exp.size() == 3)
          {
            if (var_exp.get(1).equals(":"))
            {
              var_name = var_exp.get(0).toUpperCase();
              if (vars.exists(var_name))
              {
                var_type = vars.get_type(var_name);
                if (var_type.toString().equals(var_exp.get(2).toUpperCase()))
                {
                  result = true;
                }
                else
                {
                  System.out.println("?SYNTAX ERROR: VARIABLE " + var_name + " IS OF TYPE " + var_type.toString());
                }
              }
              else
              {
                if (var_exp.get(2).toUpperCase().equals("INTEGER"))
                {
                  var_type = Variables.Type.INTEGER;
                  result = true;
                }
                if (var_exp.get(2).toUpperCase().equals("FLOAT"))
                {
                  var_type = Variables.Type.FLOAT;
                  result = true;
                }
                if (var_exp.get(2).toUpperCase().equals("STRING"))
                {
                  var_type = Variables.Type.STRING;
                  result = true;
                }
              }
            }
          }
          if (!result)
          {
            var_exp = Utl.split_separator(Utl.exp_to_str(var_exp), '(', true, true, false);
            var_exp = Utl.list_split_separator(var_exp, ',', true, true, false);
            var_exp = Utl.list_split_separator(var_exp, ')', true, true, false);
            if (var_exp.size() == 1)
            {
              var_name = var_exp.get(0).toUpperCase();
              if (vars.exists(var_name))
              {
                if (vars.is_array(var_name))
                {
                  System.out.println("?SYNTAX ERROR: EXPECTING INDEX FOR ARRAY " + var_name);
                }
                else
                {
                  var_type = vars.get_type(var_name);
                  result = true;
                }
              }
              else
              {
                System.out.println("?SYNTAX ERROR: EXPECTING TYPE FOR NEW VARIABLE " + var_name);
              }
            }
            if (var_exp.size() > 3)
            {
              if (var_exp.get(1).equals("("))
              {
                if (var_exp.get(var_exp.size() - 1).equals(")"))
                {
                  var_name = var_exp.get(0).toUpperCase();
                  if (vars.is_array(var_name))
                  {
                    var_type = Variables.Type.ARRAY;
                    List<String> dims = var_exp.subList(2, var_exp.size() - 1);
                    if (parse_dim_size(register, dims))
                    {
                      result = true;
                    }
                  }
                  else
                  {
                    System.out.println("?SYNTAX ERROR: UNKNOWN ARRAY " + var_name);
                  }
                }
              }
            }
          }
          if (result)
          {
            result = false;
            if (var_type.equals(Variables.Type.INTEGER))
            {
              if (parse_integer_exp(register, val_exp, true))
              {
                if (vars.exists(var_name))
                {
                  assembly.add("move %" + vars.get_register(var_name) + " local %" + register + " local");
                }
                else
                {
                  vars.set_var(var_name, var_type, register++);
                }
                result = true;
              }
            }
            if (var_type.equals(Variables.Type.FLOAT))
            {
              if (parse_float_exp(register, val_exp, true))
              {
                if (vars.exists(var_name))
                {
                  assembly.add("move %" + vars.get_register(var_name) + " local %" + register + " local");
                }
                else
                {
                  vars.set_var(var_name, var_type, register++);
                }
                result = true;
              }
            }
            if (var_type.equals(Variables.Type.STRING))
            {
              if (parse_print_list(register, val_exp))
              {
                if (vars.exists(var_name))
                {
                  assembly.add("move %" + vars.get_register(var_name) + " local %" + register + " local");
                }
                else
                {
                  vars.set_var(var_name, var_type, register++);
                }
                result = true;
              }
            }
            if (var_type.equals(Variables.Type.ARRAY))
            {
              if (vars.get_array_type(var_name).equals(Variables.Type.INTEGER))
              {
                result = parse_integer_exp(register + 1, val_exp, true);
              }
              if (vars.get_array_type(var_name).equals(Variables.Type.FLOAT))
              {
                result = parse_float_exp(register + 1, val_exp, true);
              }
              if (vars.get_array_type(var_name).equals(Variables.Type.STRING))
              {
                result = parse_print_list(register + 1, val_exp);
              }
              if (result)
              {
                assembly.add("ptr %" + (register + 2) + " local %" + vars.get_register(var_name) + " local");
                assembly.add("frame ^[(param %0 %" + (register + 2) + " local) (param %1 %" + register + " local) (param %2 %" + (register + 1) + " local)]");
                assembly.add("call array_set/3");
              }
            }
          }
        }
      }
    }
    if (!result)
    {
      System.out.println("?SYNTAX ERROR: ILLEGAL ASSIGNMENT");
      list(line_num, line_num);
    }
    return result;
  }

  private boolean parse_for(long line_num, List<String> parts)
  {
    boolean result = false;
    parts = Utl.list_split_separator(parts, '=', true, true, false);
    ForLoop for_loop = new ForLoop();
    for_loop.line_num = line_num;
    boolean stop = false;
    int idx = 0;
    if (parts.size() > 5)
    {
      if (parts.get(2).equals("="))
      {
        String var_name = parts.get(1).toUpperCase();
        if (for_loops.containsKey(var_name))
        {
          System.out.println("?SYNTAX ERROR: NESTED LOOP FOR VARIABLE " + var_name);
          list(for_loops.get(var_name).line_num, line_num);
          return false;
        }
        if (vars.exists(var_name))
        {
          if (vars.get_type(var_name).equals(Variables.Type.INTEGER))
          {
            for_loop.register = vars.get_register(var_name);
            for_loop.integer = true;
          }
          else if (vars.get_type(var_name).equals(Variables.Type.FLOAT))
          {
            for_loop.register = vars.get_register(var_name);
            for_loop.integer = false;
          }
          else
          {
            System.out.println("?SYNTAX ERROR: VARIABLE " + var_name + " IS NOT NUMERIC");
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
        List<String> from_exp = Utl.take_until(3, "TO", parts);
        if ((for_loop.integer && parse_integer_exp(register, from_exp, true)) || ((!for_loop.integer) && parse_float_exp(register, from_exp, true)))
        {
          for_loop.from = register++;
          idx = 3 + from_exp.size();
        }
        else
        {
          System.out.println("?SYNTAX ERROR: EXPECTING NUMERIC FROM EXPRESSION");
          list(line_num, line_num);
          return false;
        }
        stop = true;
        if (parts.size() > (idx + 1))
        {
          if (parts.get(idx).toUpperCase().equals("TO"))
          {
            List<String> to_exp = Utl.take_until(idx + 1, "STEP", parts);
            if ((for_loop.integer && parse_integer_exp(register, to_exp, true)) || ((!for_loop.integer) && parse_float_exp(register, to_exp, true)))
            {
              for_loop.to = register++;
              idx = idx + 1 + to_exp.size();
              stop = false;
            }
          }
        }
        if (stop)
        {
          System.out.println("?SYNTAX ERROR: EXPECTING NUMERIC TO EXPRESSION");
          list(line_num, line_num);
          return false;
        }
        for_loop.step = 0;
        if (parts.size() > idx)
        {
          stop = true;
          if (parts.size() > (idx + 1))
          {
            if (parts.get(idx).toUpperCase().equals("STEP"))
            {
              List<String> step_exp = parts.subList(idx + 1, parts.size());
              if ((for_loop.integer && parse_integer_exp(register, step_exp, true)) || ((!for_loop.integer) && parse_float_exp(register, step_exp, true)))
              {
                for_loop.step = register++;
                stop = false;
              }
            }
          }
          if (stop)
          {
            System.out.println("?SYNTAX ERROR: EXPECTING NUMERIC STEP EXPRESSION");
            list(line_num, line_num);
            return false;
          }
        }
        if (for_loop.step == 0)
        {
          for_loop.step = register++;
          if (for_loop.integer)
          {
            assembly.add("integer %" + for_loop.step + " local 1");
          }
          else
          {
            assembly.add("float %" + for_loop.step + " local 1");
          }
        }
        assembly.add("copy %" + for_loop.register + " local %" + for_loop.from + " local");
        assembly.add(".mark: for_" + for_loop.line_num + "_begin");
        assembly.add("if (lt %" + register + " local %" + for_loop.step + " local (float %" + (register + 1) + " local 0)) for_" + for_loop.line_num + "_descend");
        assembly.add("if (gt %" + register + " local %" + for_loop.register + " local %" + for_loop.to + " local) for_" + for_loop.line_num + "_end");
        assembly.add("jump for_" + for_loop.line_num + "_step");
        assembly.add(".mark: for_" + for_loop.line_num + "_descend");
        assembly.add("if (lt %" + register + " local %" + for_loop.register + " local %" + for_loop.to + " local) for_" + for_loop.line_num + "_end");
        assembly.add(".mark: for_" + for_loop.line_num + "_step");
        for_loops.put(var_name, for_loop);
        result = true;
      }
    }
    if (!result)
    {
      System.out.println("?SYNTAX ERROR: ILLEGAL LOOP ARGUMENT");
      list(line_num, line_num);
    }
    return result;
  }

  private boolean parse_next(long line_num, List<String> parts)
  {
    if (parts.size() == 2)
    {
      String var_name = parts.get(1).toUpperCase();
      if (for_loops.containsKey(var_name))
      {
        ForLoop for_loop = for_loops.get(var_name);
        assembly.add("add %" + for_loop.register + " local %" + for_loop.register + " local %" + for_loop.step + " local");
        assembly.add("jump for_" + for_loop.line_num + "_begin");
        assembly.add(".mark: for_" + for_loop.line_num + "_end");
        for_loops.remove(var_name);
        return true;
      }
      else
      {
        System.out.println("?SYNTAX ERROR: UNKNOWN LOOP VARIABLE " + var_name);
        list(line_num, line_num);
        return false;
      }
    }
    else
    {
      System.out.println("?SYNTAX ERROR: EXPECTING FOR LOOP VARIABLE");
      list(line_num, line_num);
      return false;
    }
  }

  private boolean parse_if(long line_num, List<String> parts)
  {
    boolean result = false;
    int idx = 0;
    String label_if = "";
    String label_else = "";
    if (parts.size() > 2)
    {
      List<String> cond_exp = Utl.take_until(1, "THEN", parts);
      idx = 1 + cond_exp.size();
      if (parts.get(idx).toUpperCase().equals("THEN"))
      {
        if_idx++;
        if (parts.size() > (idx + 1))
        {
          label_if = parts.get(idx + 1).toLowerCase();
          long goto_line = 0;
          try
          {
            goto_line = Long.parseLong(label_if);
            result = goto_lines.contains(goto_line);
            label_if = "goto_line_" + goto_line;
          }
          catch (Exception ex)
          {
            result = labels.contains(label_if);
            label_if = "label_" + label_if;
          }
          if (result)
          {
            if (parts.size() > (idx + 2))
            {
              result = false;
              if (parts.get(idx + 2).toUpperCase().equals("ELSE"))
              {
                if (parts.size() == idx + 4)
                {
                  label_else = parts.get(idx + 3).toLowerCase();
                  try
                  {
                    goto_line = Long.parseLong(label_else);
                    result = goto_lines.contains(goto_line);
                    label_else = "goto_line_" + goto_line;
                  }
                  catch (Exception ex)
                  {
                    result = labels.contains(label_else);
                    label_else = "label_" + label_else;
                  }
                }
              }
            }
          }
          if (!result)
          {
            System.out.println("?SYNTAX ERROR: EXPECTING LINE NUMBER OR LABEL");
            list(line_num, line_num);
            return false;
          }
        }
        else
        {
          nested_ifs.push(if_idx);
          nested_elses.add(if_idx);
          label_if = "if_" + if_idx;
          label_else = "else_" + if_idx;
          result = true;
        }
        if (result)
        {
          result = false;
          if (parse_logic_exp(register, cond_exp, true))
          {
            String cond = "if %" + register + " local " + label_if;
            if (label_else.length() > 0)
            {
              cond = cond + " " + label_else;
            }
            assembly.add(cond);
            if (nested_ifs.size() > 0)
            {
              if (nested_ifs.peek().equals(if_idx))
              {
                assembly.add(".mark: " + label_if);
              }
            }
            result = true;
          }
        }
      }
    }
    if (!result)
    {
      System.out.println("?SYNTAX ERROR: ILLEGAL IF ARGUMENT");
      list(line_num, line_num);
    }
    return result;
  }

  private boolean parse_logic_exp(int cond_reg, List<String> exp, boolean show_err)
  {
    List<String> rpn = Utl.exp_to_logic_rpn(exp, vars);
    assembly.add("vector %" + (cond_reg + 1) + " local");
    int stack = 0;
    for (String arg : rpn)
    {
      if (arg.equals("=") || arg.equals(">") || arg.equals("<") || arg.equals(">=") || arg.equals("<=") || arg.equals("<>") || arg.equals("OR") || arg.equals("AND"))
      {
        if (stack < 2)
        {
          if (show_err)
          {
            System.out.println("?SYNTAX ERROR: ILLEGAL LOGICAL EXPRESSION " + Utl.exp_to_str(exp));
          }
          return false;
        }
        else
        {
          assembly.add("vpop %" + (cond_reg + 2) + " local %" + (cond_reg + 1) + " local");
          stack--;
          assembly.add("vpop %" + (cond_reg + 3) + " local %" + (cond_reg + 1) + " local");
          stack--;
          if (arg.equals("="))
          {
            assembly.add("eq %" + (cond_reg + 2) + " local %" + (cond_reg + 3) + " local %" + (cond_reg + 2) + " local");
          }
          if (arg.equals(">"))
          {
            assembly.add("gt %" + (cond_reg + 2) + " local %" + (cond_reg + 3) + " local %" + (cond_reg + 2) + " local");
          }
          if (arg.equals("<"))
          {
            assembly.add("lt %" + (cond_reg + 2) + " local %" + (cond_reg + 3) + " local %" + (cond_reg + 2) + " local");
          }
          if (arg.equals(">="))
          {
            assembly.add("gte %" + (cond_reg + 2) + " local %" + (cond_reg + 3) + " local %" + (cond_reg + 2) + " local");
          }
          if (arg.equals("<="))
          {
            assembly.add("lte %" + (cond_reg + 2) + " local %" + (cond_reg + 3) + " local %" + (cond_reg + 2) + " local");
          }
          if (arg.equals("<>"))
          {
            assembly.add("eq %" + (cond_reg + 2) + " local %" + (cond_reg + 3) + " local %" + (cond_reg + 2) + " local");
            assembly.add("not %" + (cond_reg + 2) + " local %" + (cond_reg + 2) + " local");
          }
          if (arg.equals("OR"))
          {
            assembly.add("or %" + (cond_reg + 2) + " local %" + (cond_reg + 3) + " local %" + (cond_reg + 2) + " local");
          }
          if (arg.equals("AND"))
          {
            assembly.add("and %" + (cond_reg + 2) + " local %" + (cond_reg + 3) + " local %" + (cond_reg + 2) + " local");
          }
          assembly.add("vpush %" + (cond_reg + 1) + " local %" + (cond_reg + 2) + " local");
          stack++;
        }
      }
      else if (arg.equals("NOT"))
      {
        if (stack < 1)
        {
          if (show_err)
          {
            System.out.println("?SYNTAX ERROR: ILLEGAL LOGICAL EXPRESSION " + Utl.exp_to_str(exp));
          }
          return false;
        }
        else
        {
          assembly.add("vpop %" + (cond_reg + 2) + " local %" + (cond_reg + 1) + " local");
          stack--;
          assembly.add("not %" + (cond_reg + 2) + " local %" + (cond_reg + 2) + " local");
          assembly.add("vpush %" + (cond_reg + 1) + " local %" + (cond_reg + 2) + " local");
          stack++;
        }
      }
      else
      {
        List<String> math_exp = new ArrayList<String>();
        math_exp.add(arg);
        if (parse_float_exp(cond_reg + 2, math_exp, false))
        {
          assembly.add("vpush %" + (cond_reg + 1) + " local %" + (cond_reg + 2) + " local");
          stack++;
        }
        else
        {
          if (show_err)
          {
            System.out.println("?SYNTAX ERROR: ILLEGAL ARGUMENT " + arg + " IN LOGICAL EXPRESSION " + Utl.exp_to_str(exp));
          }
          return false;
        }
      }
    }
    if (stack == 1)
    {
      assembly.add("vpop %" + cond_reg + " local %" + (cond_reg + 1) + " local");
      return true;
    }
    else
    {
      if (show_err)
      {
        System.out.println("?SYNTAX ERROR: ILLEGAL LOGICAL EXPRESSION " + Utl.exp_to_str(exp));
      }
      return false;
    }
  }

  private boolean parse_else(long line_num, List<String> parts)
  {
    if (nested_ifs.size() > 0)
    {
      assembly.add("jump endif_" + nested_ifs.peek());
      assembly.add(".mark: else_" + nested_ifs.peek());
      nested_elses.remove(nested_ifs.peek());
      return true;
    }
    else
    {
      System.out.println("?SYNTAX ERROR: UNMATCHED ELSE");
      list(line_num, line_num);
      return false;
    }
  }

  private boolean parse_endif(long line_num, List<String> parts)
  {
    if (nested_ifs.size() > 0)
    {
      if (nested_elses.contains(nested_ifs.peek()))
      {
        assembly.add(".mark: else_" + nested_ifs.peek());
      }
      assembly.add(".mark: endif_" + nested_ifs.pop());
      return true;
    }
    else
    {
      System.out.println("?SYNTAX ERROR: UNMATCHED ENDIF");
      list(line_num, line_num);
      return false;
    }
  }

  private boolean parse_dim(long line_num, List<String> parts)
  {
    boolean result = false;
    parts = Utl.list_split_separator(parts, '(', true, true, false);
    parts = Utl.list_split_separator(parts, ',', true, true, false);
    parts = Utl.list_split_separator(parts, ')', true, true, false);
    parts = Utl.list_split_separator(parts, '=', true, true, false);
    if (parts.size() > 5)
    {
      String var_name = parts.get(1).toUpperCase();
      if (parts.get(2).equals("("))
      {
        if (vars.exists(var_name))
        {
          System.out.println("?SYNTAX ERROR: VARIABLE " + var_name + " ALREADY EXISTS");
        }
        else
        {
          List<String> dims = Utl.take_until(3, "=", parts);
          if (dims.size() > 2)
          {
            if (dims.get(dims.size() - 2).equals(")"))
            {
              Variables.Type var_type = Variables.Type.UNKNOWN;
              if (dims.get(dims.size() - 1).toUpperCase().equals("INTEGER"))
              {
                var_type = Variables.Type.INTEGER;
              }
              if (dims.get(dims.size() - 1).toUpperCase().equals("FLOAT"))
              {
                var_type = Variables.Type.FLOAT;
              }
              if (dims.get(dims.size() - 1).toUpperCase().equals("STRING"))
              {
                var_type = Variables.Type.STRING;
              }
              if (var_type.equals(Variables.Type.UNKNOWN))
              {
                System.out.println("?SYNTAX ERROR: EXPECTING TYPE FOR ARRAY " + var_name);
              }
              else
              {
                int array_reg = register++;
                int init_reg = 0;
                result = true;
                if (parts.size() > (3 + dims.size()))
                {
                  result = false;
                  int idx = 3 + dims.size();
                  if (parts.get(idx).equals("="))
                  {
                    if (parts.size() > (idx + 1))
                    {
                      idx++;
                      if (var_type.equals(Variables.Type.INTEGER))
                      {
                        List<String> init_exp = parts.subList(idx, parts.size());
                        if (parse_integer_exp(array_reg + 1, init_exp, true))
                        {
                          init_reg = array_reg + 1;
                          result = true;
                        }
                      }
                      if (var_type.equals(Variables.Type.FLOAT))
                      {
                        List<String> init_exp = parts.subList(idx, parts.size());
                        if (parse_float_exp(array_reg + 1, init_exp, true))
                        {
                          init_reg = array_reg + 1;
                          result = true;
                        }
                      }
                      if (var_type.equals(Variables.Type.STRING))
                      {
                        List<String> init_exp = parts.subList(idx, parts.size());
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
                  if (init_reg == 0)
                  {
                    init_reg = array_reg + 1;
                    if (var_type.equals(Variables.Type.INTEGER))
                    {
                      assembly.add("integer %" + init_reg + " 0");
                    }
                    if (var_type.equals(Variables.Type.FLOAT))
                    {
                      assembly.add("float %" + init_reg + " 0");
                    }
                    if (var_type.equals(Variables.Type.STRING))
                    {
                      assembly.add("text %" + init_reg + " local \"\"");
                    }
                  }
                  dims.subList(dims.size() - 2, dims.size()).clear();
                  if (parse_dim_size(init_reg + 1, dims))
                  {
                    assembly.add("frame ^[(param %0 %" + (init_reg + 1) + " local) (param %1 %" + init_reg + " local)]");
                    assembly.add("call %" + array_reg + " local array_create/2");
                    vars.set_array(var_name, var_type, array_reg);
                    use_array = true;
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
      System.out.println("?SYNTAX ERROR: ILLEGAL ARRAY DECLARATION");
      list(line_num, line_num);
    }
    return result;
  }

  private boolean parse_dim_size(int size_reg, List<String> exp)
  {
    boolean result = false;
    exp = Utl.split_separator(Utl.exp_to_str(exp), ',', true, true, false);
    if ((exp.size() % 2) != 0)
    {
      result = true;
      int idx = 0;
      assembly.add("vector %" + size_reg + " local");
      while (idx < exp.size())
      {
        if (idx > 0)
        {
          result = result && (exp.get(idx - 1).equals(","));
          if (!result)
          {
            break;
          }
        }
        List<String> math_exp = new ArrayList<String>();
        math_exp.add(exp.get(idx));
        if (parse_integer_exp(size_reg + 1, math_exp, false))
        {
          assembly.add("vpush %" + size_reg + " local %" + (size_reg + 1) + " local");
        }
        else
        {
          result = false;
          break;
        }
        idx = idx + 2;
      }
    }
    return result;
  }
}
