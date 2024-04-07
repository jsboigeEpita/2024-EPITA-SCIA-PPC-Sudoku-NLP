using System.CodeDom.Compiler;
using aima.core.search.csp;
using Sudoku.CSPwithAIMA;
using IBT = aima.core.search.csp.ImprovedBacktrackingStrategy;
using Sudoku.Shared;

using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using ikvm.extensions;

namespace Sudoku.CSPwithAIMA
{
    public class TestSudoku
    {
        public string filepath_;
        public string StrSelection_;
        public string StrInference_;

        public string StrLCV_;
        
        public StreamWriter Writer;
        public TestSudoku(string filepath)
        {
            filepath_ = "../../../../Puzzles/" + filepath;
            string filePath = "../../../../data.csv";
            Writer = new StreamWriter(filePath);
        }

        
        public void testMain()
        {
            var items = new List<string>
            {
                "Strategy", "Inference", "LCV", "Selection", "Time"
                ,"DomainChanges", "AssignementChanges", "Resolved"
                , "MaxStep", "Difficulty", "Sudoku"
            };
            Writer.WriteLine(string.Join(",", items));
            
            var sudokuList = SudokuGrid.ReadSudokuFile(filepath_);
            foreach (var s in sudokuList)
            {
                testImprovedBT(s, "easy");
            }
            Writer.Close();
        }
        
        public void testImprovedBT(SudokuGrid s, String difficulty)
        {
            foreach (var selection in IBT.Selection.values())
            {
                Console.WriteLine(selection);
                foreach (var  inference in IBT.Inference.values())
                {
                    //foreach (var LCV in new bool[] { true, false })
                    //{
                        // var inference = IBT.Inference.AC3;
                        var LCV = false;
                        var improved =  new ImprovedBacktrackingStrategy();
                        improved.enableLCV(LCV);
                        improved.setVariableSelection(selection);
                        improved.setInference(inference);

                        // StrInference_ = inference.toString();
                        // StrLCV_ = LCV.ToString();
                        // StrSelection_ = selection.ToString();
                        // RunFunctionWithTimeout(() => Solve(s, improved, "IBT", difficulty));
                        Solve(s.CloneSudoku(), improved, "IBT", difficulty, inference.ToString(), selection.ToString(), LCV.ToString());
                        // Solve(s, improved, "IBT", inference.toString(),
                        //     selection.ToString(),
                        //     LCV.ToString(), difficulty);
                        // }
                }
            }
        }
        
        public SudokuGrid Solve(SudokuGrid s, SolutionStrategy _Strategy, String name
            ,  string difficulty, string inference, string selection, string LCV)
        {
            //Construction du CSP en utilisant CspHelper
            var objCSp = SudokuCSPHelper.GetSudokuCSP(s);

            var infos = new StepCounter();
            _Strategy.addCSPStateListener(infos);

            var cpySudoku = s.CloneSudoku();
            string sudokuRaw = new string(cpySudoku.ToString().Where(char.IsDigit).ToArray());
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            // Utilisation de la stratégie pour résoudre le CSP
            var assignment = _Strategy.solve(objCSp);
            
            stopwatch.Stop();
            TimeSpan elapsedTime = stopwatch.Elapsed;
            
            //Utilisation de CSPHelper pour traduire l'assignation en SudokuGrid
            SudokuCSPHelper.SetValuesFromAssignment(assignment, s);

            var items2 = new List<string>
            {
                name, inference, LCV, selection
                , elapsedTime.toString(), infos.DomainCount.ToString()
                , infos.AssignmentCount.ToString(), s.IsValid(cpySudoku).ToString()
                , "5000", difficulty, sudokuRaw
            };


            // if (!(infos.DomainCount == 0 && infos.AssignmentCount == 81))
            // {
                Console.WriteLine(infos.getResults() + " " + elapsedTime);
                Writer.WriteLine(string.Join(',', items2));
            // }

            return s;
        }
        
        // public bool RunFunctionWithTimeout(Action function)
        // {
        //     var timeout = TimeSpan.FromSeconds(20);
        //     using (var cancellationTokenSource = new CancellationTokenSource())
        //     {
        //         Task task = Task.Run(() => function(), cancellationTokenSource.Token);
        //
        //         if (!task.Wait(timeout))
        //         {
        //             cancellationTokenSource.Cancel();
        //             return false;
        //         }
        //         return true;
        //     }
        // }
        
    }
}
