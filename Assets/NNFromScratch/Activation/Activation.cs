using System.Diagnostics;
using static System.Math;
namespace MonoRL
{


    public readonly struct Activation
    {

        public enum ActivationType
        {
            ReLU,
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
                default:
                    UnityEngine.Debug.LogError("Unhandled activation type");
                    return new Linear();
            }
        }

        public readonly struct ReLU : IActivation
        {
            public double Activate(double z)
            {
                return Max(0, z);
            }

            public double Derivative(double z)
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
            public double Activate(double z)
            {
                return z;
            }

            public double Derivative(double z)
            {
                return 1;
            }

            public ActivationType GetActivationType()
            {
                return ActivationType.Linear;
            }
        }
    }
}