using System.ComponentModel;
using System.Dynamic;
using System.Security.Cryptography;

namespace Sudoku.Human;

partial class Solver {

    private Cell start;
    private bool Medusas()
    {
        for (int x = 0; x < 9; x++) {
            for (int y = 0; y < 9; y++) {
                if(solve_3d_medusas_from(x,y)) {
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
        int c1 = 0;
        int c2 = 0;
       
        if( start.CandI.TryGetCount2(out c2, out c2)) {
            Console.WriteLine($" - Start chains from cell {start.Point.ToString}, coloring {c1} {start.colors[c1]} and {c2} {start.colors[c2]}");
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

    /*
    def medusa_color_bi_location_units(sudoku, verbose):
        colored = False
        for unit_type, i in product(Sudoku.UNIT_TYPES, range(9)):
            unit = sudoku.unit(unit_type, i)
            unsolved_ds = union(c.ds for c in unit if not c.solved()) c.solved = c.candidates.count == 1
            for d in unsolved_ds:
                filtered_unit = [c for c in unit if not c.solved() and d in c.ds]
                if len(filtered_unit) != 2:
                    continue
                cell_colored, cell_uncolored = filtered_unit
                if d not in cell_colored.dcs:
                    cell_colored, cell_uncolored = cell_uncolored, cell_colored
                if d in cell_uncolored.dcs or d not in cell_colored.dcs:
                    continue
                cell_uncolored.dcs[d] = ~cell_colored.dcs[d]
                colored = True
        return colored
    */
    private bool medusa_color_bi_location_units() {
        bool colored = false;

        return colored;
    }

    private bool medusa_eliminate_color(Color color) {
        bool changed = false;
        foreach(Cell c in Puzzle.GetBoard()) {
            foreach (KeyValuePair<int,Color> kvp in c.colors) {
                if (kvp.Value != color) {
                    continue;
                }
                changed |= c.Exclude(kvp.Key);
            }
        }
        return changed;
    }

    private bool medusa_check_cell_contradictions() {
        foreach(Cell c in Puzzle.GetBoard()) {
            Color dup_color = Color.NEITHER;
            if(c.colors.Count(kvp => kvp.Value == Color.RED) > 1) {
                dup_color = Color.RED;
            }
            else if(c.colors.Count(kvp => kvp.Value == Color.BLUE) > 1) {
                dup_color = Color.BLUE;
            }
            else {
                continue;
            }
            //print pour test
            m3d_medusa_print_chain_start();
            Console.WriteLine(" - Find a cell with multiple candidates in the same color");
            List<int> dup_candidates = new List<int>();
            foreach(KeyValuePair<int, Color> kvp in c.colors) {
                if(kvp.Value == dup_color) {
                    dup_candidates.Add(kvp.Key);
                }
            }
            Console.WriteLine($"- Cell {c.Point.ToString} has multiple candidates {dup_candidates.ToString} colored {dup_color}");
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

    private bool medusa_check_unit_contradictions() {
        return false;
    }

    /*
    def medusa_check_seen_contradictions(sudoku, print_start, verbose):
        for cell in sudoku.cells():
            if cell.dcs:
                continue
            seen = sudoku.seen_from(cell.x, cell.y)
            seen_colors = {d: {c.dcs[d] for c in seen if d in c.dcs} for d in cell.ds}
            seen_color = None
            if all(Color.RED in seen_colors[d] for d in cell.ds):
                seen_color = Color.RED
            elif all(Color.BLUE in seen_colors[d] for d in cell.ds):
                seen_color = Color.BLUE
            else:
                continue
            if verbose:
                print_start()
                print(' - Find cells that can see all their candidates in the same color')
                print(' * Cell %s can see all its candidates %s in %s' %
                    (cell.cell_name(), cell.value_string(), seen_color))
            return medusa_eliminate_color(sudoku, seen_color, verbose)
        return False
    */

    private bool medusa_check_seen_contradictions() {
        return false;
    }

    /*
    def medusa_check_full_cells(sudoku, print_start, verbose):
        changed = False
        for cell in sudoku.cells():
            if len(set(cell.dcs.values())) != 2:
                continue
            cell_changed = cell.include_only(cell.dcs.keys())
            if verbose and cell_changed:
                if not changed:
                    print_start()
                    print(' - Find cells with candidates in both colors and others uncolored')
                print('    * Cell %s can only be %s' % (cell.cell_name(),
                    cell.value_string()))
            changed |= cell_changed
        return changed
    */

    private bool medusa_check_full_cells() {
        bool changed = false;
        return changed;
    }

    /*
    def medusa_check_emptied_cells(sudoku, print_start, verbose):
        changed = False
        for cell in sudoku.cells():
            if cell.solved():
                continue
            seen = sudoku.seen_from(cell.x, cell.y)
            for d in cell.ds - set(cell.dcs):
                d_colors = {c.dcs[d] for c in seen if d in c.dcs}
                if len(d_colors) != 2:
                    continue
                cell_changed = cell.exclude({d})
                if verbose and cell_changed:
                    if not changed:
                        print_start()
                        print(' - Find cells with an uncolored candidate that can be seen in both colors')
                    print('    * Cell %s can only be %s, since it can see %d in both colors' %
                        (cell.cell_name(), cell.value_string(), d))
                changed |= cell_changed
        return changed
    */

    private bool medusa_check_emptied_cells() {
        bool changed = false;
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

    private bool medusa_check_partial_cells() {
        bool changed = false;
        return changed;
    }

}