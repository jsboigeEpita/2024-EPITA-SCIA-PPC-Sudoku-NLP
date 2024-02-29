using System;
using System.Collections.Generic;
using System.IO;
using Sudoku.Shared;

namespace Sudoku.Benchmark
{
    public class SudokuHelper
    {
        private const string PUZZLES_FOLDER_NAME = "Puzzles";

        public static List<Shared.SudokuGrid> GetSudokus(SudokuDifficulty difficulty)
        {
            string fileName = difficulty switch
            {
                SudokuDifficulty.Easy => "Sudoku_Easy51.txt",
                SudokuDifficulty.Medium => "Sudoku_hardest.txt",
                _ => "Sudoku_top95.txt"
            };

            DirectoryInfo puzzlesDirectory = null;
            var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);
            do
            {
                var subDirectories = currentDirectory.GetDirectories();
                foreach (var subDirectory in subDirectories)
                {
                    if (subDirectory.Name == PUZZLES_FOLDER_NAME)
                    {
                        puzzlesDirectory = subDirectory;
                        break;
                    }
                }
                currentDirectory = currentDirectory.Parent;
                if (currentDirectory == null)
                {
                    throw new ApplicationException("couldn't find puzzles directory");
                }
            } while (puzzlesDirectory == null);
            string filePath = System.IO.Path.Combine(puzzlesDirectory.ToString(), fileName);
            var sudokus = Shared.SudokuGrid.ReadSudokuFile(filePath);
            return sudokus;
        }



    }
}