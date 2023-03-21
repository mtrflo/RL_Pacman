namespace MonoRL
{

	public interface ICost
	{
		double CostFunction(double predictedOutput, double expectedOutput);

		double CostDerivative(double predictedOutput, double expectedOutput);

		Cost.CostType CostFunctionType();
	}
}