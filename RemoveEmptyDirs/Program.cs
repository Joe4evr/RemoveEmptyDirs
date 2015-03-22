using System;
using System.IO;
using System.Linq;

namespace RemoveEmptyDirs
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("RemoveEmptyDirs v1.0 - Utility to automatically remove empty directories\n");
            if (args.Length == 0)
            {
                Console.WriteLine("No argument specified. Please use /? for help, specify a path or choose /all.");
                return;
            }

            if(args[0] == "/?")
            {
                Console.WriteLine("Usage: RemoveEmptyDirs.exe <path> [/R, /L, /V]");
                Console.WriteLine("       RemoveEmptyDirs.exe /all [/L, /V]\n");
                Console.WriteLine("/R: Recursive - Scan subdirectories of the given path as well.");
                Console.WriteLine("/L: List only - Only look for empty directories, but don't delete anything.");
                Console.WriteLine("/V: Verbose - Show which folders are deleted (or would be when used with /L).");
                Console.WriteLine("/all: Scan all drives for empty directories. Implies /R. Can be dangerous.");
                return;
            }

            int RemDirs = 0;
            bool recur = false;
            bool listonly = false;
            bool verbose = false;
            foreach (string arg in args)
            {
                if(arg.ToUpper() == "/R")
                {
                    recur = true;
                }
                if(arg.ToUpper() == "/L")
                {
                    listonly = true;    
                }
                if (arg.ToUpper() == "/V")
                {
                    verbose = true;
                }
            }
            
            if (args[0].ToLower() == "/all")
            {
                string[] dr = Directory.GetLogicalDrives();
                Console.WriteLine("Removing empty directories from ALL the drives. This may or may not have bad consequences.");
                Console.WriteLine("Current connected drives are: ");
                foreach (string d in dr)
                {
                    Console.WriteLine(d);
                }
                Console.WriteLine("If you are sure you want to continue, press Y, otherwise choose something else or leave blank.");
                string r = Console.ReadLine();
                if (r.ToUpper() == "Y")
                {
                    Console.WriteLine("Your funeral if something breaks. . .");
                    foreach (string drive in dr)
                    {
                        RemoveDirs(drive, ref RemDirs,recur,listonly,verbose);
                    }
                    if (listonly)
                    {
                        Console.WriteLine(String.Format("Counted {0} empty directories in ALL connected drives.",RemDirs));
                    }
                    else
                    {
                        Console.WriteLine(String.Format("Removed {0} empty directories from ALL connected drives.",RemDirs));
                    }
                }
                else
                {
                    Console.WriteLine("Operation aborted. Exiting.");
                }
                return;
            }
            
            if (!Directory.Exists(args[0]))
            {
                Console.WriteLine("Specified path does not exist or is invalid.");
                return;
            }
            
            string indir = Path.GetFullPath(args[0]);
            if (verbose && !listonly)
            {
                Console.WriteLine("Deleting the following directories:");
            }
            if (recur)
            {
                if(!listonly)
                {
                    Console.WriteLine("Removing empty directories including empty subdirectories. . .");
                }
                RemoveDirs(indir, ref RemDirs,recur,listonly,verbose);
                if (listonly)
                {
                    Console.WriteLine(String.Format("Counted {0} empty directories in {1} and its subdirectories.", RemDirs, indir));
                }
                else
                {
                    Console.WriteLine(String.Format("Removed {0} empty directories from {1} and its subdirectories.",RemDirs,indir));
                }
            }
            else
            {
                if(!listonly)
                {
                    Console.WriteLine("Removing empty directories. . .");
                }
                RemoveDirs(indir, ref RemDirs,recur,listonly,verbose);
                if (listonly)
                {
                    Console.WriteLine(String.Format("Counted {0} empty directories in {1}.", RemDirs, indir));
                }
                else
                {
                    Console.WriteLine(String.Format("Removed {0} empty directories from {1}.", RemDirs, indir));
                }
            }
        }

        private static void RemoveDirs(string InputDir,ref int RemDirs,bool recur = false,bool listonly = false,bool verbose = false)
        {
            if (!Enum.GetNames(typeof(Environment.SpecialFolder)).Contains(InputDir))
            {
                string[] SubDirs = Directory.GetDirectories(InputDir);
                foreach (string dir in SubDirs)
                {
                    string[] d;
                    try
                    {
                        d = Directory.GetDirectories(dir);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.WriteLine("Permission denied in " + dir);
                        return;
                    }

                    if(recur && d.Length >= 1)
                    {
                        RemoveDirs(dir,ref RemDirs,listonly,verbose,recur);
                        d = Directory.GetDirectories(dir);
                    }
                    
                    string[] f = Directory.GetFiles(dir);
                    string[] content = f.Concat(d).ToArray();
                    
                    if (content.Length == 0)
                    {
                        if (verbose)
                        {
                            Console.WriteLine(Path.GetFullPath(dir));
                        }
                        if(!listonly)
                        {
                            try
                            {
                                Directory.Delete(dir);
                            }
                            catch (UnauthorizedAccessException)
                            {
                                Console.WriteLine(String.Format("ERROR: Directory {0} is read-only.", dir));
                                RemDirs--;
                            }
                            catch(IOException)
                            {
                                Console.WriteLine(String.Format("ERROR: Could not delete {0}.", dir));
                                RemDirs--;
                            }
                        }
                        RemDirs++;
                    }
                }
            }
        }

        static void RemoveDirs(string InputDir,bool recur = false,bool listonly = false,bool verbose = false)
        {
            int i = 0;
            RemoveDirs(InputDir, ref i, recur, listonly, verbose);
        }
    }
}