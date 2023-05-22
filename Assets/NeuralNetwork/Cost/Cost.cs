using System;
using System.Diagnostics;
using System.Security.Principal;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

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
            public float CostFunction(float a, float y)
            {
                return a - y;
            }

            public float CostDerivative(float a, float y)
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
            public float CostFunction(float a, float y)
            {
                //UnityEngine.Debug.Log("CostFunction : " + Math.Pow(a - y, 2));

                return Mathf.Pow(a - y, 2);
            }

            public float CostDerivative(float a, float y)
            {
                //UnityEngine.Debug.Log("CostDerivative : " + (2 * (a - y)));
                return 2*(a - y);
            }

            public CostType CostFunctionType()
            {
                return CostType.DistanceError;
            }
        }
    }
}