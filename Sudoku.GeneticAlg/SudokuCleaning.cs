using GeneticSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Sudoku.Shared;


/*
    The purpose of this class is to fill to the given Sudoku with all the values that can be deduced by inference without to do any prediction.
    To do that, we will need a Dictionnary that computes every possibles value for each cell of the given Sudoku.
    In this class, we are doing 3 big manipulations :


    1. Mask : Adds a Mask of all possibles values for each cell according to the already filled value. For example, if the first row contains a 1.
        we can remove this value for each cell of the row  (except the one containing it)

    2. Fill the sudoku with the self-evident value : If there is only one possible value for a cell, we can directly fill the sudoku with this value,

        Ex : Dict -> (0: [1,2,3,4]) ; (1: [2]) ; ... ; (80: [5,3])
        In the example, we can see that the cell 1 can only compute 2 so we can fill the sudoku and update the Dict:
            Dict -> (0: [1,3,4]) ; (1: []) ; ... ; (80: [5,3])
        
        The dict don't have any possible value for the cell 1 since it has been filled. Noticed that we also updated the cell 0 that can't be 2 anymore 


    3. Fill the sudoku with value present only in one cell (of a specific row, column or square) : If there is only one cell C that can compute a value X, 
        then we can deduce that the value  of is X

        Ex:  Dict -> (0: [1,3,4]) ; (1: []) ;  (2: [1,3]) ;  (3: []) ; (4: []) ;  (5: [1,3]) ;  (6: []) ; (7: []) ;  (8: []) ;   ... ; (80: [5,3])

        On the first row (0 -> 8), we can see that the value 4 can only computed by the cell 0, we can fill the sudoku and update the dict
        Dict -> (0: []) ; (1: []) ;  (2: [1,3]) ;  (3: []) ; (4: []) ;  (5: [1,3]) ;  (6: []) ; (7: []) ;  (8: []) ;   ... ; (80: [5,3])


        NB: Even if it is not showed on the example, we update according to the column and the square, so if on the column of the cell 0 there were a value
            that could be both 4 or 5 for example, it is very useful cause we have eliminated the 4 and are now sure that the other cell of the colum is 4


    Since the step 2 and step 3 imply to change the sudoku, we need to keep doing theses steps until there is no update.

*/ 

namespace Sudoku.GeneticAlg
{
    
    public class SudokuCleaning
    {
        

        public SudokuGrid sudoku;
        public Dictionary<int, List<int>> mask; 

        private bool gotChangement = false;
        private int[,] newCells; 

        List<int>[] rows_position = new List<int>[]
        {
            new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
            new List<int> { 9, 10, 11, 12, 13, 14, 15, 16, 17 },
            new List<int> { 18, 19, 20, 21, 22, 23, 24, 25, 26 },
            new List<int> { 27, 28, 29, 30, 31, 32, 33, 34, 35 },
            new List<int> { 36, 37, 38, 39, 40, 41, 42, 43, 44 },
            new List<int> { 45, 46, 47, 48, 49, 50, 51, 52, 53 },
            new List<int> { 54, 55, 56, 57, 58, 59, 60, 61, 62 },
            new List<int> { 63, 64, 65, 66, 67, 68, 69, 70, 71 },
            new List<int> { 72, 73, 74, 75, 76, 77, 78, 79, 80 }
        };

        List<int>[] columns_position = new List<int>[]
        {
            new List<int> { 0, 9, 18, 27, 36, 45, 54, 63, 72 },
            new List<int> { 1, 10, 19, 28, 37, 46, 55, 64, 73 },
            new List<int> { 2, 11, 20, 29, 38, 47, 56, 65, 74 },
            new List<int> { 3, 12, 21, 30, 39, 48, 57, 66, 75 },
            new List<int> { 4, 13, 22, 31, 40, 49, 58, 67, 76 },
            new List<int> { 5, 14, 23, 32, 41, 50, 59, 68, 77 },
            new List<int> { 6, 15, 24, 33, 42, 51, 60, 69, 78 },
            new List<int> { 7, 16, 25, 34, 43, 52, 61, 70, 79 },
            new List<int> { 8, 17, 26, 35, 44, 53, 62, 71, 80 }
        };

