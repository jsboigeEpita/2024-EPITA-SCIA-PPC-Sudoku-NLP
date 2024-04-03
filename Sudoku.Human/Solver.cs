﻿using Sudoku.Shared;
using System;
 using System.Collections;
 using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace Sudoku.Human;

public sealed partial class Solver
{
	public Puzzle Puzzle { get; }

	public Solver(SudokuGrid s) {
		Puzzle = Puzzle.CreateFromGrid(s);
		_techniques = InitSolverTechniques();
	}

	public bool TrySolve()
	{
		bool solved = false;
		bool full = false;
		bool deadEnd = false;
		
		List<Cell> allCells = null;
		Stack<BackTrackingState> exploredCellValues = null;

		Puzzle.RefreshCandidates();
		
		do
		{ 
			deadEnd = false;

			// First we do human inference
			do 
			{
				if (CheckForNakedSinglesOrCompletion(out bool changed)) 
				{ 
					break;
				}
				if (changed) 
				{ 
					continue;
				} 
				if (!RunTechnique() && !changed) 
				{ 
					break;
				}
			} while (true);
			
			full = Puzzle.Rows.All(row => row.All(c => c.Value != 0)); 
			//full = s.SetCell(x, y, monPuzzle.Rows[x][y].Value);

			// If puzzle isn't full, we do exploration
			if (!full) 
			{ 
				// Les Sudokus les plus difficiles ne peuvent pas être résolus avec un stylo bille, c'est à dire en inférence pure.
				// Il va falloir lacher le stylo bille et prendre le crayon à papier et la gomme pour commencer une exploration fondée sur des hypothèses avec possible retour en arrière
				if (allCells == null) 
				{ 
					allCells = Puzzle.Rows.SelectMany((row, rowIdx) => row).ToList(); 
					exploredCellValues = new Stack<BackTrackingState>();
				} 
				//puzzle.RefreshCandidates();

				// Pour accélérer l'exploration et éviter de traverser la feuille en gommant trop souvent, on va utiliser les heuristiques des problèmes à satisfaction de contraintes
				// cf. les slides et le problème du "coffre de voiture" abordé en cours

				//heuristique MRV
				var minCandidates = allCells.Min(cell => cell.Candidates.Count > 0 ? cell.Candidates.Count : int.MaxValue);
				
				if (minCandidates != int.MaxValue) 
				{ 
					// Utilisation de l'heuristique Deg: de celles qui ont le moins de candidats à égalité, on choisi celle la plus contraignante, celle qui a le plus de voisins (on pourrait faire mieux avec le nombre de candidats en commun avec ses voisins)
					var candidateCells = allCells.Where(cell => cell.Candidates.Count == minCandidates);
					//var degrees = candidateCells.Select(candidateCell => new {Cell = candidateCell, Degree = candidateCell.GetCellsVisible().Aggregate(0, (sum, neighbour) => sum + neighbour.Candidates.Count) });
					var degrees = candidateCells.Select(candidateCell => new { Cell = candidateCell, Degree = candidateCell.VisibleCells.Count(c => c.Value == 0) }).ToList(); 
					//var targetCell = list_cell.First(cell => cell.Candidates.Count == minCandidates);
					var maxDegree = degrees.Max(deg1 => deg1.Degree); 
					var targetCell = degrees.First(deg => deg.Degree == maxDegree).Cell;

					//dernière exploration pour ne pas se mélanger les pinceaux
					
					BackTrackingState currentlyExploredCellValues; 
					if (exploredCellValues.Count == 0 || !exploredCellValues.Peek().Cell.Equals(targetCell)) 
					{
						currentlyExploredCellValues = new BackTrackingState() { Board = Puzzle.CreateFromPuzzle(Puzzle), Cell = targetCell, ExploredValues = new List<int>() }; 
						exploredCellValues.Push(currentlyExploredCellValues);
					}
					else 
					{
						currentlyExploredCellValues = exploredCellValues.Peek();
					}


					//utilisation de l'heuristique LCV: on choisi la valeur la moins contraignante pour les voisins
					var candidateValues = targetCell.Candidates.Where(i => !currentlyExploredCellValues.ExploredValues.Contains(i)); 
					var neighbourood = targetCell.VisibleCells; 
					var valueConstraints = candidateValues.Select(v => new 
					{ 
						Value = v, 
						ContraintNb = neighbourood.Count(neighboor => neighboor.Candidates.Contains(v))
					}).ToList(); 
					var minContraints = valueConstraints.Min(vc => vc.ContraintNb);
					var exploredValue = valueConstraints.First(vc => vc.ContraintNb == minContraints).Value; 
					currentlyExploredCellValues.ExploredValues.Add(exploredValue); 
					targetCell.SetValue(exploredValue); 
					//targetCell.Set(exploredValue, true);
					
				}
				else
				{ 
					//Plus de candidats possibles, on atteint un cul-de-sac
					if (IsValided(Puzzle)) 
					{ 
						solved = true;
					}
					else 
					{ 
						deadEnd = true;
					}


					//deadEnd = true;
				}
			}
			else 
			{ 
				//If puzzle is full, it's either solved or a deadend
				if (IsValided(Puzzle))
				{
					solved = true;
				}
				else 
				{ 
					deadEnd = true;
				}
			}

			
			if (deadEnd) 
			{ 
				//On se retrouve bloqué, il faut gommer et tenter d'autres hypothèses
				BackTrackingState currentlyExploredCellValues = exploredCellValues.Peek(); 
				//On annule la dernière assignation
				currentlyExploredCellValues.Backtrack(Puzzle); 
				var targetCell = currentlyExploredCellValues.Cell; 
				//targetCell.Set(0, true);
				while (targetCell.Candidates.All(i => currentlyExploredCellValues.ExploredValues.Contains(i))) 
				{ 
					//on a testé toutes les valeurs possibles, On est à un cul de sac, il faut revenir en arrière
					exploredCellValues.Pop(); 
					if (exploredCellValues.Count == 0) 
					{ 
						Console.Error.WriteLine("bug in the algorithm techniques humaines");
					} 
					currentlyExploredCellValues = exploredCellValues.Peek(); 
					//On annule la dernière assignation
					currentlyExploredCellValues.Backtrack(Puzzle); 
					targetCell = currentlyExploredCellValues.Cell; 
					//targetCell.Set(0, true);
				} 
				// D'autres valeurs sont possible pour la cellule courante, on les tente
				//utilisation de l'heuristique LCV
				var candidateValues = targetCell.Candidates.Where(i => !currentlyExploredCellValues.ExploredValues.Contains(i));
				var neighbourood = targetCell.VisibleCells;
				var valueConstraints = candidateValues.Select(v => new 
				{ 
					Value = v, 
					ContraintNb = neighbourood.Count(neighboor => neighboor.Candidates.Contains(v))
				}).ToList(); 
				int minContraints = valueConstraints.Min(vc => vc.ContraintNb); 
				var exploredValue = valueConstraints.First(vc => vc.ContraintNb == minContraints).Value; 
				currentlyExploredCellValues.ExploredValues.Add(exploredValue); 
				targetCell.SetValue(exploredValue);
			}


		} while (!solved);

		return true;
	}
	public bool TrySolveAsync(CancellationToken ct)
	{
		Puzzle.RefreshCandidates();

		do
		{
			if (CheckForNakedSinglesOrCompletion(out bool changed))
			{
				return true;
			}
			if (ct.IsCancellationRequested)
			{
				break;
			}
			if (changed)
			{
				continue;
			}
			if (!RunTechnique())
			{
				return false;
			}
			if (ct.IsCancellationRequested)
			{
				break;
			}
		} while (true);

		throw new OperationCanceledException(ct);
	}
	private bool CheckForNakedSinglesOrCompletion(out bool changed)
	{
		changed = false;
		bool solved = true;
	again:
		for (int col = 0; col < 9; col++)
		{
			for (int row = 0; row < 9; row++)
			{
				Cell cell = Puzzle[col, row];
				if (cell.Value != Cell.EMPTY_VALUE)
				{
					continue;
				}

				// Empty cell... check for naked single
				solved = false;
				if (cell.CandI.TryGetCount1(out int nakedSingle))
				{
					cell.SetValue(nakedSingle);

					changed = true;
					goto again; // Restart the search for naked singles since we have the potential to create new ones
				}
			}
		}
		return solved;
	}

