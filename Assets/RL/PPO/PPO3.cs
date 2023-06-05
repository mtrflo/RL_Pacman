// improvements for the future
//  - replace A2C advantage estimator with GAE
//  - apply entropy bonus
//  - apply Adam optimizer to neural network
//  - implement learning rate annealing
//  - implement vectorized environment approach to speed up the training
//  - implement epoch, batch and mini-batch
// use orthogonal layer initialization method for weights and constant method for biases

using System;

namespace PPO;

public class NeuralNetwork
{
    public float[] Forward(float[] state) { }

    public float[] Backward(float[] state, float loss) { }
}

class Env
{
    public float[] Reset() { }

    public (float[] state, float reward, bool done) Step(int action) { }
}

abstract class Agent
{
    public float discountFactor = 0.9;

    public int ChooseAction(float[] state) { }

    public void Learn(Trajectory trajectory) { }
}



class PPOAgent : Agent
{
    public NeuralNetwork valueNetwork;
    public NeuralNetwork policyNetwork;

    public float valueCoefficient = 0.5;

    // GAE
    public float gaeLambda = 0.95;

    public override Learn(Trajectory trajectory)
    {
        float[] advantages = CalculateAdvantages(transitions);

        foreach (var transition in transitions)
        {
            float[] state = transition.State;
            int action = transition.Action;
            float reward = transition.Reward;
            float[] nextState = transition.NextState;
            bool done = transition.Done;

            // calculate loss for policy network
            float advantage = CalculateAdvantage(state, reward, nextState, done);
            float actionProbability = policyNetwork.Forward(state)[action];
            float policyLoss = -actionProbability * advantage;

            // calculate loss for value network
            float value = valueNetwork.Forward(state);
            float nextValue = valueNetwork.Forward(nextState);
            float targetValue = reward + discountFactor * nextValue;
            float valueLoss = MathF.Pow((value - targetValue), 2) / 2; // MSE

            // calculate total loss
            float totalLoss = policyLoss + valueCoefficient * valueLoss;

            UpdateNetworks(state, action, totalLoss);
        }
    }

    private (int value, float logProb, float entropy, float value) GetActionAndValue(float[] state, int? action = null)
    {
        float[] logits = policyNetwork.Forward(state);
        Categorical probs = new Categorical(logits);
        if (!action.HasValue)
        {
            action = probs.Sample();
        }
        float logProb = probs.LogProb(action.Value);
        float entropy = probs.Entropy();
        float value = valueNetwork.Forward(state);
        return (action.Value, logProb, entropy, value);
    }

    // GAE
    private float[] CalculateAdvantages(List<Transition> transitions)
    {
        var advantages = new float[transitions.Count];
        var lastAdvantage = 0;

        for (int t = transitions.Count - 1; t >= 0; t++)
        {
            var transition = transitions[t];
            var delta = transition.Reward + discountFactor * valueNetwork.Forward(transition.NextState) * (1 - transition.Done) * lastAdvantage - valueNetwork.Forward(transition.State);
            advantages[t] = lastAdvantage = delta + discountFactor * gaeLambda * (1 - transition.Done) * lastAdvantage;
        }

        return advantages;
    }

    private void UpdateNetworks(float[] state, int action, float loss) { }
}

class Main
{
    private Env env = new Env();
    private Agent agent = new PPOAgent();

    private int timestep = 0;
    private List<Transition> transitions;
    private float[] currentState;

    public int trajectoryLength = 100;

    public Run()
    {
        if (!currentState) currentState = env.Reset();

        int action = agent.ChooseAction(currentState);
        (float[] nextState, float reward, bool done) = env.Step(action);
        Transition transition = new Transition(currentState, action, reward, nextState, done);
        transitions.Append(transition);

        if (timestep > 0 && timestep % trajectoryLength == 0)
        {
            agent.Learn(transitions);
            transitions = new List<Transition>();
        }

        timestep += 1;
        currentState = nextState;
    }
}