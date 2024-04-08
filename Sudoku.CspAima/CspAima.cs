using System;
using System.Diagnostics;
using aima.core.search.csp;
using aima.core.search.framework;
using Sudoku.Shared;


namespace Sudoku.CSPwithAIMA
{
    public abstract class CspAima : ISudokuSolver
    {

        public CspAima()
        {
            _Strategy = GetStrategy();
        }

        private readonly SolutionStrategy _Strategy;

        // Vous n'avez plus qu'à suivre les commentaires, ça devrait aller vite.
        public SudokuGrid Solve(SudokuGrid s)
        {
            //Construction du CSP en utilisant CspHelper
            var objCSp = SudokuCSPHelper.GetSudokuCSP(s);

            // Utilisation de la stratégie pour résoudre le CSP
            var assignment = _Strategy.solve(objCSp);

            //Utilisation de CSPHelper pour traduire l'assignation en SudokuGrid
            SudokuCSPHelper.SetValuesFromAssignment(assignment, s);

            // var t = new TestSudoku();
            // t.testMain();
            
            return s;
        }

        protected abstract SolutionStrategy GetStrategy();

    }

    //Voilà un premier solver, je vous laisse implémenter les autres avec différentes combinaisons de paramètres pour pouvoir contraster leurs performances

    //----------------------------------------------------------------------------------------------------------------
    //Avec EnableLCV = false,
    public class CSPMRVDegLCVFCSolver : CspAima
    {
        protected override SolutionStrategy GetStrategy()
        {
            var objStrategyInfo = new CSPStrategyInfo
            {
                EnableLCV = false,
                Inference = CSPInference.AC3,
                Selection = CSPSelection.MRVDeg,
                StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
                MaxSteps = 5000
            };
            return objStrategyInfo.GetStrategy();
        }
    }

    // public class CSPMVRFCSolver : CspAima
    // {
    //     protected override SolutionStrategy GetStrategy()
    //     {
    //         var objStrategyInfo = new CSPStrategyInfo
    //         {
    //             EnableLCV = false,
    //             Inference = CSPInference.ForwardChecking,
    //             Selection = CSPSelection.MRV,
    //             StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
    //             MaxSteps = 5000
    //         };
    //         return objStrategyInfo.GetStrategy();
    //     }
    // }
    //
    // public class CSPMRVAC3Solver : CspAima
    // {
    //     protected override SolutionStrategy GetStrategy()
    //     {
    //         var objStrategyInfo = new CSPStrategyInfo
    //         {
    //             EnableLCV = false,
    //             Inference = CSPInference.AC3,
    //             Selection = CSPSelection.MRV,
    //             StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
    //             MaxSteps = 5000
    //         };
    //         return objStrategyInfo.GetStrategy();
    //     }
    // }
    //
    // public class CSPMVDegAC3Solver : CspAima
    // {
    //     protected override SolutionStrategy GetStrategy()
    //     {
    //         var objStrategyInfo = new CSPStrategyInfo
    //         {
    //             EnableLCV = false,
    //             Inference = CSPInference.AC3,
    //             Selection = CSPSelection.MRVDeg,
    //             StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
    //             MaxSteps = 5000
    //         };
    //         return objStrategyInfo.GetStrategy();
    //     }
    // }
    // //----------------------------------------------------------------------------------------------------------------
    // //Avec EnableLCV = true,
    //
    // public class CSPMRVDegLCVFC2Solver : CspAima
    // {
    //     protected override SolutionStrategy GetStrategy()
    //     {
    //         var objStrategyInfo = new CSPStrategyInfo
    //         {
    //             EnableLCV = true,
    //             Inference = CSPInference.ForwardChecking,
    //             Selection = CSPSelection.MRVDeg,
    //             StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
    //             MaxSteps = 5000
    //         };
    //         return objStrategyInfo.GetStrategy();
    //     }
    // }
    //
    // public class CSPMVRFC2Solver : CspAima
    // {
    //     protected override SolutionStrategy GetStrategy()
    //     {
    //         var objStrategyInfo = new CSPStrategyInfo
    //         {
    //             EnableLCV = true,
    //             Inference = CSPInference.ForwardChecking,
    //             Selection = CSPSelection.MRV,
    //             StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
    //             MaxSteps = 5000
    //         };
    //         return objStrategyInfo.GetStrategy();
    //     }
    // }
    //
    // public class CSPMRVAC32Solver : CspAima
    // {
    //     protected override SolutionStrategy GetStrategy()
    //     {
    //         var objStrategyInfo = new CSPStrategyInfo
    //         {
    //             EnableLCV = true,
    //             Inference = CSPInference.AC3,
    //             Selection = CSPSelection.MRV,
    //             StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
    //             MaxSteps = 5000
    //         };
    //         return objStrategyInfo.GetStrategy();
    //     }
    // }
    //
    // public class CSPMVDegAC32Solver : CspAima
    // {
    //     protected override SolutionStrategy GetStrategy()
    //     {
    //         var objStrategyInfo = new CSPStrategyInfo
    //         {
    //             EnableLCV = true,
    //             Inference = CSPInference.AC3,
    //             Selection = CSPSelection.MRVDeg,
    //             StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
    //             MaxSteps = 5000
    //         };
    //         return objStrategyInfo.GetStrategy();
    //     }
    // }
}

