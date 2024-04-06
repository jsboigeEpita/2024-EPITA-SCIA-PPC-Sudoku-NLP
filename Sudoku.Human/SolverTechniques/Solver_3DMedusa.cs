using System.ComponentModel;
using System.Dynamic;
using System.Security.Cryptography;

namespace Sudoku.Human;

partial class Solver {

    private Cell start;

    private bool Medusas()
    {
        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                if (solve_3d_medusas_from(x, y))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool solve_3d_medusas_from(int x, int y) {
        Cell start_cell = Puzzle[x,y];
        start = start_cell;
        int p;
        int q;
        if (!start_cell.Candidates.TryGetCount2(out p, out q)) {
            return false;
        }
        start_cell.colors.Add(p, Color.RED);
        start_cell.colors.Add(q, Color.BLUE);
        while (medusa_color_bi_value_cells() || medusa_color_bi_location_units()) {}
        bool changed = medusa_check_cell_contradictions()
                    || medusa_check_unit_contradictions()
                    || medusa_check_seen_contradictions();
        
        if (!changed) {
            changed |= medusa_check_full_cells();
            changed |= medusa_check_emptied_cells();
            changed |= medusa_check_partial_cells();
        }
        Puzzle.GetBoard().ToList().ForEach(c => c.colors.Clear());
        return changed;
    }

    //debug
    private void m3d_medusa_print_chain_start() {
        int c1;
        int c2;
       
        if(start.CandI.TryGetCount2(out c1, out c2)) {
            Console.WriteLine($" - Start chains from cell {start.Point.ToString()}, coloring {c1} {start.colors[c1]} and {c2} {start.colors[c2]}");
        }
        
    }

    private bool medusa_color_bi_value_cells() {
        bool colored = false;
        foreach(Cell c in Puzzle.GetBoard()) {
            int d_colored;
            int d_uncolored;
            if(c.colors.Count == 1 && c.Candidates.TryGetCount2(out d_colored, out d_uncolored)) {
                if (c.colors.ContainsKey(d_uncolored)) {

                    (d_colored, d_uncolored) = (d_uncolored, d_colored);
                }
                c.colors.Add(d_uncolored, ~c.colors[d_colored]);
                colored = true;
            }
        }
        return colored;
    }

    private bool apply_bi_loc(Region[] unit_type)
    {
        for (int i = 0; i < 9; i++)
        {
            Region unit = unit_type[i];
            HashSet<int> unsolved_ds = new HashSet<int>();
            foreach (Cell cell in unit) if (!(cell.Candidates.Count <= 1))
                {
                    for (int el = 1; el < 10; el++)
                    {
                        if (!unsolved_ds.Contains(el))
                            unsolved_ds.Add(el);
                    }
                }
            foreach (int candi in unsolved_ds)
            {
                List<Cell> filtered_unit = [];
                foreach (Cell cell in unit) if (!(cell.Candidates.Count <= 1 && cell.CandI.IsCandidate(candi)))
                    {
                        if (!filtered_unit.Contains(cell))
                            filtered_unit.Add(cell);
                    }
                if (filtered_unit.Count != 2)
                    continue;
                Cell cell_colored = filtered_unit[0];
                Cell cell_uncolored = filtered_unit[1];
                if (!cell_colored.colors.ContainsKey(candi))
                {
                    (cell_uncolored, cell_colored) = (cell_colored, cell_uncolored);
                }
                if (cell_uncolored.colors.ContainsKey(candi) || !cell_colored.colors.ContainsKey(candi))
                    continue;
                cell_uncolored.colors[candi] = ~cell_colored.colors[candi];
                return true;
            }
        }
        return false;
    }


    private bool medusa_color_bi_location_units()
    {
        return apply_bi_loc(Puzzle.BlocksI) || apply_bi_loc(Puzzle.ColumnsI) || apply_bi_loc(Puzzle.RowsI);
    }

    private bool medusa_eliminate_color(Color color)
    {
        bool changed = false;
        //debug
        Span<int> commonCandidates = stackalloc int[1];
        Console.WriteLine($" - Eliminate all candidates colored {color}");
        foreach(Cell c in Puzzle.GetBoard()) {
            foreach (KeyValuePair<int,Color> kvp in c.colors) {
                if (kvp.Value != color) {
                    continue;
                }
                changed |= c.Exclude(kvp.Key);

                //debug
                Console.Write($"    > Cell {c.Point.ToString()} can only be {c.CandI.Print()}");
            }
        }
        return changed;
    }

    private bool medusa_check_cell_contradictions()
    {
        List<Cell> allcells = Puzzle.GetBoard().ToList();

        for (int i = 0; i < allcells.Count; i++)
        {
            Cell c = allcells[i];
            Color dup_color = Color.NEITHER;
            if (c.colors.Count(kvp => kvp.Value == Color.RED) > 1)
            {
                dup_color = Color.RED;
            }
            else if (c.colors.Count(kvp => kvp.Value == Color.BLUE) > 1)
            {
                dup_color = Color.BLUE;
            }
            else
            {
                continue;
            }
            //debug
            m3d_medusa_print_chain_start();
            Console.WriteLine(" - Find a cell with multiple candidates in the same color");
            List<int> dup_candidates = new List<int>();
            foreach(KeyValuePair<int, Color> kvp in c.colors) {
                if(kvp.Value == dup_color) {
                    dup_candidates.Add(kvp.Key);
                }
            }
            Console.WriteLine($"- Cell {c.Point.ToString()} has multiple candidates {dup_candidates.ToString()} colored {dup_color}");
            return medusa_eliminate_color(dup_color);
        }
        return false;
    }

    /*
    def medusa_check_unit_contradictions(sudoku, print_start, verbose):
        for unit_type, i, d in product(Sudoku.UNIT_TYPES, range(9), Cell.VALUES):
            unit = sudoku.unit(unit_type, i)
            colors = [c.dcs[d] for c in unit if d in c.dcs]
            dup_color = Color.NEITHER
            if colors.count(Color.RED) > 1:
                dup_color = Color.RED
            elif colors.count(Color.BLUE) > 1:
                dup_color = Color.BLUE
            else:
                continue
            if verbose:
                print_start()
                print(' - Find a unit with multiple cells with the same candidate in the same color')
                dup_cell_names = [c.cell_name() for c in unit
                    if c.dcs.get(d, Color.NEITHER) == dup_color]
                print(' - %s %s has multiple cells (%s) with candidate %d colored %s' %
                    (unit_type.capitalize(), sudoku.unit_name(unit_type, i),
                        ', '.join(dup_cell_names), d, dup_color))
            return medusa_eliminate_color(sudoku, dup_color, verbose)
        return False
    */

    private bool medusa_check_unit_contradictions()
    {
        return false;
    }

    private bool all_color(Color color, Cell c, Dictionary<int, HashSet<Color>> seen_colors) {
        foreach(int candidate in c.Candidates) {
            foreach(Color col in seen_colors[candidate]) {
                if(col != color) {
                    return false;
                }
            }
        }
        return true;
    }

    private bool medusa_check_seen_contradictions()
    {
        foreach (Cell c in Puzzle.GetBoard()) {
            Dictionary<int, HashSet<Color>> seen_colors = new Dictionary<int, HashSet<Color>>();
            foreach(int can in c.Candidates) {
                HashSet<Color> tempSeen = new HashSet<Color>();
                foreach( Cell cell in c.VisibleCells) {
                    if (cell.colors.ContainsKey(can)) {
                        tempSeen.Add(cell.colors[can]);
                    }
                }
                seen_colors.Add(can, tempSeen);
            }
            Color seen_color = Color.NEITHER;
            if (all_color(Color.RED, c, seen_colors)) {
                seen_color = Color.RED;
            }
            else if(all_color(Color.BLUE, c, seen_colors)) {
                seen_color = Color.BLUE;
            }
            else {
                continue;
            }

            //debug
            m3d_medusa_print_chain_start();
            Console.WriteLine(" - Find cells that can see all their candidates in the same color");
            Console.WriteLine($" * Cell {c.Point.ToString()} can see all its candidates {c.Candidates.Print()} in {seen_color}");
            return medusa_eliminate_color(seen_color);
            
        }
        return false;
    }

    private bool medusa_check_full_cells()
    {
        bool changed = false;
        foreach(Cell c in Puzzle.GetBoard()) {
            if (c.colors.Values.ToHashSet().Count != 2) {
                continue;
            }
            bool cell_changed = c.include_only(c.colors.Keys.ToList());
            //debug
            if(!changed) {
                m3d_medusa_print_chain_start();
                Console.WriteLine(" - Find cells with candidates in both colors and others uncolored");
            }
            Console.WriteLine($"    * Cell {c.Point.ToString()} can only be {c.CandI.Print()}");

            changed |= cell_changed;
        }
        return changed;
    }

    private bool medusa_check_emptied_cells()
    {
        bool changed = false;
        Span<int> toSee = stackalloc int[1];
        foreach (Cell c in Puzzle.GetBoard()) {
            if (c.Candidates.Count == 1) {
                continue;
            }
            Candidates toExclude = new Candidates(c.colors.Keys.ToList());
            toSee = c.Candidates.Except(toExclude, toSee);
            foreach(int candi in toSee) {
                HashSet<Color> d_colors = new HashSet<Color>();
                foreach(Cell cell in c.VisibleCells) {
                    if(cell.colors.ContainsKey(candi)) {
                        d_colors.Add(cell.colors[candi]);
                    }
                }
                if (d_colors.Count !=2 ) {
                    continue;
                }
                bool cell_changed = c.Exclude(candi);
                //debug
                if (!changed) {
                    m3d_medusa_print_chain_start();
                    Console.WriteLine(" - Find cells with an uncolored candidate that can be seen in both colors");
                }
                Console.WriteLine($"    * Cell {c.Point.ToString()} can only be {c.Candidates.Print()}, since it can see {candi} in both colors");
                changed |= cell_changed;
            }
            

        }
        return changed;
    }

    /*
    def medusa_check_partial_cells(sudoku, print_start, verbose):
        changed = False
        for cell in sudoku.cells():
            if len(cell.dcs) != 1:
                continue
            d_colored = list(cell.dcs.keys())[0]
            d_color = cell.dcs[d_colored]
            seen = sudoku.seen_from(cell.x, cell.y)
            for d in cell.ds - {d_colored}:
                if not any(c for c in seen if c.dcs.get(d, Color.NEITHER) == ~d_color):
                    continue
                cell_changed = cell.exclude({d})
                if verbose and cell_changed:
                    if not changed:
                        print_start()
                        print(' - Find cells with a candidate in one color that can see it in the other color')
                    print('    * Cell %s can only be %s, since its %d is %s and it can see %d in %s' %
                        (cell.cell_name(), cell.value_string(), d_colored,
                            d_color, d, ~d_color))
                changed |= cell_changed
        return changed
    */

    private bool medusa_check_partial_cells()
    {
        bool changed = false;
        return changed;
    }

}