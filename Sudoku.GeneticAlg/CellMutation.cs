using GeneticSharp;

namespace Sudoku.GeneticAlg;

public class CellMutation : MutationBase
{
    
    public SudokuFitness Fitness;
    public CellMutation(SudokuFitness fitness)
    {
        IsOrdered = true;
        Fitness = fitness;
    }

    protected override void PerformMutate(IChromosome chromosome, float probability)
    {
        var fitness = Fitness.Evaluate(chromosome);
        var sudokuChromosome = (SudokuChromosome)chromosome;
        probability = (float)(1.0f - fitness);
        probability = Math.Max(0.01f, Math.Min(probability, 0.2f)); // Example range: 1% to 50%
        
        if (RandomizationProvider.Current.GetDouble() <= probability)
        {
            var mask = sudokuChromosome.mask;
            var geneIndex = RandomizationProvider.Current.GetInt(0, 9);

            var gene = (int[])sudokuChromosome.GetGene(geneIndex).Value;
            var cellPositionInGene = RandomizationProvider.Current.GetInt(0, 9);
            var cellPosition = geneIndex * 9 + cellPositionInGene;
            var possibleValues = mask[cellPosition];
            while (possibleValues.Count == 0)
            {
                cellPositionInGene = RandomizationProvider.Current.GetInt(0, 9);
                cellPosition = geneIndex * 9 + cellPositionInGene;
                possibleValues = mask[cellPosition];
            }

            var randomValue = possibleValues[RandomizationProvider.Current.GetInt(0, possibleValues.Count)];
            gene[cellPositionInGene] = randomValue;
            sudokuChromosome.ReplaceGene(geneIndex, new Gene(gene));
        }
    }
}