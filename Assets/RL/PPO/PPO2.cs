public class Transition
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

public class Observation
{
    public float[] State { get; set; }
    public int Action { get; set; }
    public float Reward { get; set; }
    public bool Done { get; set; }

    public Observation(float[] state, int action, float reward, bool done)
    {
        State = state;
        Action = action;
        Reward = reward;
        Done = done;
    }
}

class VecEnv
{
    private int numEnv = 1;

    public VecEnv(int numEnv)
    {
        this.numEnv = numEnv;
    }

    public List<float[]> reset()
    {
        // reset the env and return N states for N players*
    }

    public List<Observation> step()
    {
        // take N steps for N players*
    }
}

class Agent
{
    public int ChooseAction(float[] state) { }

    public List<int> ChooseActions(List<float[]> states) { }

    public void Learn(List<Transition> transitions) { }
}

class NeuralNetwork
{
    public float Predict(float[] state) { }

    public float[] Update(float[] state, int action, float loss) { }
}


class PPOAgent
{
    private NeuralNetwork policyNetwork;
    private NeuralNetwork valueNetwork;

    // generic hyper-parameters
    private int epochSize = 10;
    private float discountFactor = 0.9;
    private float learningRate = 0.001;

    // PPO specific hyper-parameters
    private float clipRatio = 0.2;
    private float valueCoefficient = 0.5;
    private float entropyCoefficient = 0.01;



    public void Train(List<Transition> transitions)
    {
        float[] states = transitions.Select(t => t.State).ToArray();
        float[] actions = transitions.Select(t => t.Action).ToArray();
        float[] rewards = transitions.Select(t => t.Reward).ToArray();
        float[] nextStates = transitions.Select(t => t.NextState).ToArray();
        float[] dones = transitions.Select(t => t.Done).ToArray();

        float[] advantages = CalculateAdvantage(states, rewards, nextStates, dones);

        for (int epoch = 0; epoch < epochSize; epoch++)
        {
            UpdatePolicy(transitions, advantages);
        }
    }

    public float GetActionProbabilities(float[] state)
    {
        return policyNetwork.Predict(state);
    }

    private float CalculateAdvantage(float[] states, float rewards, float[] nextStates, float[] dones)
    {
        int n = states.Length;
        float[] advantages = new float[n];

        for (int i = 0; i < n; i++)
        {
            float value = valueNetwork.Predict(states[i]);
            float nextValue = valueNetwork.Predict(nextStates[i]);
            float delta = rewards[i] + discountFactor * (1 - dones[i]) - value;
            advantages[i] = delta;
        }

        return advantages;
    }

    private void UpdatePolicy(List<Transition> transitions, float[] advantages)
    {
        int n = transitions.Count;

        for (int i = 0; i < n; i++)
        {
            float[] state = transitions[i].State;
            int action = transitions[i].Action;
            float oldProb = policyNetwork.Predict(state)[c]


        }
    }
}