/*L'attribut LCV (Least Constraining Value) permet de mettre l'accent sur les valeurs qui limitent le moins le nombre d'autres valeurs possibles pour les variables non assignées.

L'attribut Inference est une méthode utilisée pour développer des algorithmes informatisés afin de résoudre un problème donné.
Ici, la méthode "Forward Checking" est utilisée pour conserver et vérifier de nouvelles inférences qui découlent des affectations.

L'attribut Selection représente la façon dont la variable à affecter est choisie.
Dans ce cas, le "MRV-Deg" est utilisé, qui se compose du plus petit Reste à Affecter (MRV) couplé à la technique de sommation des incidentes (Deg).

L'attribut StrategyType détermine quelle stratégie de recherche backtrack sera utilisée.
Dans ce cas, l'algorithme à recherche arrière améliorée est utilisé.

Enfin, l’attribut MaxSteps détermine le nombre maximal d'étapes que l'algorithme effectuera avant d'interrompre la recherche et de retourner un échec en option.*/


    //----------------------------------------------------------------------------------------------------------------
    //Avec StrategyType = CSPStrategy.MinConflictsStrategy,

    // public class CSPMRVDegLCVFC8Solver : CSPSolverBase
    // {
    //     protected override SolutionStrategy GetStrategy()
    //     {
    //         var objStrategyInfo = new CSPStrategyInfo
    //         {
    //             EnableLCV = true,
    //             Inference = CSPInference.ForwardChecking,
    //             Selection = CSPSelection.MRVDeg,
    //             StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
    //             MaxSteps = 5000
    //         };
    //         return objStrategyInfo.GetStrategy();
    //     }
    // }
    // public class CSPMVRFC8Solver : CSPSolverBase
    // {
    //     protected override SolutionStrategy GetStrategy()
    //     {
    //         var objStrategyInfo = new CSPStrategyInfo
    //         {
    //             EnableLCV = false,
    //             Inference = CSPInference.ForwardChecking,
    //             Selection = CSPSelection.MRV,
    //             StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
    //             MaxSteps = 5000
    //         };
    //         return objStrategyInfo.GetStrategy();
    //     }
    // }
    // public class CSPMRVAC38Solver : CSPSolverBase
    // {
    //     protected override SolutionStrategy GetStrategy()
    //     {
    //         var objStrategyInfo = new CSPStrategyInfo
    //         {
    //             EnableLCV = false,
    //             Inference = CSPInference.AC3,
    //             Selection = CSPSelection.MRV,
    //             StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
    //             MaxSteps = 5000
    //         };
    //         return objStrategyInfo.GetStrategy();
    //     }
    // }

    // public class CSPMVDegAC38Solver : CSPSolverBase
    // {
    //     protected override SolutionStrategy GetStrategy()
    //     {
    //         var objStrategyInfo = new CSPStrategyInfo
    //         {
    //             EnableLCV = false,
    //             Inference = CSPInference.AC3,
    //             Selection = CSPSelection.MRVDeg,
    //             StrategyType = CSPStrategy.ImprovedBacktrackingStrategy,
    //             MaxSteps = 5000
    //         };
    //         return objStrategyInfo.GetStrategy();
    //     }
    // }

    //Voilà un deuxième solver. Il existe encore plein d'autres combinaisons possibles, je vous laisse le soin de proposer tous les solvers possibles de façon à en comparer les performances.
    //Le paramètre MaxSteps concerne la stratégie MinConflicts




// PYTHONNNN NUL


// using Sudoku.Shared;
// using Python.Runtime;
// using System.Runtime.Versioning;
// using Sudoku.CspAima;
//
// namespace Sudoku.CspAima
// {
//     public class CspAima : PythonSolverBase
//     {
//         public override SudokuGrid Solve(SudokuGrid s)
//         {
//             using (PyModule scope = Py.CreateScope())
//             {
//
// 	            InitializePythonComponents();
//             		// Injectez le script de conversion
//             		AddNumpyConverterScript(scope);
//
//             		// Convertissez le tableau .NET en tableau NumPy
//             		var pyCells = AsNumpyArray(s.Cells, scope);
//
//             		// create a Python variable "instance"
//             		scope.Set("instance", pyCells);
//
//             		// run the Python script
//             		string code = Resources.Solver_py;
//                     Console.WriteLine($"------\nCode : '{code}'\n-----");
//             		scope.Exec(code);
//                     
//             		PyObject result = scope.Get("result");
//
//             		// Convertissez le résultat NumPy en tableau .NET
//             		var managedResult = AsManagedArray(scope, result);
//
//             		return new SudokuGrid() { Cells = managedResult };
//             	}
//             s.Cells[0,0] = 9;
//             s.Cells[0,1] = 9;
//             s.Cells[0,2] = 9;
//             return s;
//         }
//         
//          protected override void InitializePythonComponents()
//          {
// 	        // declare your pip packages here
// 	         InstallPipModule("numpy");
// 	         InstallPipModule("pycsp3");
// 	         // dynamic pycsp3 = Py.Import("pycsp3");
// 	         base.InitializePythonComponents();
//          }
//     }
// }