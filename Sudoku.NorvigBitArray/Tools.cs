using System.Collections;

namespace Sudoku.Norvig;

public class Tools
{
    public const int SIZE = 9;
    public const int SURFACE = 81;

    public static readonly HashSet<int>[] peers = FillPeers();
    public static readonly HashSet<int>[,] units = FillUnits();
    public static HashSet<int>[] FillPeers()
    {
        var peersToFill = new HashSet<int>[SURFACE];
        Parallel.For(0, SURFACE, (i, state) =>
        {
            HashSet<int> set = new HashSet<int>();
            AddColumn(i, set);
            AddRow(i, set);
            AddSquare(i, set);
            set.Remove(i);
            peersToFill[i] = set;
        });
        return peersToFill;
    }
    
    public static HashSet<int>[,] FillUnits()
    {
        var unitsToFill = new HashSet<int>[SURFACE, 3];
        Parallel.For(0, SURFACE, (i, state) =>
        {
            HashSet<int> columns = new HashSet<int>();
            HashSet<int> rows = new HashSet<int>();
            HashSet<int> squares = new HashSet<int>();

            AddColumn(i, columns);
            AddRow(i, rows);
            AddSquare(i, squares);

            unitsToFill[i, 0] = columns;
            unitsToFill[i, 1] = rows;
            unitsToFill[i, 2] = squares;

        });
        return unitsToFill;
    }

    public static string Picture(BitArray[] possibleValues)
    {
        int maxWidth = 0;
        for (int i = 0; i < SURFACE; i++)
        {
            int width = CellToString(possibleValues[i]).Length;
            if (width > maxWidth)
                maxWidth = width;
        }

        string dash1 = "";
        for (int i = 0; i < (maxWidth * 3 + 2); i++)
            dash1 += "-";

        string dash3 = "\n";
        for (int i = 0; i < 3; i++)
            dash3 += dash1 + (i == 2 ? "" : "+");

        string buffer = "";

        for (int i = 0; i < SURFACE; i++)
        {
            if (i != 0 && i % 27 == 0)
                buffer += dash3;
            buffer += (i % 3 == 0 && i % 9 != 0 ? "|" : i % 9 == 0 ? "\n" : "") +
                      CenterString(CellToString(possibleValues[i]), maxWidth);
        }

        return buffer;
    }

    private static string CellToString(BitArray possibleValue)
    {
        if (possibleValue.HasAllSet())
            return ".";

        if (IsOnlyOneBitSet(possibleValue))
            return ConvertBitwiseToDecimal(possibleValue).ToString();

        string representation = "{";

        for (int i = 1; i < SIZE + 1; i++)
        {
            if (!possibleValue.Get(i - 1))
                continue;

            representation += i.ToString();
        }

        return representation + "}";
    }

    private static string CenterString(string s, int width)
    {
        if (s.Length >= width)
        {
            return s;
        }

        int leftPadding = (width - s.Length) / 2;
        int rightPadding = width - s.Length - leftPadding;

        return new string(' ', leftPadding) + s + new string(' ', rightPadding);
    }

    public static int ConvertBitwiseToDecimal(BitArray value)
    {
        for (int i = 0; i < SIZE; i++)
            if (value.Get(i))
                return i + 1;
        return -1;
    }

    public static int ConvertBitwiseToDecimal(short value) => int.TrailingZeroCount(value) + 1;

    public static bool IsOnlyOneBitSet(BitArray value)
    {
        byte count = 0;
        for (int i = 0; i < SIZE; i++)
            if (value.Get(i))
                count++;

        return count == 1;
    }

    private static void AddRow(int cell, HashSet<int> set)
    {
        int inferiorLimit = cell / SIZE * SIZE;
        int superiorLimit = inferiorLimit + SIZE;
        for (int i = inferiorLimit; i < superiorLimit; i++)
            set.Add(i);
    }
    
    private static void AddSquare(int cell, HashSet<int> set)
    {
        int coordinateY = (cell / SIZE) / 3 * 3;
        int coordinateX = (cell % SIZE) / 3 * 3;
        int inferiorLimit = coordinateY * SIZE + coordinateX;
        for (int i = 0; i < SIZE; i++)
        {
            int target = inferiorLimit + (i / 3) * SIZE + (i % 3);
            set.Add(target);
        }
    }
    
    private static void AddColumn(int cell, HashSet<int> set)
    {
        int inferiorLimit = cell % SIZE;
        for (int i = inferiorLimit; i < SURFACE; i += SIZE)
            set.Add(i);
    }
}