from ChocoSolver import solve_sudoku

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
search_types = ['default', 'dom_over_w_deg', 'dom_over_w_deg_ref', 'activity_based', 'min_dom_lb', 'min_dom_ub', 'random', 'conflict_history', 'input_order_lb', 'input_order_ub', 'failure_length_based', 'failure_rate_based']

for search_type in search_types:
    print("\nTesting search type: {}".format(search_type))
    r = solve_sudoku(initial_grid, search_type, print_stats=True)