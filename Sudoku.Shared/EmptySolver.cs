using System;
using System.Collections.Generic;
using GeneticSharp;

namespace Sudoku.Shared
{
    public class EmptySolver : ISudokuSolver
    {
        int population_size = 1000;
        
        public SudokuGrid Solve(SudokuGrid s)
        {
            var cleanSudoku = new SudokuCleaning(s);
            Dictionary<int, List<int>> mask = cleanSudoku.mask; 
            SudokuGrid sudoku = cleanSudoku.sudoku;

            var selection = new TournamentSelection();
            var crossover = new UniformCrossover();
            var mutation = new UniformMutation();
            var fitness = new SudokuFitness();
            var chromosome = new SudokuChromosome(sudoku);
            
            Console.WriteLine("GA running...");
            while (true)
            {
                var population = new Population(population_size, population_size, chromosome);
                var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
                ga.Termination = new OrTermination(new FitnessStagnationTermination(20), new FitnessThresholdTermination(1));
                // ga.OperatorsStrategy = new TplOperatorsStrategy();
	            // ga.TaskExecutor = new TplTaskExecutor();

                ga.GenerationRan += (sender, e) =>
                {
                    var bestFitness = ga.BestChromosome.Fitness;
                    Console.WriteLine($"Generation {ga.GenerationsNumber}: Best Fitness = {bestFitness}");
                };

                ga.Start();

                if (ga.BestChromosome.Fitness == 1)
                {
                    var sudokuChromosome = ga.BestChromosome as SudokuChromosome;
                    return sudokuChromosome.getSolution();
                }
                population_size = (int)Math.Round(population_size * 1.5);
            }
        }
    }
}