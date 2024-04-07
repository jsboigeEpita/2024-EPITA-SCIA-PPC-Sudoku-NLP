import numpy as np
import random as rd

class Particle:
    def __init__(self, grid, velocity, bestScore):
        self.grid = grid
        self.best = grid
        self.bestScore = bestScore
        self.velocity = velocity

class Swarm:
    def __init__(self, particles, best, bestScore):
        self.particles = particles
        self.best = best
        self.bestScore = bestScore

class Solver:
    def __init__(self, toSolve, inertiaWeight, rhoP, rhoG, limit, nbParticles):
        self.toSolve = toSolve
        self.inertiaWeight = inertiaWeight
        self.rhoP = rhoP
        self.rhoG = rhoG
        self.limit = limit
        self.nbParticles = nbParticles
        self.swarm = None
    
    # Init the swarm attribute of the solver
    #
    # The swarm is set with nbParticles particles, which contains the preFilledValues, and are filled with random values from 1 to 9
    def initSwarm(self):
        best = self.toSolve
        bestScore = 0
        particles = np.array([])

        for _ in range(self.nbParticles):
            # Fill grid with randoms
            indices = np.where(self.toSolve == 0)
            grid = self.toSolve.copy()
            grid[indices] = np.random.randint(1, 10, size=len(indices[0]))

            # Create velocity and set velocity to 0 for pre filled indexes
            velocity = np.zeros(81)

            # Score the grid
            _, gridScore = self.fitness(grid)
            if gridScore > bestScore:
                bestScore = gridScore
                best = grid

            particles = np.append(particles, Particle(grid, velocity, gridScore))

        _, bestScore = self.fitness(best)
        self.swarm = Swarm(particles, best, bestScore)
        
    # Solve the sudoku
    #
    # Solve the sudoku with a main loop which stops when "limit" is reached or when a solution is found
    # It loops over each particles to update their velocity, position, and update the best sudoku in case the new one gets a better score from fitness function
    def solve(self):
        count = 0
        while count < self.limit:
            for particle in self.swarm.particles:
                for i in range(81):
                    r = np.random.uniform(size=2)
                    # update new velocity
                    particle.velocity[i] = (particle.velocity[i] * self.inertiaWeight) + (self.rhoP * r[0] * (particle.best[i] - particle.grid[i])) + (self.rhoG * r[1] * (self.swarm.best[i] - particle.grid[i]))
                
                # update new position
                particle.grid = np.add(particle.grid, particle.velocity)
                particle.grid[particle.grid >= 9.5] = 1
                particle.grid[particle.grid < .5] = 9

                # compare new particle score
                isSolved, particleScore = self.fitness(particle.grid)
                if isSolved:
                    return particle.grid
                if particleScore > particle.bestScore:
                    particle.best = particle.grid
                    particle.bestScore = particleScore
                    if particleScore > self.swarm.bestScore:
                        self.swarm.best = particle.grid
                        self.swarm.bestScore = particleScore
            count += 1
        return self.swarm.best

    # evaluate grid
    # our aim is to maximize the fitness
    def fitness(self, grid):
        vfunc = np.vectorize(lambda x: int(round(x)))
        copyGrid = vfunc(grid)

        (squareCount, squareRows, squareColumns) = self.checkSquare(copyGrid)
        (rowSolution, rowsCount, indexRows) = self.checkLine(copyGrid)
        (colSolution, columnsCount, indexColumns) = self.checkColumn(copyGrid)

        fitness = (squareCount * 10) + (rowsCount * 9) + (columnsCount * 9) + (rowsCount * columnsCount)
        
        for i in range(len(indexRows)):
            fitness += squareRows.count(indexRows[i]/3) * 3

        for i in range(len(indexColumns)):
            fitness += squareColumns.count(indexColumns[i]/3) * 3

        return (rowSolution and colSolution, fitness)

    def checkLine(self, grid, index = -1):
        '''
            grid (np.ndarray) : Array of the sudoku
            index (int) : if between 0-81 -> specific row containing index is checked 
                          else (-1) -> all rows are checked
        '''

        # Check a specific row containing the index
        if index >= 0:
            row = index // 9
            start = row * 9
            end = start + 9
            values = grid[start:end]
            return (True, 1, [row]) if len(set(values)) == len(values) else (False, 0, [])
                

        # Check all rows
        else:
            count = 0
            boolean = True
            l = []
            for row in range(9):
                start = row * 9
                end = start + 9
                values = grid[start:end]
                if len(set(values)) != 9:
                    boolean = False
                else:
                    count += 1
                    l.append(row)
            return (boolean, count, l)

    def checkColumn(self, grid, index = -1):
        '''
            grid (np.ndarray) : Array of the sudoku
            index (int) : if between 0-81 -> specific column containing index is checked 
                          else (-1) -> all columns are checked
        '''

        # Check a specific column containing the index
        if index >= 0:
            column = index % 9
            values = [grid[i * 9 + column] for i in range(9)]
            return (True, 1, [column] ) if len(set(values)) == len(values) else (False, 0, [])
                

        # Check all columns
        else:
            count = 0
            boolean = True
            l = []
            for column in range(9):
                values = [grid[i * 9 + column] for i in range(9)]
                if len(set(values)) != 9:
                    boolean = False
                else:
                    count += 1
                    l.append(column)
            return (boolean, count, l)
        
    def getSquare(self, x, y, grid):
        startIndex = 27 * y + 3 * x
        square = [grid[startIndex + j + 9 * i] for i in range(3) for j in range(3)]
        return square

    def checkSquare(self, grid):
        rows = [] 
        columns = []
        count = 0
        for y in range(3):
            for x in range(3):
                if len(set(self.getSquare(x, y, grid))) == 9:
                    count += 1
                    rows.append(y)
                    columns.append(x)
        return (count, rows, columns)

rd.seed()

solver = Solver(np.array(instance).flatten(), inertiaWeight=.75, rhoP=.6, rhoG=.8, nbParticles=100, limit=200)
solver.initSwarm()
result = solver.solve()
r = result.astype("int").reshape((9, 9)).tolist()