using System;

// Categorical Distribution
class Categorical
{
    private double[] logits;
    private Random random;

    public Categorical(double[] logits)
    {
        this.logits = logits;
        this.random = new Random();
    }

    public int Sample()
    {
        double[] probs = Softmax.Compute(logits);
        double uniformSample = random.NextDouble();
        double cumulativeProb = 0;
        int i = 0;
        while (i < probs.Length && uniformSample > cumulativeProb)
        {
            cumulativeProb += probs[i];
            i++;
        }
        return i - 1;
    }

    public double LogProb(int action)
    {
        double[] probs = Softmax.Compute(logits);
        return Math.Log(probs[action]);
    }

    public double Entropy()
    {
        double[] probs = Softmax.Compute(logits);
        double entropy = 0;
        for (int i = 0; i < probs.Length; i++)
        {
            if (probs[i] > 0)
            {
                entropy -= probs[i] * Math.Log(probs[i]);
            }
        }
        return entropy;
    }
}