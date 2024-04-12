from timeit import default_timer as timer
import numpy as np
import pulp
import os
import sys


# import subprocess
# import platform

# def install_glpk_package():
#     if platform.system() == "Darwin":
#         # macOS: Installer le paquet glpk avec Homebrew
#         print("Installation du paquet glpk avec Homebrew...")
#         subprocess.run(["brew", "install", "glpk"])
#     elif platform.system() == "Linux":
#         # Linux: Vérifier si glpk-utils est déjà installé
#         result = subprocess.run("dpkg -s glpk-utils", shell=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
#         if result.returncode == 0:
#             print("Le paquet glpk-utils est déjà installé.")
#         else:
#             # glpk-utils n'est pas installé, exécuter la commande sudo apt-get install glpk-utils
#             print("Le paquet glpk-utils n'est pas installé. Exécution de la commande pour l'installer...")
#             subprocess.run("sudo apt-get install glpk-utils", shell=True)
#     else:
#         print("Système d'exploitation non pris en charge.")

# # Appel de la fonction pour installer le paquet glpk
# install_glpk_package()



current_path = os.path.dirname(os.path.abspath(sys.argv[0]))

while not os.path.exists(os.path.join(current_path, "Sudoku.ChocoRange")):
    current_path = os.path.dirname(current_path)
    if current_path == os.path.dirname(current_path):
        raise FileNotFoundError("Unable to find the root directory of the project.")

jar_path = os.path.join(current_path, "Sudoku.ChocoRange", "solver", "choco-parsers-4.10.14-light.jar")

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
    print("---------------Avant résolution----------------")
    print(np.array(ourSudoku))
    print()


def solve_with_solver(solver, options, instance=ourSudoku):
    problem = pulp.LpProblem("SudokuSolver", pulp.LpMinimize)
    choices = pulp.LpVariable.dicts("Choice", (range(N), range(N), range(1, N + 1)), cat="Binary")

    problem += 0, "Arbitrary Objective"

    for r in range(N):
        for c in range(N):
            problem += pulp.lpSum([choices[r][c][v] for v in range(1, N + 1)]) == 1

    for v in range(1, N + 1):
        for r in range(N):
            problem += pulp.lpSum([choices[r][c][v] for c in range(N)]) == 1
            problem += pulp.lpSum([choices[c][r][v] for c in range(N)]) == 1

        for r in range(0, N, 3):
            for c in range(0, N, 3):
                problem += pulp.lpSum([choices[r + i][c + j][v] for i in range(3) for j in range(3)]) == 1

    for r in range(N):
        for c in range(N):
            if instance[r][c] != 0:
                problem += choices[r][c][instance[r][c]] == 1

    # Lookup table pour les solveurs
    solver_lookup = {
        "GLPK_CMD": lambda: problem.solve(pulp.PULP_CBC_CMD(msg=False, options=options)),
        "PYGLPK": lambda: problem.solve(pulp.PYGLPK(msg=False, options=options)),
        "CPLEX_CMD": lambda: problem.solve(pulp.CPLEX_CMD(msg=False, options=options)),
        "CPLEX_PY": lambda: problem.solve(pulp.CPLEX_PY(msg=False, options=options)),
        "GUROBI": lambda: problem.solve(pulp.GUROBI(msg=False, options=options)),
        "GUROBI_CMD": lambda: problem.solve(pulp.GUROBI_CMD(msg=False, options=options)),
        "MOSEK": lambda: problem.solve(pulp.MOSEK(msg=False, options=options)),
        "XPRESS": lambda: problem.solve(pulp.XPRESS(msg=False, options=options)),
        "CBC": lambda: problem.solve(pulp.PULP_CBC_CMD(msg=False, options=options)),
        "COIN_CMD": lambda: problem.solve(pulp.COIN_CMD(msg=False, options=options)),
        "COINMP_DLL": lambda: problem.solve(pulp.COINMP_DLL(msg=False, options=options)),
        "CHOCO": lambda: problem.solve(pulp.CHOCO_CMD(path=jar_path, msg=False, keepFiles=True, options=options)),
        "MIPCL_CMD": lambda: problem.solve(pulp.MIPCL_CMD(msg=False, options=options)),
        "SCIP_CMD": lambda: problem.solve(pulp.SCIP_CMD(msg=False, options=options)),
        "FSCIP_CMD": lambda: problem.solve(pulp.FSCIP_CMD(msg=False, options=options)),
        "SCIP_PY": lambda: problem.solve(pulp.SCIP_PY(msg=False, options=options)),
        "HiGHS": lambda: problem.solve(pulp.HiGHS(msg=False, options=options)),
        "HiGHS_CMD": lambda: problem.solve(pulp.HiGHS_CMD(msg=False, options=options)),
        "COPT": lambda: problem.solve(pulp.COPT(msg=False, options=options)),
        "COPT_DLL": lambda: problem.solve(pulp.COPT_DLL(msg=False, options=options)),
        "COPT_CMD": lambda: problem.solve(pulp.COPT_CMD(msg=False, options=options))
    }

    # Exécute la fonction appropriée selon le solveur donné
    solver_function = solver_lookup.get(solver)
    if solver_function:
        solver_function()
    else:
        raise ValueError(f"Solveur inconnu : {solver}")

    solution = [[0 for _ in range(N)] for _ in range(N)]
    for r in range(N):
        for c in range(N):
            for v in range(1, N + 1):
                if pulp.value(choices[r][c][v]) == 1:
                    solution[r][c] = v

    if os.path.exists("SudokuSolver-pulp.mps"):
        os.remove("SudokuSolver-pulp.mps")
    if os.path.exists("SudokuSolver-pulp.sol"):
        os.remove("SudokuSolver-pulp.sol")
    
    return solution


# print(pulp.listSolvers())

# Configuration des solveurs et des paramètres à tester
configs = [
    {"solver": "CHOCO", "options": []},
    {"solver": "CBC", "options": []},
    {"solver": "GLPK_CMD", "options": []}
]

# Mesurer le temps d'exécution pour chaque configuration
results = []
for config in configs:
    start_time = timer()
    result = solve_with_solver(config["solver"], config["options"])
    end_time = timer()
    execution_time = end_time - start_time
    results.append({"solver": config["solver"], "time": execution_time, "solution": np.array(result)})

# Afficher les résultats et le sudoku résolu

for result in results:
    print(f"Solveur: {result['solver']}, Temps d'exécution: {result['time']} secondes")
    print("Solution:")
    print(np.array(result['solution']))
    print()

# Identifier le solveur le plus rapide
fastest = min(results, key=lambda x: x['time'])
print(f"Le solveur le plus rapide est {fastest['solver']} avec un temps d'exécution de {fastest['time']} secondes.")

result = results[0]['solution'] #CHOCO SOLVER EST PLACER EN [0]