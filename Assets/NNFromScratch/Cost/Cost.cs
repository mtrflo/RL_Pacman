using System;

public class Cost
{

    public enum CostType
    {
        DistanceError,
    }

    public static ICost GetCostFromType(CostType type)
    {
        switch (type)
        {
            case CostType.DistanceError:
                return new DistanceError();

            default:
                UnityEngine.Debug.LogError("Unhandled cost type");
                return new DistanceError();
        }
    }

    public class DistanceError : ICost
    {
        public double CostFunction(double[] a, double[] y)
        {
            return a - y;
        }

        public double CostDerivative(double a, double y)
        {
            return 1;
        }

        public CostType CostFunctionType()
        {
            return CostType.DistanceError;
        }
    }
}