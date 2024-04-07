from pychoco.model import Model

def solve_sudoku(initial_grid):
    model = Model("Sudoku Solver")

    grid = [[model.intvar(1, 9, name="cell_{}_{}".format(i, j)) for j in range(9)] for i in range(9)]

    for i in range(9):
        model.all_different([grid[i][j] for j in range(9)]).post()  # Lignes
        model.all_different([grid[j][i] for j in range(9)]).post()  # Colonnes

    for block_row in range(3):
        for block_col in range(3):
            square = [grid[3 * block_row + i][3 * block_col + j] for i in range(3) for j in range(3)]
            model.all_different(square).post()

    for i in range(9):
        for j in range(9):
            if initial_grid[i][j] != 0:
                model.arithm(grid[i][j], "=", initial_grid[i][j]).post()

    solver = model.get_solver()
    if solver.solve():
        solved_grid = [[grid[i][j].get_value() for j in range(9)] for i in range(9)]
        return solved_grid
    else:
        print("No solution found.")
        return None

initial_grid = [
    [5, 3, 0, 0, 7, 0, 0, 0, 0],
    [6, 0, 0, 1, 9, 5, 0, 0, 0],
    [0, 9, 8, 0, 0, 0, 0, 6, 0],
    [8, 0, 0, 0, 6, 0, 0, 0, 3],
    [4, 0, 0, 8, 0, 3, 0, 0, 1],
    [7, 0, 0, 0, 2, 0, 0, 0, 6],
    [0, 6, 0, 0, 0, 0, 2, 8, 0],
    [0, 0, 0, 4, 1, 9, 0, 0, 5],
    [0, 0, 0, 0, 8, 0, 0, 7, 9]
]

r = solve_sudoku(instance)

