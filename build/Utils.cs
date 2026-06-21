using Microsoft.Build.Tasks;
using Nuke.Common.IO;
using Octokit;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace _build
{
    public static class Utils
    {
        public static void EnsureCleanDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentException("Directory path cannot be null or empty.", nameof(directoryPath));

            var directoryInfo = new DirectoryInfo(directoryPath);

            if (!directoryInfo.Exists)
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

            // Delete all files
            foreach (FileInfo file in directoryInfo.EnumerateFiles())
            {
                file.Delete();
            }

            // Delete all subdirectories
            foreach (DirectoryInfo dir in directoryInfo.EnumerateDirectories())
            {
                dir.Delete(true);
            }
        }
    }
}