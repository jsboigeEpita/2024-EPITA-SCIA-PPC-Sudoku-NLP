# from timeit import default_timer
import numpy as np
import pulp

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
    print("---------------Avant resolution----------------")
    print(np.array(ourSudoku))
    

def solveSudoku(instance):
    # Créer le problème de programmation linéaire
    problem = pulp.LpProblem("SudokuSolver", pulp.LpMinimize)
    
    # Créer les variables
    choices = pulp.LpVariable.dicts("Choice", (range(N), range(N), range(1, N + 1)), cat="Binary")
    
    # Ajouter la contrainte arbitraire (objectif non nécessaire)
    problem += 0, "Arbitrary Objective"
    
    # Contrainte: une valeur par cellule
    for r in range(N):
        for c in range(N):
            problem += pulp.lpSum([choices[r][c][v] for v in range(1, N + 1)]) == 1
    
    # Contraintes pour lignes, colonnes et blocs 3x3
    for v in range(1, N + 1):
        for r in range(N):
            problem += pulp.lpSum([choices[r][c][v] for c in range(N)]) == 1
            problem += pulp.lpSum([choices[c][r][v] for c in range(N)]) == 1
            
        for r in range(0, N, 3):
            for c in range(0, N, 3):
                problem += pulp.lpSum([choices[r+i][c+j][v] for i in range(3) for j in range(3)]) == 1
    
    # Remplir les valeurs initiales
    for r in range(N):
        for c in range(N):
            if instance[r][c] != 0:
                problem += choices[r][c][instance[r][c]] == 1
    
    # Résoudre le problème
    problem.solve(pulp.PULP_CBC_CMD(msg=False))
    
    # Construire la solution
    solution = [[0 for _ in range(N)] for _ in range(N)]
    for r in range(N):
        for c in range(N):
            for v in range(1, N + 1):
                if pulp.value(choices[r][c][v]) == 1:
                    solution[r][c] = v
    return solution

result = np.array(solveSudoku(ourSudoku))

print()
print("--------------Après resolution----------------------")
print(result)