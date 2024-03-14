using System.IO.Pipes;
using Sudoku.Shared;

namespace Sudoku.Norvig;

public class NorvigSolver : ISudokuSolver
{
    private short[] _possibleValues; // a 1D array that represent all possible value of a given cell of the sudoku

    private HashSet<int>[,] _units;
    
    public NorvigSolver()
    {
        _possibleValues = new short[Tools.SURFACE];
        _units = new HashSet<int>[Tools.SURFACE, 3];
        Tools.FillUnits(_units);
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

    private bool Fill(int cell, int digit)
    {
        throw new NotImplementedException();
    }

    private bool Eliminate(int cell, int digit)
    {
        short bitmask = (short)(0x1 << (digit - 1));
        
        if ((_possibleValues[cell] & bitmask) == 0)
            return true;
        
        short inverseBitmask = (short)(bitmask ^ 0x1FF);
        short buffer = (short)(_possibleValues[cell] & inverseBitmask);

        if (buffer == 0)
            return false;

        short[] save = (short[])_possibleValues.Clone();
        _possibleValues[cell] = buffer;
        
        if ((buffer & (buffer - 1)) == 0)
        {
            var neighbours = SudokuGrid.CellNeighbours[cell / Tools.SIZE][cell % Tools.SIZE];

            int digit2 = int.TrailingZeroCount(buffer) + 1;

            foreach ((int row, int column) neighbour in neighbours)
            {
                if (!Eliminate(neighbour.row * Tools.SIZE + neighbour.column, digit2))
                {
                    _possibleValues = save;
                    return false;
                }
            }
        }

        for (int i = 0; i < 3; i++)
        {
            HashSet<int> unit = _units[cell, i];
            IEnumerable<int> possibleDigitPlaces = unit.Where(x => (inverseBitmask & _possibleValues[x]) != 0);
            if (possibleDigitPlaces.Any() ||
                (possibleDigitPlaces.Count() == 1 && !Fill(possibleDigitPlaces.First(), digit)))
            {
                _possibleValues = save;
                return false;
            }
        }

        return true;
    }
}