using GeneticSharp;

namespace Sudoku.GeneticAlg;

public class CellMutation : MutationBase
{
    public CellMutation()
    {
        IsOrdered = true;
    }

    protected override void PerformMutate(IChromosome chromosome, float probability)
    {
        var sudokuChromosome = (SudokuChromosome)chromosome;
        var fitness = chromosome.Fitness.HasValue ? chromosome.Fitness.Value : 0;
        probability = (float)(1.0f - fitness);
        probability = Math.Max(0.01f, Math.Min(probability, 0.7f)); // Example range: 1% to 50%
        

        if (RandomizationProvider.Current.GetDouble() > probability)
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