import random
import numpy as np

def blocks_index():
    blocks = []
    for row in range(9):
        row_start = 3 * (row % 3)
        column_start = 3 * (row // 3)
        block = [[x, y] for x in range(row_start, row_start + 3) for y in range(column_start, column_start + 3)]
        blocks.append(block)
    return np.array(blocks)

class Solver:
    def __init__(self, grid: np.ndarray, cooling_rate: float):
        self.basic_grid = grid
        self.index = blocks_index()
        self.grid = self.random_matrix(grid)
        self.grid_score = self.compute_error(self.grid)

        self.pre_filled_indexes = [tuple(l) for l in np.argwhere(grid != 0)]
        self.temperature = self.init_temperature()
        self.limit = np.count_nonzero(grid == 0)

        self.cooling_rate = cooling_rate

    def compute_error(self, matrix: np.ndarray):
        '''
            Compute errors by rows and columns
        '''
        nb_error = 0
        rows = [set() for _ in range(9)]
        cols = [set() for _ in range(9)]
        for i in range(9):
            for j in range(9):
                val = matrix[i][j]
                nb_error += (val in rows[i]) + (val in cols[j])
                rows[i].add(val)
                cols[j].add(val)
        return nb_error
    
    def random_matrix(self, sudoku: np.ndarray) -> np.ndarray :
        result = sudoku.copy()
        for blocks in self.index:
            L = np.arange(1,10)
            np.random.shuffle(L)
            for block in blocks:
                if (sudoku[block[0]][block[1]] != 0):
                    L = L[L != sudoku[block[0]][block[1]]]
            
            for block in blocks:
                if (len(L) != 0 and result[block[0]][block[1]] == 0):
                    result[block[0]][block[1]] = L[0]
                    L = L[1:]
        return result
    
    def neighbor_sudoku(self) -> np.ndarray:
        '''
            Because of the 3x3 sub-grid invariant, a neighbor solution must confine itself to being a permutation of a sub-grid.
            Put more concretely, to determine a neighbor matrix, the algorithm selects a block at random,
            then selects two cells in the block (where neither cell contains a fixed value from the problem definition),
            and exchanges the values in the two cells.
        '''

        result = np.copy(self.grid)
        block = random.randint(0, 8)
        cells_available = []
        for b in self.index[block]:
            if (b[0],b[1]) not in self.pre_filled_indexes:
                cells_available.append(b)
        if len(cells_available) < 2 :
            return result
        

        cell1, cell2 = random.sample(cells_available, 2)
        result[cell1[0], cell1[1]],  result[cell2[0], cell2[1]] = result[cell2[0], cell2[1]], result[cell1[0], cell1[1]]

        return result
        
    def init_temperature(self):
        sudoku_list = []
        for _ in range(10):
            neighbor = self.neighbor_sudoku()
            neighbor_score = self.compute_error(neighbor)
            sudoku_list.append(neighbor_score)
            self.grid = neighbor
            self.grid_score = neighbor_score
        return np.std(sudoku_list)

    def solve(self) -> np.ndarray:
        reheats = 0
        i = 0
        
        while i < 1000000:
            if self.compute_error(self.grid) == 0:
                return self.grid, True
            previousScore = self.grid_score
            for _ in range(self.limit):
                neighbor = self.neighbor_sudoku()
                neighbor_score = self.compute_error(neighbor)
                p = np.exp((self.grid_score - neighbor_score) / self.temperature)
                if np.random.uniform(1,0,1) < p:
                    self.grid = neighbor
                    self.grid_score += (neighbor_score - self.grid_score)
                if self.grid_score <= 0:
                    return self.grid, True
            
            if self.grid_score >= previousScore:
                reheats += 1
            else:
                reheats = 0

            self.temperature *= self.cooling_rate
            if reheats > 80:
                self.temperature += 2
                reheats = 0
            i += 1

        return self.grid, False

solver = Solver(np.array(instance).flatten(), 0.99)
result, solved = solver.solve()
r = result.astype("int").reshape((9, 9)).tolist()