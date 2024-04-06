import numpy as np

# Définir `instance` uniquement si non déjà défini par PythonNET
if "instance" not in locals():
    instance = np.array(
        [
            [0, 0, 0, 0, 9, 4, 0, 3, 0],
            [0, 0, 0, 5, 1, 0, 0, 0, 7],
            [0, 8, 9, 0, 0, 0, 0, 4, 0],
            [0, 0, 0, 0, 0, 0, 2, 0, 8],
            [0, 6, 0, 2, 0, 1, 0, 5, 0],
            [1, 0, 2, 0, 0, 0, 0, 0, 0],
            [0, 7, 0, 0, 0, 0, 5, 2, 0],
            [9, 0, 0, 0, 6, 5, 0, 0, 0],
            [0, 4, 0, 9, 7, 0, 0, 0, 0],
        ],
        dtype=int,
    )

instance[8, 0] = 9
instance[8, 1] = 9
instance[8, 2] = 9
result = instance  # `result` sera utilisé pour récupérer la grille résolue depuis C#
