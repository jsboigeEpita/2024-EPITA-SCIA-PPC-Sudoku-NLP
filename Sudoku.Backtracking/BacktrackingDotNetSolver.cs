using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sudoku.Shared;

namespace Sudoku.Backtracking
{
	public class BacktrackingDotNetSolver : ISudokuSolver
	{

		public SudokuGrid Solve(SudokuGrid s)
		{
			//launch the solver
			Search(s, 0, 0);
			return s;
		}
		private bool Search(SudokuGrid s, int row, int col)
		{
			//pass to the next row if all the cells in the column are checked     
			if (col == 9)
			{
				col = 0; ++row;
				if (row == 9) return true;
			}
			//check if the cell is filled
			if (s.Cells[row][col] != 0)
				return Search(s, row, col + 1);
			//implement the good value
			for (int v = 1; v <= 9; v++)
			{
				if (IsValid(s, row, col, v))
				{
					s.Cells[row][col] = v;
					if (Search(s, row, col + 1)) return true;
					else s.Cells[row][col] = 0;
				}
			}
			return false;
		}
		private bool IsValid(SudokuGrid s, int row, int col, int val)
		{
			//check if value is present in column
			for (int r = 0; r < 9; r++)
				if (s.Cells[r][col] == val) return false;

			//check if value is present in the row
			for (int c = 0; c < 9; c++)
				if (s.Cells[row][c] == val) return false;

			//check for the value in the 3 X 3 block
			int i = row / 3; int j = col / 3;
			for (int a = 0; a < 3; a++)
			for (int b = 0; b < 3; b++)
				if (val == s.Cells[3 * i + a][3 * j + b]) return false;

			return true;
		}
	}
}
