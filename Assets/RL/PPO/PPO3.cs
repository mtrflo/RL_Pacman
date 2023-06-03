// improvements for the future
//  - replace A2C advantage estimator with GAE
//  - apply entropy bonus
//  - apply Adam optimizer to neural network
//  - implement vectorized environment approach to speed up the training
//  - implement epoch and batch

using System;

namespace PPO;

class Transition
{
    public float[] State { get; set; }
    public int Action { get; set; }
    public float Reward { get; set; }
    public float[] NextState { get; set; }
    public bool Done { get; set; }

    public Transition(float[] state, int action, float reward, float[] nextState, bool done)
    {
        State = state;
        Action = action;
        Reward = reward;
        NextState = nextState;
        Done = done;
    }
}


public class NeuralNetwork
{
    public float Forward(float[] state) { }

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

    public void Learn(List<Transition> transitions) { }
}

class PPOAgent : Agent
{
    public NeuralNetwork valueNetwork;
    public NeuralNetwork policyNetwork;

    public float valueCoefficient = 0.5;


    public override Learn(List<Transition> transitions)
    {
        for (int i = 0; i < transitions; i++)
        {
            Transition transition = transitions[i];

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
            float targetValue = reward + discountFactor * valueNetwork.Forward(nextState);
            float valueLoss = MathF.Pow((value - targetValue), 2) / 2; // MSE

            // calculate total loss
            float totalLoss = policyLoss + valueCoefficient * valueLoss;

            UpdateNetworks(state, action, totalLoss);
        }
    }

    private float CalculateAdvantage(float[] state, float reward, float[] nextState, float[] done)
    {
        float value = valueNetwork.Forward(state);
        float nextValue = valueNetwork.Forward(nextState);
        float advantage = reward + discountFactor * nextValue - value;

        return advantage;
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