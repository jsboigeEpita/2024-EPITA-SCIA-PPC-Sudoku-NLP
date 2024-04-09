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
        
        public StreamWriter Writer;
        public TestSudoku()
        {
            filepath_ = "../../../../Puzzles/";
            string filePath = "../../../../data2.csv";
            Writer = new StreamWriter(filePath);
            
            var items = new List<string>
            {
                "Strategy", "Inference", "LCV", "Selection", "Time"
                ,"DomainChanges", "AssignementChanges", "Resolved"
                , "MaxStep", "Difficulty", "Sudoku"
            };
            Writer.WriteLine(string.Join(",", items));
        }
        
        public void RunCspOnSudokus()
        {
            var sudokuListEasy = SudokuGrid.ReadSudokuFile(filepath_ + "Sudoku_Easy51.txt");
            var sudokuListHard = SudokuGrid.ReadSudokuFile(filepath_ + "Sudoku_hardest.txt");
            var sudokuListTop = SudokuGrid.ReadSudokuFile(filepath_ + "Sudoku_top95.txt");

            foreach (var s in sudokuListEasy)
                testImprovedBT(s, "easy");
            foreach (var s in sudokuListHard)
                testImprovedBT(s, "hard");
            foreach (var s in sudokuListTop)
                testImprovedBT(s, "top");
            
            Writer.Close();
        }

        public void testBacktracking(SudokuGrid s, string difficulty)
        {
            var backtracking = new BacktrackingStrategy();
            Solve(s.CloneSudoku(), backtracking, "BT", "NULL", "NULL",
                "NULL", difficulty);
        }
        
        public void MinConflictsStrategy(SudokuGrid s, string difficulty)
        {
            var minConflicts = new MinConflictsStrategy(5000);
            Solve(s.CloneSudoku(), minConflicts, "MC", "NULL", "NULL",
                "NULL", difficulty);
        }

        public void testImprovedBT(SudokuGrid s, string difficulty, bool removeOptions = true)
        {
            var SelectionList = new List<IBT.Selection>(IBT.Selection.values());
            var InferenceList = new List<IBT.Inference>(IBT.Inference.values());
            if (removeOptions)
            {
                SelectionList.Remove(IBT.Selection.DEFAULT_ORDER);
                InferenceList.Remove(IBT.Inference.NONE);
            }
            
            int i = 0;
            foreach (var selection in SelectionList)
            {
                foreach (var inference in InferenceList)
                {
                    foreach (var LCV in new bool[] { true, false })
                    {
                        var improved = new ImprovedBacktrackingStrategy();
                        improved.enableLCV(LCV);
                        improved.setVariableSelection(selection);
                        improved.setInference(inference);

                        Solve(s.CloneSudoku(), improved, "IBT", inference.ToString(), selection.ToString(),
                            LCV.ToString(), difficulty);
                        i++;
                    }
                }
            }
        }

        public SudokuGrid Solve(SudokuGrid s, SolutionStrategy _Strategy, String name
            ,  string inference, string selection, string LCV, string difficulty)
        {
            //Construction of CSP using CspHelper
            var objCSp = SudokuCSPHelper.GetSudokuCSP(s);

            var infos = new StepCounter();
            _Strategy.addCSPStateListener(infos);

            var cpySudoku = s.CloneSudoku();
            string sudokuRaw = new string(cpySudoku.ToString().Where(char.IsDigit).ToArray());
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var assignment = _Strategy.solve(objCSp);
            
            stopwatch.Stop();
            TimeSpan elapsedTime = stopwatch.Elapsed;
            
            if (assignment == null)
                Console.WriteLine("Not Working!");

            SudokuCSPHelper.SetValuesFromAssignment(assignment, s);

            var items2 = new List<string>
            {
                name, inference, LCV, selection
                , elapsedTime.toString(), infos.DomainCount.ToString()
                , infos.AssignmentCount.ToString(), s.IsValid(cpySudoku).ToString()
                , "5000", difficulty, sudokuRaw
            };
            
            Console.WriteLine(infos.getResults() + "  " + elapsedTime);
            Writer.WriteLine(string.Join(',', items2));

            return s;
        }
    }
}