	public struct BackTrackingState
	{ 
		public Cell Cell { get; set; }
		public List<int> ExploredValues { get; set; }
		public Puzzle Board { get; set; }
		
		public void Backtrack(Puzzle objPuzzle) 
		{ 
			for (int i = 0; i < 9; i++) 
			{ 
				for (int j = 0; j < 9; j++)
				{ 
					if (objPuzzle[i, j].Value != Board[i, j].Value) 
					{ 
						objPuzzle[i, j].SetValue(Board[i, j].Value);
					}
				}
			} 
			objPuzzle.RefreshCandidates();
		}
	}
	
	public bool TryBacktrack2()
	{
		bool solved = false;
		bool full = false;
		bool deadEnd = false;
		
		List<Cell> allCells = null;
		Stack<BackTrackingState> exploredCellValues = null;

		Puzzle.RefreshCandidates();
		
		do
		{ 
			deadEnd = false;

			// First we do human inference
			do 
			{
				if (CheckForNakedSinglesOrCompletion(out bool changed)) 
				{ 
					break;
				}
				if (changed) 
				{ 
					continue;
				} 
				if (!RunTechnique() && !changed) 
				{ 
					break;
				}
			} while (true);
			
			full = Puzzle.Rows.All(row => row.All(c => c.Value != 0)); 
			//full = s.SetCell(x, y, monPuzzle.Rows[x][y].Value);

			// If puzzle isn't full, we do exploration
			if (!full) 
			{ 
				// Les Sudokus les plus difficiles ne peuvent pas être résolus avec un stylo bille, c'est à dire en inférence pure.
				// Il va falloir lacher le stylo bille et prendre le crayon à papier et la gomme pour commencer une exploration fondée sur des hypothèses avec possible retour en arrière
				if (allCells == null) 
				{ 
					allCells = Puzzle.Rows.SelectMany((row, rowIdx) => row).ToList(); 
					exploredCellValues = new Stack<BackTrackingState>();
				} 
				//puzzle.RefreshCandidates();

				// Pour accélérer l'exploration et éviter de traverser la feuille en gommant trop souvent, on va utiliser les heuristiques des problèmes à satisfaction de contraintes
				// cf. les slides et le problème du "coffre de voiture" abordé en cours

				//heuristique MRV
				var minCandidates = allCells.Min(cell => cell.Candidates.Count > 0 ? cell.Candidates.Count : int.MaxValue);
				
				if (minCandidates != int.MaxValue) 
				{ 
					// Utilisation de l'heuristique Deg: de celles qui ont le moins de candidats à égalité, on choisi celle la plus contraignante, celle qui a le plus de voisins (on pourrait faire mieux avec le nombre de candidats en commun avec ses voisins)
					var candidateCells = allCells.Where(cell => cell.Candidates.Count == minCandidates);
					//var degrees = candidateCells.Select(candidateCell => new {Cell = candidateCell, Degree = candidateCell.GetCellsVisible().Aggregate(0, (sum, neighbour) => sum + neighbour.Candidates.Count) });
					var degrees = candidateCells.Select(candidateCell => new { Cell = candidateCell, Degree = candidateCell.VisibleCells.Count(c => c.Value == 0) }).ToList(); 
					//var targetCell = list_cell.First(cell => cell.Candidates.Count == minCandidates);
					var maxDegree = degrees.Max(deg1 => deg1.Degree); 
					var targetCell = degrees.First(deg => deg.Degree == maxDegree).Cell;

					//dernière exploration pour ne pas se mélanger les pinceaux
					
					BackTrackingState currentlyExploredCellValues; 
					if (exploredCellValues.Count == 0 || !exploredCellValues.Peek().Cell.Equals(targetCell)) 
					{
						currentlyExploredCellValues = new BackTrackingState() { Board = Puzzle.CreateFromPuzzle(Puzzle), Cell = targetCell, ExploredValues = new List<int>() }; 
						exploredCellValues.Push(currentlyExploredCellValues);
					}
					else 
					{
						currentlyExploredCellValues = exploredCellValues.Peek();
					}


					//utilisation de l'heuristique LCV: on choisi la valeur la moins contraignante pour les voisins
					var candidateValues = targetCell.Candidates.Where(i => !currentlyExploredCellValues.ExploredValues.Contains(i)); 
					var neighbourood = targetCell.VisibleCells; 
					var valueConstraints = candidateValues.Select(v => new 
					{ 
						Value = v, 
						ContraintNb = neighbourood.Count(neighboor => neighboor.Candidates.Contains(v))
					}).ToList(); 
					var minContraints = valueConstraints.Min(vc => vc.ContraintNb);
					var exploredValue = valueConstraints.First(vc => vc.ContraintNb == minContraints).Value; 
					currentlyExploredCellValues.ExploredValues.Add(exploredValue); 
					targetCell.SetValue(exploredValue); 
					//targetCell.Set(exploredValue, true);
					
				}
				else
				{ 
					//Plus de candidats possibles, on atteint un cul-de-sac
					if (IsValided(Puzzle)) 
					{ 
						solved = true;
					}
					else 
					{ 
						deadEnd = true;
					}


					//deadEnd = true;
				}
			}
			else 
			{ 
				//If puzzle is full, it's either solved or a deadend
				if (IsValided(Puzzle))
				{ 
					solved = true;
				}
				else 
				{ 
					deadEnd = true;
				}
			}

			
			if (deadEnd) 
			{ 
				//On se retrouve bloqué, il faut gommer et tenter d'autres hypothèses
				BackTrackingState currentlyExploredCellValues = exploredCellValues.Peek(); 
				//On annule la dernière assignation
				currentlyExploredCellValues.Backtrack(Puzzle); 
				var targetCell = currentlyExploredCellValues.Cell; 
				//targetCell.Set(0, true);
				while (targetCell.Candidates.All(i => currentlyExploredCellValues.ExploredValues.Contains(i))) 
				{ 
					//on a testé toutes les valeurs possibles, On est à un cul de sac, il faut revenir en arrière
					exploredCellValues.Pop(); 
					if (exploredCellValues.Count == 0) 
					{ 
						//("bug in the algorithm techniques humaines");
					} 
					currentlyExploredCellValues = exploredCellValues.Peek(); 
					//On annule la dernière assignation
					currentlyExploredCellValues.Backtrack(Puzzle); 
					targetCell = currentlyExploredCellValues.Cell; 
					//targetCell.Set(0, true);
				} 
				// D'autres valeurs sont possible pour la cellule courante, on les tente
				//utilisation de l'heuristique LCV
				var candidateValues = targetCell.Candidates.Where(i => !currentlyExploredCellValues.ExploredValues.Contains(i));
				var neighbourood = targetCell.VisibleCells;
				var valueConstraints = candidateValues.Select(v => new 
				{ 
					Value = v, 
					ContraintNb = neighbourood.Count(neighboor => neighboor.Candidates.Contains(v))
				}).ToList(); 
				int minContraints = valueConstraints.Min(vc => vc.ContraintNb); 
				var exploredValue = valueConstraints.First(vc => vc.ContraintNb == minContraints).Value; 
				currentlyExploredCellValues.ExploredValues.Add(exploredValue); 
				targetCell.SetValue(exploredValue);
			}


		} while (!solved);

		return true;
	}
	
