using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;
using aima.core.search.csp;
using Sudoku.Shared;
using java.util;


namespace Sudoku.CSPwithAIMA
{
    public class SudokuCSPHelper
    {
        private readonly static CSP _BaseSudokuCSP;
        static SudokuCSPHelper()
        {
            _BaseSudokuCSP = BuildSudokuBaseCSP();
        }

        // Main Function to create CSP from given SudokuGrid s
        public static CSP GetSudokuCSP(SudokuGrid s)
        {
            // Wait the end of the _BaseSudokuCSP build
            while (_BaseSudokuCSP == null)
                continue;

            var csp = _BaseSudokuCSP.copyDomains();

            foreach (Variable objVar in csp.getVariables().toArray())
            {
                GetIndices(objVar, out int row, out int col);
                var cell = s.Cells[row,col];
                if (cell == 0)
                    continue;

                csp.setDomain(objVar, new Domain(new object[] { cell }));
            }

            return csp;
        }

        // Récupération de la solution
        public static void SetValuesFromAssignment(Assignment a, SudokuGrid s)
        {
            foreach (Variable objVar in a.getVariables().toArray())
            {
                GetIndices(objVar, out int rowIdx, out int colIdx);
                int value = (int)a.getAssignment(objVar);
                s.Cells[rowIdx,colIdx]= value;
            }
        }

        // CSP Object
        private static CSP BuildSudokuBaseCSP()
        {
            // VARIABLES definition
            Variable[,] variables = new Variable[9,9];
            for (int rowIndex = 0; rowIndex < 9; rowIndex++)
            {
                for (int colIndex = 0; colIndex < 9; colIndex++)
                {
                    var varName = GetVarName(rowIndex, colIndex);
                    var cellVariable = new Variable(varName);
                    variables[rowIndex, colIndex] = cellVariable;
                }
            }

            var java_variables = new ArrayList();
            foreach (var v in variables)
                java_variables.Add(v);

            CSP csp = new CSP(java_variables);

            var cellDomain = new Domain(Enumerable.Range(1, 9).Cast<object>().ToArray());
            foreach (var v in variables)
                csp.setDomain(v, cellDomain);
            
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
                csp.addConstraint(constraint);

            return csp;
        }
        
        
        private static Regex _NameRegex =
            new Regex(@"cell(?<row>\d)(?<col>\d)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static void GetIndices(Variable obVariable, out int rowIdx, out int colIdx)
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
            var constraints = new List<Constraint>();
            for (int i = 0; i < vars.Count; i++)
            {
                for (int j = i + 1; j < vars.Count; j++)
                {
                    var diffContraint = new NotEqualConstraint(vars[i], vars[j]);
                    constraints.Add(diffContraint);
                }
            }

            return constraints;
        }
    }
}