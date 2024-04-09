using Sudoku.Shared;

namespace CustomDlxLib
{
    /// <summary>
    /// Dlx solver from scratch optimized type wise
    /// </summary>
    public class OptimizedDlx
    {
        private SudokuGrid s;
        private readonly ColumnNode _root = new();
        private readonly LinkedList<Node> _solution = new();

        public void Solve(SudokuGrid sudoku)
        {
            s = sudoku;
            
            // The following DateTime comments are used to measure the time taken by each step of the algorithm to make graphs
            // var start = DateTime.Now;
            Init();
            // var initTime = (DateTime.Now - start).TotalMilliseconds;

            // start = DateTime.Now;
            Search();
            // var searchTime = (DateTime.Now - start).TotalMilliseconds;

            // start = DateTime.Now;
            foreach (Node node in _solution)
            {
                int value = node.RowIndex % 9;
                int j = (node.RowIndex / 9) % 9;
                int i = node.RowIndex / 81;
                s.Cells[i, j] = value + 1;
            }
            // var convertTime = (DateTime.Now - start).TotalMilliseconds;

            // using var file = new StreamWriter("T_time.csv", true);
            // file.WriteLine($"{initTime},{searchTime},{convertTime}");
        }

        /// <summary>
        /// Init the four-way-linked representation of the exact cover problem into a matrix of nodes.
        /// </summary>
        private void Init()
        {
            Node c = _root;
            ColumnNode[] columnsNodes = new ColumnNode[324];

            for (short i = 0; i < 324; i++)
            {
                ColumnNode newColumn = new ColumnNode();
                columnsNodes[i] = newColumn;
                newColumn.Up = newColumn;
                newColumn.Down = newColumn;

                c.Right = newColumn;
                newColumn.Left = c;
                c = newColumn;
            }

            c.Right = _root;
            _root.Left = c;


            // create all nodes and link them to their neighbors
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    int blockIndex = j / 3 + i / 3 * 3;
                    int singleColumnIndex = 9 * i + j;

                    int value = s.Cells[i, j] - 1;
                    int rowNumberConstraintIndex = 81 + 9 * i;
                    int columnNumberConstraintIndex = 162 + 9 * j;
                    int boxNumberConstraintIndex = 243 + blockIndex * 9;

                    // if a cell in the sudoku is filled, only one row of nodes is created
                    if (value >= 0)
                    {
                        int rowIndex = 81 * i + 9 * j + value;

                        // RC=RowColumn RN=RowNumber CN=ColumnNumber BN=BoxNumber
                        var rcColumnNode = columnsNodes[singleColumnIndex];
                        var rnColumnNode = columnsNodes[rowNumberConstraintIndex + value];
                        var cnColumnNode = columnsNodes[columnNumberConstraintIndex + value];
                        var bnColumnNode = columnsNodes[boxNumberConstraintIndex + value];

                        rcColumnNode.Size++;
                        rnColumnNode.Size++;
                        cnColumnNode.Size++;
                        bnColumnNode.Size++;


                        // always 4 1s in the row RC, RN, CN, BN
                        var rcNode = new Node(rcColumnNode, rowIndex);
                        var rnNode = new Node(rnColumnNode, rowIndex);
                        var cnNode = new Node(cnColumnNode, rowIndex);
                        var bnNode = new Node(bnColumnNode, rowIndex);

                        rcNode.Left = bnNode;
                        rcNode.Right = rnNode;

                        rnNode.Left = rcNode;
                        rnNode.Right = cnNode;

                        cnNode.Left = rnNode;
                        cnNode.Right = bnNode;

                        bnNode.Left = cnNode;
                        bnNode.Right = rcNode;

                        rcNode.Down = rcColumnNode;
                        rcNode.Up = rcColumnNode.Up;
                        rcColumnNode.Up.Down = rcNode;
                        rcColumnNode.Up = rcNode;

                        rnNode.Down = rnColumnNode;
                        rnNode.Up = rnColumnNode.Up;
                        rnColumnNode.Up.Down = rnNode;
                        rnColumnNode.Up = rnNode;

                        cnNode.Down = cnColumnNode;
                        cnNode.Up = cnColumnNode.Up;
                        cnColumnNode.Up.Down = cnNode;
                        cnColumnNode.Up = cnNode;

                        bnNode.Down = bnColumnNode;
                        bnNode.Up = bnColumnNode.Up;
                        bnColumnNode.Up.Down = bnNode;
                        bnColumnNode.Up = bnNode;
                    }
                    // otherwise 9 rows of nodes are created
                    else
                    {
                        int rowIndex = 81 * i + 9 * j;
                        for (byte d = 0; d < 9; d++)
                        {
                            var rcColumnNode = columnsNodes[singleColumnIndex];
                            var rnColumnNode = columnsNodes[rowNumberConstraintIndex + d];
                            var cnColumnNode = columnsNodes[columnNumberConstraintIndex + d];
                            var bnColumnNode = columnsNodes[boxNumberConstraintIndex + d];

                            rcColumnNode.Size++;
                            rnColumnNode.Size++;
                            cnColumnNode.Size++;
                            bnColumnNode.Size++;

                            // always 4 1s in the row RC, RN, CN, BN
                            var rcNode = new Node(rcColumnNode, rowIndex);
                            var rnNode = new Node(rnColumnNode, rowIndex);
                            var cnNode = new Node(cnColumnNode, rowIndex);
                            var bnNode = new Node(bnColumnNode, rowIndex++);

                            rcNode.Left = bnNode;
                            rcNode.Right = rnNode;

                            rnNode.Left = rcNode;
                            rnNode.Right = cnNode;

                            cnNode.Left = rnNode;
                            cnNode.Right = bnNode;

                            bnNode.Left = cnNode;
                            bnNode.Right = rcNode;


                            rcNode.Down = rcColumnNode;
                            rcNode.Up = rcColumnNode.Up;
                            rcColumnNode.Up.Down = rcNode;
                            rcColumnNode.Up = rcNode;

                            rnNode.Down = rnColumnNode;
                            rnNode.Up = rnColumnNode.Up;
                            rnColumnNode.Up.Down = rnNode;
                            rnColumnNode.Up = rnNode;


                            cnNode.Down = cnColumnNode;
                            cnNode.Up = cnColumnNode.Up;
                            cnColumnNode.Up.Down = cnNode;
                            cnColumnNode.Up = cnNode;

                            bnNode.Down = bnColumnNode;
                            bnNode.Up = bnColumnNode.Up;
                            bnColumnNode.Up.Down = bnNode;
                            bnColumnNode.Up = bnNode;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Covers the column, it removes all the nodes of the column of the linked list
        /// </summary>
        /// <param name="c">The node to cover</param>
        private void Cover(Node c)
        {
            c.Right.Left = c.Left;
            c.Left.Right = c.Right;
            for (var i = c.Down; i != c; i = i.Down)
            {
                for (var j = i.Right; j != i; j = j.Right)
                {
                    j.Down.Up = j.Up;
                    j.Up.Down = j.Down;
                    j.Column.Size--;
                }
            }
        }

        /// <summary>
        /// Uncovers the column, it adds back all the nodes of the column to the linked list
        /// </summary>
        /// <param name="c">The node to uncover</param>
        private void Uncover(Node c)
        {
            for (var i = c.Up; i != c; i = i.Up)
            {
                for (var j = i.Left; j != i; j = j.Left)
                {
                    j.Column.Size++;
                    j.Down.Up = j;
                    j.Up.Down = j;
                }
            }

            c.Right.Left = c;
            c.Left.Right = c;
        }

        /// <summary>
        /// Solve the exact cover problem.
        /// Recursively search the solution using the principles of backtracking with the cover and uncover methods.
        /// While all the conditions are met, we continue to cover the columns, otherwise we uncover them.
        /// The solution is progressively stored in "solution", a linked list of nodes.
        /// </summary>
        private bool Search()
        {
            if (_root.Right == _root)
            {
                return true;
            }

            var selected = (ColumnNode)_root.Right;
            int selectedSize = selected.Size;
            for (var i = (ColumnNode)_root.Right; i != _root; i = (ColumnNode)i.Right)
            {
                if (i.Size < selectedSize)
                {
                    selected = i;
                    selectedSize = i.Size;
                }
            }

            Cover(selected);

            for (var i = selected.Down; i != selected; i = i.Down)
            {
                _solution.AddLast(i);

                for (var j = i.Right; j != i; j = j.Right)
                {
                    Cover(j.Column);
                }

                if (Search())
                {
                    return true;
                }

                _solution.RemoveLast();

                for (var j = i.Left; j != i; j = j.Left)
                {
                    Uncover(j.Column);
                }
            }

            Uncover(selected);

            return false;
        }

        /// <summary>
        /// The class Node contains all the information about the left, right, up and down nodes as well as the rowIndex
        /// in order to find the value once we resolve the exact cover problem and column in which the node is in.
        /// We created a class ColumnNode inherited from Node that is a property of the columns. This ColumnNode
        /// contains the information about the size of the column.
        /// </summary>
        public class Node
        {
            public Node Left = null!;
            public Node Right = null!;
            public Node Up = null!;
            public Node Down = null!;
            public readonly ColumnNode Column;
            public readonly int RowIndex;

            public Node()
            {
            }

            public Node(ColumnNode column, int rowIndex)
            {
                Column = column;
                RowIndex = rowIndex;
            }
        }

        public class ColumnNode : Node
        {
            internal short Size;
        }
    }
}