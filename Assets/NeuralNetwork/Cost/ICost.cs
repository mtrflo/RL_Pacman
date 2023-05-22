namespace MonoRL
{

	public interface ICost
	{
		float CostFunction(float predictedOutput, float expectedOutput);

		float CostDerivative(float predictedOutput, float expectedOutput);

		Cost.CostType CostFunctionType();
	}
}