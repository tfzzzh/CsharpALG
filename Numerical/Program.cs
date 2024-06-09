using CsharpALG.Numerical;
test_linspace();
test_bspline();

void test_linspace()
{
    // foreach(var x in Utility.linspace(1.0, 17.0, 17))
    // {
    //     Console.Write($"{x} ");
    // }
    // Console.WriteLine();
    double[] elems = Utility.linspace(1.0, 17.0, 17).ToArray();
    Console.WriteLine($"{Utility.ArrToString(elems)}");
}


void test_bspline()
{
    _test_bspline(-5.0, 5.0, 8, 64, 4);
}

void _test_bspline(double start, double end, int knotSize, int gridSize, int order)
{
    double[] knots = Utility.linspace(start, end, knotSize).ToArray();
    BSpline splines = new BSpline(knots);

    // foreach B_{i, p} evaluate there values at start to end with gridSize
    // table index by (knot, x)
    List<double[]> table = new List<double[]>();

    // when order = 0 -> knotSize - 1 splines
    // when order = k -> knotSize - 1 - k splines
    for (int i=0; i < knotSize - 1 - order; ++i)
        table.Add(new double[gridSize]);

    int idx = 0;
    foreach(double x in Utility.linspace(start, end, gridSize))
    {
        double[] splineValues = splines.Eval(x, order);
        for (int i=0; i < splineValues.Length; ++i)
        {
            table[i][idx] = splineValues[i];
        }
        idx += 1;
    }

    // print values
    Console.WriteLine("grid:");
    double[] xs = Utility.linspace(start, end, gridSize).ToArray();
    Console.WriteLine($"{Utility.ArrToString(xs)}");

    Console.WriteLine("splines:");
    foreach(var values in table)
    {
        Console.WriteLine($"{Utility.ArrToString(values)},");
    }
}
