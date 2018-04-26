﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SpecFlow.TestProjectGenerator
{
    public class CurrentVersionDriver
    {
        private readonly Folders _folders;

        public CurrentVersionDriver(Folders folders)
        {
            _folders = folders;
            string pathToGitVersionDir = Path.Combine(_folders.GlobalPackages, "gitversion.commandline", "3.6.5", "tools");
            string pathToGitVersionExe = Path.Combine(pathToGitVersionDir, "GitVersion.exe");
            var processResult = new ProcessHelper().RunProcess(_folders.SourceRoot, pathToGitVersionExe, "");

            if (processResult.ExitCode != 0)
            {
                throw new InvalidOperationException("Failed to fetch GitVersion");
            }

            GitVersionInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<GitVersionInfo>(processResult.CombinedOutput);

            var specFlowAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == "TechTalk.SpecFlow").SingleOrDefault();
            if (specFlowAssembly != null)
            {
                var specFlowVersion = specFlowAssembly.GetName().Version;

                SpecFlowMajor = specFlowVersion.Major;
                SpecFlowMinor = specFlowVersion.Minor;

                SpecFlowVersion = $"{specFlowVersion.Major}.{specFlowVersion.Minor}.0";
                SpecFlowVersionDash = $"{specFlowVersion.Major}-{specFlowVersion.Minor}-0";
            }
        }

        public GitVersionInfo GitVersionInfo { get; }

        public string SpecFlowVersionDash { get; private set; }

        public string SpecFlowVersion { get; private set; }
        public int SpecFlowMajor { get; set; }
        public int SpecFlowMinor { get; set; }
    }
}
