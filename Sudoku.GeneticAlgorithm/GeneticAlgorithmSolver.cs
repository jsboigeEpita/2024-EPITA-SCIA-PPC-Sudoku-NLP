using Sudoku.Shared;
using System;

namespace GeneticAlgorithmSolver
{
    public class GeneticAlgorithmSolver : ISudokuSolver
    {
        int gridSize = 9;

        public SudokuGrid Solve(SudokuGrid s)
        {
            var solution = s.CloneSudoku();
            Console.WriteLine("Le Sudoku AVANT la resolution");
            Console.WriteLine(solution);

            // Appeler la fonction de résolution récursive
            if (SolveSudoku(solution))
            {
                Console.WriteLine("Le Sudoku APRES  !");
            }
            else
            {
                Console.WriteLine("Impossible de résoudre le Sudoku !");
            }

            return solution;
        }

        private bool SolveSudoku(SudokuGrid grid)
        {
            int row = 0;
            int col = 0;
            bool isEmpty = true;

            // Trouver une cellule vide dans la grille
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    if (grid.Cells[i, j] == 0)
                    {
                        row = i;
                        col = j;

                        // Marquer qu'il y a une cellule vide
                        isEmpty = false;
                        break;
                    }
                }
                if (!isEmpty)
                {
                    break;
                }
            }

            // Aucune cellule vide trouvée, le Sudoku est résolu
            if (isEmpty)
            {
                return true;
            }

            // Essayer de placer les chiffres de 1 à 9 dans la cellule vide
            for (int num = 1; num <= gridSize; num++)
            {
                if (IsSafe(grid, row, col, num))
                {
                    grid.Cells[row, col] = num;

                    // Résoudre récursivement le Sudoku
                    if (SolveSudoku(grid))
                    {
                        return true;
                    }

                    // Si la valeur ne mène pas à une solution valide, la réinitialiser
                    grid.Cells[row, col] = 0;
                }
            }

            // Aucune valeur ne mène à une solution valide
            return false;
        }

        private bool IsSafe(SudokuGrid grid, int row, int col, int num)
        {
            // Vérifier si le numéro est déjà présent dans la même ligne
            for (int x = 0; x < gridSize; x++)
            {
                if (grid.Cells[row, x] == num)
                {
                    return false;
                }
            }

            // Vérifier si le numéro est déjà présent dans la même colonne
            for (int y = 0; y < gridSize; y++)
            {
                if (grid.Cells[y, col] == num)
                {
                    return false;
                }
            }

            // Vérifier si le numéro est déjà présent dans la même sous-grille 3x3
            int sqrt = (int)Math.Sqrt(gridSize);
            int boxRowStart = row - row % sqrt;
            int boxColStart = col - col % sqrt;

            for (int i = boxRowStart; i < boxRowStart + sqrt; i++)
            {
                for (int j = boxColStart; j < boxColStart + sqrt; j++)
                {
                    if (grid.Cells[i, j] == num)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
