public interface IActivation
{
    double Activate(double z);

    double Derivative(double z);

    Activation.ActivationType GetActivationType();
}
