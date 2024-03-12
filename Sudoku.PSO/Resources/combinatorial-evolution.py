import numpy as np
import random
import time

sudoku_test = [    
    0, 0, 0, 0, 0, 0, 0, 0, 0,
    9, 6, 7, 3, 4, 5, 8, 2, 1,
    2, 5, 1, 8, 7, 6, 4, 9, 3,
    5, 4, 8, 1, 3, 2, 9, 7, 6,
    7, 2, 9, 5, 6, 4, 1, 3, 8,
    1, 3, 6, 7, 9, 8, 2, 4, 5,
    3, 7, 2, 6, 8, 9, 5, 1, 4,
    8, 1, 4, 2, 5, 3, 7, 6, 9,
    6, 9, 5, 4, 1, 7, 3, 8, 2
]

sudoku_moyen = [
    0, 0, 3, 0, 2, 0, 6, 0, 0,
    9, 0, 0, 3, 0, 5, 0, 0, 1,
    0, 0, 1, 8, 0, 6, 4, 0, 0,
    0, 0, 8, 1, 0, 2, 9, 0, 0,
    7, 0, 0, 0, 0, 0, 0, 0, 8,
    0, 0, 6, 7, 0, 8, 2, 0, 0,
    0, 0, 2, 6, 0, 9, 5, 0, 0,
    8, 0, 0, 2, 0, 3, 0, 0, 9,
    0, 0, 5, 0, 1, 0, 3, 0, 0
]

sudoku_diabolique = [
    0, 0, 0, 6, 0, 0, 0, 0, 5,
    0, 7, 0, 0, 0, 8, 2, 3, 0,
    0, 0, 0, 0, 5, 0, 0, 9, 6,
    6, 3, 0, 5, 0, 0, 0, 8, 0,
    0, 9, 8, 0, 4, 0, 0, 0, 7,
    0, 0, 2, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0, 7, 0, 0, 4, 0,
    0, 6, 0, 0, 0, 0, 7, 0, 8,
    4, 2, 0, 0, 0, 0, 0, 0, 9
]

def compute_error(sudoku: np.ndarray) -> int:
    '''
        Determine the number of errors in a sudoku grid depending on the missing/duplicate values in row and columns.
    '''

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

def random_matrix(sudoku: np.ndarray) -> np.ndarray :
    '''
        Initializes the matrix field of an Organism object to a random possible solution.
    '''

    result = sudoku.copy()
    for block in range(9):
        L = np.arange(1,10)
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
    def __init__(self, sudoku: np.ndarray, pre_filled_indexes: np.ndarray, err_rate: float):
        s = random_matrix(sudoku)
        self.sudoku = s
        self.err_rate = err_rate
        self.age = 0
        self.pre_filled_indexes = pre_filled_indexes
        self.score = compute_error(s)
        

    def evolve(self):
        neighbor = self.neighbor_sudoku()
        neighbor_score = compute_error(neighbor)
        if neighbor_score < self.score or np.random.uniform() <= self.err_rate:
            self.sudoku = neighbor
            self.score = neighbor_score
            self.age = 0
        else:
            self.age += 1

    def neighbor_sudoku(self) -> np.ndarray:
        '''
            Because of the 3x3 sub-grid invariant, a neighbor solution must confine itself to being a permutation of a sub-grid.
            Put more concretely, to determine a neighbor matrix, the algorithm selects a block at random,
            then selects two cells in the block (where neither cell contains a fixed value from the problem definition),
            and exchanges the values in the two cells.
        '''

        result = np.copy(self.sudoku)
        block = random.randint(0,8)
        
        row = (block // 3) * 3
        col = (block % 3) * 3
        cells_available = [row * 9 + col + i*9+j for i in range(3) for j in range(3)]
        cells_available = [cell for cell in cells_available if cell not in self.pre_filled_indexes]

        if len(cells_available) < 2 :
            return result

        cell1, cell2 = random.sample(cells_available, 2)
        result[cell1],  result[cell2] = result[cell2],  result[cell1]

        return result
        

class Solver:
    def __init__(self, sudoku: np.ndarray, nb_organisms: int, max_epochs: int, max_restarts: int, err_rate: float, worker_rate: float):
        self.sudoku = sudoku
        self.nb_organisms = nb_organisms
        self.max_epochs = max_epochs
        self.max_restarts = max_restarts
        self.err_rate = err_rate
        self.pre_filled_indexes = np.where(sudoku != 0)[0]
        self.worker_rate = worker_rate
    
    def merge_matrices(self, m1, m2):
        '''
            Method MergeMatrices accepts two 9x9 matrices from two Organism objects. 
            The method scans through blocks 0 to 8. For each block, a random value between 0.0 and 1.0 is generated.
            If the random value is less than 0.50 (that is, about half the time), then the values in the two blocks are exchanged.
        '''

        result = np.copy(m1)
        for block in range(9):
            pr = random.uniform(0, 1)
            if pr < 0.50 :
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
        nb_worker = int(self.worker_rate * self.nb_organisms)
        nb_explorer = self.nb_organisms - nb_worker

        workers = [Organism(self.sudoku, self.pre_filled_indexes, self.err_rate) for _ in range(nb_worker)]
        explorers = [Organism(self.sudoku, self.pre_filled_indexes, self.err_rate) for _ in range(nb_explorer)]

        epoch = 0
        while epoch < self.max_epochs:
            for worker in workers:
                worker.evolve()
                if worker.age > 500:
                    worker.sudoku = random_matrix(self.sudoku)
                    worker.age = 0
            for explorer in explorers:
                explorer.sudoku = random_matrix(self.sudoku)

            best_w_index, best_w_score = 0, 81
            best_e_index, best_e_score = 0, 81
            worst_w_index, worst_w_score = 0, 0
            
            for i in range(nb_worker):
                score = compute_error(workers[i].sudoku)
                if score == 0:
                    return workers[i].sudoku 
                if score < best_w_score:
                   best_w_index = i
                   best_w_score = score
                elif score > worst_w_score:
                   worst_w_index = i
                   worst_w_score = score

            for i in range(nb_explorer):
                score = compute_error(explorers[i].sudoku)
                if score == 0:
                    return explorers[i].sudoku 
                if score < best_e_score:
                    best_e_index = i
                    best_e_score = score

            workers[worst_w_index].sudoku = self.merge_matrices(workers[best_w_index].sudoku, explorers[best_e_index].sudoku)
            if compute_error(workers[worst_w_index].sudoku) == 0:
                return workers[worst_w_index].sudoku

            epoch += 1
            
        return np.array([])
    
    def solve(self):
        restart = 0
        while restart < self.max_restarts:
            res = self.solve_evo()
            if len(res) != 0:
                return res
            restart += 1
            print("Restarting...")
            
        return np.array([])

if __name__ == '__main__':
    random.seed()

    solver = Solver(np.array(sudoku_moyen), nb_organisms=200, max_epochs=4000, max_restarts=20, err_rate=0.001, worker_rate=0.90)

    start = time.perf_counter()
    result = solver.solve()
    end = time.perf_counter()

    if len(result) != 0:
        print(f"Solution found in {end-start} seconds !")
        np.savetxt("result", result.reshape((9, 9)), fmt='%d', delimiter='\t')
    else:
        print("No solution found...")