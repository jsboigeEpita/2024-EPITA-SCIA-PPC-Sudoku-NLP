import numpy as np
from timeit import default_timer
import networkx as nx


def position_to_index(position):
    return position[0] * 9 + position[1]


def index_to_position(index):
    return index // 9, index % 9


def get_pre_coloring(grid):
    pre_coloring = {1: [], 2: [], 3: [], 4: [], 5: [], 6: [], 7: [], 8: [], 9: []}
    pre_colored_index = []
    for i in range(9):
        for j in range(9):
            if grid[i][j] != 0:
                pre_coloring[grid[i][j]].append((i, j))
                pre_colored_index.append((i, j))
    return pre_coloring, pre_colored_index


def add_position_to_neighbors(position, neighbors, pre_coloring, grid):
    index = position_to_index(position)
    is_absent = index not in neighbors
    if is_absent and grid[position[0]][position[1]] == 0:
        neighbors.append(index)
    # If the node is pre colored, add its representative
    elif is_absent:
        neighbors.append(position_to_index(pre_coloring[grid[position[0]][position[1]]][0]))


def add_neighbors(index, neighbors, pre_coloring, grid):
    i, j = index_to_position(index)
    # add cells in same row
    for j_bis in range(9):
        if j != j_bis:
            add_position_to_neighbors((i, j_bis), neighbors, pre_coloring, grid)
    # add cells in same column
    for i_bis in range(9):
        if i != i_bis:
            add_position_to_neighbors((i_bis, j), neighbors, pre_coloring, grid)
    # add cells in same square
    square_start_row, square_start_column = i - i % 3, j - j % 3
    for i_bis in range(square_start_row, square_start_row + 3):
        if i_bis != i:
            for j_bis in range(square_start_column, square_start_column + 3):
                if j_bis != j:
                    add_position_to_neighbors((i_bis, j_bis), neighbors, pre_coloring, grid)


def add_additional_neighbors(index, neighbors, pre_coloring, grid):
    i, j = index_to_position(index)
    value = grid[i][j]
    # Add links to other pre-colored values
    for i in range(1, 10):
        if i != value and len(pre_coloring[i]) != 0:
            add_position_to_neighbors(pre_coloring[i][0], neighbors, pre_coloring, grid)
    # Add links to neighbors of other pre-colored cells of same value
    for i in range(1, len(pre_coloring[value])):
        other_index = position_to_index(pre_coloring[value][i])
        add_neighbors(other_index, neighbors, pre_coloring, grid)


def sudoku_to_graph(grid):
    pre_coloring, pre_colored_nodes = get_pre_coloring(grid)
    graph = nx.Graph()
    for row in range(9):
        for col in range(9):
            if (row, col) not in pre_colored_nodes:
                graph.add_node(position_to_index((row, col)), color=0)
            elif len(pre_coloring[grid[row][col]]) != 0 and pre_coloring[grid[row][col]][0] == (row, col):
                graph.add_node(position_to_index((row, col)), color=0)

    for node in graph.nodes:
        neighbors = []
        # Add all connected nodes
        add_neighbors(node, neighbors, pre_coloring, grid)
        i, j = index_to_position(node)
        # Handle additional edges for pre colored nodes
        if grid[i][j] != 0:
            add_additional_neighbors(node, neighbors, pre_coloring, grid)
        for neighbor in neighbors:
            graph.add_edge(node, neighbor)
    return graph, pre_coloring


def get_saturated_degree(graph, node):
    res = 0
    already_seen_colors = []
    for neighbor in graph[node]:
        neighbor_color = graph.nodes[neighbor]["color"]
        if neighbor_color != 0 and neighbor_color not in already_seen_colors:
            already_seen_colors.append(neighbor_color)
            res += 1
    return res


def get_most_used_remaining(graph, already_seen_colors):
    colors = {}
    for node in graph.nodes:
        color = graph.nodes[node]["color"]
        if graph.nodes[node]["color"] == 0 or color in already_seen_colors:
            continue
        if color not in colors:
            colors[color] = 0
        colors[color] += 1
    # if no other color use found, ex for start
    if len(colors) == 0:
        color = 1
        while color in already_seen_colors:
            color += 1
        return color
    # look for most used
    max_use = -1
    max_color = 0
    for color in colors:
        if colors[color] > max_use:
            max_use = colors[color]
            max_color = color
    return max_color


def assign_color(graph, node, color_number):
    already_seen_colors = []
    for neighbor in graph[node]:
        neighbor_color = graph.nodes[neighbor]["color"]
        if neighbor_color != 0 and neighbor_color not in already_seen_colors:
            already_seen_colors.append(neighbor_color)
    if len(already_seen_colors) == color_number:
        color_number += 1
        color = color_number
    else:
        color = get_most_used_remaining(graph, already_seen_colors)
    #print("Assigning color", color, "to node", node)
    graph.nodes[node]["color"] = color
    return color_number


def color_graph(graph):
    color_number = 1
    node_number = len(graph.nodes)
    colored_node_number = 0
    while colored_node_number < node_number:
        max_saturated_degree = -1
        max_index = -1
        for node in graph.nodes:
            if graph.nodes[node]['color'] != 0:
                continue
            saturated_degree = get_saturated_degree(graph, node)
            if saturated_degree > max_saturated_degree:
                max_saturated_degree = saturated_degree
                max_index = node
            elif saturated_degree == max_saturated_degree:
                if len(graph[node]) > len(graph[max_index]):
                    max_index = node
        color_number = assign_color(graph, max_index, color_number)
        colored_node_number += 1


def graph_to_sudoku(graph, pre_coloring, grid):
    value_dict = {}
    missing = []
    maximum = 9  # for testing purpose
    for key in pre_coloring:
        if len(pre_coloring[key]) != 0:
            index = position_to_index(pre_coloring[key][0])
            value_dict[graph.nodes[index]['color']] = key
        else:
            missing.append(key)
    for i in range(9):
        for j in range(9):
            if grid[i][j] != 0:
                continue
            index = position_to_index((i, j))
            color = graph.nodes[index]['color']
            if color not in value_dict.keys():
                if len(missing) != 0:
                    value_dict[color] = missing.pop()
                else: # for debugging, should be failing
                    maximum += 1
                    value_dict[color] = maximum
                    value_dict[color] = maximum
            grid[i][j] = value_dict[color]
    return len(value_dict) == 9


def solve_sudoku(grid):
    graph, pre_coloring = sudoku_to_graph(grid)
    color_graph(graph)
    return graph_to_sudoku(graph, pre_coloring, grid)



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
    print(instance)
    print("Aucune solution trouvée.")
execution = default_timer() - start
print("Le temps de résolution est de : ", execution * 1000, " ms")