using System;
using System.IO;
using System.Collections.Generic;

namespace ViuaBasic
{
  class MainClass
  {
    public static void Main(string[] args)
    {
      if (args.Length > 0)
      {
        if (File.Exists(args[0]))
        {
          List<string> srcBas = new List<string>();
          StreamReader sr = File.OpenText(args[0]);
          string line = sr.ReadLine();
          while (line != null)
          {
            srcBas.Add(line);
            line = sr.ReadLine();
          }
          BasicCompiler compiler = new BasicCompiler();
          if (compiler.load(srcBas))
          {
            compiler.compile();
          }
        }
        else
        {
          Console.WriteLine("file not found: " + args[0]);
        }
      }
      else
      {
        Console.WriteLine("ViuaBasic v0.1.0");
        Console.WriteLine("usage: viuabasic <src_file>");
      }
    }
  }
}
