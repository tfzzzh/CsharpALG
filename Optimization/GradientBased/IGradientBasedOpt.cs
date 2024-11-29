using CsharpALG.Numerical;
namespace CsharpALG.Optimization;

/// <summary>
/// Optimization Method Based on Gradient Descent
/// </summary>
public interface IGradientBasedMinimizer
{
    /// <summary>
    /// one step of optimization iteration
    /// </summary>
    void Step();

    /// <summary>
    /// Step with Line search Function
    /// </summary>
    /// <param name="lossfunc"></param>
    /// <param name="backwardfunc"></param> <summary>
    void Step(Func<NdArray<double>[], double> lossfunc, Action<NdArray<double>[]> backwardfunc)
    {
        Step();
    }

    // get parameters
    NdArray<double>[] Params {get;}
}