using System;
using System.IO;
using System.Collections.Generic;
using NDesk.Options;
using IISH;
using Microsoft.Web.Administration;

namespace IISUtil
{
  public class MakeApplication
  {
    private static void Usage(OptionSet opts)
    {
      Console.WriteLine("Adds applications to a site on the IIS server.");
      Console.WriteLine("Usage: ");
      Console.WriteLine("mkapp.exe <physical-path> <site-name>");
      Console.WriteLine("mkapp.exe <virtual-path> <physical-path> <site-name>");
      Console.WriteLine();
      Console.WriteLine(@"<virtual-path>: The virtual path of the application
                          on the web server. If not specified, will be auto
                          generated using the name of the physical path folder.");

      Console.WriteLine("<physical-path>: The location of the file on the disk.");
      Console.WriteLine("<site-name>: Name of the site in IIS. Use double quotes for sites with spaces.");
      Console.WriteLine();
      Console.WriteLine("Options: ");
      opts.WriteOptionDescriptions(Console.Out);
    }

    // Adds an application found in the given physical path.
    // Virtual directory is user defined.
    private static void
    AddApplication(IISHandler handler, string virPath, string physPath, string siteName)
    {
      string fullPath = Path.GetFullPath(physPath);
      Console.WriteLine("Adding app from dir: " + fullPath);
      Console.WriteLine("With virtual dir: " + virPath);
      Site site = handler.GetSiteByName(siteName);

      try
      {
        if (site != null)
          site.Applications.Add(virPath, fullPath);
      }
      catch (Exception e) { Console.WriteLine(e.Message); }

      handler.CommitChanges();
    }

    // Adds an application found in the given physical path.
    // Creates the virtual directory name automatically.
    private static void
    AddApplication(IISHandler handler, string physPath, string siteName)
    {
      String[] paths = null;
      if (PathHelper.ContainsWildcard(physPath))
      {
        paths = PathHelper.ExpandWildcardDirectories(physPath);
        foreach (var subPath in paths)
        {
          string virPath = "";
          Site site = handler.GetSiteByName(siteName);
          if (site != null)
            virPath = PathHelper.SuggestVirtualDirectory(site, subPath);

          AddApplication(handler, virPath, subPath, siteName);
        }
      }
      else
      {
        string virPath = "";
        Site site = handler.GetSiteByName(siteName);
        if (site != null)
          virPath = PathHelper.SuggestVirtualDirectory(site, physPath);

        AddApplication(handler, virPath, physPath, siteName);
      }
    }


    public static void Main(string[] args)
    {
      bool recursiveAdd = false;
      bool help = false;

      var opts = new OptionSet () {
        { "h|help",  "show this message and exit", v => help = true },
        { "r|recursive", "recursively add all sub folders", v => recursiveAdd = true }
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
        IISHandler handler = new IISHandler();
        // Handle two possible arguments.
        if (extra.Count == 2)
        {
          if (recursiveAdd)
          {
            // Add the top level folder.
            AddApplication(handler, extra[0], extra[1]);
            // Recursively find all subfolders excluding bin and App_* folders.
            List<string> dirs = PathHelper.GetDirectoriesRecursively(extra[0]);
            // Add sub folders to the site as applications.
            foreach (var dirPath in dirs)
            {
              try
              {
                AddApplication(handler, dirPath, extra[1]);
              }
              catch (Exception e)
              {
                Console.WriteLine(e);
              }
            }
          }
          else
            AddApplication(handler, extra[0], extra[1]);
        }
        else if (extra.Count == 3)
        {
          if (recursiveAdd)
          {
            Console.WriteLine("Cannot use -r while specifying virtual path.");
          }
          else
            AddApplication(handler, extra[0], extra[1], extra[2]);
        }
        else
          Usage(opts);
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
      }
    }
  }
}
