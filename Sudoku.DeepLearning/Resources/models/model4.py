from tensorflow.keras.models import Model
from tensorflow.keras.layers import Input, Conv2D, BatchNormalization, Flatten, Dense, Reshape, Activation, Add

def create_model4(input_shape=(9, 9, 1)):
    inputs = Input(shape=input_shape)

    x = Conv2D(128, 3, activation='relu', padding='same')(inputs)
    x = BatchNormalization()(x)
    residual = x
    x = Conv2D(128, 3, activation='relu', padding='same')(x)
    x = BatchNormalization()(x)
    x = Add()([x, residual])

    x = Conv2D(256, 3, activation='relu', padding='same')(x)
    x = BatchNormalization()(x)
    residual = x
    x = Conv2D(256, 3, activation='relu', padding='same')(x)
    x = BatchNormalization()(x)
    x = Add()([x, residual])

    x = Conv2D(512, 3, activation='relu', padding='same')(x)
    x = BatchNormalization()(x)
    residual = x
    x = Conv2D(512, 3, activation='relu', padding='same')(x)
    x = BatchNormalization()(x)
    x = Add()([x, residual])

    x = Conv2D(1024, 3, activation='relu', padding='same')(x)
    x = BatchNormalization()(x)
    x = Conv2D(9, 1, activation='relu', padding='same')(x)

    # Flatten and dense layers
    x = Flatten()(x)
    x = Dense(81*9)(x)
    x = Reshape((-1, 9))(x)
    outputs = Activation('softmax')(x)

    model = Model(inputs=inputs, outputs=outputs)
    return model