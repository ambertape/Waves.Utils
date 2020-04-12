﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Fluid.Utils.Nuget.Packer
{
    /// <summary>
    /// Fluid nuget packer utility.
    /// </summary>
    class Program
    {
        private const string ProgramName = "[Fluid Nuget Packer]";

        private const string InformationKey = "[INFORMATION]";
        private const string WarningKey = "[WARNING]";
        private const string ErrorKey = "[ERROR]";

        private const string NugetVersionKey = "{$NUGETVERSION}";
        private const string PackCommandKey = "pack";
        private const string OutputDirectoryKey = "-OutputDirectory";
        private const string VersionKey = "-Version";
        private const string PropertiesKey = "-Properties";

        /// <summary>
        /// Gets or sets cmd.exe path.
        /// </summary>
        private static string CmdExePath { get; set; } = "CMD.exe";

        /// <summary>
        /// Path to NuGet.exe
        /// </summary>
        private static string NugetExePath { get; set; }

        /// <summary>
        /// Gets or sets path to directory which includes "nuspec" and "templates" folders.
        /// "nuspec" folder contains generated .nuspec files.
        /// "templates" folder contains template for nuspec files.
        /// </summary>
        private static string WorkingPath { get; set; }

        /// <summary>
        /// Gets nuspec templates folder.
        /// </summary>
        private static string WorkingTemplatesPath => Path.Combine(WorkingPath, "templates");

        /// <summary>
        /// Gets nuspec folder.
        /// </summary>
        private static string WorkingNuspecPath => Path.Combine(WorkingPath, "nuspec");

        /// <summary>
        /// Gets or sets output directory for nuget packages.
        /// </summary>
        private static string OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets nuget version.
        /// </summary>
        private static string Version { get; set; }

        /// <summary>
        /// Gets or sets other properties.
        /// </summary>
        private static string Properties { get; set; }

        /// <summary>
        /// Gets or sets template files collection.
        /// </summary>
        private static List<string> Templates { get; set; } = new List<string>();

        /// <summary>
        /// Main method.
        /// </summary>
        /// <param name="args">Arguments.</param>
        static void Main(string[] args)
        {
            Initialize(args);
        }

        /// <summary>
        /// Initialized program.
        /// </summary>
        /// <param name="args"></param>
        private static void Initialize(string[] args)
        {
            try
            {
                if (args.Length != 9)
                {
                    throw new ArgumentException("Invalid arguments specified.");
                }

                // nuget.exe
                if (!File.Exists(args[0]))
                {
                    throw new FileNotFoundException("Nuget.exe not found.", args[0]);
                }
                else
                {
                    NugetExePath = args[0];
                    Console.WriteLine("{0} {1}: {2}", ProgramName, InformationKey, "NuGet.exe found.");
                }

                // pack
                if (!args[1].Equals(PackCommandKey))
                {
                    throw new ArgumentException("Invalid arguments specified (pack).");
                }

                // working path
                if (!Directory.Exists(args[2]))
                {
                    throw new DirectoryNotFoundException("Working directory not found (" + args[2] + ").");
                }
                else
                {
                    WorkingPath = args[2];

                    if (!Directory.Exists(WorkingTemplatesPath))
                    {
                        throw new DirectoryNotFoundException("Templates directory not found (" + WorkingTemplatesPath + ").");
                    }
                    else
                    {
                        Templates.Clear();

                        foreach (var file in Directory.GetFiles(WorkingTemplatesPath))
                        {
                            var templateFileInformation = new FileInfo(file);
                            if (templateFileInformation.Extension.Equals(".template"))
                            {
                                Templates.Add(file);
                                Console.WriteLine("{0} {1}: {2}", ProgramName, InformationKey, "Template file added " + file);
                            }
                        }
                    }

                    if (!Directory.Exists(WorkingNuspecPath))
                    {
                        throw new DirectoryNotFoundException("Nuspec directory not found (" + WorkingNuspecPath + ").");
                    }

                    Console.WriteLine("{0} {1}: {2}", ProgramName, InformationKey, "Working directory found and initialized.");
                }

                // output directory key
                if (!args[3].Equals(OutputDirectoryKey))
                {
                    throw new ArgumentException("Invalid arguments specified (-OutputDirectory).");
                }

                // output directory
                OutputDirectory = args[4];
                if (!Directory.Exists(OutputDirectory))
                {
                    Directory.CreateDirectory(OutputDirectory);
                    Console.WriteLine("{0} {1}: {2}", ProgramName, InformationKey, "Output directory created " + OutputDirectory);
                }

                // version key
                if (!args[5].Equals(VersionKey))
                {
                    throw new ArgumentException("Invalid arguments specified (-Version).");
                }

                // version
                Version = args[6];
                Console.WriteLine("{0} {1}: {2}", ProgramName, InformationKey, "Version initialized - " + OutputDirectory);

                // properties key
                if (!args[7].Equals(PropertiesKey))
                {
                    throw new ArgumentException("Invalid arguments specified (-Properties).");
                }

                // properties
                Properties = args[8];
                Console.WriteLine("{0} {1}: {2}", ProgramName, InformationKey, "Properties initialized - " + Properties);

                Console.WriteLine("{0} {1}: {2}", ProgramName, InformationKey, "Utility initialized successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} {1}: {2}", ProgramName, ErrorKey, "An error occurred while initializing the utility:\r\n" + e);
            }
        }

        /// <summary>
        /// Creates packages.
        /// </summary>
        private static void Pack()
        {
            foreach (var templateFileFullName in Templates)
            {
                try
                {
                    // copy
                    var templateFileInformation = new FileInfo(templateFileFullName);
                    var nuspecFileName = templateFileInformation.Name.Replace(".template", string.Empty);
                    var nuspecFileFullName = Path.Combine(WorkingNuspecPath, nuspecFileName);

                    Console.WriteLine("{0} {1}: {2}", ProgramName, InformationKey, "Copying template... (" + templateFileFullName + ").");

                    File.Copy(templateFileFullName, nuspecFileFullName);

                    if (!File.Exists(nuspecFileFullName))
                    {
                        throw new FileNotFoundException("Nuspec file not copied (" + templateFileFullName + ")", nuspecFileFullName);
                    }
                    else
                    {
                        Console.WriteLine("{0} {1}: {2}", ProgramName, InformationKey, "Nuspec file was copied from template (" + nuspecFileFullName + ").");
                    }

                    // replace data
                    Console.WriteLine("{0} {1}: {2}", ProgramName, InformationKey, "Replacing data... (" + templateFileFullName + ").");

                    var hasChanges = false;

                    var lines = File.ReadAllLines(nuspecFileFullName);

                    for (var i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].Contains(NugetVersionKey))
                        {
                            lines[i] = lines[i].Replace(NugetVersionKey, Version);
                            hasChanges = true;
                        }
                    }

                    File.WriteAllLines(nuspecFileFullName, lines);

                    if (hasChanges)
                    {
                        Console.WriteLine("{0} {1}: {2}", ProgramName, InformationKey, "Nuspec file was changed (" + nuspecFileFullName + ").");
                    }
                    else
                    {
                        Console.WriteLine("{0} {1}: {2}", ProgramName, WarningKey, "Nuspec file wasn't changed (" + nuspecFileFullName + ")");
                    }

                    Console.WriteLine("{0} {1}: {2}", ProgramName, InformationKey, "Creating package... (" + nuspecFileFullName + ").");

                    //C:\TeamCity\buildAgent\tools\NuGet.CommandLine.5.4.0\tools\NuGet.exe pack C:\TeamCity\buildAgent\work\a7f18e0ed8272361\nuget\nuspec\Fluid.Core.Base.Enums.nuspec -OutputDirectory C:\TeamCity\buildAgent\work\a7f18e0ed8272361\bin\packages -Version 2020.1.63-alpha -Properties Configuration=Release

                    var command = NugetExePath + " " +
                                  PackCommandKey + " " +
                                  nuspecFileFullName + " " +
                                  OutputDirectoryKey + " " +
                                  OutputDirectory + " " +
                                  VersionKey + " " +
                                  Version + " " +
                                  PropertiesKey + " " +
                                  Properties;

                    System.Diagnostics.Process.Start(CmdExePath, command);

                    Console.WriteLine("{0} {1}: {2}", ProgramName, InformationKey, "Package created from nuspec file (" + nuspecFileFullName + ").");
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0} {1}: {2}", ProgramName, ErrorKey, "An error occurred while initializing the utility:\r\n" + e);
                }
            }
        }

        //

        //static void Main(string[] args)
        //{
        //    try
        //    {
        //        var watch = new Stopwatch();
        //        watch.Start();

        //        Console.WriteLine("[Fluid Version Tool] {0}", "Started.");

        //        if (args.Length != 2)
        //            throw new Exception("[Fluid Version Tool]: Input arguments not valid.");

        //        var path = args[0];
        //        var version = args[1];

        //        var files = Directory.GetFiles(path, "*.nuspec", SearchOption.TopDirectoryOnly);

        //        foreach (var file in files)
        //        {
        //            Console.WriteLine("[Fluid Version Tool] {0}: {1}", "Updating file", file);

        //            var hasChanges = false;

        //            var lines = File.ReadAllLines(file);

        //            for (var i = 0; i < lines.Length; i++)
        //            {
        //                if (lines[i].Contains(NugetVersionKey))
        //                {
        //                    lines[i] = lines[i].Replace(NugetVersionKey, version);
        //                    hasChanges = true;
        //                }
        //            }

        //            File.WriteAllLines(file, lines);

        //            if (hasChanges)
        //            {
        //                Console.ForegroundColor = ConsoleColor.Green;
        //                Console.WriteLine("[Fluid Version Tool] {0}: {1}", "File updated", file);
        //                Console.ForegroundColor = ConsoleColor.Gray;
        //            }
        //            else
        //            {
        //                Console.ForegroundColor = ConsoleColor.Yellow;
        //                Console.WriteLine("[Fluid Version Tool] {0}: {1}", "File not updated", file);
        //                Console.ForegroundColor = ConsoleColor.Gray;
        //            }
        //        }

        //        Console.WriteLine("[Fluid Version Tool] {0}", "Success.");

        //        watch.Stop();

        //        Console.WriteLine("[Fluid Version Tool] {0}", "Time ellapsed: " + watch.Elapsed.TotalSeconds + " sec.");
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //        throw;
        //    }
        //}
    }
}
