using Sudoku.Shared;

namespace CustomDlxLib
{
    /// <summary>
    /// Dlx solver from scratch
    /// </summary>
    public class CustomDlx
    {
        private SudokuGrid s;
        private ColumnNode root;
        private LinkedList<Node> solution = new LinkedList<Node>();

        public CustomDlx(SudokuGrid s)
        {
            this.s = s;
        }
        
        public void Solve()
        {
            init();
            search();
            //convert solution into a SudokuGrid 
            foreach (Node node in solution)
            {
                int value = node.RowIndex % 9;
                int i = (node.RowIndex / 9) % 9;
                int j = node.RowIndex / 81;
                s.Cells[i, j] = value + 1;
            }
        }

        /// <summary>
        /// Init the four-way-linked representation of the exact cover problem into a matrix of nodes.
        /// </summary>
        public void init()
        {
            root = new ColumnNode();
            root.Left = root;
            root.Right = root;

            Node c = root;
            ColumnNode[] columnsNodes = new ColumnNode[324];
            int columnsAppenderIdx = 0;

            // create all constraints into column node
            for (int i = 0; i < 324; i++)
            {
                ColumnNode newColumn = new ColumnNode();
                columnsNodes[columnsAppenderIdx++] = newColumn;
                newColumn.Up = newColumn;
                newColumn.Down = newColumn;

                c.Right = newColumn;
                newColumn.Left = c;
                newColumn.Right = root;
                root.Left = newColumn;

                c = newColumn;
            }

            // create all nodes and link them to their neighbors
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    int blockIndex = ((i / 3) + ((j / 3) * 3));
                    int singleColumnIndex = 9 * j + i;

                    int value = s.Cells[i, j] - 1;
                    int rowIndex = 81 * j + 9 * i + value;
                    int rowNumberConstraintIndex = 9 * 9 + 9 * j;
                    int columnNumberConstraintIndex = 9 * 9 * 2 + 9 * i;
                    int boxNumberConstraintIndex = 9 * 9 * 3 + blockIndex * 9;

                    // if a cell in the sudoku is filled, only one row of nodes is created
                    if (value >= 0)
                    {
                        // RC=RowColumn RN=RowNumber CN=ColumnNumber BN=BoxNumber
                        var RCColumnNode = columnsNodes[singleColumnIndex];
                        var RNColumnNode = columnsNodes[rowNumberConstraintIndex + value];
                        var CNColumnNode = columnsNodes[columnNumberConstraintIndex + value];
                        var BNColumnNode = columnsNodes[boxNumberConstraintIndex + value];

                        RCColumnNode.Size++;
                        RNColumnNode.Size++;
                        CNColumnNode.Size++;
                        BNColumnNode.Size++;


                        // always 4 1s in the row RC, RN, CN, BN
                        var RCNode = new Node(RCColumnNode, rowIndex);
                        var RNNode = new Node(RNColumnNode, rowIndex);
                        var CNNode = new Node(CNColumnNode, rowIndex);
                        var BNNode = new Node(BNColumnNode, rowIndex);

                        RCNode.Left = BNNode;
                        RCNode.Right = RNNode;

                        RNNode.Left = RCNode;
                        RNNode.Right = CNNode;

                        CNNode.Left = RNNode;
                        CNNode.Right = BNNode;

                        BNNode.Left = CNNode;
                        BNNode.Right = RCNode;

                        RCNode.Down = RCColumnNode;
                        RCNode.Up = RCColumnNode.Up;
                        RCColumnNode.Up.Down = RCNode;
                        RCColumnNode.Up = RCNode;

                        RNNode.Down = RNColumnNode;
                        RNNode.Up = RNColumnNode.Up;
                        RNColumnNode.Up.Down = RNNode;
                        RNColumnNode.Up = RNNode;

                        CNNode.Down = CNColumnNode;
                        CNNode.Up = CNColumnNode.Up;
                        CNColumnNode.Up.Down = CNNode;
                        CNColumnNode.Up = CNNode;

                        BNNode.Down = BNColumnNode;
                        BNNode.Up = BNColumnNode.Up;
                        BNColumnNode.Up.Down = BNNode;
                        BNColumnNode.Up = BNNode;
                    }
                    // otherwise 9 rows of nodes are created
                    else
                    {
                        for (int d = 0; d < 9; d++)
                        {
                            rowIndex = 81 * j + 9 * i + d;

                            var RCColumnNode = columnsNodes[singleColumnIndex];
                            var RNColumnNode = columnsNodes[rowNumberConstraintIndex + d];
                            var CNColumnNode = columnsNodes[columnNumberConstraintIndex + d];
                            var BNColumnNode = columnsNodes[boxNumberConstraintIndex + d];

                            RCColumnNode.Size++;
                            RNColumnNode.Size++;
                            CNColumnNode.Size++;
                            BNColumnNode.Size++;

                            // always 4 1s in the row RC, RN, CN, BN
                            var RCNode = new Node(RCColumnNode, rowIndex);
                            var RNNode = new Node(RNColumnNode, rowIndex);
                            var CNNode = new Node(CNColumnNode, rowIndex);
                            var BNNode = new Node(BNColumnNode, rowIndex);

                            RCNode.Left = BNNode;
                            RCNode.Right = RNNode;

                            RNNode.Left = RCNode;
                            RNNode.Right = CNNode;

                            CNNode.Left = RNNode;
                            CNNode.Right = BNNode;

                            BNNode.Left = CNNode;
                            BNNode.Right = RCNode;


                            RCNode.Down = RCColumnNode;
                            RCNode.Up = RCColumnNode.Up;
                            RCColumnNode.Up.Down = RCNode;
                            RCColumnNode.Up = RCNode;

                            RNNode.Down = RNColumnNode;
                            RNNode.Up = RNColumnNode.Up;
                            RNColumnNode.Up.Down = RNNode;
                            RNColumnNode.Up = RNNode;


                            CNNode.Down = CNColumnNode;
                            CNNode.Up = CNColumnNode.Up;
                            CNColumnNode.Up.Down = CNNode;
                            CNColumnNode.Up = CNNode;

                            BNNode.Down = BNColumnNode;
                            BNNode.Up = BNColumnNode.Up;
                            BNColumnNode.Up.Down = BNNode;
                            BNColumnNode.Up = BNNode;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Covers the column, it removes all the nodes of the column of the linked list
        /// </summary>
        public void cover(Node c)
        {
            c.Right.Left = c.Left;
            c.Left.Right = c.Right;
            for (Node i = c.Down; i != c; i = i.Down)
            {
                for (Node j = i.Right; j != i; j = j.Right)
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
        public void uncover(Node c)
        {
            for (Node i = c.Up; i != c; i = i.Up)
            {
                for (Node j = i.Left; j != i; j = j.Left)
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
        public bool search()
        {
            if (root.Right == root)
            {
                return true;
            }

            ColumnNode selected = (ColumnNode)root.Right;
            int cpt = 0;
            for (ColumnNode i = (ColumnNode)root.Right; i != root; i = (ColumnNode)i.Right)
            {
                if (i.Size < selected.Size)
                {
                    selected = i;
                }
            }

            cover(selected);

            for (Node i = selected.Down; i != selected; i = i.Down)
            {
                solution.AddLast(i);

                for (Node j = i.Right; j != i; j = j.Right)
                {
                    cover(j.Column);
                }

                if (search())
                {
                    return true;
                }

                solution.RemoveLast();

                for (Node j = i.Left; j != i; j = j.Left)
                {
                    uncover(j.Column);
                }
            }

            uncover(selected);

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
            public Node Left;
            public Node Right;
            public Node Up;
            public Node Down;
            public ColumnNode Column;
            public int RowIndex;

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
            internal int Size;
        }
    }
}