from pychoco.model import Model

def solve_sudoku(initial_grid, search_type='', print_stats=False):

    # Create a Choco model
    model = Model("Sudoku Solver")

    # Initialize a 9x9 grid with integer variables ranging from 1 to 9
    grid = [[model.intvar(1, 9, name="cell_{}_{}".format(i, j)) for j in range(9)] for i in range(9)]

    # Ensure all numbers in each row and column are different
    for i in range(9):  
        model.all_different([grid[i][j] for j in range(9)]).post()  # Lignes
        model.all_different([grid[j][i] for j in range(9)]).post()  # Colonnes

    # Ensure all numbers in each 3x3 block are different
    for block_row in range(3):
        for block_col in range(3):
            square = [grid[3 * block_row + i][3 * block_col + j] for i in range(3) for j in range(3)]
            model.all_different(square).post()

    # Set the initial values for the Sudoku grid
    for i in range(9):
        for j in range(9):
            if initial_grid[i][j] != 0:
                model.arithm(grid[i][j], "=", initial_grid[i][j]).post()
    
    # Solve the Sudoku
    solver = model.get_solver()
    intvars = [cell for row in grid for cell in row]

    if search_type == 'dom_over_w_deg':
        solver.set_dom_over_w_deg_search(*intvars)
    elif search_type == 'dom_over_w_deg_ref':
        solver.set_dom_over_w_deg_ref_search(*intvars)
    elif search_type == 'activity_based':
        solver.set_activity_based_search(*intvars)
    elif search_type == 'min_dom_lb':
        solver.set_min_dom_lb_search(*intvars)
    elif search_type == 'min_dom_ub':
        solver.set_min_dom_ub_search(*intvars)
    elif search_type == 'random':
        solver.set_random_search(*intvars)
    elif search_type == 'conflict_history':
        solver.set_conflict_history_search(*intvars)
    elif search_type == 'input_order_lb':
        solver.set_input_order_lb_search(*intvars)
    elif search_type == 'input_order_ub':
        solver.set_input_order_ub_search(*intvars)
    elif search_type == 'failure_length_based':
        solver.set_failure_length_based_search(*intvars)
    elif search_type == 'failure_rate_based':
        solver.set_failure_rate_based_search(*intvars)
    else:
        solver.set_default_search()

    if print_stats:
        print("Solver statistics with search type: {}".format(search_type))
        solver.show_statistics()
        
    if solver.solve():
        # If a solution is found, extract the values and return the solved grid
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

r = solve_sudoku(initial_grid)
