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

    // get parameters
    NdArray<double>[] Params {get;}
}