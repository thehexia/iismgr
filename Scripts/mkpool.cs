using System;
using System.IO;
using System.Collections.Generic;
using NDesk.Options;
using IISH;
using Microsoft.Web.Administration;

namespace IISUtil
{

  public class MakePool
  {
    private static void Usage(OptionSet opts)
    {
      Console.WriteLine("Usage: ");
      Console.WriteLine("mkpool add <site-name> <file-path>");
      Console.WriteLine("mkpool remove <site-name>");
      Console.WriteLine();
      Console.WriteLine("Options: ");
      opts.WriteOptionDescriptions(Console.Out);
    }

    public static void Main(string[] args)
    {
      bool help = false;
      var opts = new OptionSet () {
        { "h|help",  "show this message and exit", v => help = true },
      };

      // All arguments that are not options or optional arguments.
      List<string> extra;
      try
      {
        extra = opts.Parse (args);
      }
      catch (OptionException e) {
        Console.WriteLine (e.Message);
        Console.WriteLine ("Try `--help' for more information.");
        return;
      }

      if (help)
      {
        Usage(opts);
        return;
      }

      try
      {
        IISHandler handler =  new IISHandler();
        string kind = "";
        if (!(extra.Count > 1))
          throw new ArgumentException("Not enough arguments.");

        kind = extra[0];
        if (kind.Equals("add"))
        {
          handler.AddApplicationPool(extra[1]);
        }
        else if(kind.Equals("remove"))
        {
          handler.RemoveApplicationPool(extra[1]);
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
      }
    }
  }
}