	bool IsValided(Puzzle puz)
	{
		HashSet<int>[] rows = new HashSet<int>[9]; 
		HashSet<int>[] cols = new HashSet<int>[9]; 
		HashSet<int>[] subgrids = new HashSet<int>[9];
		
		for (int i = 0; i < 9; i++) { 
			rows[i] = new HashSet<int>(); 
			cols[i] = new HashSet<int>(); 
			subgrids[i] = new HashSet<int>();
		}
		
		for (int i = 0; i < 9; i++) { 
			for (int j = 0; j < 9; j++) { 
				int num = puz[i, j].Value;
				if (num == 0)
					return false;
				
				int subgrid_index = (i / 3) * 3 + j / 3;
				
				if (rows[i].Contains(num) || cols[j].Contains(num) || subgrids[subgrid_index].Contains(num)) 
					return false;

				rows[i].Add(num); 
				cols[j].Add(num); 
				subgrids[subgrid_index].Add(num);
			}
		} 
		return true;
	}
	
	public bool TryBacktrack()
	{
		Search(Puzzle, 0, 0);

		bool Search(Puzzle s, int row, int col)
		{
			//pass to the next row if all the cells in the column are checked     
			if (col == 9)
			{
				col = 0; ++row;
				if (row == 9) return true;
			}
			//check if the cell is filled
			Cell cell = Puzzle[row, col];
			if (cell.Value != 0)
				return Search(s, row, col + 1);
			//implement the good value
			for (int v = 1; v <= 9; v++)
			{
				cell = Puzzle[row, col];
				if (IsValid(s, row, col, v))
				{
					cell.SetValue(v);
					if (Search(s, row, col + 1)) return true;
					else cell.SetValue(0);
				}
			}
			return false;
		}
		
		bool IsValid(Puzzle s, int row, int col, int val)
		{
			Cell cell = Puzzle[row, col];
			//check if value is present in column
			for (int r = 0; r < 9; r++)
			{
				cell = Puzzle[r, col];
				if (cell.Value == val) return false;
			}

			//check if value is present in the row
			for (int c = 0; c < 9; c++)
			{
				cell = Puzzle[row, c];
				if (cell.Value == val) return false;
			}

			//check for the value in the 3 X 3 block
			int i = row / 3;
			int j = col / 3;
			for (int a = 0; a < 3; a++)
			for (int b = 0; b < 3; b++)
			{
				cell = Puzzle[3 * i + a, 3 * j + b];
				if (val == cell.Value) return false;
			}

			return true;
		}
		
		do
		{
			if (CheckForNakedSinglesOrCompletion(out bool changed))
			{
				return true;
			}
			if (changed)
			{
				continue;
			}
			if (!RunTechnique())
			{
				return false;
			}
		} while (true);
	}

	private bool RunTechnique()
	{
		foreach (SolverTechnique t in _techniques)
		{
			if (t.Function.Invoke())
			{
				return true;
			}
		}
		return false;
	}
}