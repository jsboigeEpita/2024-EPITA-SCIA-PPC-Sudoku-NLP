using System.Dynamic;
using System.Globalization;
using System.Security;

namespace Sudoku.Human;

partial class Solver
{
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

    /*
    def solve_3d_medusas_from(sudoku, x, y, verbose):
        start_cell = sudoku.cell(x, y)
        if not start_cell.bi_value():
            return False
        p, q = sorted(start_cell.ds)
        start_cell.dcs[p], start_cell.dcs[q] = Color.RED, Color.BLUE
        while (medusa_color_bi_value_cells(sudoku, verbose) or
            medusa_color_bi_location_units(sudoku, verbose)):
            pass
        print_start = lambda: (m3d_medusa_print_chain_start(sudoku, start_cell), print(sudoku))
        changed = (medusa_check_cell_contradictions(sudoku, print_start, verbose) or
            medusa_check_unit_contradictions(sudoku, print_start, verbose) or
            medusa_check_seen_contradictions(sudoku, print_start, verbose))
        if not changed:
            changed |= medusa_check_full_cells(sudoku, print_start, verbose)
            if changed: print_start = lambda: None
            changed |= medusa_check_emptied_cells(sudoku, print_start, verbose)
            if changed: print_start = lambda: None
            changed |= medusa_check_partial_cells(sudoku, print_start, verbose)
        for cell in sudoku.cells():
            cell.dcs = {}
        return changed
    */

    private bool solve_3d_medusas_from(int x, int y)
    {
        medusa_color_bi_value_cells();
        return false;
    }

    private bool medusa_color_bi_value_cells()
    {
        bool colored = false;
        Puzzle.GetBoard().ToList().ForEach(c =>
        {
            int d_colored = 0;
            int d_uncolored = 0;
            if (c.colors.Count == 1 && c.Candidates.TryGetCount2(out d_colored, out d_uncolored))
            {
                if (c.colors.ContainsKey(d_uncolored))
                {
                    (d_colored, d_uncolored) = (d_uncolored, d_colored);
                }
                c.colors.Add(d_uncolored, ~c.colors[d_colored]);
                colored = true;
            }
        });
        return colored;
    }

    /*
    def medusa_color_bi_location_units(sudoku, verbose):
        colored = False
        for unit_type, i in product(Sudoku.UNIT_TYPES, range(9)):
            unit = sudoku.unit(unit_type, i)
            unsolved_ds = union(c.ds for c in unit if not c.solved()) //c.solved = c.candidates.count == 1
            for d in unsolved_ds: //d : Candidats
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

    /*
    def medusa_eliminate_color(sudoku, color, verbose):
        if verbose:
            print(' - Eliminate all candidates colored %s' % color)
        changed = False
        for cell in sudoku.cells():
            for d in cell.dcs:
                if cell.dcs[d] != color:
                    continue
                changed |= cell.exclude({d})
                if verbose:
                    print('    > Cell %s can only be %s' % (cell.cell_name(),
                        cell.value_string()))
        return changed
    */

    private bool medusa_eliminate_color(Color color)
    {
        bool changed = false;
        return changed;
    }

    /*
    def medusa_check_cell_contradictions(sudoku, print_start, verbose):
        for cell in sudoku.cells():
            colors = cell.dcs.values()
            dup_color = None
            if sum(1 for v in colors if v == Color.RED) > 1:
                dup_color = Color.RED
            elif sum(1 for v in colors if v == Color.BLUE) > 1:
                dup_color = Color.BLUE
            else:
                continue
            if verbose:
                print_start()
                print(' - Find a cell with multiple candidates in the same color')
                dup_candidates = {d for d in cell.dcs if cell.dcs[d] == dup_color}
                print(' - Cell %s has multiple candidates %s colored %s' %
                    (cell.cell_name(), set_string(dup_candidates), dup_color))
            return medusa_eliminate_color(sudoku, dup_color, verbose)
        return False
    */

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

    private bool medusa_check_seen_contradictions()
    {
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

    private bool medusa_check_full_cells()
    {
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

    private bool medusa_check_emptied_cells()
    {
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

    private bool medusa_check_partial_cells()
    {
        bool changed = false;
        return changed;
    }

}