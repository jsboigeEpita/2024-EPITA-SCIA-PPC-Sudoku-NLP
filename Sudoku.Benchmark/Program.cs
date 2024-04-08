using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.Configuration;
//using Humanizer;
using Sudoku.Shared;

namespace Sudoku.Benchmark
{
	class Program
	{

#if DEBUG
		private static bool IsDebug = true;
#else
        private static bool IsDebug = false;
#endif

		static IConfiguration Configuration;

		static void Main(string[] args)
		{

			Console.WriteLine("Benchmarking GrilleSudoku Solvers");


			// Configuration Builder
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

			Configuration = builder.Build();

			PythonConfiguration pythonConfig = null;

			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				pythonConfig = Configuration.GetSection("PythonConfig:OSX").Get<PythonConfiguration>();

			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
			{
				pythonConfig = Configuration.GetSection("PythonConfig:Linux").Get<PythonConfiguration>();
			}

			if (pythonConfig != null)
			{
				Console.WriteLine("Customizing MacOs/Linux Python Install from appsettings.json file");
				if (!string.IsNullOrEmpty(pythonConfig.InstallPath))
				{
					MacInstaller.InstallPath = pythonConfig.InstallPath;
				}
				if (!string.IsNullOrEmpty(pythonConfig.PythonDirectoryName))
				{
					MacInstaller.PythonDirectoryName = pythonConfig.PythonDirectoryName;
				}
				if (!string.IsNullOrEmpty(pythonConfig.LibFileName))
				{
					MacInstaller.LibFileName = pythonConfig.LibFileName;
				}
			}

			while (true)
			{
				if (IsDebug)
				{
					if (RunMenu())
					{
						break;
					}

				}
				else
				{
					try
					{
						if (RunMenu())
						{
							break;
						}
					}
					catch (Exception e)
					{
						Console.WriteLine(e);
					}
				}
			}
		}


		private static bool RunMenu()
		{

			Console.WriteLine("Select Mode: \n1-Single Solver Test, \n2-Benchmarks, \n3-Exit program");
			var strMode = Console.ReadLine();
			int.TryParse(strMode, out var intMode);
			//Console.SetBufferSize(130, short.MaxValue - 100);
			switch (intMode)
			{
				case 1:
					SingleSolverTest();
					break;
				case 2:
					Benchmark();
					break;
				default:
					return true;
			}

			return false;

		}


		private static void Benchmark()
		{
			Console.WriteLine("Select Benchmark Type: \n1-Quick Benchmark (Easy, 2 Sudokus, 10s max per sudoku, Single invocation), \n2-Quick Benchmark (Medium, 10 Sudokus, 20s max per sudoku, Single invocation), \n3-Quick Benchmark (Hard, 10 Sudokus, 30s max per sudoku, Single invocation), \n4-Complete Benchmark (All difficulties, 1 mn max per sudoku, several invocations), \n5-Return");
			var strMode = Console.ReadLine();
			int.TryParse(strMode, out var intMode);
			//Console.SetBufferSize(130, short.MaxValue - 100);
			switch (intMode)
			{
				case 1:
					var tempEasy = new QuickBenchmarkSolversEasy();

					//               if (IsDebug)
					//               {
					//	BenchmarkRunner.Run<QuickBenchmarkSolversEasy>(new DebugInProcessConfig());
					//}
					//               else
					//               {
					//	BenchmarkRunner.Run<QuickBenchmarkSolversEasy>();
					//}
					BenchmarkRunner.Run<QuickBenchmarkSolversEasy>();
					break;
				case 2:
					//Init solvers
					var tempMedium = new QuickBenchmarkSolversMedium();
					//BenchmarkRunner.Run<QuickBenchmarkSolvers>(new DebugInProcessConfig());
					BenchmarkRunner.Run<QuickBenchmarkSolversMedium>();
					break;
				case 3:
					//Init solvers
					var tempHard = new QuickBenchmarkSolversHard();
					//BenchmarkRunner.Run<QuickBenchmarkSolvers>(new DebugInProcessConfig());
					BenchmarkRunner.Run<QuickBenchmarkSolversHard>();
					break;
				case 4:
					//Init solvers
					var temp2 = new CompleteBenchmarkSolvers();
					BenchmarkRunner.Run<CompleteBenchmarkSolvers>();
					break;
				default:
					break;
			}

		}



		private static void SingleSolverTest()
		{
			var solvers = Shared.SudokuGrid.GetSolvers();
			Console.WriteLine("Select difficulty: 1-Easy, 2-Medium, 3-Hard");
			var strDiff = Console.ReadLine();
			int.TryParse(strDiff, out var intDiff);
			SudokuDifficulty difficulty = SudokuDifficulty.Hard;
			switch (intDiff)
			{
				case 1:
					difficulty = SudokuDifficulty.Easy;
					break;
				case 2:
					difficulty = SudokuDifficulty.Medium;
					break;
				case 3:
					difficulty = SudokuDifficulty.Hard;
					break;
				default:
					break;
			}
			//SudokuDifficulty difficulty = intDiff switch
			//{
			//    1 => SudokuDifficulty.Easy,
			//    2 => SudokuDifficulty.Medium,
			//    _ => SudokuDifficulty.Hard
			//};

			var sudokus = SudokuHelper.GetSudokus(difficulty);

			Console.WriteLine($"Choose a puzzle index between 1 and {sudokus.Count}");
			var strIdx = Console.ReadLine();
			int.TryParse(strIdx, out var intIdx);
			var targetSudoku = sudokus[intIdx - 1];

			Console.WriteLine("Chosen Puzzle:");
			Console.WriteLine(targetSudoku.ToString());

			Console.WriteLine("Choose a solver:");
			var solverList = solvers.ToList();
			for (int i = 0; i < solvers.Count(); i++)
			{
				Console.WriteLine($"{(i + 1).ToString(CultureInfo.InvariantCulture)} - {solverList[i].Key}");
			}
			var strSolver = Console.ReadLine();
			int.TryParse(strSolver, out var intSolver);
			var solver = solverList[intSolver - 1].Value.Value;

			var cloneSudoku = targetSudoku.CloneSudoku();
			var sw = Stopwatch.StartNew();

			cloneSudoku = solver.Solve(cloneSudoku);

			var elapsed = sw.Elapsed;
			if (!cloneSudoku.IsValid(targetSudoku))
			{
				Console.WriteLine($"Invalid Solution : Solution has {cloneSudoku.NbErrors(targetSudoku)} errors");
				Console.WriteLine("Invalid solution:");
			}
			else
			{
				Console.WriteLine("Valid solution:");
			}

			Console.WriteLine(cloneSudoku.ToString());
			Console.WriteLine($"Time to solution: {elapsed.TotalMilliseconds} ms");

		}


	}
}