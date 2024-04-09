using System;
using System.Collections.Generic;
using GeneticSharp;
using Sudoku.Shared;

namespace Sudoku.GeneticAlg
{
    public class GeneticSolver : ISudokuSolver
    {
        int population_size = 1000;

        public SudokuGrid Solve(SudokuGrid s)
        {
            var cleanSudoku = new SudokuCleaning(s);
            SudokuGrid sudoku = cleanSudoku.sudoku;
            Dictionary<int, List<int>> mask = cleanSudoku.mask; 

            var selection = new TournamentSelection();
            var crossover = new UniformCrossover();
            var mutation = new CellMutation();
            var fitness = new SudokuFitness(sudoku);
            var chromosome = new SudokuChromosome(sudoku, mask);
            
            Console.WriteLine("GA running...");
            while (true)
            {
                var population = new Population(population_size, population_size, chromosome);
                var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
                ga.Termination = new OrTermination(new FitnessStagnationTermination(20), new FitnessThresholdTermination(1));

                ga.GenerationRan += (sender, e) =>
                {
                    var bestFitness = ga.BestChromosome.Fitness;
                    Console.WriteLine($"Generation {ga.GenerationsNumber}: Best Fitness = {bestFitness}");
                };
                ga.Start();

                if (ga.BestChromosome.Fitness == 1) 
                    return ConvertChromosomeToSudokuGrid(ga.BestChromosome);

                population_size = (int)Math.Round(population_size * 1.5);
            }
        }

        public static SudokuGrid ConvertChromosomeToSudokuGrid(IChromosome bestChromosome)
        {

            int[,] sudoku = new int[9,9];
            for (int i = 0; i < 9; i++) 
            {
                var gene = (int[])bestChromosome.GetGene(i).Value;

                for (int j = 0; j < 9; j++)
                    sudoku[i,j] = gene[j];
            }
            return new SudokuGrid { Cells = sudoku};
        }

    }
}
