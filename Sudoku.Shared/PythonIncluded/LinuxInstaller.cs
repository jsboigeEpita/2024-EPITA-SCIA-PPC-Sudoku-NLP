using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Sudoku.Shared
{
    public static class LinuxInstaller
    {
        public static string LibFileName { get; set; } = "libpython3.10.so";
        public static string InstallPath { get; set; } = "/usr/lib";

        public static string PythonDirectoryName { get; set; } = "python3.10";

        public static LinuxInstaller.InstallationSource Source { get; set; } = new LinuxInstaller.DownloadInstallationSource()
        {
            DownloadUrl = "https://www.python.org/ftp/python/3.10.0/Python-3.10.0.tgz"
        };

        public static string InstallPythonHome => Path.Combine(LinuxInstaller.InstallPath, LinuxInstaller.PythonDirectoryName);

        public static event Action<string> LogMessage;

        private static void Log(string message)
        {
            Action<string> logMessage = LinuxInstaller.LogMessage;
            if (logMessage == null)
                return;
            logMessage(message);
        }

        public static async Task SetupPython(bool force = false)
        {
            Environment.SetEnvironmentVariable("PATH", LinuxInstaller.InstallPythonHome + ":" + Environment.GetEnvironmentVariable("PATH"));
            if (force || !Directory.Exists(LinuxInstaller.InstallPythonHome) || !File.Exists(Path.Combine(LinuxInstaller.InstallPythonHome, "bin", "python3")))
            {
                string tarball = await LinuxInstaller.Source.RetrievePythonTarball(LinuxInstaller.InstallPath);
                if (string.IsNullOrWhiteSpace(tarball))
                    LinuxInstaller.Log("SetupPython: Error obtaining tarball from installation source");
                else
                    await Task.Run(() =>
                    {
                        try
                        {
                            LinuxInstaller.ExtractTarball(tarball, LinuxInstaller.InstallPath);
                        }
                        catch (Exception ex)
                        {
                            LinuxInstaller.Log("SetupPython: Error extracting tarball: " + tarball);
                            LinuxInstaller.Log(ex.ToString());
                        }
                    });
            }
        }

        private static void ExtractTarball(string tarballPath, string destinationPath)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "tar";
                process.StartInfo.Arguments = $"xzf {tarballPath} -C {destinationPath}";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.OutputDataReceived += (sender, args) => LinuxInstaller.Log(args.Data);
                process.ErrorDataReceived += (sender, args) => LinuxInstaller.Log(args.Data);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
        }

        public static async Task PipInstallModule(string moduleName, string version = "", bool force = false)
        {
            await LinuxInstaller.TryInstallPip();
            if (LinuxInstaller.IsModuleInstalled(moduleName) && !force)
                return;

            string pipCommand = Path.Combine(LinuxInstaller.InstallPythonHome, "bin", "pip3");
            string forceFlag = force ? " --force-reinstall" : "";
            if (!string.IsNullOrEmpty(version))
                version = "==" + version;

            LinuxInstaller.RunCommand($"{pipCommand} install {moduleName}{version} {forceFlag}");
        }

        public static async Task InstallPip()
        {
            string pythonPath = Path.Combine(LinuxInstaller.InstallPythonHome, "bin", "python3");
            LinuxInstaller.RunCommand($"curl https://bootstrap.pypa.io/get-pip.py | {pythonPath}");
        }

        public static async Task<bool> TryInstallPip(bool force = false)
        {
            if (!LinuxInstaller.IsPipInstalled() || force)
            {
                try
                {
                    await LinuxInstaller.InstallPip();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception trying to install pip: {ex}");
                    return false;
                }
            }
            return false;
        }

        public static bool IsPythonInstalled() => File.Exists(Path.Combine(LinuxInstaller.InstallPythonHome, "bin", "python3"));

        public static bool IsPipInstalled() => File.Exists(Path.Combine(LinuxInstaller.InstallPythonHome, "bin", "pip3"));

        public static bool IsModuleInstalled(string module)
        {
            if (!LinuxInstaller.IsPythonInstalled())
                return false;

            string modulePath = Path.Combine(LinuxInstaller.InstallPythonHome, "lib", $"python{LinuxInstaller.PythonDirectoryName}", "site-packages", module);
            return Directory.Exists(modulePath) && File.Exists(Path.Combine(modulePath, "__init__.py"));
        }

        public static void RunCommand(string command) => LinuxInstaller.RunCommand(command, CancellationToken.None).Wait();

        public static async Task RunCommand(string command, CancellationToken token)
        {
            using (Process process = new Process())
            {
                try
                {
                    process.StartInfo.FileName = "/bin/bash";
                    process.StartInfo.Arguments = $"-c \"{command}\"";
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    LinuxInstaller.Log($"> {process.StartInfo.FileName} {process.StartInfo.Arguments}");

                    process.OutputDataReceived += (sender, args) => LinuxInstaller.Log(args.Data);
                    process.ErrorDataReceived += (sender, args) => LinuxInstaller.Log(args.Data);

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    token.Register(() =>
                    {
                        try
                        {
                            if (!process.HasExited)
                                process.Kill();
                        }
                        catch (Exception ex)
                        {
                            LinuxInstaller.Log(ex.ToString());
                        }
                    });

                    await Task.Run(() => process.WaitForExit(), token);

                    if (process.ExitCode != 0)
                        LinuxInstaller.Log($" => exit code {process.ExitCode}");
                }
                catch (Exception ex)
                {
                    LinuxInstaller.Log($"RunCommand: Error with command: '{command}'\r\n{ex.Message}");
                }
            }
        }

        public class DownloadInstallationSource : LinuxInstaller.InstallationSource
        {
            public string DownloadUrl { get; set; }

            public override async Task<string> RetrievePythonTarball(string destinationDirectory)
            {
                string tarballFile = Path.Combine(destinationDirectory, this.GetPythonTarballFileName());
                if (!this.Force && File.Exists(tarballFile))
                    return tarballFile;

                await LinuxInstaller.RunCommand($"curl {this.DownloadUrl} -o {tarballFile}", CancellationToken.None);
                return tarballFile;
            }

            public override string GetPythonTarballFileName() => Path.GetFileName(new Uri(this.DownloadUrl).LocalPath);
        }

        public abstract class InstallationSource
        {
            public abstract Task<string> RetrievePythonTarball(string destinationDirectory);

            public bool Force { get; set; }

            public virtual string GetPythonDistributionName()
            {
                string tarballFileName = this.GetPythonTarballFileName();
                return tarballFileName == null ? null : Path.GetFileNameWithoutExtension(tarballFileName);
            }

            public abstract string GetPythonTarballFileName();

            public virtual string GetPythonVersion()
            {
                Match match = Regex.Match(this.GetPythonDistributionName(), @"Python-(?<major>\d+)\.(?<minor>\d+)");
                if (match.Success)
                    return $"python{match.Groups["major"]}.{match.Groups["minor"]}";

                LinuxInstaller.Log("Unable to get python version from distribution name.");
                return null;
            }
        }
    }
}