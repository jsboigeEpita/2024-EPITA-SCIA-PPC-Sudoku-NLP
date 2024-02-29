using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Python.Deployment;
using Python.Runtime;

namespace Sudoku.Shared
{
    public abstract class PythonSolverBase : ISudokuSolver
    {

        static PythonSolverBase()
        {


        }

        public PythonSolverBase()
        {
            if (!pythonInstalled)
            {
                InstallPythonComponents();
                pythonInstalled = true;
            }
            InitializePythonComponents();
        }

        private static bool pythonInstalled = false;

        public static void InstallPythonComponents()
        {
			Task task = Task.Run(() => InstallPythonComponentsAsync());
			task.Wait();

		}

		protected static async Task InstallPythonComponentsAsync()
		{

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {

				await InstallMac();
            }
            else
            {
				await InstallEmbedded();
            }
        }
		public static void InstallPipModule(string moduleName, string version = "", bool force = false)
		{
			Task task = Task.Run( () => InstallPipModuleAsync(moduleName, version, force));
			task.Wait();

		}


		private static async Task InstallPipModuleAsync(string moduleName, string version = "", bool force = false)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
               await MacInstaller.PipInstallModule(moduleName, version, force);
            }
            else
            {
               await Installer.PipInstallModule(moduleName, version, force);
            }
        }

        private static async Task InstallMac()
        {

            Console.WriteLine($"PythonDll={MacInstaller.LibFileName}");
            Runtime.PythonDLL = MacInstaller.LibFileName;

            MacInstaller.LogMessage += Console.WriteLine;
            // Installer.SetupPython().Wait();

            //MacInstaller.InstallPath = "/Library/Frameworks/Python.framework/Versions";
            //MacInstaller.PythonDirectoryName = "3.7/";

            var localInstallPath = MacInstaller.InstallPythonHome;
            var existingPathEnvVar = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine);

            var path = $"{localInstallPath}/lib;{localInstallPath};{existingPathEnvVar}";
            var pythonPath = $"{localInstallPath}/lib/site-packages;{localInstallPath}/lib";

            Environment.SetEnvironmentVariable("Path", path, EnvironmentVariableTarget.Process);
            Console.WriteLine($"Path={Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Process)}");

            // Environment.SetEnvironmentVariable("PYTHONHOME", localInstallPath, EnvironmentVariableTarget.Process);
            // Console.WriteLine($"PYTHONHOME={Environment.GetEnvironmentVariable("PYTHONHOME", EnvironmentVariableTarget.Process)}");
            // Environment.SetEnvironmentVariable("PythonPath", pythonPath, EnvironmentVariableTarget.Process);
            // Console.WriteLine($"PythonPath={Environment.GetEnvironmentVariable("PythonPath", EnvironmentVariableTarget.Process)}");


            var aliasPath = $"{MacInstaller.LibFileName}";
            if (!File.Exists(aliasPath))
            {
                var libPath = $"{localInstallPath}/lib/{MacInstaller.LibFileName}";
                var aliasCommand =
                    $"sudo ln -s {libPath} {aliasPath}";
                Console.WriteLine($"run command={aliasCommand}");
                MacInstaller.RunCommand(aliasCommand);
            }



            var dynamicLinkingCommnad = $@"export DYLD_LIBRARY_PATH={localInstallPath}/lib";
            Console.WriteLine($"run command={dynamicLinkingCommnad}");
            MacInstaller.RunCommand(dynamicLinkingCommnad);

            // PythonEngine.PythonHome = localInstallPath;
            // PythonEngine.PythonPath = pythonPath;

            await MacInstaller.TryInstallPip();
        }




        private static async Task InstallEmbedded()
        {

            // // install in local directory. if you don't set it will install in local app data of your user account
            //Python.Deployment.Installer.InstallPath = Path.GetFullPath(".");
            //


            Runtime.PythonDLL = "python37.dll";
            Python.Deployment.Installer.Source = new Installer.DownloadInstallationSource()
            {
                DownloadUrl = @"https://www.python.org/ftp/python/3.7.3/python-3.7.3-embed-amd64.zip",
            };

            //Runtime.PythonDLL = "python38.dll";
            //Python.Deployment.Installer.Source = new Installer.DownloadInstallationSource()
            //{
            //    DownloadUrl = @"https://www.python.org/ftp/python/3.8.9/python-3.8.9-embed-amd64.zip",
            //};

            //Runtime.PythonDLL = "python39.dll";
            //Python.Deployment.Installer.Source = new Installer.DownloadInstallationSource()
            //{
            //    DownloadUrl = @"https://www.python.org/ftp/python/3.9.9/python-3.9.9-embed-amd64.zip",
            //};
            //Runtime.PythonDLL = "python37.dll";

            // // set the download source
            // Python.Deployment.Installer.Source = new Installer.DownloadInstallationSource()
            // {
            //     DownloadUrl = @"https://www.python.org/ftp/python/3.7.3/python-3.7.3-embed-amd64.zip",
            // };
            //
            // // install in local directory. if you don't set it will install in local app data of your user account
            //Python.Deployment.Installer.InstallPath = Path.GetFullPath(".");
            //
            // see what the installer is doing

            //Runtime.PythonDLL = "python310.dll";
            //Python.Deployment.Installer.Source = new Installer.DownloadInstallationSource()
            //{
            //	DownloadUrl = @"https://www.python.org/ftp/python/3.10.8/python-3.10.8-embed-amd64.zip",
            //};


            //Runtime.PythonDLL = "python311.dll";
            //Python.Deployment.Installer.Source = new Installer.DownloadInstallationSource()
            //{
            //    DownloadUrl = @"https://www.python.org/ftp/python/3.11.2/python-3.11.2-embed-amd64.zip",
            //};


            // see what the installer is doing
            Installer.LogMessage += Console.WriteLine;
            //
            // install from the given source
			await Python.Deployment.Installer.SetupPython();

			await Installer.TryInstallPip();

			Python.Deployment.Installer.SetupPython().Wait();
			Installer.TryInstallPip();

        }


        protected virtual void InitializePythonComponents()
        {


            PythonEngine.Initialize();
            //dynamic sys = PythonEngine.ImportModule("sys");
            //Console.WriteLine("Python version: " + sys.version);
        }


        public abstract Shared.SudokuGrid Solve(Shared.SudokuGrid s);


        protected void AddNumpyConverterScript(PyModule scope)
        {
			string numpyConverterCode = Resources.numpy_converter_py;
			scope.Exec(numpyConverterCode);
		}
			



    }

}