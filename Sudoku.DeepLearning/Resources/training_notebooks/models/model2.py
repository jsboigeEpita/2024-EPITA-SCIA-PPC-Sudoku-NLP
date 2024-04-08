import tensorflow as tf
from tensorflow.keras.layers import Input, Conv2D, BatchNormalization, Flatten, Dense, Reshape, Activation, Concatenate

def create_model2(input_shape=(9, 9, 1)):
    input_layer = Input(shape=input_shape)

    # Convolutional layers for rows
    row_conv = Conv2D(64, kernel_size=(1, 9), activation='relu', padding='valid')(input_layer)
    row_conv = BatchNormalization()(row_conv)
    row_conv = Reshape((9, 64, 1))(row_conv)

    # Convolutional layers for columns
    col_conv = Conv2D(64, kernel_size=(9, 1), activation='relu', padding='valid')(input_layer)
    col_conv = BatchNormalization()(col_conv)
    col_conv = Reshape((9, 64, 1))(col_conv)

    # Convolutional layers for boxes
    box_conv = Conv2D(64, kernel_size=(3, 3), activation='relu', padding='valid', strides=(3, 3))(input_layer)
    box_conv = BatchNormalization()(box_conv)
    box_conv = Flatten()(box_conv)
    box_conv = Dense(9 * 64)(box_conv)
    box_conv = Reshape((9, 64, 1))(box_conv)

    # Concatenate the parallel convolutional layers
    merged = Concatenate(axis=-1)([row_conv, col_conv, box_conv])

    # Additional convolutional layers
    conv = Conv2D(128, kernel_size=(1, 1), activation='relu', padding='same')(merged)
    conv = BatchNormalization()(conv)

    # Flatten and dense layers
    flattened = Flatten()(conv)
    dense = Dense(81*9)(flattened)
    reshaped = Reshape((-1, 9))(dense)
    output_layer = Activation('softmax')(reshaped)

    model = tf.keras.Model(inputs=input_layer, outputs=output_layer)
    return model