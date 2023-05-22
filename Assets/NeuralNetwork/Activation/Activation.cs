using System.Diagnostics;
using Unity.Mathematics;
using UnityEngine;
using static System.Math;
namespace MonoRL
{


    public readonly struct Activation
    {

        public enum ActivationType
        {
            ReLU,
            LReLU,
            Sigmoid,
            TanH,
            Linear
        }

        public static IActivation GetActivationFromType(ActivationType type)
        {
            switch (type)
            {
                case ActivationType.ReLU:
                    return new ReLU();
                case ActivationType.Linear:
                    return new Linear();
                case ActivationType.Sigmoid:
                    return new Sigmoid();
                case ActivationType.TanH:
                    return new TanH();
                case ActivationType.LReLU:
                    return new LReLU();
                default:
                    UnityEngine.Debug.LogError("Unhandled activation type");
                    return new Linear();
            }
        }

        public readonly struct ReLU : IActivation
        {
            public float Activate(float z)
            {
                return Max(0, z);
            }

            public float Derivative(float z)
            {
                return (z > 0) ? 1 : 0;
            }

            public ActivationType GetActivationType()
            {
                return ActivationType.ReLU;
            }
        }

        public readonly struct Linear : IActivation
        {
            public float Activate(float z)
            {
                return z;
            }

            public float Derivative(float z)
            {
                return 1;
            }

            public ActivationType GetActivationType()
            {
                return ActivationType.Linear;
            }
        }

        public readonly struct Sigmoid : IActivation
        {
            public float Activate(float z)
            {
                return 1.0f / (1 + math.exp(-z));
            }

            public float Derivative(float z)
            {
                float a = Activate(z);
                return a * (1 - a);
            }

            public ActivationType GetActivationType()
            {
                return ActivationType.Sigmoid;
            }
        }


        public readonly struct TanH : IActivation
        {
            public float Activate(float z)
            {
                float e2 = math.exp(2 * z);
                return (e2 - 1) / (e2 + 1);
            }

            public float Derivative(float z)
            {
                float e2 = math.exp(2 * z);
                float t = (e2 - 1) / (e2 + 1);
                return 1 - t * t;
            }

            public ActivationType GetActivationType()
            {
                return ActivationType.TanH;
            }
        }

        public readonly struct LReLU : IActivation
        {
            private const float a = 0.01f;
            public float Activate(float z)
            {
                return (z >= 0) ? z : a * z;
            }

            public float Derivative(float z)
            {
                return (z >= 0) ? 1 : a;
            }

            public ActivationType GetActivationType()
            {
                return ActivationType.LReLU;
            }
        }
    }
}