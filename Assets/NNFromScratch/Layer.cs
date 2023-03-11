using static System.Math;


namespace MonoRL
{

    public class Layer
    {
        public readonly int InputSize;//size
        public readonly int NodeSize;//size

        public double[,] Weights;//data
        public double[] Biases;
        public IActivation Activation;

        private double[] _Delta;
        private double[] _Inputs;//data
        private double[] _Outputs;
        
        public Layer(int inputSize, int nodeSize, Activation.ActivationType activationType)
        {
            InputSize = inputSize;
            NodeSize = nodeSize;
            Activation = MonoRL.Activation.GetActivationFromType(activationType);
            Weights = new double[nodeSize,inputSize];
            Biases = new double[nodeSize];
            _Delta = new double[nodeSize];
            _Inputs = new double[nodeSize];
            _Outputs = new double[nodeSize];

            InitializeWeights();
            InitializeBiases();
        }

        public double[] Forward(double[] inputs)
        {
            double[] calculatedOutputs = new double[NodeSize];
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
            {
                calculatedOutputs[nodeIndex] = 0;
                for (int inputIndex = 0; inputIndex  < InputSize; inputIndex++)
                {
                    calculatedOutputs[nodeIndex] += Weights[nodeIndex,inputIndex] * inputs[inputIndex] + Biases[nodeIndex];
                }
            }

            double[] activatedValues = new double[NodeSize];
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
            {
                activatedValues[nodeIndex] = Activation.Activate(calculatedOutputs[nodeIndex]);
            }

            _Inputs = inputs;
            _Outputs = calculatedOutputs;

            return activatedValues;
        }

        public double[] Backward(double lr, double[] deltas)
        {
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
            {
                _Delta[nodeIndex] = 0;
                for (int i = 0; i < deltas.Length; i++)
                {
                    double delta_ = _Delta[i];
                    double output = _Outputs[nodeIndex];
                    _Delta[nodeIndex] += delta_ * Activation.Derivative(output);
                }
            }

            UpdateWeights(lr);
            UpdateBiases(lr);

            double[] propagatedDelta = new double[InputSize];
            for (int inputIndex = 0; inputIndex < InputSize; inputIndex++)
            {
                propagatedDelta[inputIndex] = 0;
                for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
                {
                    propagatedDelta[inputIndex] += _Delta[nodeIndex] * Weights[nodeIndex, inputIndex];
                }
            }

            return propagatedDelta;
        }

        private void UpdateWeights(double lr)
        {
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
            {
                for (int inputIndex = 0; inputIndex < InputSize; inputIndex++)
                {
                    double gradW = _Delta[nodeIndex] * _Inputs[inputIndex];
                    Weights[nodeIndex, inputIndex] -= lr * gradW;
                }
            }
        }

        private void UpdateBiases(double lr)
        {
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
            {
                double gradB = _Delta[nodeIndex];
                Biases[nodeIndex] -= lr * gradB;
            }
        }

        private void InitializeWeights()
        {
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
                for (int inputIndex = 0; inputIndex < InputSize; inputIndex++)
                    Weights[nodeIndex, inputIndex] = 0;
        }

        private void InitializeBiases()
        {
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
                Biases[nodeIndex] = 0;
        }
    }

}