using System;
using System.Diagnostics;
using System.Security.Principal;
using Unity.VisualScripting.FullSerializer;

namespace MonoRL
{

    public class Cost
    {

        public enum CostType
        {
            DistanceError,
            SquaredError
        }

        public static ICost GetCostFromType(CostType type)
        {
            switch (type)
            {
                case CostType.DistanceError:
                    return new DistanceError();
                case CostType.SquaredError:
                    return new SquaredError();
                default:
                    UnityEngine.Debug.LogError("Unhandled cost type");
                    return new DistanceError();
            }
        }

        public class DistanceError : ICost
        {
            public double CostFunction(double a, double y)
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

        public class SquaredError : ICost
        {
            public double CostFunction(double a, double y)
            {
                UnityEngine.Debug.Log("CostFunction : " + Math.Pow(a - y, 2));

                return Math.Pow(a - y, 2);
            }

            public double CostDerivative(double a, double y)
            {
                UnityEngine.Debug.Log("CostDerivative : " + (2 * (a - y)));
                return 2*(a - y);
            }

            public CostType CostFunctionType()
            {
                return CostType.DistanceError;
            }
        }
    }
}