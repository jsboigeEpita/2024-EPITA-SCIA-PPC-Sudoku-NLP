from veroku.factors.sparse_categorical import (
    SparseCategorical,
    SparseCategoricalTemplate,
)
from veroku.cluster_graph import ClusterGraph
import numpy as np
import pandas as pd
import itertools


def has_int_square_root(value):
    root = value**0.5
    root = float(round(root))
    return root**2.0 == value


class SudokuBoard:

    def __init__(self, starting_board_state):
        """
        Construct a Sudoku board instance.
        Note: cell indices and cell variable names are the same.
        """
        assert (
            starting_board_state.shape[0] == starting_board_state.shape[1]
        ), "Error: board must be square"
        assert has_int_square_root(
            starting_board_state.shape[0]
        ), "board dim must have integer square root."
        self.starting_board_state = starting_board_state.copy()
        self.board_dim = starting_board_state.shape[0]
        self.board_array = np.array(range(starting_board_state.shape[0] ** 2)).reshape(
            [self.board_dim, self.board_dim]
        )
        assert (
            starting_board_state.shape == self.board_array.shape
        ), "Error: Inconsistent starting board and block dim"
        self.solution = None
        self.clustergraph = None

        # get observations dictionary
        self.evidence_dict = dict()
        for i in range(self.board_array.shape[0]):
            for j in range(self.board_array.shape[1]):
                if starting_board_state[i, j] != 0:
                    var_name = str(self.board_array[i, j])
                    self.evidence_dict[var_name] = starting_board_state[i, j]

        self.block_dim = int(self.board_dim**0.5)
        block_index = 0
        self.block_cell_lists = (
            dict()
        )  # a dictionary mapping the block id to the cells (their cell ids) contained in it
        for i_ in np.linspace(
            0, self.board_array.shape[0], self.block_dim, endpoint=False
        ):
            for j_ in np.linspace(
                0, self.board_array.shape[1], self.block_dim, endpoint=False
            ):
                i, j = int(i_), int(j_)
                cell_array = self.board_array[
                    i : i + self.block_dim, j : j + self.block_dim
                ]
                self.block_cell_lists[block_index] = list(cell_array.ravel())
                block_index += 1

        self.row_cell_lists = (
            dict()
        )  # a dictionary mapping the row id to the cells (their cell ids) contained in it
        for i in range(self.board_array.shape[0]):
            self.row_cell_lists[i] = list(self.board_array[i, :])

        self.col_cell_lists = (
            dict()
        )  # a dictionary mapping the row id to the cells (their cell ids) contained in it
        for i in range(self.board_array.shape[1]):
            self.col_cell_lists[i] = list(self.board_array[:, i])

    @staticmethod
    def get_formatable_cell_strings(num_cells):
        cell_fstrings = [
            "cell_({})".format(i).replace("(", "{").replace(")", "}")
            for i in range(num_cells)
        ]
        return cell_fstrings

    def get_cells_probs_table(self):
        num_cell_possible_values = self.block_dim**2
        permutations = list(
            itertools.permutations(range(1, num_cell_possible_values + 1))
        )
        prob = 1.0 / len(permutations)
        probs_table = {assign: prob for assign in permutations}
        return probs_table

    def get_all_factors(self):
        # get block factors
        num_cell_possible_values = self.block_dim**2
        cell_value_cardinalities = [num_cell_possible_values] * num_cell_possible_values
        factor_template = SparseCategoricalTemplate(
            probs_table=self.get_cells_probs_table(),
            cardinalities=cell_value_cardinalities,
        )
        factors = []
        for block_cell_vars in self.block_cell_lists.values():
            var_names = [str(v) for v in block_cell_vars]
            factors.append(factor_template.make_factor(var_names=var_names))
        # get row factors
        for row_cell_vars in self.row_cell_lists.values():
            var_names = [str(v) for v in row_cell_vars]
            factors.append(factor_template.make_factor(var_names=var_names))
        # get column factors
        for col_cell_vars in self.col_cell_lists.values():
            var_names = [str(v) for v in col_cell_vars]
            factors.append(factor_template.make_factor(var_names=var_names))
        return factors

    def build_cluster_graph(self):
        factors = self.get_all_factors()
        print("num factors: ", len(factors))
        self.clustergraph = ClusterGraph(factors=factors, evidence=self.evidence_dict)

    def build_from_factors(self, factors):
        self.clustergraph = ClusterGraph(factors=factors, evidence=self.evidence_dict)

    def compute_solution(self):
        if self.clustergraph is None:
            self.build_cluster_graph()
        self.clustergraph.process_graph()
        self.solution = np.zeros(self.board_array.shape)
        for i in range(self.board_array.shape[0]):
            for j in range(self.board_array.shape[1]):
                final_var_value = self.starting_board_state[i, j]
                if final_var_value == 0:  # var was not given
                    cell_var = self.board_array[i, j]
                    cell_posterior_marginal = self.clustergraph.get_marginal(
                        [str(cell_var)]
                    )
                    final_var_value = cell_posterior_marginal.argmax()[0]
                self.solution[i, j] = final_var_value
        return self.solution.copy()


# instance = np.array(
#     [
#         [0, 0, 0, 0, 9, 4, 0, 3, 0],
#         [0, 0, 0, 5, 1, 0, 0, 0, 7],
#         [0, 8, 9, 0, 0, 0, 0, 4, 0],
#         [0, 0, 0, 0, 0, 0, 2, 0, 8],
#         [0, 6, 0, 2, 0, 1, 0, 5, 0],
#         [1, 0, 2, 0, 0, 0, 0, 0, 0],
#         [0, 7, 0, 0, 0, 0, 5, 2, 0],
#         [9, 0, 0, 0, 6, 5, 0, 0, 0],
#         [0, 4, 0, 9, 7, 0, 0, 0, 0],
#     ],
#     dtype=int,
# )


def veroku_solver(puzzle: np.ndarray) -> np.ndarray:
    try:
        sudoku_board = SudokuBoard(puzzle)
        infered_solution_array = sudoku_board.compute_solution().reshape(9, 9).astype(np.int32)
        return infered_solution_array
    except:
        return puzzle


if "instance" in locals():
    result = veroku_solver(np.array(instance, dtype=int))
