using System.Diagnostics;
using static System.Math;
namespace MonoRL
{


    public readonly struct Activation
    {

        public enum ActivationType
        {
            ReLU,
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

        public readonly struct Sigmoid : IActivation
        {
            public double Activate(double z)
            {
                return 1.0 / (1 + Exp(-z));
            }

            public double Derivative(double z)
            {
                double a = Activate(z);
                return a * (1 - a);
            }

            public ActivationType GetActivationType()
            {
                return ActivationType.Sigmoid;
            }
        }


        public readonly struct TanH : IActivation
        {
            public double Activate(double z)
            {
                double e2 = Exp(2 * z);
                return (e2 - 1) / (e2 + 1);
            }

            public double Derivative(double z)
            {
                double e2 = Exp(2 * z);
                double t = (e2 - 1) / (e2 + 1);
                return 1 - t * t;
            }

            public ActivationType GetActivationType()
            {
                return ActivationType.TanH;
            }
        }
    }
}