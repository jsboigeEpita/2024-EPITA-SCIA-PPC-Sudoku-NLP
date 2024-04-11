﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Toolchains.CoreRun;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using BenchmarkDotNet.Toolchains.Roslyn;
using Sudoku.Shared;



namespace Sudoku.Benchmark
{

	public class QuickBenchmarkSolversHard : QuickBenchmarkSolversEasy
	{
		public QuickBenchmarkSolversHard()
		{
			NbPuzzles = 10;
			MaxSolverDuration = TimeSpan.FromSeconds(30);
			Difficulty = SudokuDifficulty.Hard;
		}
	}


	public class QuickBenchmarkSolversMedium : QuickBenchmarkSolversEasy
	{
		public QuickBenchmarkSolversMedium()
		{
			NbPuzzles = 10;
			MaxSolverDuration = TimeSpan.FromSeconds(20);
			Difficulty = SudokuDifficulty.Medium;
		}
	}


	[Config(typeof(Config))]
	public class QuickBenchmarkSolversEasy : BenchmarkSolversBase
	{
		public QuickBenchmarkSolversEasy()
		{
			MaxSolverDuration = TimeSpan.FromSeconds(10);
			NbPuzzles = 2;
		}
		private class Config : SudokuBenchmarkConfigBase
		{
			public Config(): base()
			{
				var baseJob = GetBaseJob()
					.WithIterationCount(1)
					.WithInvocationCount(1);
				this.AddJob(baseJob);
			}
		}

		public override SudokuDifficulty Difficulty { get; set; } = SudokuDifficulty.Easy;

	}
	



	[Config(typeof(Config))]
	public class CompleteBenchmarkSolvers : BenchmarkSolversBase
	{

		public CompleteBenchmarkSolvers()
		{
			MaxSolverDuration = TimeSpan.FromMinutes(1);
		}

		private class Config : SudokuBenchmarkConfigBase
		{
			public Config() : base()
			{
				var baseJob = GetBaseJob();
				this.AddJob(baseJob);
			}
		}

		[ParamsAllValues]
		public override SudokuDifficulty Difficulty { get; set; }


	}


	public abstract class SudokuBenchmarkConfigBase : ManualConfig
	{

		public SudokuBenchmarkConfigBase()
		{
			if (Program.IsDebug)
			{
				Options |= ConfigOptions.DisableOptimizationsValidator;
			}
			this.AddColumnProvider(DefaultColumnProviders.Instance);
			this.AddColumn(new RankColumn(NumeralSystem.Arabic));
			
			this.AddLogger(ConsoleLogger.Default);
			//this.AddExporter(new CsvExporter(CsvSeparator.Comma, SummaryStyle.Default));
			this.UnionRule = ConfigUnionRule.AlwaysUseLocal;
			//AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
		}

		public static Job GetBaseJob()
		{
			var baseJob = Job.Default
				.WithId("Solving Sudokus")
				.WithPlatform(Platform.X64)
				.WithJit(Jit.Default)
				.WithRuntime(CoreRuntime.Core31)
				.WithLaunchCount(1)
				.WithWarmupCount(1)
				.WithIterationCount(3)
				.WithInvocationCount(3)
				.WithUnrollFactor(1)
				.WithToolchain(InProcessEmitToolchain.Instance);
			//if (Program.IsDebug)
			//{
			//	baseJob = baseJob.WithCustomBuildConfiguration("Debug");
			//}

			return baseJob;
		}


		//static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
		//{
		//	Console.WriteLine(args.Name);
		//	if (!args.Name.StartsWith("Python.Runtime"))
		//		return null;
		//	//string pythonRuntimeDll = Environment.GetEnvironmentVariable(EnvironmentVariableName);
		//	//if (string.IsNullOrEmpty(pythonRuntimeDll))
		//	//	pythonRuntimeDll = Path.Combine(BenchmarkTests.DeploymentRoot, "baseline", "Python.Runtime.dll");
		//	string pythonRuntimeDll = Path.Combine(Environment.CurrentDirectory, "Python.Runtime.dll");
		//	return Assembly.LoadFrom(pythonRuntimeDll);
		//}

	}



