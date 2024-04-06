import numpy as np
import tensorflow as tf
import copy
import os

path = os.getcwd()
path = path[:path.rfind('/')]
path += '/Sudoku.DeepLearning/Resources/cnn1.h5'

model = tf.keras.models.load_model(path)

def solve_sudoku_with_nn(model, puzzle):
    # Preprocess the input Sudoku puzzle
    initial_board = np.array(puzzle).reshape((9, 9))
    initial_board = (initial_board / 9) - 0.5

    while True:
        # Use the neural network to predict values for empty cells
        predictions = model.predict(initial_board.reshape((1, 9, 9, 1))).squeeze()
        pred = np.argmax(predictions, axis=1).reshape((9, 9)) + 1
        prob = np.around(np.max(predictions, axis=1).reshape((9, 9)), 2)
        initial_board = ((initial_board + 0.5) * 9).reshape((9, 9))
        mask = (initial_board == 0)
        if mask.sum() == 0:
            # Puzzle is solved
            break
        prob_new = prob * mask
        ind = np.argmax(prob_new)
        x, y = (ind // 9), (ind % 9)
        val = pred[x][y]
        initial_board[x][y] = val
        initial_board = (initial_board / 9) - 0.5
        # print_sudoku_grid(initial_board)

    # Convert the solved puzzle back to a jagged array
    solved_puzzle = initial_board.astype(int).tolist()
    return solved_puzzle

def print_sudoku_grid(puzzle):
    for i in range(9):
        if i % 3 == 0 and i != 0:
            print("-"*21)
        for j in range(9):
            if j % 3 == 0 and j != 0:
                print("|", end=" ")
            print(puzzle[i][j], end=" ")
        print()

# # Create a sample Sudoku puzzle
# instance = [
#     [0, 0, 0, 7, 0, 0, 0, 9, 6],
#     [0, 0, 3, 0, 6, 9, 1, 7, 8],
#     [0, 0, 7, 2, 0, 0, 5, 0, 0],
#     [0, 7, 5, 0, 0, 0, 0, 0, 0],
#     [9, 0, 1, 0, 0, 0, 3, 0, 0],
#     [0, 0, 0, 0, 0, 0, 0, 0, 0],
#     [0, 0, 9, 0, 0, 0, 0, 0, 1],
#     [3, 1, 8, 0, 2, 0, 4, 0, 7],
#     [2, 4, 0, 0, 0, 5, 0, 0, 0]
# ]

# Solve the Sudoku puzzle using the neural network
solved_puzzle_nn = solve_sudoku_with_nn(model, instance)

# Update the 'r' variable with the solved Sudoku puzzle
r = solved_puzzle_nn

# # Print the solved puzzle as a grid
# print("Sudoku Solution (NN):")
# print_sudoku_grid(r)

# def validate_sudoku(puzzle):
#     for i in range(9):
#         row = puzzle[i]
#         col = [puzzle[j][i] for j in range(9)]
#         if len(set(row)) != 9 or len(set(col)) != 9:
#             return False
#     for i in range(0, 9, 3):
#         for j in range(0, 9, 3):
#             block = [puzzle[x][y] for x in range(i, i + 3) for y in range(j, j + 3)]
#             if len(set(block)) != 9:
#                 return False
#     return True

# print("Is the solution valid?", validate_sudoku(r))