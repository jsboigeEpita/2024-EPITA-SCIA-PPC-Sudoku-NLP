import numpy as np
import random as rd

'''
sudoku_solution = [
    4, 8, 3, 9, 2, 1, 6, 5, 7,
    9, 6, 7, 3, 4, 5, 8, 2, 1,
    2, 5, 1, 8, 7, 6, 4, 9, 3,
    5, 4, 8, 1, 3, 2, 9, 7, 6,
    7, 2, 9, 5, 6, 4, 1, 3, 8,
    1, 3, 6, 7, 9, 8, 2, 4, 5,
    3, 7, 2, 6, 8, 9, 5, 1, 4,
    8, 1, 4, 2, 5, 3, 7, 6, 9,
    6, 9, 5, 4, 1, 7, 3, 8, 2
]
'''

sudoku_og = [
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

class Particle:
    def __init__(self, grid: np.ndarray, velocity: np.ndarray):
        self.grid = grid
        self.best = grid
        self.velocity = velocity

class Swarm:
    def __init__(self, particles: list[Particle], best: np.ndarray):
        self.particles = particles
        self.best = best

class Solver:
    def __init__(self, toSolve: np.ndarray, inertiaWeight: float, rhoP: float, rhoG: float, limit: int, nbParticles: int):
        self.toSolve = toSolve
        self.inertiaWeight = inertiaWeight
        self.rhoP = rhoP
        self.rhoG = rhoG
        self.limit = limit
        self.nbParticles = nbParticles
        self.preFilledIndexes = np.nonzero(toSolve)
        self.swarm = None
    
    # Init the swarm attribute of the solver
    #
    # The swarm is set with nbParticles particles, which contains the preFilledValues, and are filled with random values from 1 to 9
    def initSwarm(self):
        best = self.toSolve
        bestScore = self.evaluateGrid(best)
        particles = np.array([])

        for _ in range(self.nbParticles):
            grid = np.where(grid == 0, rd.randint(1, 9), grid)
            velocity = np.random.randint(17, size=81) - 8 # or np.zeros(81)
            np.append(particles, Particle(grid, velocity))
            
            gridScore = self.evaluateGrid(grid)
            if gridScore > bestScore:
                bestScore = gridScore
                best = grid

        self.swarm = Swarm(particles, best)

    def solve(self):
        count = 0
        while count < self.limit and not self.isSolution():
            for particle in self.swarm.particles:
                r = np.random.uniform(size=2)
                particle.velocity = np.add(np.add(particle.velocity * self.inertiaWeight, self.rhoP * r[0] * (np.subtract(particle.best, particle.grid))), self.rhoG * r[1] * (np.subtract(self.swarm.best, particle.grid))).astype('i')
                particle.grid = np.add(particle.grid, particle.velocity)

                # check with evaluteGrid
            count += 1

    def evaluateGrid(grid: np.ndarray) -> int:
        pass

    def isSolution(self) -> bool:
        '''
            Check if the best grid of the swarm is the solution
        '''
        grid = self.swarm.best # voir si il est bon
        return self.checkLine(grid, -1) and self.checkColumn(grid, -1) and self.checkSquare(grid)


    def checkLine(self, grid: np.ndarray, index = -1):
        '''
            grid (np.ndarray) : Array of the sudoku
            index (int) : if between 0-81 -> specific row containing index is checked 
                          else -> all rows are checked
        '''

        # Check a specific row containing the index
        if index >= 0:
            row = index // 9
            start = row * 9
            end = start + 9
            values = grid[start:end]
            return len(set(values)) == len(values)
                

        # Check all rows
        else:
            for row in range(9):
                start = row * 9
                end = start + 9
                values = grid[start:end]
                if len(set(values)) != len(values):
                    return False
            return True

    def checkColumn(self, grid: np.ndarray, index = -1):
        '''
            grid (np.ndarray) : Array of the sudoku
            index (int) : if between 0-81 -> specific column containing index is checked 
                          else -> all columns are checked
        '''

        # Check a specific column containing the index
        if index >= 0:
            column = index % 9
            values = [grid[i * 9 + column] for i in range(9)]
            return len(set(values)) == len(values)
                

        # Check all columns
        else:
            for column in range(9):
                values = [grid[i * 9 + column] for i in range(9)]
                if len(set(values)) != len(values):
                    return False
            return True

    def getSquare(self, x, y, grid):
        startIndex = 27 * y + 3 * x
        square = [grid[startIndex + j + 9 * i] for i in range(3) for j in range(3)]
        return square

    def checkSquare(self, grid):
        for x in range(3):
            for y in range(3):
                square = self.getSquare(x, y, grid)
                seen = set()
                for num in square:
                    if num in seen:
                        return False
                    seen.add(num)
        return True

def main():
    solver = Solver(np.array(sudoku_og), inertiaWeight=0.5, rhoP=2, rhoG=2, nbParticles=100, limit=1000)
    solver.initSwarm()

main()