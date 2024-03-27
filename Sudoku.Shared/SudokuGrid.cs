using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Text;

namespace Sudoku.Shared
{
    public class SudokuGrid : ICloneable
    {

       
        /// <summary>
        /// The list of row indexes is used many times and thus stored for quicker access.
        /// </summary>
        public static readonly ReadOnlyCollection<int> NeighbourIndices =
            new ReadOnlyCollection<int>(Enumerable.Range(0, 9).ToList());

        private static readonly (int row, int column)[][] _LineNeighbours =
            NeighbourIndices.Select(r => NeighbourIndices.Select(c => (r, c)).ToArray()).ToArray();
            

        private static readonly (int row, int column)[][] _ColumnNeighbours =
            NeighbourIndices.Select(c => NeighbourIndices.Select(r => (r, c)).ToArray()).ToArray();

        private static readonly (int row, int column)[][] _BoxNeighbours = GetBoxNeighbours();

        public static readonly (int row, int column)[][] AllNeighbours =
            _LineNeighbours.Concat(_ColumnNeighbours).Concat(_BoxNeighbours).ToArray();


        private static (int row, int column)[][] GetBoxNeighbours()
        {
            var toreturn = new (int row, int column)[9][];
            for (int boxIndex = 0; boxIndex < 9; boxIndex++)
            {
                var currentBox = new List<(int row, int column)>();
                (int row, int column) startIndex = (boxIndex / 3 * 3, boxIndex % 3 * 3);
                for (int r = 0; r < 3; r++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        currentBox.Add((startIndex.row+r, startIndex.column+c));
                    }
                }

                toreturn[boxIndex] = currentBox.ToArray();
            }

            return toreturn;
        }

        public static readonly (int row, int column)[][][] CellNeighbours;


        static SudokuGrid()
        {
            CellNeighbours = new (int row, int column)[9][][];
            foreach (var rowIndex in NeighbourIndices)
            {
                CellNeighbours[rowIndex] = new (int row, int column)[9][];
                foreach (var columnIndex in NeighbourIndices)
                {
                    var cellVoisinage = new List<(int row, int column)>();
                    
                    foreach (var voisinage in AllNeighbours)
                    {
                        if (voisinage.Contains((rowIndex, columnIndex)))
                        {
                            foreach (var voisin in voisinage)
                            {
                                //We don't include the current cell
                                if (!cellVoisinage.Contains(voisin) && voisin.row != rowIndex || voisin.column != columnIndex)
                                {
                                    cellVoisinage.Add(voisin);
                                }
                            }
                        }
                    }
                    CellNeighbours[rowIndex][columnIndex] = cellVoisinage.ToArray();
                }
                
            }
        }

        

        public SudokuGrid()
        {
        }

        

        // The List property makes it easier to manipulate cells,
        public int[,] Cells { get; set; } = NeighbourIndices.Select(r => new int[9]).ToArray().To2D();



        /// <summary>
        /// Displays a SudokuGrid in an easy-to-read format
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var lineSep = new string('-', 31);
            var blankSep = new string(' ', 8);

            var output = new StringBuilder();
            output.Append(lineSep);
            output.AppendLine();

            for (int row = 1; row <= 9; row++)
            {
                output.Append("| ");
                for (int column = 1; column <= 9; column++)
                {

                    var value = Cells[row - 1,column - 1];

                    output.Append(value);
                    if (column % 3 == 0)
                    {
                        output.Append(" | ");
                    }
                    else
                    {
                        output.Append("  ");
                    }
                }

                output.AppendLine();
                if (row % 3 == 0)
                {
                    output.Append(lineSep);
                }
                else
                {
                    output.Append("| ");
                    for (int i = 0; i < 3; i++)
                    {
                        output.Append(blankSep);
                        output.Append("| ");
                    }
                }

                output.AppendLine();
            }

