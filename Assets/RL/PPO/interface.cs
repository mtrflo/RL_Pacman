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

class Env
{
    public float[] Reset() { }

    public (float[] state, float reward, bool done) Step(int action) { }
}

class Agent
{
    public int ChooseAction(float[] state) { }

    public void Learn(List<Transition> transitions) { }
}

class Main
{
    Env env = new Env();
    Agent agent = new Agent();

    private int totalTimesteps = 0;
    private List<Transition> transitions;
    private float[] currentState;

    public int trajectoryLength = 100;

    public Run()
    {
        if (!currentState) currentState = env.Reset();

        var action = agent.ChooseAction(currentState);
        (var nextState, var reward, var done) = env.Step(action);
        var transition = new Transition(currentState, action, reward, nextState, done);
        transitions.Append(transition);

        if (totalTimesteps >= trajectoryLength && totalTimesteps % trajectoryLength == 0)
        {
            agent.Learn(transitions);
            transitions = new List<Transition>();
        }

        totalTimesteps += 1;
        currentState = nextState;
    }
}