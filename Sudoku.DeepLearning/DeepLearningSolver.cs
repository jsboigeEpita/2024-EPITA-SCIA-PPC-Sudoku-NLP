using Python.Runtime;
using Sudoku.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.DeepLearning
{
	public class DeepLearningSolver : PythonSolverBase

	{
		private static string _modelAbsolutePath;

		private const string ModelRelativePath = "cnn_2024_04_08.keras";

		private const string ModelUrl =
			@"https://drive.usercontent.google.com/download?id=19MVgdm-HiR4RonH-JTMNUXda68-0v6vX&export=download&authuser=0&confirm=t&uuid=5133564c-9c5d-4034-8c6f-1270073735c3&at=APZUnTV2ZGVvBN5IFKa3RktC4eQ7%3A1712614910524";

		static DeepLearningSolver()
		{
			_modelAbsolutePath = Path.Combine(Environment.CurrentDirectory, ModelRelativePath);
			EnsureModelDownloaded(ModelUrl, _modelAbsolutePath);
		}


		public override Shared.SudokuGrid Solve(Shared.SudokuGrid s)
		{

			//System.Diagnostics.Debugger.Break();

			//For some reason, the Benchmark runner won't manage to get the mutex whereas individual execution doesn't cause issues
			//using (Py.GIL())
			//{
			// print the Python version
			Console.WriteLine($"Python Version: {PythonEngine.Version}");
			// create a Python scope
			using (PyModule scope = Py.CreateScope())
			{
				// convert the Cells array object to a PyObject
				PyObject pyCells = s.Cells.ToPython();

				// create a Python variable "instance"
				scope.Set("instance", pyCells);

				scope.Set("path", _modelAbsolutePath);

				//Console.WriteLine("Solving Sudoku with DeepLearning 1");

				// run the Python script
				string code = Resources.DeepLearning_py;
				scope.Exec(code);

				// Print the result
				//Console.WriteLine(scope.Get("r"));
				//Console.WriteLine("Solving Sudoku with DeepLearning 2");


				//Retrieve solved Sudoku variable
				var result = scope.Get("r");

				// Clear the scope
				scope.Dispose();

				//Convert back to C# object
				var managedResult = result.As<int[][]>();
				//Console.WriteLine("Solving Sudoku with DeepLearning 3");

				//var convertesdResult = managedResult.Select(objList => objList.Select(o => (int)o).ToArray()).ToArray();
				return new Shared.SudokuGrid() { Cells = managedResult.To2D() };
			}
			//}

		}

		protected override void InitializePythonComponents()
		{
			//declare your pip packages here
			InstallPipModule("numpy");
			InstallPipModule("tensorflow");
			base.InitializePythonComponents();
		}


		public static async Task EnsureModelDownloaded(string modelUrl, string modelAbsolutePath)
		{
			
			if (!File.Exists(modelAbsolutePath))
			{
				Console.WriteLine("Le modèle Keras n'est pas trouvé. Téléchargement en cours...");

				try
				{
					HttpClient client = new();
					HttpResponseMessage response = await client.GetAsync(modelUrl, HttpCompletionOption.ResponseHeadersRead);
					response.EnsureSuccessStatusCode();

					using (var fs = new FileStream(modelAbsolutePath, FileMode.CreateNew))
					{
						await response.Content.CopyToAsync(fs);
					}

					Console.WriteLine("Le modèle Keras a été téléchargé avec succès.");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Une erreur s'est produite lors du téléchargement du modèle : {ex.Message}");
				}
			}
		}

	}
}
