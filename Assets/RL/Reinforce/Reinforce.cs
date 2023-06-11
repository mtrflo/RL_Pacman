using System;

class CategoricalDistribution
{
    private double[] probabilities;
    private double[] cumulativeProbabilities;
    private int numCategories;

    public CategoricalDistribution(double[] probabilities)
    {
        this.probabilities = probabilities;
        this.numCategories = probabilities.Length;
        this.cumulativeProbabilities = new double[numCategories];

        // Compute cumulative probabilities
        double sum = 0;
        for (int i = 0; i < numCategories; i++)
        {
            sum += probabilities[i];
            cumulativeProbabilities[i] = sum;
        }
    }

    public int Sample()
    {
        double rand = new Random().NextDouble();
        for (int i = 0; i < numCategories; i++)
        {
            if (rand < cumulativeProbabilities[i])
            {
                return i;
            }
        }
        return numCategories - 1; // edge case
    }

    public double LogProb(int category)
    {
        if (category < 0 || category >= probabilities.Length)
        {
            throw new ArgumentException("Invalid category index.");
        }
        return Math.Log(probabilities[category]);
    }
}

class Transition
{
    public float[] state;
    public int action;
    public float[] nextState;
    public float reward;
}

class NeuralNetwork
{
    public float[] Forward(float[] state) { }

    public void Backward(float loss) { }
}


class Reinforce
{
    private NeuralNetwork _policyNetwork;

    public float discountFactor = 0.99;
    public int trajectoryLength = 100;
    public float learningRate = 0.0001;

    int ChooseAction(float[] state) { }

    int SampleAction(float[] state)
    {
        float[] actionProbs = _policyNetwork.Forward(state);
        CategoricalDistribution catDist = new CategoricalDistribution(actionProbs);
        return catDist.Sample();
    }

    public Run()
    {
        Transition[] transitions;
        float[] currentState;
        int step = 0;

        // Rollout Phase
        while (env.stopped())
        {
            int action = agent.SampleAction(currentState);
            (nextState, reward, done) = env.step(action);

            transitions.Add(new Transition(currentState, action, nextState, reward, done));

            currentState = nextState;

            step++;


            // Learning Phase
            if (step %= trajectoryLength)
            {
                agent.Learn(transitions);
                transitions = [];
            }
        }

    }



    public Learn(Transition[] transitions)
    {
        float G = 0;

        for (int k = transitions.Count - 1; k >= 0; k--)
        {
            Transition transition = transitions[k];

            G += MathF.Pow(discountFactor, k) * transition.reward;

            float[] actionProbs = _policyNetwork.Forward(transition.state);
            CategoricalDistribution categoricalDist = new CategoricalDistribution(actionProbs);
            float logProb = categoricalDist.LogProb(transition.action);
            float loss = -logProb * G;

            _policyNetwork.Backward(loss);
        }
    }
}