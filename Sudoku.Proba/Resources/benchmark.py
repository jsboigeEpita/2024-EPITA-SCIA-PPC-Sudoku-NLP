import os
from Veroku import veroku_solver
from Proba import proba_solver
from typing import Callable
import time
import pandas as pd
import numpy as np
from tqdm import tqdm
import signal


TIMEOUT_SECONDS = 60

def handler(signum, frame):
    raise Exception("Timeout")


puzzles_dir = os.path.join("Sudoku.Proba/Resources/puzzles")
puzzles_paths = os.listdir(puzzles_dir)

easy_puzzles = []
medium_puzzles = []
hard_puzzles = []

puzzles_lists = [medium_puzzles,easy_puzzles, hard_puzzles]


def puzzle_str_to_np_array(puzzle_str):
    puzzle = []
    for i in range(9):
        row = []
        for j in range(9):
            row.append(int(puzzle_str[i * 9 + j]))
        puzzle.append(row)
    return np.array(puzzle, dtype=int)



for puzzle_path, puzzles_list in zip(puzzles_paths, puzzles_lists):
    puzzle_path = os.path.join(puzzles_dir, puzzle_path)
    with open(puzzle_path, "r") as f:
        puzzles = f.read()
        # iterate over each line
        for puzzle in puzzles.split("\n"):
            if len(puzzle) == 0:
                continue
            puzzles_list.append(puzzle_str_to_np_array(puzzle))

# puzzles_lists[0] = puzzles_lists[1][:1]
# puzzles_lists[1] = puzzles_lists[1][:10]
# puzzles_lists[2] = puzzles_lists[1][:1]

def check_valid_solution(solution: np.ndarray):
    ## check rows
    rows = solution.reshape(9, 9)
    for row in rows:
        if len(np.unique(row)) != 9:
            return False
    ## check columns
    cols = solution.reshape(9, 9).T
    for col in cols:
        if len(np.unique(col)) != 9:
            return False
    ## check blocks
    blocks = solution.reshape(3, 3, 3, 3)
    for block_row in blocks:
        for block in block_row:
            if len(np.unique(block)) != 9:
                return False
    return True

check_valid_solution(proba_solver(easy_puzzles[-1]))

def compute_benchmark_result(
    solver: Callable[[np.ndarray], np.ndarray], puzzle: np.ndarray
) -> np.ndarray:
    signal.signal(signal.SIGALRM, handler)
    signal.alarm(TIMEOUT_SECONDS)
    start_time = time.time()
    try:
        infered_solution_array = solver(puzzle)
    except Exception as e:
        signal.alarm(0)
        return np.array([False, TIMEOUT_SECONDS * 1000])    
    signal.alarm(0)
    is_valid_solution = check_valid_solution(infered_solution_array)
    time_to_resolve_ms = (time.time() - start_time) * 1000
    print(infered_solution_array)
    return np.array([is_valid_solution, time_to_resolve_ms])


def solver_benchmark(solver: Callable[[np.ndarray], np.ndarray]):
    easy_benchmark_results: list[np.ndarray] = []
    medium_benchmark_results: list[np.ndarray] = []
    hard_benchmark_results: list[np.ndarray] = []
    for benchmark_result, puzzles_list in zip(
        [
            easy_benchmark_results,
            medium_benchmark_results,
            hard_benchmark_results,
        ],
        puzzles_lists,
    ):
        for puzzle in tqdm(puzzles_list):
            benchmark_result.append(compute_benchmark_result(solver, puzzle))
    return (
        # np.random.randint(2, size=(10, 2)),
        # np.random.randint(2, size=(10, 2)),
        # np.random.randint(2, size=(10, 2)),
        np.vstack(easy_benchmark_results),
        np.vstack(medium_benchmark_results),
        np.vstack(hard_benchmark_results),
    )


def print_benchmark_results(benchmark_results):
    tests = ["easy", "medium", "hard"]
    for test, result in zip(tests,benchmark_results):
        solution_found = result.sum(axis=0)[0]
        result_found_filtered = result[result[:,0] == True]
        df = pd.DataFrame(result_found_filtered, columns=["is_valid_solution", "time_to_resolve_ms"])
        print(f"{test}:")
        print("Solution found: ", solution_found, "out of", len(result))
        print(f"Time to resolve {solution_found} solutions:")
        print(df["time_to_resolve_ms"].describe())
        print("---")

print("Proba solver, Benchmark:")
benchmark_results = solver_benchmark(proba_solver)
print_benchmark_results(benchmark_results)


print("Veroku solver, Benchmark:")
benchmark_results_2 = solver_benchmark(veroku_solver)
print_benchmark_results(benchmark_results_2)