            return output.ToString();
        }

       


        public int[] GetAvailableNumbers(int x, int y)
        {
            if (x < 0 || x >= 9 || y < 0 || y >= 9)
            {
                throw new ApplicationException("Invalid Coordinates");
            }


            bool[] used = new bool[9];
            foreach (var cellNeighbour in CellNeighbours[x][y])
            {
                var neighbourValue = Cells[cellNeighbour.row,cellNeighbour.column];
                if (neighbourValue > 0)
                {
                    used[neighbourValue - 1] = true;
                }
            }

            
            List<int> res = new List<int>();

            for (int i = 0; i < 9; i++)
            {
                if (used[i] == false)
                {
                    res.Add(i + 1);
                }
            }

            return res.ToArray();
        }



        /// <summary>
        /// Parses a single SudokuGrid
        /// </summary>
        /// <param name="sudokuAsString">the string representing the sudoku</param>
        /// <returns>the parsed sudoku</returns>
        public static SudokuGrid ReadSudoku(string sudokuAsString)
        {
            return ReadMultiSudoku(new[] { sudokuAsString })[0];
        }


        /// <summary>
        /// Parses a file with one or several sudokus
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>the list of parsed Sudokus</returns>
        public static List<SudokuGrid> ReadSudokuFile(string fileName)
        {
            return ReadMultiSudoku(File.ReadAllLines(fileName));
        }

        /// <summary>
        /// Parses a list of lines into a list of sudoku, accounting for most cases usually encountered
        /// </summary>
        /// <param name="lines">the lines of string to parse</param>
        /// <returns>the list of parsed Sudokus</returns>
        public static List<SudokuGrid> ReadMultiSudoku(string[] lines)
        {
            var toReturn = new List<SudokuGrid>();
            var rows = new List<int[]>();
            var rowCells = new List<int>(9);
            // we ignore lines not starting with a sudoku character
            foreach (var line in lines.Where(l => l.Length > 0
                                                  && IsSudokuChar(l[0])))
            {
                foreach (char c in line)
                {
                    //we ignore lines not starting with cell chars
                    if (IsSudokuChar(c))
                    {
                        if (char.IsDigit(c))
                        {
                            // if char is a digit, we add it to a cell
                            rowCells.Add((int)Char.GetNumericValue(c));
                        }
                        else
                        {
                            // if char represents an empty cell, we add 0
                            rowCells.Add(0);
                        }
                    }

                    // when 9 cells are entered, we create a row and start collecting cells again.
                    if (rowCells.Count == 9)
                    {
                        rows.Add(rowCells.ToArray());

                        // we empty the current row collector to start building a new row
                        rowCells.Clear();
                        
                    }

                    // when 9 rows are collected, we create a Sudoku and start collecting rows again.
                    if (rows.Count == 9)
                    {
                        toReturn.Add(new SudokuGrid() { Cells = rows.ToArray().To2D() });
                        // we empty the current cell collector to start building a new SudokuGrid
                        rows.Clear();
                    }

                }
            }

            return toReturn;
        }


        /// <summary>
        /// identifies characters to be parsed into sudoku cells
        /// </summary>
        /// <param name="c">a character to test</param>
        /// <returns>true if the character is a cell's char</returns>
        private static bool IsSudokuChar(char c)
        {
            return char.IsDigit(c) || c == '.' || c == 'X' || c == '-';
        }

        public object Clone()
        {
            return CloneSudoku();
        }

        
        public SudokuGrid CloneSudoku()
        {
            // Performs a deep copy of the 2D cells array
            var toReturn = new SudokuGrid();
            toReturn.Cells = (int[,])Cells.Clone();
            return toReturn;
        }


        private static IDictionary<string,Lazy<ISudokuSolver>> _CachedSolvers;

        public static IDictionary<string, Lazy<ISudokuSolver>> GetSolvers()
        {
            if (_CachedSolvers == null)
            {
                var solvers = new Dictionary<string, Lazy<ISudokuSolver>>();


                foreach (var file in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory))
                {
                    if (file.EndsWith("dll") && !(Path.GetFileName(file).StartsWith("libz3")))
                    {
                        try
                        {
                            
                            Type[] assemblyTypes;

                            try
                            {
	                            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
								assemblyTypes = assembly.GetTypes();
                            }
                            catch
                            {
	                            assemblyTypes = new Type[] { };

                            }

                            foreach (var type in assemblyTypes)
                            {
                                if (typeof(ISudokuSolver).IsAssignableFrom(type) && !(type.IsAbstract) && !(typeof(ISudokuSolver) == type))
                                {
                                    try
                                    {
                                        var solver = new Lazy<ISudokuSolver>(()=>(ISudokuSolver)Activator.CreateInstance(type));
                                        solvers.Add(type.Name, solver);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                    }

                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }

                    }

                }

                _CachedSolvers = solvers;
            }

            return _CachedSolvers;
        }


        public int NbErrors(SudokuGrid originalPuzzle)
        {
            // We use a large lambda expression to count duplicates in rows, columns and boxes
            var toReturn = SudokuGrid.AllNeighbours.Select(n => n.Select(nx => this.Cells[nx.row,nx.column]))
                .Sum(n => n.GroupBy(x => x).Select(g => g.Count() - 1).Sum());
            // We use a loop to count cells differing from original Puzzle Mask
            foreach (var rowIndex in NeighbourIndices)
            {
                foreach (var colIndex in NeighbourIndices)
                {
                    if (originalPuzzle.Cells[rowIndex, colIndex]>0 && originalPuzzle.Cells[rowIndex, colIndex] != Cells[rowIndex,colIndex])
                    {
                        toReturn += 1;
                    }
                }
            }
            
            return toReturn;
        }

        public bool IsValid(SudokuGrid originalPuzzle)
        {
            return NbErrors(originalPuzzle) == 0;
        }

        public int NbEmptyCells() => Cells.Cast<int>().Count(c => c == 0);

    }
}