	[Orderer(SummaryOrderPolicy.FastestToSlowest)]
	[MinColumn, MaxColumn, MeanColumn, MedianColumn]
	public abstract class BenchmarkSolversBase
	{

		static BenchmarkSolversBase()
		{

			_Solvers = new[] { new EmptySolver() }.Concat(Shared.SudokuGrid.GetSolvers().Select(s =>
			{
				try
				{
					return s.Value.Value;
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					return new EmptySolver();
				}

			}).Where(s => s.GetType() != typeof(EmptySolver))).Select(s => new SolverPresenter() { Solver = s }).ToList();
			//_Solvers = SudokuGrid.GetSolvers().Where(s => s.Value.Value.GetType().Name.ToLowerInvariant().StartsWith("backtrackingpython")).Select(s => new SolverPresenter() { Solver = s.Value.Value }).ToList();
			
		}


		[GlobalSetup]
		public virtual void GlobalSetup()
		{
			AllPuzzles = new Dictionary<SudokuDifficulty, IList<Shared.SudokuGrid>>();
			foreach (var difficulty in Enum.GetValues(typeof(SudokuDifficulty)).Cast<SudokuDifficulty>())
			{
				AllPuzzles[difficulty] = SudokuHelper.GetSudokus(Difficulty);
			}

		}

		private static SudokuGrid _WarmupSudoku = SudokuGrid.ReadSudoku("483921657967345001001806400008102900700000008006708200002609500800203009005010300");

		[IterationSetup]
		public void IterationSetup()
		{
			IterationPuzzles = new List<Shared.SudokuGrid>(NbPuzzles);
			for (int i = 0; i < NbPuzzles; i++)
			{
				IterationPuzzles.Add(AllPuzzles[Difficulty][i].CloneSudoku());
			}

			try
			{
				SolverPresenter.SolveWithTimeLimit(_WarmupSudoku, TimeSpan.FromSeconds(100));
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

		}

		private static readonly Stopwatch Clock = Stopwatch.StartNew();

		public TimeSpan MaxSolverDuration;

		public int NbPuzzles { get; set; } = 10;

		public virtual SudokuDifficulty Difficulty { get; set; }

		public IDictionary<SudokuDifficulty, IList<Shared.SudokuGrid>> AllPuzzles { get; set; }
		public IList<Shared.SudokuGrid> IterationPuzzles { get; set; }

		[ParamsSource(nameof(GetSolvers))]
		public SolverPresenter SolverPresenter { get; set; }


		private static IList<SolverPresenter> _Solvers;



		public IEnumerable<SolverPresenter> GetSolvers()
		{
			return _Solvers;

		}


		[Benchmark(Description = "Benchmarking GrilleSudoku Solvers")]
		public void Benchmark()
		{
			foreach (var puzzle in IterationPuzzles)
			{
				try
				{
					Console.WriteLine($"//Solver {SolverPresenter} solving sudoku: \n {puzzle}");
					var startTime = Clock.Elapsed;
					var solution = SolverPresenter.SolveWithTimeLimit(puzzle, MaxSolverDuration);
					if (!solution.IsValid(puzzle))
					{
						throw new ApplicationException($"sudoku has {solution.NbErrors(puzzle)} errors");
					}

					var duration = Clock.Elapsed - startTime;
					var durationSeconds = (int)duration.TotalSeconds;
					var durationMilliSeconds = duration.TotalMilliseconds - (1000 * durationSeconds);
					Console.WriteLine($"//Valid Solution found: \n {solution} \n Solver {SolverPresenter} found the solution  in {durationSeconds} s {durationMilliSeconds} ms");
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					throw;
				}
			}
		}

	}
}