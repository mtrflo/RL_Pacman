public class Trajectory
{
    public int length { get; }

    List<Transition> transitions;

    public Trajectory(List<Transition> transitions)
    {
        length = transitions.Count;
        this.transitions = transitions;
    }
}