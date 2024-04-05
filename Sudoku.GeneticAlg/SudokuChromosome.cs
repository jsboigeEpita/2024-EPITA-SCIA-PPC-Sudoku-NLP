using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSharp;
using Sudoku.Shared;


namespace Sudoku.GeneticAlg;

public class SudokuChromosome : ChromosomeBase
{
    private readonly SudokuGrid intialSudoku;
    public Dictionary<int, List<int>> mask;
    private static readonly Random Random = new Random();

    int newCellValue = -1;

    public SudokuChromosome(SudokuGrid intialSudoku, Dictionary<int, List<int>> mask) : base(9)
    {
        this.intialSudoku= intialSudoku;
        this.mask = mask;
        CreateGenes();
    }

    public override IChromosome CreateNew()
    {
        return new SudokuChromosome(intialSudoku, mask);
    }



    // Remove a given a value of our temporary dict 
    public Dictionary<int, List<int>> removeValue(Dictionary<int, List<int>> dict, int cell_value, int cell_position)
    {
        dict[cell_position] = new List<int>();

        for (int j = 0; j < 9; j++ ) 
            dict[j].Remove(cell_value);

        return dict;
    }

    // Take value that can be used only in a specific posiiton of the row
    // Return the tuple (Position of the cell, Value of the cell)
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

        

    // Create A random Gene among the possible values according to the mask
    public override Gene GenerateGene(int geneIndex)
    {

        int[] NewGene = Enumerable.Range(0,9).Select(i => intialSudoku.Cells[geneIndex,i]).ToArray();  //  Select the intial row of the actual gene 


        /* Retrieve all the possibles values of the row corresponding to the geneIndex */
        Dictionary<int, List<int>> tmp_dict = new Dictionary<int, List<int>>();
        for(int i = 0; i < 9; i++)
            tmp_dict.Add(i, new List<int>(mask[geneIndex * 9 + i]));


        List<int> randomOrder = Shuffle(Enumerable.Range(0, 9).ToList()); 

        foreach (int cellPositionOfNewGene in randomOrder)
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

            // We edited the mask -> we might have some obvious values that poped up after the edit

            while (true)
            {
                (int Key, int Value)= get_unique_values(tmp_dict);
                if (Key == -1)
                    break;

                NewGene[Key]= Value;
                removeValue(tmp_dict, Value, Key);
            }

        }

        return new Gene(NewGene);

    }


     static List<int> Shuffle(List<int> list, int list_size = 9)
    {
        int n = list_size;
        while (n > 1)
        {
            n--;
            int k = Random.Next(n + 1);
            int value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list;
    }

    /*Another implementaion that  generate gene according to the probabily : we fill the cell that has less possibility according to the mask first*/
    public Gene GenerateGene2(int geneIndex)
    {

        int[] NewGene = Enumerable.Range(0,9).Select(i => intialSudoku.Cells[geneIndex,i]).ToArray();  //  Select the intial row of the actual gene 

        Dictionary<int, List<int>> tmp_dict = new Dictionary<int, List<int>>();
        for(int i = 0; i < 9; i++)
            tmp_dict.Add(i, new List<int>(mask[geneIndex * 9 + i]));


        // Sort the cell index of a row by probability -> We start by the gene with less possible values  
        List<int> rowPositionByHighestPriority = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 }.OrderBy(x => tmp_dict[x].Count).ToList();

        
        foreach (int cellPositionOfNewGene in rowPositionByHighestPriority)             
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

            // We edited the mask -> According to this we might have new priority
            while (true)
            {
                (int Key, int Value)= get_unique_values(tmp_dict);
                if (Key == -1)
                    break;

                NewGene[Key]= Value;
                removeValue(tmp_dict, Value, Key);
            }

            // Update the priority
            rowPositionByHighestPriority = rowPositionByHighestPriority.OrderBy(x => tmp_dict[x].Count).ToList();

        }

        return new Gene(NewGene);

    }










}