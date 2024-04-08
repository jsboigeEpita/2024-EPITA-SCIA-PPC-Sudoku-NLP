using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Sudoku.Shared;

namespace Sudoku.Human;

public sealed class Puzzle
{
    public ReadOnlyCollection<Region> Rows { get; }
    public ReadOnlyCollection<Region> Columns { get; }
    public ReadOnlyCollection<Region> Blocks { get; }
    public ReadOnlyCollection<ReadOnlyCollection<Region>> Regions { get; }

    private readonly Cell[] _board;
    internal readonly Region[] RowsI;
    internal readonly Region[] ColumnsI;
    internal readonly Region[] BlocksI;
    internal readonly Region[][] RegionsI;

    public Cell this[int col, int row] => _board[Utils.CellIndex(col, row)];


    private Puzzle(SudokuGrid s)
    {
        _board = new Cell[81];
        for (int col = 0; col < 9; col++)
        {
            for (int row = 0; row < 9; row++)
            {
                _board[Utils.CellIndex(col, row)] = new Cell(this, s.Cells[row,col], new SPoint(col, row));
            }
        }

        RowsI = new Region[9];
        Rows = new ReadOnlyCollection<Region>(RowsI);
        ColumnsI = new Region[9];
        Columns = new ReadOnlyCollection<Region>(ColumnsI);
        BlocksI = new Region[9];
        Blocks = new ReadOnlyCollection<Region>(BlocksI);
        RegionsI = [RowsI, ColumnsI, BlocksI];
        Regions = new ReadOnlyCollection<ReadOnlyCollection<Region>>([Rows, Columns, Blocks]);

        var cellsCache = new Cell[9];
        for (int i = 0; i < 9; i++)
        {
            int j;
            for (j = 0; j < 9; j++)
            {
                cellsCache[j] = _board[Utils.CellIndex(j, i)];
            }
            RowsI[i] = new Region(cellsCache);

            for (j = 0; j < 9; j++)
            {
                cellsCache[j] = _board[Utils.CellIndex(i, j)];
            }
            ColumnsI[i] = new Region(cellsCache);

            j = 0;
            int x = i % 3 * 3;
            int y = i / 3 * 3;
            for (int col = x; col < x + 3; col++)
            {
                for (int row = y; row < y + 3; row++)
                {
                    cellsCache[j++] = _board[Utils.CellIndex(col, row)];
                }
            }
            BlocksI[i] = new Region(cellsCache);
        }

        for (int i = 0; i < 81; i++)
        {
            _board[i].InitRegions();
        }
        for (int i = 0; i < 81; i++)
        {
            _board[i].InitVisibleCells();
        }
    }

    private Puzzle(Puzzle puzzle)
    {
        _board = new Cell[81];
        for (int col = 0; col < 9; col++)
        {
            for (int row = 0; row < 9; row++)
            {
                _board[Utils.CellIndex(col, row)] = new Cell(this, puzzle[col,row]);
            }
        }

        RowsI = new Region[9];
        Rows = new ReadOnlyCollection<Region>(RowsI);
        ColumnsI = new Region[9];
        Columns = new ReadOnlyCollection<Region>(ColumnsI);
        BlocksI = new Region[9];
        Blocks = new ReadOnlyCollection<Region>(BlocksI);
        RegionsI = [RowsI, ColumnsI, BlocksI];
        Regions = new ReadOnlyCollection<ReadOnlyCollection<Region>>([Rows, Columns, Blocks]);

        var cellsCache = new Cell[9];
        for (int i = 0; i < 9; i++)
        {
            int j;
            for (j = 0; j < 9; j++)
            {
                cellsCache[j] = _board[Utils.CellIndex(j, i)];
            }
            RowsI[i] = new Region(cellsCache);

            for (j = 0; j < 9; j++)
            {
                cellsCache[j] = _board[Utils.CellIndex(i, j)];
            }
            ColumnsI[i] = new Region(cellsCache);

            j = 0;
            int x = i % 3 * 3;
            int y = i / 3 * 3;
            for (int col = x; col < x + 3; col++)
            {
                for (int row = y; row < y + 3; row++)
                {
                    cellsCache[j++] = _board[Utils.CellIndex(col, row)];
                }
            }
            BlocksI[i] = new Region(cellsCache);
        }

        for (int i = 0; i < 81; i++)
        {
            _board[i].InitRegions();
        }
        for (int i = 0; i < 81; i++)
        {
            _board[i].InitVisibleCells();
        }
    }

    public static Puzzle CreateFromGrid(SudokuGrid s)
    {
        return new Puzzle(s);
    }

    public static Puzzle CreateFromPuzzle(Puzzle puzzle)
    {
        Puzzle puzz = new Puzzle(puzzle);
        return puzz;
    }

	public SudokuGrid toGrid(SudokuGrid s) {
		
		for (int col = 0; col < 9; col++)
        {
            for (int row = 0; row < 9; row++)
            {
                s.Cells[row, col] = _board[Utils.CellIndex(col,row)].Value;
                _board[Utils.CellIndex(col, row)] = new Cell(this, s.Cells[row,col], new SPoint(col, row));
            }
        }
		return s;
	}

    internal void RefreshCandidates()
    {
        for (int i = 0; i < 81; i++)
        {
            Cell cell = _board[i];
            for (int digit = 1; digit <= 9; digit++)
            {
                cell.CandI.Set(digit, true);
            }
        }
        for (int i = 0; i < 81; i++)
        {
            Cell cell = _board[i];
            if (cell.Value != Cell.EMPTY_VALUE)
            {
                cell.SetValue(cell.Value);
            }
        }
    }

    public Cell[] GetBoard() {
        return _board;
    }
}