using Sudoku.Shared;
using Microsoft.Z3;

public class Z3_V1_IntArray : Z3Optimization
    {
        private IntExpr[][] CreateEvalMatrix(Context ctx)
        {
            IntExpr[][] X = new IntExpr[9][];

            for (uint i = 0; i < 9; i++)
            {
                X[i] = new IntExpr[9];
                for (uint j = 0; j < 9; j++)
                    X[i][j] = (IntExpr)ctx.MkConst(ctx.MkSymbol("x_" + (i + 1) + "_" + (j + 1)), ctx.IntSort);
            }

            return X;
        }

        private Expr[][] EvalCell(Context ctx, IntExpr[][] X)
        {
            Expr[][] cells_c = new Expr[9][];

            for (uint i = 0; i < 9; i++)
            {
                cells_c[i] = new BoolExpr[9];
                for (uint j = 0; j < 9; j++)
                    cells_c[i][j] = ctx.MkAnd(ctx.MkLe(ctx.MkInt(1), X[i][j]),
                                              ctx.MkLe(X[i][j], ctx.MkInt(9)));
            }

            return cells_c;
        }

        private BoolExpr[] EvalRow(Context ctx, IntExpr[][] X)
        {
            BoolExpr[] rows_c = new BoolExpr[9];

            for (uint i = 0; i < 9; i++)
                rows_c[i] = ctx.MkDistinct(X[i]);

            return rows_c;
        }

        private BoolExpr[] EvalCol(Context ctx, IntExpr[][] X)
        {
            BoolExpr[] cols_c = new BoolExpr[9];

            for (uint j = 0; j < 9; j++)
            {
                IntExpr[] column = new IntExpr[9];
                for (uint i = 0; i < 9; i++)
                    column[i] = X[i][j];

                cols_c[j] = ctx.MkDistinct(column);
            }

            return cols_c;
            {
                
            }
        }

        private BoolExpr[][] EvalSquare(Context ctx, IntExpr[][] X)
        {
            BoolExpr[][] sq_c = new BoolExpr[3][];

            for (uint i0 = 0; i0 < 3; i0++)
            {
                sq_c[i0] = new BoolExpr[3];
                for (uint j0 = 0; j0 < 3; j0++)
                {
                    IntExpr[] square = new IntExpr[9];
                    for (uint i = 0; i < 3; i++)
                        for (uint j = 0; j < 3; j++)
                            square[3 * i + j] = X[3 * i0 + i][3 * j0 + j];
                    sq_c[i0][j0] = ctx.MkDistinct(square);
                }
            }

            return sq_c;
        }

        private BoolExpr EvalSudoku(Context ctx, Expr[][] cells_c, BoolExpr[] rows_c, BoolExpr[] cols_c, BoolExpr[][] sq_c)
        {
            BoolExpr sudoku_c = ctx.MkTrue();

            foreach (BoolExpr[] t in cells_c)
                sudoku_c = ctx.MkAnd(ctx.MkAnd(t), sudoku_c);

            sudoku_c = ctx.MkAnd(ctx.MkAnd(rows_c), sudoku_c);
            sudoku_c = ctx.MkAnd(ctx.MkAnd(cols_c), sudoku_c);

            foreach (BoolExpr[] t in sq_c)
                sudoku_c = ctx.MkAnd(ctx.MkAnd(t), sudoku_c);

            return sudoku_c;
        }

        private BoolExpr CreateSudokuToSolve(Context ctx, int[,] instance, IntExpr[][] X)
        {
            BoolExpr instance_c = ctx.MkTrue();

            for (uint i = 0; i < 9; i++)
                for (uint j = 0; j < 9; j++)
                    instance_c = ctx.MkAnd(instance_c,
                        (BoolExpr)
                        ctx.MkITE(ctx.MkEq(ctx.MkInt(instance[i, j]), ctx.MkInt(0)),
                                    ctx.MkTrue(),
                                    ctx.MkEq(X[i][j], ctx.MkInt(instance[i, j]))));

            return instance_c;
        }

        private SudokuGrid SolveSudoku(Solver solver, IntExpr[][] X)
        {
            var sudoku_g = new SudokuGrid();
            Model m = solver.Model;
            Expr[,] R = new Expr[9, 9];

            for (uint i = 0; i < 9; i++)
                for (uint j = 0; j < 9; j++)
                {
                    R[i, j] = m.Evaluate(X[i][j]);
                    sudoku_g.Cells[i, j] = (int)((IntNum)R[i, j]).UInt64; 
                }

            return sudoku_g;
        }

        public override SudokuGrid solve(SudokuGrid s)
        {
            Context ctx = new Context(new Dictionary<string, string>() { { "model", "true" } });

            // 9x9 matrix of integer variables
            IntExpr[][] X = CreateEvalMatrix(ctx);

            // each cell contains a value in {1, ..., 9}
            Expr[][] cells_c = EvalCell(ctx, X);

            // each row contains a digit at most once
            BoolExpr[] rows_c = EvalRow(ctx, X);

            // each column contains a digit at most once
            BoolExpr[] cols_c = EvalCol(ctx, X);

            // each 3x3 square contains a digit at most once
            BoolExpr[][] sq_c = EvalSquare(ctx, X);

            // evaluate the sudoku with all the previous Evaluations
            BoolExpr sudoku_c = EvalSudoku(ctx, cells_c, rows_c, cols_c, sq_c);

            int[,] instance = s.Cells;

            BoolExpr instance_c = CreateSudokuToSolve(ctx, instance, X);

            Solver solver = ctx.MkSolver();
            solver.Assert(sudoku_c);
            solver.Assert(instance_c);

            if (solver.Check() != Status.SATISFIABLE)
            {
                return s;
            }
            
            return SolveSudoku(solver, X);
        }
    }