        List<int>[] squares_position = new List<int>[]
        {
            new List<int> { 0, 1, 2, 9, 10, 11, 18, 19, 20 },
            new List<int> { 3, 4, 5, 12, 13, 14, 21, 22, 23 },
            new List<int> { 6, 7, 8, 15, 16, 17, 24, 25, 26 },
            new List<int> { 27, 28, 29, 36, 37, 38, 45, 46, 47 },
            new List<int> { 30, 31, 32, 39, 40, 41, 48, 49, 50 },
            new List<int> { 33, 34, 35, 42, 43, 44, 51, 52, 53 },
            new List<int> { 54, 55, 56, 63, 64, 65, 72, 73, 74 },
            new List<int> { 57, 58, 59, 66, 67, 68, 75, 76, 77 },
            new List<int> { 60, 61, 62, 69, 70, 71, 78, 79, 80 }
        };


        public SudokuCleaning(SudokuGrid sudoku)
        {
            this.newCells = sudoku.Cells; 
            this.mask = Enumerable.Range(0, 81).ToDictionary(x => x, x => Enumerable.Range(1, 9).ToList());

            fill_mask(newCells);
            do
            {
                gotChangement = false;
                fillPlainValues();
                fillUniquesValues();
            }
            while(gotChangement);

            this.sudoku = new SudokuGrid() { Cells = newCells};
        }


        // Check if cell Neigbours also acces to the cell_position position else remove it mantually
        public void removeValueInNeighbours(int cell_value, int cell_position)
        {
            mask[cell_position] =  new List<int>();
            foreach(var neighbor in SudokuGrid.CellNeighbours[cell_position / 9][cell_position % 9])
                mask[neighbor.row * 9 + neighbor.column].Remove(cell_value);
            
            gotChangement = true;
        }


        /* STEP 1 */

        public void fill_mask(int[,] Cells)
        {
            for (int i = 0 ; i < 81; i++)
            {
                int cell = Cells[i / 9,i  % 9];
                if (cell == 0)
                    continue;

                removeValueInNeighbours(cell, i);
            }

        }

        /* STEP 2 */

        public void fillPlainValues()
        {
            Enumerable.Range(0, 81)
                    .Where(i => mask[i].Count == 1) // Only one value possible for the cell i
                    .ToList()
                    .ForEach(i => {newCells[i / 9,i % 9] = mask[i][0]; removeValueInNeighbours(mask[i][0], i);}); // Update the sudoku & the mask
            
        }


        /* STEP 3 */


        public int UniqueValueIndex(Dictionary<int, List<int>> dict, int cell_value, List<int> positions) 
        {
            var dict_filtered = dict.Where(KeyValuePair => positions.Contains(KeyValuePair.Key) && KeyValuePair.Value.Contains(cell_value));
            return dict_filtered.Count() == 1 ? dict_filtered.First().Key : -1;
        }


        void ProcessPositions(List<int>[] positions, int cell_value)
        {
            foreach(var position in positions)
            {
                int uniqueValueIndex = UniqueValueIndex(mask, cell_value, position);
                if (uniqueValueIndex == -1)
                    continue;

                newCells[uniqueValueIndex / 9,uniqueValueIndex % 9] = cell_value;
                removeValueInNeighbours(cell_value, uniqueValueIndex);
            }
        }

        public void fillUniquesValues()
        {
            // Find if there are potential unique value for every rows, columns and square of the sudoku
            for (int sudoku_value = 1; sudoku_value <= 9; sudoku_value++)
            {
                ProcessPositions(rows_position, sudoku_value);
                ProcessPositions(columns_position, sudoku_value);
                ProcessPositions(squares_position, sudoku_value);
            }
        }




 

    }
}
