using System;

namespace Sudoku.Human;

partial class Solver
{
	private bool HiddenSingle()
	{
		bool changed = false;
		bool restartSearch;

		do
		{
			restartSearch = false;

			foreach (Region[] regions in Puzzle.RegionsI)
			{
				foreach (Region region in regions)
				{
					for (int digit = 1; digit <= 9; digit++)
					{
						Span<Cell> c = region.GetCellsWithCandidate(digit, _cellCache);
						if (c.Length == 1)
						{
							c[0].SetValue(digit);
							changed = true;
							restartSearch = true;
						}
					}
				}
			}

		} while (restartSearch);

		return changed;
	}
}