using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class NNLayer
{
    public int input_size, output_size;
    public IActivation activation;
    public double lr;
    public double[][] weights;
    public double[] bias;
    
    public double[] dot_bias;
    public double[] inputs;

    public NNLayer(int input_size,int output_size, IActivation activation, double lr = 0.001f)
    {
        this.input_size = input_size;
        this.output_size = output_size;
        this.activation = activation;
        this.lr = lr;
        
        dot_bias = new double[output_size];
        
        weights = new double[output_size][];
        for (int i = 0; i < output_size; i++)
        {
            weights[i] = new double[input_size];
            for (int j = 0; j < input_size; j++)
                weights[i][j] = Random.Range(-0.5f, 0.5f);
        }
        
        bias = new double[output_size];
        for (int i = 0; i < bias.Length; i++)
            bias[i] = 1;
    }

    public double[] Forward(double[] inputs)
    {
        this.inputs = inputs;
        double[] output = new double[output_size];
        for (int i = 0; i < output_size; i++)
            dot_bias[i] = MathUtils.Dot(inputs, weights[i]) + bias[i];

        for (int i = 0; i < output.Length; i++)
            output[i] = activation.Activate(dot_bias,i);

        return output;
    }

    public void UpdateWeights(double[] gradient)
    {
        for (int i = 0; i < input_size; i++)
            for (int j = 0; j < output_size; j++)
                weights[i][j] -= lr * gradient[i];
    }

    public double[] Backward(double[] gradient)
    {
        if (activation.GetActivationType() != Activation.ActivationType.Linear)
        {
            //gradient = MathUtils.Multiply(activation.Derivative(dot_bias), gradient);
            for (int i = 0; i < gradient.Length; i++)
            {
                gradient[i] = activation.Derivative(dot_bias, i) * gradient[i];
            }
        }

        double[] dW = new double[output_size];
        //for (int i=0; i<output_size; i++) 
        //{
        //    dW[i] = MathUtils.Dot();
        //}
        //this.UpdateWeights(dW);
        
        //double[] dA = new dounle [];
        //for () {
        //MathUtils.Dot(gradient, this.weights);

        //}

        return default;
    }
}

public static class MathUtils
{
    public static double Dot(double[] v1, double value)
    {
        double res = 0;
        for (int i = 0; i < v1.Length; i++)
            res += v1[i] * value;

        return res;
    }
    public static double Dot(double[] v1, double[] v2)
    {
        double res = 0;
        for (int i = 0; i < v1.Length; i++)
            res += v1[i] * v2[i];

        return res;
    }

    public static double[] Multiply(double[] v1, double[] v2)
    {
        double[] res = new double[v1.Length];
        for (int i = 0; i < res.Length; i++)
            res[i] = v1[i] * v2[i];
        return res;
    }
}