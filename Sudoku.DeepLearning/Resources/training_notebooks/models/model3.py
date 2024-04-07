from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Input, Conv2D, BatchNormalization, Flatten, Dense, Reshape, Activation, Concatenate

def create_model3(input_shape=(9, 9, 1)):
    model = Sequential()
    model.add(Conv2D(128, 3, activation='relu', padding='same', input_shape=input_shape))
    model.add(BatchNormalization())
    model.add(Conv2D(128, 3, activation='relu', padding='same'))
    model.add(BatchNormalization())
    model.add(Conv2D(256, 3, activation='relu', padding='same'))
    model.add(BatchNormalization())
    model.add(Conv2D(256, 3, activation='relu', padding='same'))
    model.add(BatchNormalization())
    model.add(Conv2D(512, 3, activation='relu', padding='same'))
    model.add(BatchNormalization())
    model.add(Conv2D(512, 3, activation='relu', padding='same'))
    model.add(BatchNormalization())
    model.add(Conv2D(1024, 3, activation='relu', padding='same'))
    model.add(BatchNormalization())
    model.add(Conv2D(9, 1, activation='relu', padding='same'))
    
    # Flatten and dense layers
    model.add(Flatten())
    model.add(Dense(81*9))
    model.add(Reshape((-1, 9)))
    model.add(Activation('softmax'))
    
    return model