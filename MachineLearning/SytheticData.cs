namespace CSharpALG.MachineLearning.Data;
using CsharpALG.Numerical;
using CsharpALG.Numerical.ArrayExtension;

public class BinaryClassificationLinearSeperable
{
    public BinaryClassificationLinearSeperable(int d, int[]? labelIds = null)
    {
        this.d = d;

        // initalize weight to [-3, 3]
        weight = (double[]) LinearAlg.RandomUniform(
            -3.0, 3.0, [d]
        );

        // init label id
        if (labelIds is not null)
            this.labelIds = labelIds;
        else
            this.labelIds = [0, 1];
    }

    public void Generate(int n, out double[,] feature, out int[] label)
    {
        // generate feature
        feature = (double[,]) LinearAlg.RandomUniform(-1.0, 1.0, [n, d]);

        // generate [0, 1] label
        // double[] score = LinearAlg.MatMul(feature, weight);
        double[] score = feature.MatMul(weight);
        label = new int[score.Length];
        for (int i=0; i < n; ++i)
        {
            label[i] = score[i] >= 0 ? 1 : 0;
            label[i] = labelIds[label[i]];
        }
    }

    int d;
    double[] weight;
    int[] labelIds;
}