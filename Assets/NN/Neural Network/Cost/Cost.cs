using System;

public class Cost
{

	public enum CostType
	{
		MeanSquareError
	}

	public static ICost GetCostFromType(CostType type)
	{
		switch (type)
		{
			case CostType.MeanSquareError:
				return new MeanSquaredError();
			default:
				UnityEngine.Debug.LogError("Unhandled cost type");
				return new MeanSquaredError();
		}
	}

	public class MeanSquaredError : ICost
	{
		public double CostFunction(double predictedOutput, double expectedOutput)
		{
			double cost = Math.Pow(predictedOutput - expectedOutput, 2);
			
			return 0.5 * cost;
		}

		public double CostDerivative(double predictedOutput, double expectedOutput)
		{
			return predictedOutput - expectedOutput;
		}

		public CostType CostFunctionType()
		{
			return CostType.MeanSquareError;
		}
	}
	
}