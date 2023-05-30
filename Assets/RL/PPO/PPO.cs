class Observation
{
    public double[] State;
    public int Action;
    public float Reward;
    public double[] NextState;
    public bool Terminated;
}

// TODO: apply Adam Optimizer to NN is a (seems like must have)
class Network
{
    public float[] Backward(float[] gradients) { }

    public float[] Forward(float[] input) { }

    public float[] ComputeGradients(float[] loss) { }

}



class Agent
{
    protected float gamma; // discount factor

    int ChooseAction(double[] state) { }
}


class PPOAgent : Agent
{
    // generic hyper-parameters
    private int _BatchSize = 64;
    private float _ClipRatio = 0.2;
    private float _DiscountFactor = 0.9;
    // this can be splitted into 2: one for actor network and the other for critic
    private float _LearningRate = 0.001;

    // PPO specific hyper-parameters
    private float _ValueCoefficient = 0.5;
    private float _EntropyCoefficient = 0.01;

    private Network _Actor;  // policy network;
    private Network _Critic; // value network;

    public void Train()
    {
        // Collect samples
        List<Observation> batchSamples = CollectSamples();

        // Compute action advantages
        float[] advantages = ComputeAdvantages(batchSamples.Select(o => o.State).ToArray());

        // Compute policy loss
        float[] oldProbs = _Actor.forward(batchSamples);
        float[] newProbs = ComputeNewActionProbabilities(batchSamples);
        float[] ratios = ComputeRatio(oldProbs, newProbs);
        float[] clippedRatios = ApplyClipping(ratios, _ClipRatio);
        float[] policyLoss = ComputePolicyLoss(clippedRatios, advantages);

        // Compute value loss
        float[] trueRewards = ComputeTrueRewards(batchSamples);
        float[] estimatedValues = _Critic.Forward(batchSamples.Select(o => o.state).ToArray()).Select(v => v[0]).ToArray();
        float[] valueLoss = ComputeValueLoss(estimatedValues, trueRewards);

        // Compute entropy bonus
        float[] entropyBonus = ComputeEntropyBonus(newProbs);

        // Compute total loss
        float[] actorTotalLoss = policyLoss.Zip(_EntropyCoefficient * entropyBonus, (p, e) => p - e).ToArray();
        float[] criticTotalLoss = _ValueCoefficient * valueLoss;

        // Update the actor and critic network parameters separately using their respective losses
        float[] actorGradients = _Actor.ComputeGradients(actorTotalLoss);
        _Actor.Backward(actorGradients);

        float[] criticGradients = _Critic.ComputeGradients(criticTotalLoss);
        _Critic.Backward(criticGradients);
    }

    // Collect a batch of samples and return them as a list of Experience objects
    private List<Observation> CollectSamples()
    {
        // _BatchSize is used here
    }

    // Compute the advantages of each action in the batch using the advantage function and the critic network
    // Return the advantages as an array of doubles
    private float[] ComputeAdvantages(List<Observation> observations)
    {
        // _Critic is used here
    }

    // Compute the new action probabilities using the current policy and the batch of experiences
    // Return the action probabilities as an array of doubles
    private float[] ComputeNewActionProbabilities(List<Observation> observations)
    {
        // _Actor is used here
    }

    // Compute the ratio between the new and old action probabilities
    // Return the ratio as an array of doubles
    private float[] ComputeRatio(float[] oldProbs, float[] newProbs) { }

    // Apply the clipping function to the ratios
    // Return the clipped ratios as an array of doubles
    private float[] ApplyClipping(float[] ratios)
    {
        // _ClipRatio is used here
    }

    // Compute the policy loss using the clipped ratios and advantages
    // Return thepolicy loss as an array of doubles
    private float[] ComputePolicyLoss(float[] clippedRation, float[] advantages) { }

    // Compute the true rewards obtained in the batch using the discount factor
    // Return the true rewards as an array of doubles
    private float[] ComputeTrueRewards(List<Observation> observations)
    {
        // _DiscountFactor is used here
    }

    // Compute the value loss by regressing the estimated value function on the true rewards
    // Return the value loss as an array of doubles
    private float[] ComputeValueLoss(float[] estimatedValues, float[] trueRewards) { }


    // Compute the entropy bonus to encourage exploration
    // Return the entropy bonus as an array of doubles
    private float[] ComputeEntropyBonus(float[] actionProbabilities) { }

}
