using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace ViuaBasic
{
  class MainClass
  {
    public static void Main(string[] args)
    {
      Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Name + " v" + Assembly.GetExecutingAssembly().GetName().Version);
      if (args.Length > 1)
      {
        if (File.Exists(args[0]))
        {
          Console.WriteLine("reading: " + args[0]);
          List<string> srcBas = new List<string>();
          StreamReader sr = File.OpenText(args[0]);
          string line_in = sr.ReadLine();
          while (line_in != null)
          {
            srcBas.Add(line_in);
            line_in = sr.ReadLine();
          }
          BasicCompiler compiler = new BasicCompiler();
          if (compiler.load(srcBas))
          {
            compiler.compile();
            if (File.Exists(args[1]))
            {
              Console.WriteLine("overwriting: " + args[1]);
            }
            else
            {
              Console.WriteLine("writing: " + args[1]);
            }
            StreamWriter sw = new StreamWriter(args[1]);
            foreach (string line_out in compiler.output())
            {
              sw.WriteLine(line_out);
            }
            sw.Close();
            Console.WriteLine("compilation complete");
          }
        }
        else
        {
          Console.WriteLine("file not found: " + args[0]);
        }
      }
      else
      {
        Console.WriteLine("usage: " + Assembly.GetExecutingAssembly().GetName().Name + " <input_file.bas> <output_file.asm>");
      }
    }
  }
}
