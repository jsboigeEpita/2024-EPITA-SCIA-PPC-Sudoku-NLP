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
	
	public bool TryBacktrack()
	{
		Search(Puzzle, 0, 0);

		bool Search(Puzzle s, int row, int col)
		{
			//pass to the next row if all the cells in the column are checked     
			if (col == 9)
			{
				col = 0; ++row;
				if (row == 9) return true;
			}
			//check if the cell is filled
			Cell cell = Puzzle[row, col];
			if (cell.Value != 0)
				return Search(s, row, col + 1);
			//implement the good value
			for (int v = 1; v <= 9; v++)
			{
				cell = Puzzle[row, col];
				if (IsValid(s, row, col, v))
				{
					cell.SetValue(v);
					if (Search(s, row, col + 1)) return true;
					else cell.SetValue(0);
				}
			}
			return false;
		}
		
		bool IsValid(Puzzle s, int row, int col, int val)
		{
			Cell cell = Puzzle[row, col];
			//check if value is present in column
			for (int r = 0; r < 9; r++)
			{
				cell = Puzzle[r, col];
				if (cell.Value == val) return false;
			}

			//check if value is present in the row
			for (int c = 0; c < 9; c++)
			{
				cell = Puzzle[row, c];
				if (cell.Value == val) return false;
			}

			//check for the value in the 3 X 3 block
			int i = row / 3;
			int j = col / 3;
			for (int a = 0; a < 3; a++)
			for (int b = 0; b < 3; b++)
			{
				cell = Puzzle[3 * i + a, 3 * j + b];
				if (val == cell.Value) return false;
			}

			return true;
		}
		
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