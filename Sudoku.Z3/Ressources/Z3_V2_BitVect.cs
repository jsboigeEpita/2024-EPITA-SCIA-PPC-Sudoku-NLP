using Sudoku.Shared;
using Microsoft.Z3;

public class Z3V2_BitVector : Z3Optimization
    {
        private static readonly int Size = 9;
        private static readonly int BlockSize = 3;

        private readonly Context ctx = new(new Dictionary<string, string> { { "model", "true" } });

        public override SudokuGrid solve(SudokuGrid s)
        {
            var X = CreateEvalMatrix();
            var constraints = GenerateConstraints(X);

            AddSudokuInstanceConstraints(s, X, ref constraints);

            using var solver = ctx.MkSolver();
            solver.Assert(constraints.ToArray());

            if (solver.Check() != Status.SATISFIABLE)
                return s;

            return ExtractSolution(solver, X);
        }

        private BitVecExpr[][] CreateEvalMatrix()
        {
            return Enumerable.Range(0, Size)
                             .Select(i => Enumerable.Range(0, Size)
                                                    .Select(j => ctx.MkBVConst($"x_{i + 1}_{j + 1}", 4))
                                                    .ToArray())
                             .ToArray();
        }

        private List<BoolExpr> GenerateConstraints(BitVecExpr[][] X)
        {
            var constraints = new List<BoolExpr>
            {
                GenerateCellConstraints(X),
                GenerateRowConstraints(X),
                GenerateColumnConstraints(X),
                GenerateBlockConstraints(X)
            };

            return constraints;
        }

        private BoolExpr GenerateCellConstraints(BitVecExpr[][] X)
        {
            var one = ctx.MkBV(1, 4);
            var nine = ctx.MkBV(9, 4);

            return ctx.MkAnd(X.SelectMany(row =>
                row.Select(cell => ctx.MkAnd(ctx.MkBVULE(one, cell), ctx.MkBVULE(cell, nine)))
            ).ToArray());
        }

        private BoolExpr GenerateRowConstraints(BitVecExpr[][] X)
        {
            return ctx.MkAnd(X.Select(row => ctx.MkDistinct(row)).ToArray());
        }

        private BoolExpr GenerateColumnConstraints(BitVecExpr[][] X)
        {
            return ctx.MkAnd(Enumerable.Range(0, Size).Select(j =>
                ctx.MkDistinct(Enumerable.Range(0, Size).Select(i => X[i][j]).ToArray())
            ).ToArray());
        }

        private BoolExpr GenerateBlockConstraints(BitVecExpr[][] X)
        {
            return ctx.MkAnd(Enumerable.Range(0, BlockSize).SelectMany(i =>
                Enumerable.Range(0, BlockSize).Select(j =>
                    ctx.MkDistinct(Enumerable.Range(0, BlockSize).SelectMany(di =>
                        Enumerable.Range(0, BlockSize).Select(dj => X[BlockSize * i + di][BlockSize * j + dj])
                    ).ToArray())
                )
            ).ToArray());
        }

        private void AddSudokuInstanceConstraints(SudokuGrid s, BitVecExpr[][] X, ref List<BoolExpr> constraints)
        {
            for (int i = 0; i < Size; i++)
                for (int j = 0; j < Size; j++)
                    if (s.Cells[i, j] != 0)
                        constraints.Add(ctx.MkEq(X[i][j], ctx.MkBV(s.Cells[i, j], 4)));
        }

        private SudokuGrid ExtractSolution(Solver solver, BitVecExpr[][] X)
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