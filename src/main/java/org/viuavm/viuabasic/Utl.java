package org.viuavm.viuabasic;

import java.util.ArrayDeque;
import java.util.ArrayList;
import java.util.Deque;
import java.util.List;

public class Utl
{
  public static List<String> split_line(String line)
  {
    List<String> parts = new ArrayList<String>();
    String part = "";
    boolean in_quote = false;
    boolean out_quote = false;
    for (Character c : line.toCharArray())
    {
      if (Character.isWhitespace(c))
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
          if (part.length() > 0)
          {
            parts.add(part);
            part = "";
          }
        }
      }
      else if (c.equals('"'))
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
    if (part.length() > 0)
    {
      parts.add(part);
    }
    return parts;
  }

  public static List<String> split_separator(String line, char sep, boolean trim, boolean keep_sep, boolean use_parentheses)
  {
    List<String> parts = new ArrayList<String>();
    String part = "";
    boolean in_quote = false;
    boolean out_quote = false;
    int parentheses = 0;
    for (Character c : line.toCharArray())
    {
      if (c.equals(sep))
      {
        if (use_parentheses && (parentheses > 0))
        {
          part = part + c;
        }
        else if (in_quote)
        {
          part = part + c;
        }
        else
        {
          if (trim)
          {
            part = part.trim();
          }
          if (part.length() > 0)
          {
            parts.add(part);
          }
          part = "";
          if (keep_sep)
          {
            parts.add(part + c);
          }
        }
      }
      else if (c.equals('"'))
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
        if (use_parentheses)
        {
          if (c.equals('('))
          {
            parentheses++;
          }
          if (c.equals(')'))
          {
            parentheses--;
          }
        }
      }
    }
    if (trim)
    {
      part = part.trim();
    }
    if (part.length() > 0)
    {
      parts.add(part);
    }
    return parts;
  }

  public static List<String> list_split_separator(List<String> lines, char sep, boolean trim, boolean keep_sep, boolean use_parentheses)
  {
    List<String> result = new ArrayList<String>();
    for (String line : lines)
    {
      result.addAll(split_separator(line, sep, trim, keep_sep, use_parentheses));
    }
    return result;
  }

  public static boolean is_quoted(String arg)
  {
    return (arg.length() > 1) && (arg.startsWith("\"")) && (arg.endsWith("\""));
  }

  public static String escape_quotes(String arg)
  {
    String result = arg;
    if (arg.length() > 3)
    {
      result = arg.substring(1, arg.length() - 1);
      result = result.replace("\"\"", "\\\"");
      result = "\"" + result + "\"";
    }
    return result;
  }

  public static String exp_to_str(List<String> arg)
  {
    String buf = "";
    for (String s : arg)
    {
      buf = buf + s;
    }
    return buf;
  }

  public static List<String> exp_to_math_rpn(List<String> exp, Variables vars)
  {
    List<String> math = exp_to_math(exp);
    List<String> rpn = new ArrayList<String>();
    Deque<String> stack = new ArrayDeque<String>();
    boolean unary = true;
    boolean negate = false;
    for (String arg : math)
    {
      if (arg.equals("("))
      {
        if (negate)
        {
          rpn.add("-1");
          while (stack.size() > 0)
          {
            if (stack.peek().equals("*") || stack.peek().equals("/") || stack.peek().equals("%") || stack.peek().equals("^"))
            {
              rpn.add(stack.pop());
              continue;
            }
            else
            {
              break;
            }
          }
          stack.push("*");
          negate = false;
        }
        stack.push(arg);
        unary = true;
      }
      else if (arg.equals(")"))
      {
        while (stack.size() > 0)
        {
          String elem = stack.pop();
          if (elem.equals("("))
          {
            if (stack.size() > 0)
            {
              elem = stack.peek();
              if (elem.equals("ABS") || elem.equals("EXP") || elem.equals("LOG") || vars.is_array(elem))
              {
                rpn.add(stack.pop());
              }
            }
            break;
          }
          else
          {
            rpn.add(elem);
          }
        }
        unary = false;
        negate = false;
      }
      else if (arg.equals(","))
      {
        while (stack.size() > 0)
        {
          if (stack.peek().equals(",") || stack.peek().equals("+") || stack.peek().equals("-") || stack.peek().equals("*") || stack.peek().equals("/") || stack.peek().equals("%") || stack.peek().equals("^"))
          {
            rpn.add(stack.pop());
            continue;
          }
          else
          {
            break;
          }
        }
        stack.push(arg);
        unary = true;
        negate = false;
      }
      else if (arg.equals("+") || arg.equals("-"))
      {
        if (unary)
        {
          if (arg.equals("-"))
          {
            negate = !negate;
          }
        }
        else
        {
          while (stack.size() > 0)
          {
            if (stack.peek().equals("+") || stack.peek().equals("-") || stack.peek().equals("*") || stack.peek().equals("/") || stack.peek().equals("%") || stack.peek().equals("^"))
            {
              rpn.add(stack.pop());
              continue;
            }
            else
            {
              break;
            }
          }
          stack.push(arg);
          unary = true;
          negate = false;
        }
      }
      else if (arg.equals("*") || arg.equals("/") || arg.equals("%"))
      {
        while (stack.size() > 0)
        {
          if (stack.peek().equals("*") || stack.peek().equals("/") || stack.peek().equals("%") || stack.peek().equals("^"))
          {
            rpn.add(stack.pop());
            continue;
          }
          else
          {
            break;
          }
        }
        stack.push(arg);
        unary = true;
        negate = false;
      }
      else if (arg.equals("^"))
      {
        while (stack.size() > 0)
        {
          if (stack.peek().equals("^"))
          {
            rpn.add(stack.pop());
            continue;
          }
          else
          {
            break;
          }
        }
        stack.push(arg);
        unary = true;
        negate = false;
      }
      else if (arg.toUpperCase().equals("ABS") || arg.toUpperCase().equals("EXP") || arg.toUpperCase().equals("LOG") || vars.is_array(arg.toUpperCase()))
      {
        stack.push(arg.toUpperCase());
        unary = false;
        negate = false;
      }
      else
      {
        if (negate)
        {
          rpn.add("-" + arg);
        }
        else
        {
          rpn.add(arg);
        }
        unary = false;
        negate = false;
      }
    }
    while (stack.size() > 0)
    {
      rpn.add(stack.pop());
    }
    return rpn;
  }

  public static List<String> exp_to_math(List<String> exp)
  {
    List<String> tmp = list_split_separator(exp, '(', true, true, false);
    tmp = list_split_separator(tmp, ')', true, true, false);
    tmp = list_split_separator(tmp, ',', true, true, false);
    tmp = list_split_separator(tmp, '+', true, true, false);
    tmp = list_split_separator(tmp, '-', true, true, false);
    tmp = list_split_separator(tmp, '*', true, true, false);
    tmp = list_split_separator(tmp, '/', true, true, false);
    tmp = list_split_separator(tmp, '%', true, true, false);
    tmp = list_split_separator(tmp, '^', true, true, false);
    return tmp;
  }

  public static List<String> exp_to_logic_rpn(List<String> exp, Variables vars)
  {
    List<String> cond = exp_to_logic(exp, vars);
    List<String> rpn = new ArrayList<String>();
    Deque<String> stack = new ArrayDeque<String>();
    for (String arg : cond)
    {
      if (arg.equals("("))
      {
        stack.push(arg);
      }
      else if (arg.equals(")"))
      {
        while (stack.size() > 0)
        {
          String elem = stack.pop();
          if (elem.equals("("))
          {
            break;
          }
          else
          {
            rpn.add(elem);
          }
        }
      }
      else if (arg.equals("=") || arg.equals(">") || arg.equals("<") || arg.equals(">=") || arg.equals("<=") || arg.equals("<>"))
      {
        while (stack.size() > 0)
        {
          if (stack.peek().equals("=") || stack.peek().equals(">") || stack.peek().equals("<") || stack.peek().equals(">=") || stack.peek().equals("<=") || stack.peek().equals("<>") || stack.peek().equals("OR") || stack.peek().equals("AND") || stack.peek().equals("NOT"))
          {
            rpn.add(stack.pop());
            continue;
          }
          else
          {
            break;
          }
        }
        stack.push(arg);
      }
      else if (arg.equals("OR"))
      {
        while (stack.size() > 0)
        {
          if (stack.peek().equals("OR") || stack.peek().equals("AND") || stack.peek().equals("NOT"))
          {
            rpn.add(stack.pop());
            continue;
          }
          else
          {
            break;
          }
        }
        stack.push(arg);
      }
      else if (arg.equals("AND"))
      {
        while (stack.size() > 0)
        {
          if (stack.peek().equals("AND") || stack.peek().equals("NOT"))
          {
            rpn.add(stack.pop());
            continue;
          }
          else
          {
            break;
          }
        }
        stack.push(arg);
      }
      else if (arg.equals("NOT"))
      {
        while (stack.size() > 0)
        {
          if (stack.peek().equals("NOT"))
          {
            rpn.add(stack.pop());
            continue;
          }
          else
          {
            break;
          }
        }
        stack.push(arg);
      }
      else
      {
        rpn.add(arg);
      }
    }
    while (stack.size() > 0)
    {
      rpn.add(stack.pop());
    }
    return rpn;
  }

  public static List<String> exp_to_logic(List<String> exp, Variables vars)
  {
    List<String> tmp1 = list_split_separator(exp, '(', true, true, false);
    tmp1 = list_split_separator(tmp1, ')', true, true, false);
    tmp1 = list_split_separator(tmp1, '<', true, true, false);
    tmp1 = list_split_separator(tmp1, '>', true, true, false);
    tmp1 = list_split_separator(tmp1, '=', true, true, false);
    List<String> tmp2 = new ArrayList<String>();
    int idx = 0;
    while (idx < tmp1.size())
    {
      if ((idx + 1) < tmp1.size())
      {
        if (tmp1.get(idx).equals("<"))
        {
          if (tmp1.get(idx + 1).equals("="))
          {
            tmp2.add("<=");
            idx = idx + 2;
            continue;
          }
          if (tmp1.get(idx + 1).equals(">"))
          {
            tmp2.add("<>");
            idx = idx + 2;
            continue;
          }
        }
        if (tmp1.get(idx).equals(">"))
        {
          if (tmp1.get(idx + 1).equals("="))
          {
            tmp2.add(">=");
            idx = idx + 2;
            continue;
          }
        }
      }
      if (tmp1.get(idx).toUpperCase().equals("OR"))
      {
        tmp2.add("OR");
      }
      else if (tmp1.get(idx).toUpperCase().equals("AND"))
      {
        tmp2.add("AND");
      }
      else if (tmp1.get(idx).toUpperCase().equals("NOT"))
      {
        tmp2.add("NOT");
      }
      else
      {
        tmp2.add(tmp1.get(idx));
      }
      idx++;
    }
    idx = 0;
    tmp1.clear();
    while (idx < tmp2.size())
    {
      if (tmp2.get(idx).equals("(") && ((idx + 1) < tmp2.size()))
      {
        int math_idx = idx + 1;
        int math_cnt = 0;
        String math = "";
        while (math_idx < tmp2.size())
        {
          if (tmp2.get(math_idx).equals("("))
          {
            math_cnt++;
            math = math + tmp2.get(math_idx++);
            continue;
          }
          else if (tmp2.get(math_idx).equals(")"))
          {
            if (math_cnt > 0)
            {
              math_cnt--;
              math = math + tmp2.get(math_idx++);
              continue;
            }
            else
            {
              break;
            }
          }
          else if (tmp2.get(math_idx).equals("=") || tmp2.get(math_idx).equals(">") || tmp2.get(math_idx).equals("<") || tmp2.get(math_idx).equals(">=") || tmp2.get(math_idx).equals("<=") || tmp2.get(math_idx).equals("<>") || tmp2.get(math_idx).equals("OR") || tmp2.get(math_idx).equals("AND") || tmp2.get(math_idx).equals("NOT"))
          {
            math = "";
            break;
          }
          math = math + tmp2.get(math_idx++);
        }
        if (math.isEmpty())
        {
          tmp1.add(tmp2.get(idx));
          idx++;
        }
        else
        {
          if ((tmp1.size() > 0) && (vars.is_array(tmp1.get(tmp1.size() - 1).toUpperCase())))
          {
            tmp1.set(tmp1.size() - 1, tmp1.get(tmp1.size() - 1) + "(" + math + ")");
          }
          else
          {
            tmp1.add(math);
          }
          idx = math_idx + 1;
        }
      }
      else
      {
        tmp1.add(tmp2.get(idx));
        idx++;
      }
    }
    return tmp1;
  }

  public static List<String> take_until(int from_idx, String to_ident, List<String> parts)
  {
    List<String> result = new ArrayList<String>();
    int idx = from_idx;
    while (idx < parts.size())
    {
      if (parts.get(idx).toUpperCase().equals(to_ident))
      {
        return result;
      }
      result.add(parts.get(idx++));
    }
    return result;
  }
}
