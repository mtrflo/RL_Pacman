using static System.Math;


namespace MonoRL
{

    public class Layer
    {
        public readonly int inputs;
        public readonly int nodes;

        public double[][] weights;
        public double[] biases;
        public IActivation activation;

        private double[] delta;
        private double[] X;
        private double[] Z;
        private double[] A;

        double learningRate = 0.3;

        public Layer(int inputs, int nodes, Activation.ActivationType activationType)
        {
            this.inputs = inputs;
            this.nodes = nodes;
            this.activation = Activation.GetActivationFromType(activationType);
            this.weights = new double[inputs][];
            this.biases = new double[nodes];
            this.delta = new double[nodes];
            this.X = new double[nodes];
            this.Z = new double[nodes];
            this.A = new double[nodes];

            InitializeWeights();
            InitializeBiases();
        }

        public double[] Forward(double[] X)
        {
            double[] Z = new double[nodes];
            for (int node = 0; node < this.nodes; node++)
            {
                Z[node] = 0;
                for (int input = 0; input  < this.inputs; input++)
                {
                    Z[node] += this.weights[node][input] * X[input] + this.biases[node];
                }
            }

            double[] A = new double[nodes];
            for (int node = 0; node < this.nodes; node++)
            {
                A[node] = this.activation.Activate(Z[node]);
            }

            this.X = X;
            this.Z = Z;
            this.A = A;

            return A;
        }

        public double[] Backward(double lr, double[] deltas)
        {
            for (int node = 0; node < this.nodes; node++)
            {
                this.delta[node] = 0;
                for (int i = 0; i < deltas.Length; i++)
                {
                    double delta_ = delta[i];
                    double z = this.Z[node];
                    this.delta[node] += delta_ * this.activation.Derivative(z);
                }
            }

            UpdateWeights(lr);
            UpdateBiases(lr);

            double[] propagatedDelta = new double[this.inputs];
            for (int input = 0; input < this.inputs; input++)
            {
                propagatedDelta[input] = 0;
                for (int node = 0; node < this.nodes; node++)
                {
                    propagatedDelta[input] += this.delta[node] * this.weights[node][input];
                }
            }

            return propagatedDelta;
        }

        private void UpdateWeights(double lr)
        {
            for (int node = 0; node < this.nodes; node++)
            {
                for (int input = 0; input < this.inputs; input++)
                {
                    double gradW = this.delta[node] * this.X[input];
                    this.weights[node][input] -= lr * gradW;
                }
            }
        }

        private void UpdateBiases(double lr)
        {
            for (int node = 0; node < this.nodes; node++)
            {
                double gradB = this.delta[node];
                this.biases[node] -= lr * gradB;
            }
        }

        private void InitializeWeights()
        {
            for (int node = 0; node < this.nodes; node++)
            {
                for (int input = 0; input < this.inputs; input++)
                {
                    this.weights[node][input] = 0;
                }
            }
        }

        private void InitializeBiases()
        {
            for (int node = 0; node < this.nodes; node++)
            {
                this.biases[node] = 0;
            }
        }
    }

}