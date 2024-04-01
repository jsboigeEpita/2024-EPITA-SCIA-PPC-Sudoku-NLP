import keras
from keras.layers import Conv2D, BatchNormalization, MaxPooling2D, Flatten, Dense, Reshape, Activation

def get_model():
    model = keras.models.Sequential()

    # Augmentation de la profondeur des couches de convolution
    model.add(Conv2D(64, kernel_size=(3,3), activation='relu', padding='same', input_shape=(9,9,1)))
    model.add(BatchNormalization())
    model.add(Conv2D(64, kernel_size=(3,3), activation='relu', padding='same'))
    model.add(BatchNormalization())
    model.add(MaxPooling2D(pool_size=(2,2)))

    model.add(Conv2D(128, kernel_size=(3,3), activation='relu', padding='same'))
    model.add(BatchNormalization())
    model.add(Conv2D(128, kernel_size=(3,3), activation='relu', padding='same'))
    model.add(BatchNormalization())
    model.add(MaxPooling2D(pool_size=(2,2)))

    model.add(Conv2D(256, kernel_size=(3,3), activation='relu', padding='same'))
    model.add(BatchNormalization())

    # Ajout de couches de max pooling
    # Normalisation par lots

    # Flatten() et Dense() pour classifier
    model.add(Flatten())
    model.add(Dense(512, activation='relu')) # Augmentation de la taille de la couche dense
    model.add(Dense(81*9))
    model.add(Reshape((-1, 9)))
    model.add(Activation('softmax')) # Activation softmax

    return model
