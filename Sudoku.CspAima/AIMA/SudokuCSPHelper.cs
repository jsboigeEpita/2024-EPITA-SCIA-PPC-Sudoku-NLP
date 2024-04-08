using System.Globalization;
using System.Text.RegularExpressions;
using aima.core.search.csp;
using Sudoku.Shared;


namespace Sudoku.CSPwithAIMA
{
    public class SudokuCSPHelper
    {
        private static object _BaseSudokuCSPLock = new object();
        private static DynamicCSP _BaseSudokuCSP;
        static SudokuCSPHelper()
        {
            GetSudokuBaseCSP();
        }

        // Main Function to create CSP from given SudokuGrid s
        public static CSP GetSudokuCSP(SudokuGrid s)
        {
            // Construct full CSP
            var toReturn = GetSudokuBaseCSP();
            var cellVars = toReturn.getVariables();


            // Masque of initial grid
            var mask = new Dictionary<int, int>();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    var cellVal = s.Cells[i,j];
					if (cellVal != 0)
                        mask[i * 9 + j] = cellVal;
                }
            }

            // Add constraints corresponding to the mask
            var maskQueue = new Queue<int>(mask.Keys);

            var currentMaskIdx = maskQueue.Dequeue();
            var currentVarName = GetVarName(currentMaskIdx / 9, currentMaskIdx % 9);
            foreach (Variable objVar in cellVars.toArray())
            {
                if (objVar.getName() == currentVarName)
                {
                    var cellValue = mask[currentMaskIdx];
                    toReturn.setDomain(objVar, new Domain(new object[] { cellValue }));
                    if (maskQueue.Count == 0)
                        break;
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

        // CSP Object
        private static CSP GetSudokuBaseCSP()
        {
            if (_BaseSudokuCSP == null)
            {
                lock (_BaseSudokuCSPLock)
                {
                    if (_BaseSudokuCSP == null)
                    {
                        var toReturn = new DynamicCSP();

                        // DOMAIN definition
                        var cellPossibleValues = Enumerable.Range(1, 9);
                        //constructor Domain(java.util.List<?> values)
                        //var cellDomain = new Domain(Enumerable.Range(1, 9).ToList());
                        var cellDomain = new Domain(cellPossibleValues.Cast<object>().ToArray());
                        
                        // VARIABLES definition
                        Variable[,] variables = new Variable[9,9];
                        for (int rowIndex = 0; rowIndex < 9; rowIndex++)
                        {
                            for (int colIndex = 0; colIndex < 9; colIndex++)
                            {
                                var varName = GetVarName(rowIndex, colIndex);
                                var cellVariable = new Variable(varName);
                                toReturn.AddNewVariable(cellVariable);
                                toReturn.setDomain(cellVariable, cellDomain);
                                variables[rowIndex, colIndex] = cellVariable;
                            }
                        }
                        
                        // CONSTRAINTS definition
                        var contraints = new List<Constraint>();
                        
                        // Constraints: rows
                        for  (int row = 0; row < 9; row++)
                        {
                            Variable[] tmp = Enumerable.Range(0, variables.GetLength(1))
                                .Select(x => variables[row, x])
                                .ToArray();
                            var ligneContraintes = SudokuCSPHelper.GetAllDiffConstraints(tmp);
                            contraints.AddRange(ligneContraintes);
                        }

                        // Constraints: columns
                        for (int col = 0; col < 9; col++)
                        {
                            Variable[] tmp = Enumerable.Range(0, variables.GetLength(0))
                                .Select(x => variables[x, col])
                                .ToArray();
                            var colContraintes = SudokuCSPHelper.GetAllDiffConstraints(tmp);
                            contraints.AddRange(colContraintes);
                        }

                        // Constraints: innerSquare
                        for (int b = 0; b < 9; b++)
                        {
                            var boiteVars = new List<Variable>();
                            var iStart = 3 * (b / 3);
                            var jStart = 3 * (b % 3);
                            for (int i = 0; i < 3; i++)
                            {
                                for (int j = 0; j < 3; j++)
                                    boiteVars.Add(variables[iStart + i, jStart + j]);
                            }
                            var boitesContraintes = SudokuCSPHelper.GetAllDiffConstraints(boiteVars);
                            contraints.AddRange(boitesContraintes);
                        }
                        
                        // Add constraints to CSP
                        foreach (var constraint in contraints)
                            toReturn.addConstraint(constraint);

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
        
        public static IEnumerable<Constraint> GetAllDiffConstraints(IList<Variable> vars)
        {
            var toReturn = new List<Constraint>();
            for (int i = 0; i < vars.Count; i++)
            {
                for (int j = i + 1; j < vars.Count; j++)
                {
                    var diffContraint = new NotEqualConstraint(vars[i], vars[j]);
                    toReturn.Add(diffContraint);
                }
            }

            return toReturn;
        }
    }
}