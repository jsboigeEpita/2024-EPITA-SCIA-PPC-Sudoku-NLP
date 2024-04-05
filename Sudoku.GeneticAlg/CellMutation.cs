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
        probability = 0.6f;
        if (RandomizationProvider.Current.GetDouble() > probability)
        {
            var sudokuChromosome = (SudokuChromosome)chromosome;
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
            Console.WriteLine($"Old value : {gene[cellPositionInGene]} New value : {randomValue}");
            gene[cellPositionInGene] = randomValue;
            sudokuChromosome.ReplaceGene(geneIndex, new Gene(gene));
        }
    }
}