using System;
using GeneticSharp;
using Sudoku.Shared;

namespace Sudoku.GeneticAlg;

public class SudokuFitness : IFitness
{

    public SudokuGrid initialSudoku;

    public SudokuFitness(SudokuGrid initialSudoku)
    {
        this.initialSudoku = initialSudoku;
    }

    public double Evaluate(IChromosome chromosome)
    {
        var solution = GeneticSolver.ConvertChromosomeToSudokuGrid(chromosome);
        var errors = solution.NbErrors(initialSudoku);
        return 1.0 / (errors + 1);
    }

}