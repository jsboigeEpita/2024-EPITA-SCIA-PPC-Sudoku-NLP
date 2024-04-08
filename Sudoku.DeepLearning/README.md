<!-- PROJECT LOGO -->
<br />
<p align="center">
  <a href="https://github.com/rom1trt/DeepLearningGridSolver">
    <img align=top src="Resources/img/logo.jpeg" alt="Logo" width="270" height="270">
  </a>

  <h3 align="center">Deep Learning Grid Solver</h3>

  <p align="center">
    Some Convolutional Neural Networks that can solve sudokus.
    <br />
    <a href="https://github.com/rom1trt/DeepLearningGridSolver"><strong>Explore the docs »</strong></a>
    <br />
    <br />
    <a href="https://github.com/rom1trt/DeepLearningGridSolver">View Demo</a>
    ·
    <a href="https://github.com/rom1trt/DeepLearningGridSolver/issues">Report Bug</a>
    ·
    <a href="https://github.com/rom1trt/DeepLearningGridSolver/issues">Request Feature</a>
  </p>
</p>

<!-- TABLE OF CONTENTS -->
<details open="open">
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
  </ol>
</details>

<!-- ABOUT THE PROJECT -->

## About the Project

Ce projet vise à exploiter les réseaux neuronaux convolutifs (CNN) pour résoudre les puzzles Sudoku. Nous utilisons un ensemble de données provenant de Kaggle, qui fournit 1 et 9 millions d'exemples de puzzles Sudoku à des fins d'entraînement et d'évaluation.

La méthode de résolution présentée dans [Sudoku-Solver](https://github.com/shivaverma/Sudoku-Solver/blob/master/model.py) nous sert de base, et le code Python correspondant est disponible en utilisant TensorFlow. Pour rendre le code compatible avec le benchmark et l'environnement .NET, nous avons lié et référencé notre code python au préalable.

### Opportunités d'amélioration du projet

- **Amélioration des couches d'entrée** : En tenant compte des règles du Sudoku, nous avons proposé d'implémenter des couches d'entrée dérivées pour les lignes, les colonnes et les boîtes. Cela implique d'expérimenter les formes de noyau (3*3, 9*1, et 1\*9), le stride, et les différentes caractéristiques extraites.

- **Exploration de l'architecture du réseau** : Il existe de nombreuses possibilités entre la couche initiale et la couche finale, ce qui offre de nombreuses possibilités d'expérimentation. Les options comprennent l'ajout de couches résiduelles, l'adoption d'architectures U-Net, le nombre et le choix des couches convolutives et des couches denses parmi les couches intermédiaires.

- **Optimisation des hyperparamètres** : Pour rationaliser le processus d'investigation, nous suggérons d'explorer les techniques de Grid/Random Search pour l'optimisation des hyperparamètres. Le réglage des hyperparamètres peut vous aider à éviter l'overfitting et l'underfitting. Cela permet d'équilibrer le compromis entre la complexité et la simplicité de votre modèle.

### Built With

#### Languages & Libraries

- [![Python](https://img.shields.io/badge/python-c2a90f?style=for-the-badge&logo=python&logoColor=white)](https://www.python.org/)
- [![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=.net&logoColor=white)](https://dotnet.microsoft.com/)
- [![Pandas](https://img.shields.io/badge/pandas-150458?style=for-the-badge&logo=pandas&logoColor=white)](https://pandas.pydata.org/)
- [![TensorFlow](https://img.shields.io/badge/TensorFlow-FF6F00?style=for-the-badge&logo=TensorFlow&logoColor=white)](https://www.tensorflow.org/)

#### Datasets

- [9 Millions Sudokus Dataset](https://www.kaggle.com/datasets/rohanrao/sudoku)
- [1 Million Sudokus Dataset](https://www.kaggle.com/datasets/bryanpark/sudoku)

#### References

- [Sudoku Solver using PyTorch](https://github.com/chingisooinar/sudoku-solver.pytorch)
- [Residual Neural Network Wikipedia Article](https://en.wikipedia.org/wiki/Residual_neural_network)
- [Original ResNet Paper](https://arxiv.org/abs/1512.03385)
- [Solving Sudoku with Convolution Neural Network | Keras](https://towardsdatascience.com/solving-sudoku-with-convolution-neural-network-keras-655ba4be3b11)
- [Solve Sudoku with CNN acc 97%](https://www.kaggle.com/code/lyly123/solve-sudoku-with-cnn-acc-97)
- [Solving Sudokus with Neural Networks](https://cs230.stanford.edu/files_winter_2018/projects/6939771.pdf)

<!-- GETTING STARTED -->

## Getting Started

This is a guide on how you may set up your project locally.
To get a local copy up and running follow these simple example steps.

### Installation

1. Clone the repository
   ```sh
   git clone https://github.com/rom1trt/DeepLearningGridSolver
   ```
2. Setup your own home path in the `.env` file
3. Install all the required python packages using either:

```sh
 pip install -r requirements.txt
```

or

```sh
 conda install --file requirements.txt
```

4. Download pre-trained model from Google Drive and move it within `Resources` folder </br>
[Google Drive](https://drive.google.com/file/d/19MVgdm-HiR4RonH-JTMNUXda68-0v6vX/view?usp=sharing)
5. Download appropriate datasets </br>
   [9 Millions Sudokus](https://www.kaggle.com/datasets/rohanrao/sudoku) </br>
   [1 Million Sudokus](https://www.kaggle.com/datasets/bryanpark/sudoku)
7. Indicate the pre-trained model path in `DeepLearning.py` and in notebook if necessary
8. Run `Program.cs`script in `Sudoku.Benchmark` folder
9. Interact with terminal and choose our `DeepLearningSolver` (7th position) among all sudoku solvers.
