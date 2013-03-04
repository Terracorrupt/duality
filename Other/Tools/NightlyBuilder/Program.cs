﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;

using Microsoft.Win32;
using Microsoft.Build.Execution;
using Microsoft.Build.Logging;
using Microsoft.Build.Framework;

using Ionic.Zip;

namespace NightlyBuilder
{
	public class Program
	{
		public static void Main(string[] args)
		{
			ConfigFile config = ConfigFile.Load("BuildConfig.xml");
			string packagePath = Path.Combine(config.PackageDir, config.PackageName);

			// Do an SVN Revert of the package
			Console.WriteLine("================================== SVN Revert =================================");
			{
				string commandLine = string.Format("svn revert *");
				Console.WriteLine(commandLine);

				ProcessStartInfo info = new ProcessStartInfo("cmd.exe", "/C " + commandLine);
				info.UseShellExecute = false;
				info.RedirectStandardOutput = true;
				info.WindowStyle = ProcessWindowStyle.Hidden;
				info.WorkingDirectory = config.PackageDir;
				Process proc = Process.Start(info);
				proc.WaitForExit();
				string output = proc.StandardOutput.ReadToEnd();

				Console.WriteLine(output);
			}
			Console.WriteLine("===============================================================================");
			Console.WriteLine();
			Console.WriteLine();

			// Build the target Solution
			Console.WriteLine("================================ Build Solution ===============================");
			{
				var buildProperties = new Dictionary<string,string>(){ { "Configuration", "Release"} };
				var buildRequest = new BuildRequestData(config.SolutionPath, buildProperties, null, new string[] { "Build" }, null);
				var buildParameters = new BuildParameters();
				buildParameters.Loggers = new[] { new ConsoleLogger(LoggerVerbosity.Minimal) };
				var buildResult = BuildManager.DefaultBuildManager.Build(buildParameters, buildRequest);
				if (buildResult.OverallResult != BuildResultCode.Success)
					throw new ApplicationException("Project Build Failure");
			}
			Console.WriteLine("===============================================================================");
			Console.WriteLine();
			Console.WriteLine();

			// Build the documentation
			Console.WriteLine("================================== Build Docs =================================");
			{
				var buildProperties = new Dictionary<string,string>(){ { "Configuration", "Release"} };
				var buildRequest = new BuildRequestData(config.DocSolutionPath, buildProperties, null, new string[] { "Build" }, null);
				var buildParameters = new BuildParameters();
				buildParameters.Loggers = new[] { new ConsoleLogger(LoggerVerbosity.Normal) };
				var buildResult = BuildManager.DefaultBuildManager.Build(buildParameters, buildRequest);
				if (buildResult.OverallResult != BuildResultCode.Success)
					throw new ApplicationException("Documentation Build Failure");
				File.Copy(
					Path.Combine(config.DocBuildResultDir, config.DocBuildResultFile), 
					Path.Combine(config.BuildResultDir, config.DocBuildResultFile),
					true);
			}
			Console.WriteLine("===============================================================================");
			Console.WriteLine();
			Console.WriteLine();

			// Copy the results to the target directory
			Console.WriteLine("================================ Copy to Target ===============================");
			{
				Console.WriteLine("Creating target directory '{0}'", config.TargetDir);
				if (Directory.Exists(config.TargetDir))
					Directory.Delete(config.TargetDir, true);
				CopyDirectory(config.BuildResultDir, config.TargetDir, true, path => 
					{
						string fileName = Path.GetFileName(path);
						foreach (string blackListEntry in config.FileCopyBlackList)
						{
							if (Regex.IsMatch(fileName, WildcardToRegex(blackListEntry), RegexOptions.IgnoreCase))
							{
								Console.ForegroundColor = ConsoleColor.DarkGray;
								Console.WriteLine("Ignore {0}", path);
								Console.ForegroundColor = ConsoleColor.Gray;
								return false;
							}
						}
						Console.WriteLine("Copy   {0}", path);
						return true;
					});
				CopyDirectory(config.AdditionalFileDir, config.TargetDir, true);
			}
			Console.WriteLine("===============================================================================");
			Console.WriteLine();
			Console.WriteLine();

			// Create the ZIP package
			Console.WriteLine("================================ Create Package ===============================");
			{
				Console.WriteLine("Package Path: {0}", packagePath);
				if (!Directory.Exists(config.PackageDir))
					Directory.CreateDirectory(config.PackageDir);

				ZipFile package = new ZipFile();
				string[] files = Directory.GetFiles(config.TargetDir, "*", SearchOption.AllDirectories);
				package.AddFiles(files);
				package.Save(packagePath);
			}
			Console.WriteLine("===============================================================================");
			Console.WriteLine();
			Console.WriteLine();
			
			// Cleanup
			Console.WriteLine("=================================== Cleanup ===================================");
			{
				Console.WriteLine("Deleting target directory '{0}'", config.TargetDir);
				if (Directory.Exists(config.TargetDir))
					Directory.Delete(config.TargetDir, true);
			}
			Console.WriteLine("===============================================================================");
			Console.WriteLine();
			Console.WriteLine();

			// Do an SVN Commit of the package
			Console.WriteLine("================================== SVN Commit =================================");
			{
				string commandLine = string.Format("svn commit -m \"{0}\"", "Updated Binary Package");
				Console.WriteLine(commandLine);

				ProcessStartInfo info = new ProcessStartInfo("cmd.exe", "/C " + commandLine);
				info.UseShellExecute = false;
				info.RedirectStandardOutput = true;
				info.WindowStyle = ProcessWindowStyle.Hidden;
				info.WorkingDirectory = config.PackageDir;
				Process proc = Process.Start(info);
				proc.WaitForExit();
				string output = proc.StandardOutput.ReadToEnd();

				Console.WriteLine(output);
			}
			Console.WriteLine("===============================================================================");
			Console.WriteLine();
			Console.WriteLine();
		}

		public static string WildcardToRegex(string pattern)
		{
			return "^" + Regex.Escape(pattern).
							   Replace(@"\*", ".*").
							   Replace(@"\?", ".") + "$";
		}
		public static void CopyDirectory(string sourcePath, string targetPath, bool overwrite = false, Predicate<string> filter = null)
		{
			if (!Directory.Exists(sourcePath)) throw new DirectoryNotFoundException("Source path not found");
			if (!overwrite && Directory.Exists(targetPath)) return;

			if (!Directory.Exists(targetPath)) 
				Directory.CreateDirectory(targetPath);

			foreach (string file in Directory.GetFiles(sourcePath))
			{
				if (filter != null && !filter(file)) continue;
				File.Copy(file, Path.Combine(targetPath, Path.GetFileName(file)), overwrite);
			}
			foreach (string subDir in Directory.GetDirectories(sourcePath))
			{
				if (filter != null && !filter(subDir)) continue;
				CopyDirectory(subDir, Path.Combine(targetPath, Path.GetFileName(subDir)), overwrite, filter);
			}
		}
	}
}