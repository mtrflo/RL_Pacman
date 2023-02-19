using System;

public class Cost
{

    public enum CostType
    {
        MeanSquareError
    }

    public static ICost GetCostFromType(CostType type)
    {
        switch (type)
        {
            case CostType.MeanSquareError:
                return new MeanSquaredError();
            default:
                UnityEngine.Debug.LogError("Unhandled cost type");
                return new MeanSquaredError();
        }
    }

    public class MeanSquaredError : ICost
    {
        public double CostFunction(double[] predictedOutputs, double[] expectedOutputs)
        {
            // cost is sum (for all x,y pairs) of: 0.5 * (x-y)^2
            double cost = 0;
            for (int i = 0; i < predictedOutputs.Length; i++)
            {
                double error = predictedOutputs[i] - expectedOutputs[i];
                cost += error * error;
            }
            return 0.5 * cost;
        }

        public double CostDerivative(double predictedOutput, double expectedOutput)
        {
            return predictedOutput - expectedOutput;
        }

        public CostType CostFunctionType()
        {
            return CostType.MeanSquareError;
        }
    }

}