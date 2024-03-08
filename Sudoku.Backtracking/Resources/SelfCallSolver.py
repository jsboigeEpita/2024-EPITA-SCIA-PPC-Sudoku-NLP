import clr
clr.AddReference("Sudoku.Shared")
clr.AddReference("Sudoku.Backtracking")
from Sudoku.Backtracking import BacktrackingDotNetSolver
netSolver = BacktrackingDotNetSolver()
solvedSudoku = netSolver.Solve(sudoku)