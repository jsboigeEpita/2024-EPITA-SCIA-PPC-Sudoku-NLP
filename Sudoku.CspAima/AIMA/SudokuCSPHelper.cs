using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using aima.core.search.csp;
using Sudoku.Shared;

//using aima.core.search.csp.examples;

namespace Sudoku.CSPwithAIMA
{
    public class SudokuCSPHelper

    {

        static SudokuCSPHelper()
        {
            GetSudokuBaseCSP();
        }

        // Fonction principale pour construire le CSP à partir d'un masque de Sudoku à résoudre
        public static CSP GetSudokuCSP(SudokuGrid s)
        {

            //initialisation à l'aide des contraintes communes

            var toReturn = GetSudokuBaseCSP();

            // Ajout des contraintes spécifiques au masque fourni

            //var sArray = s.getInitialSudoku();
            var cellVars = toReturn.getVariables();


            //récupération du masque
            var mask = new Dictionary<int, int>();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
					// var cellVal = s.Cells[i][j];
                    var cellVal = s.Cells[i,j];
					if (cellVal != 0)
                    {
                        mask[i * 9 + j] = cellVal;

                    }
                }
            }

            //Ajout des contraintes de masque en faisant défiler les variables existantes

            var maskQueue = new Queue<int>(mask.Keys);

            var currentMaskIdx = maskQueue.Dequeue();
            var currentVarName = GetVarName(currentMaskIdx / 9, currentMaskIdx % 9);
            foreach (Variable objVar in cellVars.toArray())
            {
                if (objVar.getName() == currentVarName)
                {
                    //toReturn.addConstraint(new ValueConstraint(objVar, mask[currentMaskIdx]));
                    var cellValue = mask[currentMaskIdx];
                    toReturn.setDomain(objVar, new Domain(new object[] { cellValue }));
                    if (maskQueue.Count == 0)
                    {
                        break;
                    }
                    currentMaskIdx = maskQueue.Dequeue();
                    currentVarName = GetVarName(currentMaskIdx / 9, currentMaskIdx % 9);
                }

            }

            return toReturn;

        }

        // Récupération de la solution
        public static void SetValuesFromAssignment(Assignment a, SudokuGrid s)
        {

            foreach (Variable objVar in a.getVariables().toArray())
            {
                int rowIdx = 0;
                int colIdx = 0;
                GetIndices(objVar, ref rowIdx, ref colIdx);
                int value = (int)a.getAssignment(objVar);
                s.Cells[rowIdx,colIdx]= value;
            }
        }

        // CSP de Sudoku sans masque (les règles)
        private static CSP GetSudokuBaseCSP()
        {
            if (_BaseSudokuCSP == null)
            {
                lock (_BaseSudokuCSPLock)
                {
                    if (_BaseSudokuCSP == null)
                    {
                        var toReturn = new DynamicCSP();


                        //Domaine

                        var cellPossibleValues = Enumerable.Range(1, 9);
                        var cellDomain = new Domain(cellPossibleValues.Cast<object>().ToArray());



                        //Variables

                        var variables = new Dictionary<int, Dictionary<int, Variable>>();
                        for (int rowIndex = 0; rowIndex < 9; rowIndex++)
                        {
                            var rowVars = new Dictionary<int, Variable>();
                            for (int colIndex = 0; colIndex < 9; colIndex++)
                            {
                                var varName = GetVarName(rowIndex, colIndex);
                                var cellVariable = new Variable(varName);
                                toReturn.AddNewVariable(cellVariable);
                                toReturn.setDomain(cellVariable, cellDomain);
                                rowVars[colIndex] = cellVariable;
                            }

                            variables[rowIndex] = rowVars;
                        }



                        //Contraintes

                        var contraints = new List<Constraint>();

                        // Lignes
                        foreach (var objPair in variables)
                        {
                            var ligneVars = objPair.Value.Values.ToList();

                            var ligneContraintes = SudokuCSPHelper.GetAllDiffConstraints(ligneVars);
                            contraints.AddRange(ligneContraintes);
                        }

                        //colonnes
                        for (int j = 0; j < 9; j++)
                        {
                            var jClosure = j;
                            var colVars = variables.Values.SelectMany(x => { 
                                return new Variable[] { x[jClosure] 
                            }; }).ToList();
                            var colContraintes = SudokuCSPHelper.GetAllDiffConstraints(colVars);
                            contraints.AddRange(colContraintes);
                        }

                        //Boites
                        for (int b = 0; b < 9; b++)
                        {
                            var boiteVars = new List<Variable>();
                            var iStart = 3 * (b / 3);
                            var jStart = 3 * (b % 3);
                            for (int i = 0; i < 3; i++)
                            {
                                for (int j = 0; j < 3; j++)
                                {
                                    boiteVars.Add(variables[iStart + i][jStart + j]);
                                }
                            }
                            var boitesContraintes = SudokuCSPHelper.GetAllDiffConstraints(boiteVars);
                            contraints.AddRange(boitesContraintes);
                        }


                        //Ajout de toutes les contraintes
                        foreach (var constraint in contraints)
                        {
                            toReturn.addConstraint(constraint);
                        }

                        //return toReturn;
                        _BaseSudokuCSP = toReturn;
                    }

                }
            }

            return (CSP)_BaseSudokuCSP.Clone();
        }



        private static Regex _NameRegex =
            new Regex(@"cell(?<row>\d)(?<col>\d)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static void GetIndices(Variable obVariable, ref int rowIdx, ref int colIdx)
        {
            var objMatch = _NameRegex.Match(obVariable.getName());
            rowIdx = int.Parse(objMatch.Groups["row"].Value, CultureInfo.InvariantCulture);
            colIdx = int.Parse(objMatch.Groups["col"].Value, CultureInfo.InvariantCulture);
        }

        private static string GetVarName(int rowIndex, int colIndex)
        {
            return $"cell{rowIndex}{colIndex}";

        }

        private static object _BaseSudokuCSPLock = new object();
        private static DynamicCSP _BaseSudokuCSP;



        public static IEnumerable<Constraint> GetAllDiffConstraints(IList<Variable> vars)
        {
            var toReturn = new List<Constraint>();
            for (int i = 0; i < vars.Count; i++)
            {
                for (int j = i + 1; j < vars.Count; j++)
                {
                    var diffContraint =
                        new NotEqualConstraint(vars[i],
                            vars[j]);
                    toReturn.Add(diffContraint);
                }
            }

            return toReturn;
        }



    }
}