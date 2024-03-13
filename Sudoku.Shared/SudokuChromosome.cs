using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSharp;


namespace Sudoku.Shared;

public class SudokuChromosome : ChromosomeBase
{
    private readonly SudokuGrid _target;
    private static readonly Random Random = new Random();


    public SudokuChromosome(SudokuGrid target) : base(9)
    {
        _target = target;
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

    public override Gene GenerateGene(int geneIndex)
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
        return new SudokuChromosome(_target);
    }
}