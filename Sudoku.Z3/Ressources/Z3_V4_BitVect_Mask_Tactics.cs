using Sudoku.Shared;
using Microsoft.Z3;

public class Z3_V4_BitVect_Mask_Tactics : Z3Optimization
{
    private static readonly int Size = 9;
    private static readonly int BlockSize = 3;
    private static Context ctx;
    private static Solver solver;
    private static BitVecExpr[][] X;

    static Z3_V4_BitVect_Mask_Tactics()
    {
        ctx = new Context(new Dictionary<string, string> { { "model", "true" } });
        solver = ctx.MkSolver();
        X = CreateEvalMatrix();
    }

    public override SudokuGrid solve(SudokuGrid s)
    {
        ResetSolver();

        var constraints = GenerateConstraints();

        AddSudokuInstanceConstraints(s, ref constraints);

        solver.Assert(constraints.ToArray());

        // Apply tactics to improve solving efficiency
        ApplyTactics();

        if (solver.Check() != Status.SATISFIABLE)
            return s;

        return ExtractSolution();
    }

    private static void ResetSolver()
    {
        solver.Reset();
    }

    private static BitVecExpr[][] CreateEvalMatrix()
    {
        return Enumerable.Range(0, Size)
                            .Select(i => Enumerable.Range(0, Size)
                                                .Select(j => ctx.MkBVConst($"x_{i + 1}_{j + 1}", 4))
                                                .ToArray())
                            .ToArray();
    }

    private static List<BoolExpr> GenerateConstraints()
    {
        return new List<BoolExpr>
        {
            GenerateCellConstraints(),
            GenerateRowConstraints(),
            GenerateColumnConstraints(),
            GenerateBlockConstraints()
        };
    }

    private static BoolExpr GenerateCellConstraints()
    {
        var one = ctx.MkBV(1, 4);
        var nine = ctx.MkBV(9, 4);

        return ctx.MkAnd(X.SelectMany(row =>
            row.Select(cell => ctx.MkAnd(ctx.MkBVULE(one, cell), ctx.MkBVULE(cell, nine)))
        ).ToArray());
    }

    private static BoolExpr GenerateRowConstraints()
    {
        return ctx.MkAnd(X.Select(row => ctx.MkDistinct(row)).ToArray());
    }

    private static BoolExpr GenerateColumnConstraints()
    {
        return ctx.MkAnd(Enumerable.Range(0, Size).Select(j =>
            ctx.MkDistinct(Enumerable.Range(0, Size).Select(i => X[i][j]).ToArray())
        ).ToArray());
    }

    private static BoolExpr GenerateBlockConstraints()
    {
        return ctx.MkAnd(Enumerable.Range(0, BlockSize).SelectMany(i =>
            Enumerable.Range(0, BlockSize).Select(j =>
                ctx.MkDistinct(Enumerable.Range(0, BlockSize).SelectMany(di =>
                    Enumerable.Range(0, BlockSize).Select(dj => X[BlockSize * i + di][BlockSize * j + dj])
                ).ToArray())
            )
        ).ToArray());
    }

    private static void AddSudokuInstanceConstraints(SudokuGrid s, ref List<BoolExpr> constraints)
    {
        for (int i = 0; i < Size; i++)
            for (int j = 0; j < Size; j++)
                if (s.Cells[i, j] != 0)
                    constraints.Add(ctx.MkEq(X[i][j], ctx.MkBV(s.Cells[i, j], 4)));
    }

    private static void ApplyTactics()
    {
        // Define tactics and apply them to the solver
        var tactic = ctx.MkTactic("simplify");
        var goal = ctx.MkGoal();
        goal.Assert(solver.Assertions);
        var result = tactic.Apply(goal);

        solver.Assert(result.Subgoals[0].Formulas);
    }

    private static SudokuGrid ExtractSolution()
    {
        var sudokuGrid = new SudokuGrid();

        for (int i = 0; i < Size; i++)
            for (int j = 0; j < Size; j++)
            {
                var eval = solver.Model.Evaluate(X[i][j]) as BitVecNum;
                sudokuGrid.Cells[i, j] = (int)eval.UInt64;
            }

        return sudokuGrid;
    }
}