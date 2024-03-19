﻿using Sudoku.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace Sudoku.Human;

public sealed partial class Solver
{
	public Puzzle Puzzle { get; }

	public Solver(SudokuGrid s) {
		Puzzle = Puzzle.CreateFromGrid(s);
		_techniques = InitSolverTechniques();
	}

	public bool TrySolve()
	{
		Puzzle.RefreshCandidates();

		do
		{
			if (CheckForNakedSinglesOrCompletion(out bool changed))
			{
				return true;
			}
			if (changed)
			{
				continue;
			}
			if (!RunTechnique())
			{
				return false;
			}
		} while (true);
	}
	public bool TrySolveAsync(CancellationToken ct)
	{
		Puzzle.RefreshCandidates();

		do
		{
			if (CheckForNakedSinglesOrCompletion(out bool changed))
			{
				return true;
			}
			if (ct.IsCancellationRequested)
			{
				break;
			}
			if (changed)
			{
				continue;
			}
			if (!RunTechnique())
			{
				return false;
			}
			if (ct.IsCancellationRequested)
			{
				break;
			}
		} while (true);

		throw new OperationCanceledException(ct);
	}
	private bool CheckForNakedSinglesOrCompletion(out bool changed)
	{
		changed = false;
		bool solved = true;
	again:
		for (int col = 0; col < 9; col++)
		{
			for (int row = 0; row < 9; row++)
			{
				Cell cell = Puzzle[col, row];
				if (cell.Value != Cell.EMPTY_VALUE)
				{
					continue;
				}

				// Empty cell... check for naked single
				solved = false;
				if (cell.CandI.TryGetCount1(out int nakedSingle))
				{
					cell.SetValue(nakedSingle);

					changed = true;
					goto again; // Restart the search for naked singles since we have the potential to create new ones
				}
			}
		}
		return solved;
	}

	private bool RunTechnique()
	{
		foreach (SolverTechnique t in _techniques)
		{
			if (t.Function.Invoke())
			{
				return true;
			}
		}
		return false;
	}
}