import numpy as np
import tensorflow as tf
import os

# Get the path to the trained model
path = os.getcwd()
path = path[:path.rfind('/')]
path += '/Sudoku.DeepLearning/Resources/cnn_2024_04_08.keras'

# Load the trained model
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

    # Convert the solved puzzle back to a jagged array
    solved_puzzle = initial_board.astype(int).tolist()

    return solved_puzzle

# Solve the Sudoku puzzle using the neural network
solved_puzzle_nn = solve_sudoku_with_nn(model, instance)

# Update the 'r' variable with the solved Sudoku puzzle
r = solved_puzzle_nn