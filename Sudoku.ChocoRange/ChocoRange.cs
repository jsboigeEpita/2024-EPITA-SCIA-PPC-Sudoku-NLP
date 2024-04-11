using Python.Runtime;
using Sudoku.Shared;
using System;
using System.IO;

namespace Sudoku.ChocoRange
{
    public class ChocoRange : PythonSolverBase
    {
        public override SudokuGrid Solve(SudokuGrid s)
        {
            // Exécuter le script Python pour installer glpk-utils si nécessaire
            InstallGlpkPackage();

            using (Py.GIL())
            {
                using (PyModule scope = Py.CreateScope())
                {
                    // Injecter le script de conversion
                    AddNumpyConverterScript(scope);

                    // Convertir le tableau .NET en tableau NumPy
                    var pyCells = AsNumpyArray(s.Cells, scope);
                    Console.WriteLine("LE TABLEAU AVANT CHOCO RESOLUTION");
                    Console.WriteLine(pyCells);

                    // Créer une variable Python "ourSudoku"
                    scope.Set("ourSudoku", pyCells);

                    // Lire le contenu du fichier ChocoRange.py
                    string code = Resources.ChocoRange_py;

                    // Console.WriteLine(code);

                    // Exécuter le script Python
                    scope.Exec(code);
                    PyObject result = scope.Get("result");

                    // Convertir le résultat NumPy en tableau .NET
                    var managedResult = AsManagedArray(scope, result);

                    Console.WriteLine("APRES LA CHOCO RESOLUTION !");
                    
                    return new SudokuGrid { Cells = managedResult };
                }
            }
        }

        // Méthode pour installer le paquet glpk-utils si nécessaire
        private void InstallGlpkPackage()
        {
            string command;
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                // Vérifier si le paquet glpk-utils est déjà installé
                var result = ExecuteShellCommand("dpkg -s glpk-utils");
                if (result.ExitCode == 0)
                {
                    Console.WriteLine("Le paquet glpk-utils est déjà installé.");
                    return;
                }
                // Le paquet n'est pas installé, exécuter la commande sudo apt-get install glpk-utils
                command = "sudo apt-get install glpk-utils";
            }
            else if (Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                // Exécuter la commande brew install glpk
                command = "brew install glpk";
            }
            else
            {
                Console.WriteLine("Système d'exploitation non pris en charge.");
                return;
            }

            Console.WriteLine("Exécution de la commande pour installer le paquet glpk...");
            var installResult = ExecuteShellCommand(command);
            if (installResult.ExitCode == 0)
            {
                Console.WriteLine("Le paquet glpk a été installé avec succès.");
            }
            else
            {
                Console.WriteLine("Erreur lors de l'installation du paquet glpk.");
            }
        }

        // Méthode utilitaire pour exécuter une commande shell
        private (int ExitCode, string Output) ExecuteShellCommand(string command)
        {
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = System.Diagnostics.Process.Start(processInfo))
            {
                process.WaitForExit();
                string output = process.StandardOutput.ReadToEnd();
                return (process.ExitCode, output);
            }
        }
    }
}
