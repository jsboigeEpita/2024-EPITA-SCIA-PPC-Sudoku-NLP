import numpy as np
from timeit import default_timer

def is_valid(grid, row, col, num):
    # Vérifier si num est présent dans la ligne spécifiée
    if num in grid[row]:
        return False
    
    # Vérifier si num est présent dans la colonne spécifiée
    if num in grid[:, col]:
        return False
    
    # Vérifier si num est présent dans le bloc 3x3
    start_row, start_col = 3 * (row // 3), 3 * (col // 3)
    for r in range(start_row, start_row + 3):
        for c in range(start_col, start_col + 3):
            if grid[r, c] == num:
                return False
    return True

def solve_sudoku(grid, row=0, col=0):
    # Trouver la prochaine cellule vide
    for i in range(row, 9):
        for j in range(col if i == row else 0, 9):
            if grid[i, j] == 0:
                for num in range(1, 10):
                    if is_valid(grid, i, j, num):
                        grid[i, j] = num
                        if solve_sudoku(grid, i, j + 1):
                            return True
                        grid[i, j] = 0
                return False
    return True

# Définir `instance` uniquement si non déjà défini par PythonNET
if 'instance' not in locals():
    instance = np.array([
        [0,0,0,0,9,4,0,3,0],
        [0,0,0,5,1,0,0,0,7],
        [0,8,9,0,0,0,0,4,0],
        [0,0,0,0,0,0,2,0,8],
        [0,6,0,2,0,1,0,5,0],
        [1,0,2,0,0,0,0,0,0],
        [0,7,0,0,0,0,5,2,0],
        [9,0,0,0,6,5,0,0,0],
        [0,4,0,9,7,0,0,0,0]
    ], dtype=int)

start = default_timer()
# Exécuter la résolution de Sudoku
if solve_sudoku(instance):
    # print("Sudoku résolu par backtracking avec succès.")
    result = instance  # `result` sera utilisé pour récupérer la grille résolue depuis C#
else:
    print("Aucune solution trouvée.")
execution = default_timer() - start
print("Le temps de résolution est de : ", execution * 1000, " ms")