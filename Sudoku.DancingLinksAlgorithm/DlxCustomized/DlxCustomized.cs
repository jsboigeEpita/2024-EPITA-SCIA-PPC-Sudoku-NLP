using Sudoku.Shared;

namespace Sudoku.GeneticAlgorithm.DlxCustomized;

public class DlxCustomized
{
    /// <summary>
    /// NodeHead correspondant aux colonnes Head dans les matrices de contraintes
    /// </summary>
    class NodeHead : Node
    {
        internal new Node item;
        internal int size;

        public NodeHead() : base(null)
        {
        }
    }

    /// <summary>
    /// Node correspondant à un noeud de la matrice
    /// </summary>
    class Node
    {
        //Références au noeud adjacent
        internal Node right = null;
        internal Node left = null;
        internal Node up = null;
        internal Node down = null;
        internal NodeHead nodeHead = null;
        
        //Indice de la ligne et de la colonne
        internal int rowIndex;
        internal int column ;
        
        //Valeur de la cellule
        internal int value;

        public Node(NodeHead t)
        {
            nodeHead = t;
        }
    }
    
    /// <summary>
    /// Premier node de la matrice
    /// </summary>
    private NodeHead root;
    
    /// <summary>
    /// Attribut pour arrêter l'algorithme X si la solution a été trouvé
    /// </summary>
    private bool stop = false;
    
    /// <summary>
    /// LinkedList où on met les solutions
    /// </summary>
    private LinkedList<Node> solutions = new LinkedList<Node>();
    
    /// <summary>
    /// Sudoku grid
    /// </summary>
    private SudokuGrid sudoku;
    
    public DlxCustomized(SudokuGrid sudokuGrid)
    {
        sudoku = sudokuGrid;
        root = new NodeHead();
        root.right = root;
        root.left = root;
        
    }

