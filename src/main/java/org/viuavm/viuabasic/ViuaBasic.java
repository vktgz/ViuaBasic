package org.viuavm.viuabasic;

import java.io.File;
import java.io.OutputStream;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.util.List;

public class ViuaBasic
{
  public static void main(String[] args)
  {
    System.out.println(ViuaBasic.class.getPackage().getImplementationTitle() + " v" + ViuaBasic.class.getPackage().getImplementationVersion());
    if (args.length > 1)
    {
      File srcFile = new File(args[0]);
      if (srcFile.exists() && srcFile.isFile() && srcFile.canRead())
      {
        System.out.println("reading: " + args[0]);
        List<String> srcBas;
        try
        {
          srcBas = Files.readAllLines(srcFile.toPath(), StandardCharsets.UTF_8);
        }
        catch (Exception ex)
        {
          System.out.println("error reading " + args[0] + ": " + ex.getMessage());
          return;
        }
        BasicCompiler compiler = new BasicCompiler();
        if (compiler.load(srcBas))
        {
          if (compiler.compile())
          {
            File dstFile = new File(args[1]);
            if (dstFile.exists() && dstFile.isFile() && dstFile.canWrite())
            {
              System.out.println("overwriting: " + args[1]);
            }
            else
            {
              System.out.println("writing: " + args[1]);
            }
            try
            {
              OutputStream dstAsm = Files.newOutputStream(dstFile.toPath());
              for (String line_out : compiler.output())
              {
                dstAsm.write(line_out.getBytes(StandardCharsets.UTF_8));
              }
              dstAsm.close();
            }
            catch (Exception ex)
            {
              System.out.println("error writing " + args[1] + ": " + ex.getMessage());
              return;
            }
            System.out.println("compilation complete");
          }
          else
          {
            System.out.println("compilation incomplete");
          }
        }
      }
      else
      {
        System.out.println("file not found: " + args[0]);
      }
    }
    else
    {
      System.out.println("usage: java -jar " + ViuaBasic.class.getPackage().getImplementationTitle() + "-" + ViuaBasic.class.getPackage().getImplementationVersion() + ".jar <input_file.bas> <output_file.asm>");
    }
  }
}
