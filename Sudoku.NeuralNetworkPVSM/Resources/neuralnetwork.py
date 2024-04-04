import copy
import tensorflow as tf
import keras
from keras.layers import Conv2D, BatchNormalization, MaxPooling2D, Flatten, Dense, Reshape, Activation, Dropout
from keras.callbacks import ReduceLROnPlateau, EarlyStopping
import numpy as np
import pandas as pd
from sklearn.model_selection import train_test_split

def get_data(file):

    data = pd.read_csv(file)

    feat_raw = data['quizzes']
    label_raw = data['solutions']

    feat = []
    label = []

    for i in feat_raw:

        x = np.array([int(j) for j in i]).reshape((9,9,1))
        feat.append(x)

    feat = np.array(feat)
    feat = feat/9
    feat -= .5

    for i in label_raw:

        x = np.array([int(j) for j in i]).reshape((81,1)) - 1
        label.append(x)

    label = np.array(label)

    del(feat_raw)
    del(label_raw)

    x_train, x_test, y_train, y_test = train_test_split(feat, label, test_size=0.2, random_state=42)

    return x_train, x_test, y_train, y_test

def get_model():
    model = keras.models.Sequential()

    model.add(Conv2D(32, kernel_size=(3,3), activation='relu', padding='same', input_shape=(9,9,1)))
    model.add(BatchNormalization())
    model.add(Conv2D(32, kernel_size=(3,3), activation='relu', padding='same'))
    model.add(BatchNormalization())
    model.add(MaxPooling2D(pool_size=(2,2)))

    model.add(Conv2D(64, kernel_size=(3,3), activation='relu', padding='same'))
    model.add(BatchNormalization())
    model.add(Conv2D(64, kernel_size=(3,3), activation='relu', padding='same'))
    model.add(BatchNormalization())
    model.add(MaxPooling2D(pool_size=(2,2)))

    model.add(Flatten())
    model.add(Dense(128, activation='relu'))
    model.add(Dense(81*9))
    model.add(Reshape((-1, 9)))
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


def inference_sudoku(sample):
    '''
        This function solve the sudoku by filling blank positions one by one.
    '''
    feat = copy.copy(sample)

    while (1):

        out = model.predict(feat.reshape((1, 9, 9, 1)))
        out = out.squeeze()

        pred = np.argmax(out, axis=1).reshape((9, 9)) + 1
        prob = np.around(np.max(out, axis=1).reshape((9, 9)), 2)

        feat = denorm(feat).reshape((9, 9))
        mask = (feat == 0)

        if (mask.sum() == 0):
            break

        prob_new = prob * mask

        ind = np.argmax(prob_new)
        x, y = (ind // 9), (ind % 9)

        val = pred[x][y]
        feat[x][y] = val
        feat = norm(feat)

    return pred


def test_accuracy(feats, labels):
    correct = 0

    for i, feat in enumerate(feats):

        pred = inference_sudoku(feat)

        true = labels[i].reshape((9, 9)) + 1

        if (abs(true - pred).sum() == 0):
            correct += 1

    return(correct / feats.shape[0])


def solve_sudoku(game):
    game = game.replace('\n', '')
    game = game.replace(' ', '')
    game = np.array([int(j) for j in game]).reshape((9, 9, 1))
    game = norm(game)
    game = inference_sudoku(game)
    return game

model = get_model()
train = True

if train:
    x_train, x_test, y_train, y_test = get_data(r'..\..\..\..\Sudoku.NeuralNetworkPVSM\Resources\sudoku.csv')
    rmsprop = keras.optimizers.RMSprop(learning_rate=0.01)
    model.compile(loss='sparse_categorical_crossentropy', optimizer=rmsprop)

    reduce_lr = ReduceLROnPlateau(monitor='loss', factor=0.1, patience=5, min_lr=0.00001)
    early_stop = EarlyStopping(monitor='val_loss', patience=5, restore_best_weights=True)
    model.fit(x_train, y_train, batch_size=128, epochs=20, callbacks=[reduce_lr, early_stop])
    model.save(r'..\..\..\..\Sudoku.NeuralNetworkPVSM\Resources\model.keras')
    accuracy = test_accuracy(x_test[:100], y_test[:100])
    print("Accuracy:", accuracy)
else:
    try:
        model = keras.models.load_model(r'..\..\..\..\Sudoku.NeuralNetworkPVSM\Resources\model.keras')
    except Exception as e:
        print("Model not found!")
        print("Please set train=True to train the model before inference.")
        exit()

instance_str = np_instance.flatten()
instance_str = ''.join(map(str, instance_str))
instance_str = instance_str.replace('\n', '')
instance_str = instance_str.replace(' ', '')
game = solve_sudoku(instance_str)
print("Sudoku solved:")
print(game)

result = np.array(list(map(int, game.flatten()))).reshape((9, 9))
