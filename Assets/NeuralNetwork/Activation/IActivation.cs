namespace MonoRL
{

    public interface IActivation
    {
        float Activate(float z);

        float Derivative(float z);

        Activation.ActivationType GetActivationType();
    }

}
