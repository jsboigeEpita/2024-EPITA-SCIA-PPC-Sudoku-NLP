using System;
using GeneticSharp;

namespace Sudoku.Shared;

public class SudokuFitness : IFitness
{
    public double Evaluate(IChromosome chromosome)
    {
        var genes = chromosome.GetGenes();
        var sudokuChromosome = chromosome as SudokuChromosome;
        if (sudokuChromosome == null)
        {
            throw new InvalidOperationException("Chromosome cannot be cast to SudokuChromosome");
        }
        var target = sudokuChromosome.getTarget();
        var solution = EmptySolver.ConvertChromosomeToSudokuGrid(chromosome);
        //var solution = sudokuChromosome.getSolution();

        var errors = solution.NbErrors(target);
        return 1.0 / (errors + 1);
    }
}