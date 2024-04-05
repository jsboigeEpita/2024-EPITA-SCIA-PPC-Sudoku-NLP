from timeit import default_timer
import numpy as np
from itertools import permutations
# from choco import Solution, ChocoSolver, IntVarMatrix


N = 9


if "ourSudoku" not in locals():
    ourSudoku = (
        (0, 0, 0, 4, 0, 9, 6, 7, 0),
        (0, 0, 0, 0, 7, 6, 9, 0, 0),
        (0, 0, 0, 0, 0, 0, 0, 0, 3),
        (0, 0, 0, 0, 0, 1, 7, 4, 0),
        (6, 4, 0, 0, 0, 0, 0, 1, 8),
        (0, 2, 1, 6, 0, 0, 0, 0, 0),
        (1, 0, 0, 0, 0, 0, 0, 0, 0),
        (0, 0, 4, 3, 2, 0, 0, 0, 0),
        (0, 6, 2, 9, 0, 4, 0, 0, 0),
    )


# def solveSudoku(instance):
#     solver = ChocoSolver()

#     grid = IntVarMatrix(N, N, 1, N)
#     for i in range(N):
#         for j in range(N):
#             if instance[i][j] != 0:
#                 solver.post(solver.eq(grid[i][j], instance[i][j]))

#     for i in range(N):
#         solver.post(solver.all_different([grid[i][j] for j in range(N)]))
#         solver.post(solver.all_different([grid[j][i] for j in range(N)]))

#     for i in range(0, N, 3):
#         for j in range(0, N, 3):
#             solver.post(
#                 solver.all_different(
#                     [grid[i + di][j + dj] for di in range(3) for dj in range(3)]
#                 )
#             )

#     return solver, grid


start = default_timer()
# solver, grid = solveSudoku(instance)
# solver.solve()
# result = np.array([[solver.get_value(grid[i][j]) for j in range(N)] for i in range(N)])
result = ourSudoku
execution = default_timer() - start

print("Solution du Sudoku :")
print(result)
print("Le temps de résolution est de : ", execution, " secondes")