    /// <summary>
    /// Fonction pour résoudre le sudoku grid en le transformant en une matrice de contrainte où on appliquera ensuite l'algorithme de solution
    /// et pour ensuite mettre les nouvelles valeurs dans le sudoku
    /// </summary>
    /// <returns>Sudoku résolu</returns>
    public SudokuGrid Solve()
    {
        Init();
        search();
        foreach(Node node in solutions)
        {
            sudoku.Cells[node.rowIndex, node.column] = node.value;
        }
        return sudoku;
    }
    
    
    /// <summary>
    /// Initialisation de la matrice de contrainte
    /// </summary>
    public void Init()
    {
        //Etape 1 : Création d'un array pour stocker les nodes
        Node[] tmp = new Node[9 * 9 * 4];

        
        //Etape 2 : Création du Nodehead
        root = new NodeHead();
        Node currentNode = root;
        
        //Etape 3 : Création des colonnes de contraintes pour chaque case du Sudoku
        for (int j = 0; j < 324; j++)
        {
            currentNode.right = new NodeHead();
            currentNode.right.left = currentNode;
            currentNode = currentNode.right;
            currentNode.rowIndex = j;
            currentNode.up = currentNode;
            currentNode.down = currentNode;
            tmp[j] = currentNode;
        }

        //Etape 4 : Ferme la liste chaine des nodes
        currentNode.right = root;
        root.left = currentNode;

        //Etape 5 : Loop sur chaque cellule du SudokuGrid
        int imatrix = 0;
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                int value = sudoku.Cells[i ,j];

                Node tmpRCCNode; // Noeud temporaire pour une contrainte ligne-colonne sur la cellule
                Node tmpRNCNode; // Noeud temporaire pour une contrainte ligne-valeur sur la cellule 
                Node tmpCNCNode; // Noeud temporaire pour une contrainte colonne-valeur sur la cellule
                Node tmpBNCNode; // Noeud temporaire pour une contrainte boîte-valeur sur la cellule
                
                //Etape 6 : On check si la cellule est vide correspondant donc à 1 ou 0 dans une matrice de contrainte en général
                if (value == 0)
                {
                    //Si oui on crée un node avec toute les possibilités pouvant être sur la case
                    value = 1;
                    int RCC = 9 * i + j;
                    int RNC = 81 + 9 * i + value - 1;
                    int CNC = 162 + 9 * j + value - 1;
                    int BNC = 243 + ((i / 3) * 3 + j / 3) * 9 + value - 1;

                    int end = imatrix + 9;

                    //On va créer des nodes pour toute les valeurs possible de 1 - 9 sur cette indice pour toute les contraintes
                    for (; imatrix < end; imatrix++)
                    {
                        //Création des noeud dans la matrice de contrainte
                        tmpRCCNode = new Node((NodeHead)tmp[RCC].down);
                        tmpRNCNode = new Node((NodeHead)tmp[RNC].down);
                        tmpCNCNode = new Node((NodeHead)tmp[CNC].down);
                        tmpBNCNode = new Node((NodeHead)tmp[BNC].down);

                        //Attribution des valeurs et positions des row/column dans chaque noeud
                        tmpRCCNode.rowIndex = i;
                        tmpRCCNode.column = j;
                        tmpRCCNode.value = value;
                        tmpRNCNode.rowIndex = i;
                        tmpRNCNode.column = j;
                        tmpRNCNode.value = value;
                        tmpCNCNode.rowIndex = i;
                        tmpCNCNode.column = j;
                        tmpCNCNode.value = value;
                        tmpBNCNode.rowIndex = i;
                        tmpBNCNode.column = j;
                        tmpBNCNode.value = value++;

                        //Incrémente ensuite la taille des éléments dans les colonnes de contraintes
                        ((NodeHead)tmp[RCC].down).size++;
                        ((NodeHead)tmp[RNC].down).size++;
                        ((NodeHead)tmp[CNC].down).size++;
                        ((NodeHead)tmp[BNC].down).size++;

                        //Attribution des liens entre les noeuds
                        tmpRCCNode.right = tmpRNCNode;
                        tmpRNCNode.right = tmpCNCNode;
                        tmpCNCNode.right = tmpBNCNode;
                        tmpBNCNode.right = tmpRCCNode;

                        tmpBNCNode.left = tmpCNCNode;
                        tmpCNCNode.left = tmpRNCNode;
                        tmpRNCNode.left = tmpRCCNode;
                        tmpRCCNode.left = tmpBNCNode;

                        tmpRCCNode.up = tmp[RCC];
                        tmpRCCNode.down = tmp[RCC].down;
                        tmp[RCC].down = tmpRCCNode;
                        tmp[RCC] = tmpRCCNode;

                        tmpRNCNode.up = tmp[RNC];
                        tmpRNCNode.down = tmp[RNC].down;
                        tmp[RNC].down = tmpRNCNode;
                        tmp[RNC++] = tmpRNCNode;

                        tmpCNCNode.up = tmp[CNC];
                        tmpCNCNode.down = tmp[CNC].down;
                        tmp[CNC].down = tmpCNCNode;
                        tmp[CNC++] = tmpCNCNode;

                        tmpBNCNode.up = tmp[BNC];
                        tmpBNCNode.down = tmp[BNC].down;
                        tmp[BNC].down = tmpBNCNode;
                        tmp[BNC++] = tmpBNCNode;
                    }
                }
                else
                {
                    
                    //Sinon on va juste mettre le noeud avec la valeur dans chaque contrainte
                    int RCC = 9 * i + j;
                    int RNC = 81 + 9 * i + value - 1;
                    int CNC = 162 + 9 * j + value - 1;
                    int BNC = 243 + ((i / 3) * 3 + j / 3) * 9 + value - 1;

                    tmpRCCNode = new Node((NodeHead)tmp[RCC].down);
                    tmpRNCNode = new Node((NodeHead)tmp[RNC].down);
                    tmpCNCNode = new Node((NodeHead)tmp[CNC].down);
                    tmpBNCNode = new Node((NodeHead)tmp[BNC].down);

                    tmpRCCNode.rowIndex = i;
                    tmpRCCNode.column = j;
                    tmpRCCNode.value = value;
                    tmpRNCNode.rowIndex = i;
                    tmpRNCNode.column = j;
                    tmpRNCNode.value = value;
                    tmpCNCNode.rowIndex = i;
                    tmpCNCNode.column = j;
                    tmpCNCNode.value = value;
                    tmpBNCNode.rowIndex = i;
                    tmpBNCNode.column = j;
                    tmpBNCNode.value = value;

                    tmpRCCNode.right = tmpRNCNode;
                    tmpRNCNode.right = tmpCNCNode;
                    tmpCNCNode.right = tmpBNCNode;
                    tmpBNCNode.right = tmpRCCNode;

                    tmpBNCNode.left = tmpCNCNode;
                    tmpCNCNode.left = tmpRNCNode;
                    tmpRNCNode.left = tmpRCCNode;
                    tmpRCCNode.left = tmpBNCNode;

                    tmpRCCNode.up = tmp[RCC];
                    tmpRCCNode.down = tmp[RCC].down;
                    tmp[RCC].down = tmpRCCNode;
                    tmp[RCC] = tmpRCCNode;

                    tmpRNCNode.up = tmp[RNC];
                    tmpRNCNode.down = tmp[RNC].down;
                    tmp[RNC].down = tmpRNCNode;
                    tmp[RNC++] = tmpRNCNode;

                    tmpCNCNode.up = tmp[CNC];
                    tmpCNCNode.down = tmp[CNC].down;
                    tmp[CNC].down = tmpCNCNode;
                    tmp[CNC++] = tmpCNCNode;

                    tmpBNCNode.up = tmp[BNC];
                    tmpBNCNode.down = tmp[BNC].down;
                    tmp[BNC].down = tmpBNCNode;
                    tmp[BNC++] = tmpBNCNode;

                    imatrix++;
                }
            }
        }
    }
    
    
    public void search()
    {
        //Check si la matrice est vide
        if (root.right == root)
        {
            //Si vrai alors la solution a été trouvé
            stop = true;
            return;
        }

        //Step 2 : On prend la colonne avec la moins de contrainte 
        NodeHead selected = (NodeHead)root.right;
        int c = selected.size;
        for (NodeHead currentNode = (NodeHead)root.right; currentNode != root; currentNode = (NodeHead)currentNode.right)
        {
            if (c > currentNode.size)
            {
                c = currentNode.size;
                selected = currentNode;
            }
        }

        //Step 3 : Désactive la colonne séléctionné
        cover(selected);

        //Step 4 : On refait une recherche de solution 
        for (Node iNode = selected.down; iNode != selected; iNode = iNode.down)
        {
            
            //Step 5 : On rajoute le node dans la solution possible 
            solutions.AddLast(iNode);
            
            //Step 6 : On désactive tous les nodes dans les colonnes correspondantes => On supprime les lignes
            for (Node jNode = iNode.right; jNode != iNode; jNode = jNode.right)
            {
                cover(jNode.nodeHead);
            }

            //Step 7 :On re-explore récursivement pour chercher d'autres solutions possible - Si on trouve on arrête
            search();
            if (stop)
                return;
            
            //On retire le noeud solution possible 
            solutions.RemoveLast();

            //On re-active les précédent noeuds désactivés => On remet les lignes pour revenir à la matrice de début de la boucle 
            for (Node jNode = iNode.left; jNode != iNode; jNode = jNode.left)
            {
                uncover(jNode.nodeHead);
            }
        }
        //On active la colonne 
        uncover(selected);
        //Si on retourne c'est qu'il n'y a pas eu de solution dans le sudoku
        return;
    }
    

    /// <summary>
    /// Cover (désactiver) permet de retirer la node dans la matrice de contrainte des autres noeuds
    /// </summary>
    private void cover(NodeHead node)
    {
        node.left.right = node.right;
        node.right.left = node.left;
        for (Node iNode = node.down; iNode != node; iNode = iNode.down)
        {
            for (Node jNode = iNode.right; jNode != iNode; jNode = jNode.right)
            {
                jNode.up.down = jNode.down;
                jNode.down.up = jNode.up;
                jNode.nodeHead.size--;
            }
        }
    }

    /// <summary>
    /// Inverse de cover - Remet le node dans la matrice de contrainte
    /// </summary>
    private void uncover(NodeHead node)
    {
        for (Node iNode = node.down; iNode != node; iNode = iNode.down)
        {
            for (Node jNode = iNode.right; jNode != iNode; jNode = jNode.right)
            {
                jNode.up.down = jNode;
                jNode.down.up = jNode;
                jNode.nodeHead.size++;
            }
        }
        node.left.right = node;
        node.right.left = node;
    }
}