import tensorflow as tf
import keras
import tensorflow as tf
from tensorflow.keras.layers import Reshape, Dense, Dropout, Flatten,Activation
from tensorflow.keras.layers import Conv1D, Conv2D, BatchNormalization, LayerNormalization, MaxPooling2D
import numpy as np
import pandas as pd
from urllib import request
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
    x_train, x_test, y_train, y_test = get_data(r'..\..\..\..\Sudoku.NeuralNetworkPVSM\Resources\sudoku.csv')
    optimizer = tf.keras.optimizers.Adam(0.001)
    model.compile(loss='sparse_categorical_crossentropy', optimizer=optimizer, metrics=['accuracy'])
    history = model.fit(x_train, y_train, batch_size=64, epochs=5, validation_data=(x_test, y_test))
    training_accuracy11 = history.history['accuracy'][-1]
    training_loss11 = history.history['loss'][-1]
    print(f"Training Loss: {training_loss11:.4f}, Training Accuracy: {training_accuracy11:.4f}")
    val_loss11, val_accuracy11 = model.evaluate(x_test,y_test)
    print(f"Done!\nValiation Loss: {val_loss11:.4f}, Validation Accuracy: {val_accuracy11:.4f}")
    model.save(r'..\..\..\..\Sudoku.NeuralNetworkPVSM\Resources\model.keras')
else:
    try:
        model = keras.models.load_model(r'..\..\..\..\Sudoku.NeuralNetworkPVSM\Resources\model.keras')
    except Exception as e:
        print("Model not found! Downloading pre-computed weights..")
        remote_url = "https://www.pilou.org/PPC/model_rtx.keras"
        local_path = r'..\..\..\..\Sudoku.NeuralNetworkPVSM\Resources\model.keras'
        request.urlretrieve(remote_url, local_path)
        model = keras.models.load_model(local_path)
def display(board):
    for i in range(9):
        if i in [3, 6]:
            print('------+-------+------')
        for j in range(9):
            if j in [3, 6]:
                print('| ', end='')
            print(str(board[i][j]) + ' ', end='')  # Convert integer to string before concatenating
        print()


def predict_sudoku(model, sudoku):
    # Prétraiter le Sudoku pour le rendre compatible avec le modèle
    sudoku_input = np.array(sudoku).reshape(1, 9, 9, 1) / 9 - 0.5
    # Faire la prédiction
    predicted_solution = model.predict(sudoku_input)
    # Récupérer la solution prédite et la reformater
    predicted_solution = np.argmax(predicted_solution, axis=-1).reshape(9, 9) + 1
    return predicted_solution

game = predict_sudoku(model, np_instance)
display(np_instance)
print("Sudoku solved:")
display(game)

result = np.array(list(map(int, game.flatten()))).reshape((9, 9))
