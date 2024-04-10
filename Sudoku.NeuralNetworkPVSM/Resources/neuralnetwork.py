import tensorflow as tf
import keras
import tensorflow as tf
from tensorflow.keras.layers import Reshape, Dense, Dropout, Flatten,Activation
from tensorflow.keras.layers import Conv1D, Conv2D, BatchNormalization, LayerNormalization, MaxPooling2D
import numpy as np
import pandas as pd
import os
from urllib import request

path = os.getcwd()
root_path = path[:path.rfind('/')]
model_path = 'model.keras'
sudoku_path = root_path + os.path.normpath('/Sudoku.NeuralNetworkPVSM/Resources/sudoku.csv')

def get_data(file):

    df = pd.read_csv(file)

    data = df 
    X = np.array(df.quizzes.map(lambda x: list(map(int, x))).to_list()).reshape(-1,9,9,1)
    Y = np.array(df.solutions.map(lambda x: list(map(int, x))).to_list()).reshape(-1,9,9)
    X = X / 9
    X -= .5
    Y -= 1

    training_split = 0.8

    splitidx = int(len(data) * training_split)
    # first 80% of data as training set, last 20% of data as testing set
    x_train, x_test = X[:splitidx], X[splitidx:]
    y_train, y_test = Y[:splitidx], Y[splitidx:]

    return x_train, x_test, y_train, y_test

def get_model():
    model = tf.keras.Sequential()

    model.add(Conv2D(512, kernel_size=(3,3), activation='relu', padding='same', input_shape=(9,9,1)))
    model.add(BatchNormalization())
    model.add(Conv2D(512, kernel_size=(3,3), activation='relu', padding='same'))
    model.add(BatchNormalization())
    model.add(Conv2D(512, kernel_size=(3,3), activation='relu', padding='same'))
    model.add(BatchNormalization())
    model.add(Conv2D(512, kernel_size=(3,3), activation='relu', padding='same'))
    model.add(BatchNormalization())
    model.add(Conv2D(512, kernel_size=(3,3), activation='relu', padding='same'))
    model.add(BatchNormalization())
    model.add(Conv2D(512, kernel_size=(3,3), activation='relu', padding='same'))
    model.add(BatchNormalization())
    model.add(Conv2D(512, kernel_size=(3,3), activation='relu', padding='same'))
    model.add(BatchNormalization())
    model.add(Conv2D(512, kernel_size=(3,3), activation='relu', padding='same'))
    model.add(BatchNormalization())
    model.add(Conv2D(512, kernel_size=(3,3), activation='relu', padding='same'))
    model.add(Flatten())
    model.add(Dense(81*9))
    model.add(Dropout(0.1))
    model.add(tf.keras.layers.LayerNormalization(axis=-1))
    model.add(Reshape((9, 9, 9)))
    model.add(Activation('softmax'))
    
    return model

print(tf.__version__)
print(tf.config.list_physical_devices())

# instance is a variable holding a two dimensional integer array representing the Sudoku grid
# use numpy to convert the instance to a numpy array

# initialize a sudoku grid only if not already defined by PythonNET


if 'instance' not in locals():
    instance = np.array([
        [0, 0, 0, 0, 9, 4, 0, 3, 0],
        [0, 0, 0, 5, 1, 0, 0, 0, 7],
        [0, 8, 9, 0, 0, 0, 0, 4, 0],
        [0, 0, 0, 0, 0, 0, 2, 0, 8],
        [0, 6, 0, 2, 0, 1, 0, 5, 0],
        [1, 0, 2, 0, 0, 0, 0, 0, 0],
        [0, 7, 0, 0, 0, 0, 5, 2, 0],
        [9, 0, 0, 0, 6, 5, 0, 0, 0],
        [0, 4, 0, 9, 7, 0, 0, 0, 0]
    ], dtype=int)
np_instance = np.array(instance)
print("Instance received:")
print(np_instance)


# reseau de neurone
def norm(a):
    return (a / 9) - .5


def denorm(a):
    return (a + .5) * 9


model = get_model()
train = False

if train:
    x_train, x_test, y_train, y_test = get_data(sudoku_path) # https://www.kaggle.com/datasets/bryanpark/sudoku
    optimizer = tf.keras.optimizers.Adam(0.001)
    model.compile(loss='sparse_categorical_crossentropy', optimizer=optimizer, metrics=['accuracy'])
    history = model.fit(x_train, y_train, batch_size=64, epochs=5, validation_data=(x_test, y_test))
    training_accuracy11 = history.history['accuracy'][-1]
    training_loss11 = history.history['loss'][-1]
    print(f"Training Loss: {training_loss11:.4f}, Training Accuracy: {training_accuracy11:.4f}")
    val_loss11, val_accuracy11 = model.evaluate(x_test,y_test)
    print(f"Done!\nValiation Loss: {val_loss11:.4f}, Validation Accuracy: {val_accuracy11:.4f}")
    model.save(model_path)
else:
    try:
        model = keras.models.load_model(model_path)
    except Exception as e:
        print("Model not found! Downloading pre-computed weights (500 MB)...")
        remote_url = "https://www.pilou.org/PPC/model_rtx_final.keras"
        request.urlretrieve(remote_url, model_path)
        print("Model weights downloaded. Loading..")
        model = keras.models.load_model(model_path)
def display(board):
    for i in range(9):
        if i in [3, 6]:
            print('------+-------+------')
        for j in range(9):
            if j in [3, 6]:
                print('| ', end='')
            print(str(board[i][j]) + ' ', end='')  # Convert integer to string before concatenating
        print()


def solve_itrative(model, grid):
    #Ici on vas prendre la case avec la plus grande probabilité de la solution prédite
    grid = grid.copy()

    while True:
        predict_solution = model.predict(grid)
        max_prob = 0

        # Get the index of the cell with the highest probability and is empty
        for i in range(9):
            for j in range(9):
                if grid[0, i, j] == -0.5:
                    if np.max(predict_solution[0, i, j]) > max_prob:
                        max_prob = np.max(predict_solution[0, i, j])
                        max_i = i
                        max_j = j

        # Set the value of the cell with the highest probability
        grid[0, max_i, max_j] = np.argmax(predict_solution[0, max_i, max_j]) + 1
        grid[0, max_i, max_j] = (grid[0, max_i, max_j]) / 9
        grid[0, max_i, max_j] -= 0.5

        if -0.5 not in grid:
            break
    return grid

def predict_sudoku(model, sudoku):
    #Normalize the sudoku
    sudoku = np.array(sudoku).reshape(1, 9, 9, 1) / 9 - 0.5
    predicted_sudoku = solve_itrative(model, sudoku)
    # Denormalize the predicted sudoku
    predicted_sudoku = (predicted_sudoku + 0.5) * 9
    #Reshape to a 9x9 matrix
    predicted_sudoku = predicted_sudoku.reshape(9, 9)
    #Set to int values
    predicted_sudoku = predicted_sudoku.astype(int)
    return predicted_sudoku


game = predict_sudoku(model, np_instance)
display(np_instance)
print("Sudoku solved:")
display(game)

result = np.array(list(map(int, game.flatten()))).reshape((9, 9))
