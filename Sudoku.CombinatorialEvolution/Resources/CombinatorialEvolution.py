import numpy as np
import random


def compute_error(sudoku: np.ndarray) -> int:
    """
    Determine the number of errors in a sudoku grid depending on the missing/duplicate values in row and columns.
    """

    nb_error = 0
    rows = [set() for _ in range(9)]
    cols = [set() for _ in range(9)]

    for i in range(9):
        for j in range(9):
            val = sudoku[i * 9 + j]
            nb_error += (val in rows[i]) + (val in cols[j])
            rows[i].add(val)
            cols[j].add(val)

    return nb_error


def random_matrix(sudoku: np.ndarray) -> np.ndarray:
    """
    Create a matrix by filling empty cells, filled so each block of the sudoku contains random unique values from 1 to 9.
    """

    result = sudoku.copy()
    for block in range(9):
        L = np.arange(1, 10)
        np.random.shuffle(L)
        # index of the left corner of the block
        start_index = 27 * (block // 3) + 3 * (block % 3)
        for i in range(3):
            for y in range(3):
                index = start_index + y + i * 9
                if result[index] != 0:
                    L = L[L != result[index]]
        for i in range(3):
            if len(L) == 0:
                break
            for y in range(3):
                index = start_index + y + i * 9
                if result[index] == 0 and len(L) != 0:
                    result[index] = L[0]
                    L = L[1:]
    return result


class Organism:
    def __init__(
        self, sudoku: np.ndarray, pre_filled_indexes: np.ndarray, err_rate: float
    ):
        s = random_matrix(sudoku)
        self.sudoku = s
        self.err_rate = err_rate
        self.age = 0
        self.pre_filled_indexes = pre_filled_indexes
        self.score = compute_error(s)

    def evolve(self):
        """
        Make the Organism evolve if the computed neighbor have a lower score (is better) than the current score, or with a probability <= err_rate
        """
        neighbor = self.neighbor_sudoku()
        neighbor_score = compute_error(neighbor)
        if neighbor_score < self.score or np.random.uniform() <= self.err_rate:
            self.sudoku = neighbor
            self.score = neighbor_score
            self.age = 0
        else:
            self.age += 1

    def neighbor_sudoku(self) -> np.ndarray:
        """
        Because of the 3x3 sub-grid invariant, a neighbor solution must confine itself to being a permutation of a sub-grid.
        Put more concretely, to determine a neighbor matrix, the algorithm selects a block at random,
        then selects two cells in the block (where neither cell contains a fixed value from the problem definition),
        and exchanges the values in the two cells.
        """

        result = np.copy(self.sudoku)
        block = random.randint(0, 8)

        row = (block // 3) * 3
        col = (block % 3) * 3
        cells_available = [
            row * 9 + col + i * 9 + j for i in range(3) for j in range(3)
        ]
        cells_available = [
            cell for cell in cells_available if cell not in self.pre_filled_indexes
        ]

        if len(cells_available) < 2:
            return result

        cell1, cell2 = random.sample(cells_available, 2)
        result[cell1], result[cell2] = result[cell2], result[cell1]

        return result


class Solver:
    def __init__(
        self,
        sudoku: np.ndarray,
        nb_organisms: int,
        max_epochs: int,
        max_restarts: int,
        err_rate: float,
        worker_rate: float,
    ):
        self.sudoku = sudoku
        self.nb_organisms = nb_organisms
        self.max_epochs = max_epochs
        self.max_restarts = max_restarts
        self.err_rate = err_rate
        self.pre_filled_indexes = np.where(sudoku != 0)[0]
        self.worker_rate = worker_rate

    def merge_matrices(self, m1, m2):
        """
        Creates a new matrix by merging m1 and m2. Squares are randomly swapped between m1 and m2 with a probability of 0.5
        """

        result = np.copy(m1)
        for block in range(9):
            pr = random.uniform(0, 1)
            if pr < 0.50:
                row_start = (block // 3) * 3
                row_end = row_start + 3

                col_start = (block % 3) * 3
                col_end = col_start + 3

                # Replace values in block M1 by those in M2
                for row in range(row_start, row_end):
                    for col in range(col_start, col_end):
                        index = row * 9 + col
                        result[index] = m2[index]
        return result

    def solve_evo(self):
        """
        Make the organisms evolve to find the solution, implementing the combinatorial evolution algorithm
        """
        nb_worker = int(self.worker_rate * self.nb_organisms)
        nb_explorer = self.nb_organisms - nb_worker

        workers = [
            Organism(self.sudoku, self.pre_filled_indexes, self.err_rate)
            for _ in range(nb_worker)
        ]
        explorers = [
            Organism(self.sudoku, self.pre_filled_indexes, self.err_rate)
            for _ in range(nb_explorer)
        ]

        epoch = 0
        while epoch < self.max_epochs:
            for worker in workers:
                worker.evolve()
                if worker.age > 300:
                    worker.sudoku = random_matrix(self.sudoku)
                    worker.age = 0
            for explorer in explorers:
                explorer.sudoku = random_matrix(self.sudoku)

            best_w_index, best_w_score = 0, 81
            best_e_index, best_e_score = 0, 81
            scores = []

            for i in range(nb_worker):
                score = compute_error(workers[i].sudoku)
                if score == 0:
                    return workers[i].sudoku
                if score < best_w_score:
                    best_w_index = i
                    best_w_score = score
                scores.append(score)

            for i in range(nb_explorer):
                score = compute_error(explorers[i].sudoku)
                if score == 0:
                    return explorers[i].sudoku
                if score < best_e_score:
                    best_e_index = i
                    best_e_score = score

            worstIndexes = np.argpartition(np.array(scores), -20)[-20:]
            for index in worstIndexes:
                workers[index].sudoku = self.merge_matrices(
                    workers[best_w_index].sudoku, explorers[best_e_index].sudoku
                )
                if compute_error(workers[index].sudoku) == 0:
                    return workers[index].sudoku

            epoch += 1

        return np.array([])

    def solve(self):
        while True:
            res = self.solve_evo()
            # if solution is found
            if len(res) != 0:
                return res


np.random.seed()
solver = Solver(
    np.array(instance).flatten(),
    nb_organisms=200,
    max_epochs=3000,
    max_restarts=20,
    err_rate=0.009,
    worker_rate=0.90,
)
result = solver.solve()
r = result.astype("int").reshape((9, 9)).tolist()
