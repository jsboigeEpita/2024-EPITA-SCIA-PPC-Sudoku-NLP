using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSharp;


namespace Sudoku.Shared;

public class SudokuChromosome : ChromosomeBase
{
    private readonly SudokuGrid _target;
    private readonly Dictionary<int, List<int>> mask;
    private static readonly Random Random = new Random();


    public SudokuChromosome(SudokuGrid target, Dictionary<int, List<int>> mask) : base(9)
    {
        _target = target;
        this.mask = mask;
        CreateGenes();
    }

    public SudokuGrid getTarget()
    {
        return _target;
    }

    public SudokuGrid getSolution()
    {
        var fullGrid = new int[9, 9];
        var genes = this.GetGenes();

        for (int geneIndex = 0; geneIndex < genes.Length; geneIndex++)
        {
            var subgrid = (int[][])genes[geneIndex].Value;
            int startRow = (geneIndex / 3) * 3;
            int startColumn = (geneIndex % 3) * 3;

            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    fullGrid[startRow + row, startColumn + column] = subgrid[row][column];
                }
            }
        }

        var stringBuilder = new System.Text.StringBuilder(81);

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                stringBuilder.Append(fullGrid[row, col]);
            }
        }

        SudokuGrid solution = SudokuGrid.ReadSudoku(stringBuilder.ToString());
        return solution;
    }

    public Gene GenerateGene2(int geneIndex) // Put back the override to run this fct
    {
        var availableValues = Enumerable.Range(1, 9).ToList();
        var grid = _target.GetGrid(geneIndex);

        for (int i = 0; i < grid.Length; i++)
        {
            for (int j = 0; j < grid[i].Length; j++)
            {
                if (grid[i][j] == 0)
                {
                    var randomIndex = Random.Next(availableValues.Count);
                    var randomValue = availableValues[randomIndex];
                    grid[i][j] = randomValue;
                    availableValues.Remove(randomValue);
                }
                else
                {
                    availableValues.Remove(grid[i][j]);
                }
            }
        }

        return new Gene(grid);
    }

    public override IChromosome CreateNew()
    {
        return new SudokuChromosome(_target, mask);
    }


    /* Lyes version : */


    public Dictionary<int, List<int>> removeValue(Dictionary<int, List<int>> dict, int cell_value, int cell_position)
    {

        dict[cell_position] = new List<int>();

        for (int j = 0; j < 9; j++ ) 
                dict[j].Remove(cell_value);

        return dict;
    }


    
    public (int,int) get_unique_values(Dictionary<int, List<int>> dict) 
    {
        for  (int cell_value = 1; cell_value <= 9; cell_value++)
        {
            var dict_filtered = dict.Where(KeyValuePair => KeyValuePair.Value.Contains(cell_value));
            if (dict_filtered.Count() == 1)
                return (dict_filtered.First().Key, cell_value);
        }

        return (-1, -1);
    }



    // This function is used for mutation but could be nice if it does remplace 7 by 7 for ex 
    // Genes : 0-8
    public override Gene GenerateGene(int geneIndex)
    {

        int newCellValue = -1;

        Dictionary<int, List<int>> tmp_dict = new Dictionary<int, List<int>>();
        for(int i = 0; i < 9; i++)
            tmp_dict.Add(i, new List<int>(mask[geneIndex * 9 + i]));


        List<int> colPositionByHighestPriority = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 }.OrderBy(x => tmp_dict[x].Count).ToList();
        int[] NewGene = Enumerable.Range(0,9).Select(i => _target.Cells[geneIndex,i]).ToArray();  // _target.Cells[geneIndex].ToArray(); changed because [,] instead of [][] 

        foreach (int cellPositionOfNewGene in colPositionByHighestPriority)             
        {

            if (tmp_dict[cellPositionOfNewGene].Count == 0) 
                continue;

            do 
            {
                int randomIndex = Random.Next(0, tmp_dict[cellPositionOfNewGene].Count);
                newCellValue = tmp_dict[cellPositionOfNewGene][randomIndex];

            } while (NewGene.Contains(newCellValue));

            NewGene[cellPositionOfNewGene] = newCellValue;
            removeValue(tmp_dict, newCellValue, cellPositionOfNewGene);


            while (true)
            {
                (int Key, int Value)= get_unique_values(tmp_dict);
                if (Key == -1)
                    break;

                NewGene[Key]= Value;
                removeValue(tmp_dict, Value, Key);
            }

            // Update the priority
            colPositionByHighestPriority = colPositionByHighestPriority.OrderBy(x => tmp_dict[x].Count).ToList();

        }

        return new Gene(NewGene);

    }





    // There is also another version of generateGenes that I will push if this is necessary


}