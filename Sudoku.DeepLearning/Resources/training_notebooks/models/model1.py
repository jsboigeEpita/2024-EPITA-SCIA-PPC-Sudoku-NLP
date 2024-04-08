
import tensorflow as tf

def create_model1():
    model = tf.keras.Sequential()

    model.add(tf.keras.layers.Conv2D(64, kernel_size=(3,3), activation='relu', padding='same', input_shape=(9,9,1)))
    model.add(tf.keras.layers.BatchNormalization())
    model.add(tf.keras.layers.Conv2D(64, kernel_size=(3,3), activation='relu', padding='same'))
    model.add(tf.keras.layers.BatchNormalization())
    model.add(tf.keras.layers.Conv2D(128, kernel_size=(1,1), activation='relu', padding='same'))

    model.add(tf.keras.layers.Flatten())
    model.add(tf.keras.layers.Dense(81*9))
    model.add(tf.keras.layers.Reshape((-1, 9)))
    model.add(tf.keras.layers.Activation('softmax'))