using System.IO.Pipes;
using Sudoku.Shared;

namespace Sudoku.Norvig;

public class NorvigSolver : ISudokuSolver
{
    private short[] _possibleValues; // a 1D array that represent all possible value of a given cell of the sudoku

    public NorvigSolver()
    {
        _possibleValues = new short[Tools.SURFACE];
        Parallel.For(0, Tools.SURFACE, (i, state) => _possibleValues[i] = 0x1FF);
    }

    public SudokuGrid Solve(SudokuGrid s)
    {
        RemoveInitialPossibleValues(s);
        for (int i = 0; i < Tools.SIZE; i++)
        {
            for (int j = 0; j < Tools.SIZE; j++)
            {
            }
        }

        return s;
    }

    private void RemoveInitialPossibleValues(SudokuGrid sudokuGrid)
    {
        for (int i = 0; i < Tools.SIZE; i++)
        {
            for (int j = 0; j < Tools.SIZE; j++)
            {
                int cellValue = sudokuGrid.Cells[i, j];

                if (cellValue == 0)
                    continue;

                short bitmask = (short)(0x1 << (cellValue - 1));
                _possibleValues[i * Tools.SIZE + j] = bitmask;

                var neighbours = SudokuGrid.CellNeighbours[i][j];

                short inverseBitmask = (short)(bitmask ^ 0x1FF);

                foreach ((int row, int column) neighbour in neighbours)
                {
                    short buffer = _possibleValues[neighbour.row * Tools.SIZE + neighbour.column];

                    _possibleValues[neighbour.row * Tools.SIZE + neighbour.column] = (short)(buffer & inverseBitmask);
                }
            }
        }
    }
}