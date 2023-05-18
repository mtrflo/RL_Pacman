class Network { }

class Observation
{
    public double[] State;
    public int Action;
    public float Reward;
    public double[] NextState;
    public bool Terminated;
}

class PPO
{
    private Network _Actor;
    private Network _Critic;

    public void Learn(Observation observation) { }

    public int ChooseAction(double[] State) { }
}

