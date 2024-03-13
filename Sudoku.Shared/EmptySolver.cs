using System;
using GeneticSharp;

namespace Sudoku.Shared
{
    public class EmptySolver : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            var selection = new TournamentSelection();
            var crossover = new UniformCrossover();
            var mutation = new UniformMutation();
            var fitness = new SudokuFitness();
            var chromosome = new SudokuChromosome(s);
            var population = new Population(10000, 30000, chromosome);
            
            var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
            ga.Termination = new GenerationNumberTermination(100);

            
            Console.WriteLine("GA running...");
            ga.GenerationRan += (sender, e) =>
            {
                var bestFitness = ga.BestChromosome.Fitness;
                Console.WriteLine($"Generation {ga.GenerationsNumber}: Best Fitness = {bestFitness}");
            };
            ga.Start();

            Console.WriteLine("Best solution found has {0} fitness.", ga.BestChromosome.Fitness);
            var sudokuChromosome = ga.BestChromosome as SudokuChromosome;
            return sudokuChromosome.getSolution();
        }
    }
}