using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkTrainer : MonoBehaviour
{
	public event System.Action<int> onEpochComplete;
	public event System.Action onTrainingStarted;

	public string networkSaveName = "TestNetwork";
	public TextAsset networkFile;
	[Range(0, 1)] public float trainingSplit = 0.8f;

	public HyperParameters hyperParameters;

	[Space()]
	public TrainingSessionInfo sessionInfo;

	ImageLoader imageLoader;
	DataPoint[] allData;
	DataPoint[] trainingData;
	DataPoint[] validationData;
	Batch[] trainingBatches;

	// State
	public NeuralNetwork neuralNetwork { get; private set; }
	bool trainingActive;
	int batchIndex;
	double currentLearnRate;
	bool hasLoaded;
	int epochCount;


	void Awake()
	{
		if(networkFile)
            neuralNetwork = NetworkSaveData.LoadNetworkFromData(networkFile.text);

        StartTrainingSession();
	}

	public void StartTrainingSession()
	{
		if (!hasLoaded)
		{
			LoadData();
		}

		neuralNetwork = new NeuralNetwork(hyperParameters.layerSizes);
		var activation = Activation.GetActivationFromType(hyperParameters.activationType);
		var outputLayerActivation = Activation.GetActivationFromType(hyperParameters.outputActivationType);
		neuralNetwork.SetActivationFunction(activation, outputLayerActivation);
		neuralNetwork.SetCostFunction(Cost.GetCostFromType(hyperParameters.costType));

		sessionInfo = new TrainingSessionInfo(1);
		sessionInfo.StartTimer();

		currentLearnRate = hyperParameters.initialLearningRate;
		batchIndex = 0;
		epochCount = 0;
		trainingActive = true;
		onTrainingStarted?.Invoke();
	}

	void LoadData()
	{
		return;
		imageLoader = FindObjectOfType<ImageLoader>();
		allData = imageLoader.GetAllData();
		(trainingData, validationData) = DataSetHelper.SplitData(allData, trainingSplit);
		trainingBatches = DataSetHelper.CreateMiniBatches(trainingData, hyperParameters.minibatchSize);
		hasLoaded = true;
	}


	/*
	void Run()
	{
		var sw = System.Diagnostics.Stopwatch.StartNew();

		while (sw.ElapsedMilliseconds < 16)
		{
			neuralNetwork.Learn(trainingBatches[batchIndex].data, currentLearnRate, hyperParameters.regularization, hyperParameters.momentum);
			sessionInfo.BatchCompleted();
			batchIndex++;

			if (batchIndex >= trainingBatches.Length)
			{
				EpochCompleted();
			}
		}

		UpdateSessionInfo();
	}
	*/
    public int ChooseAction(double[] inputs, int lastAction,float lastStepReward)
    {
        int action = neuralNetwork.lastOutputs == default ? 0 : neuralNetwork.lastOutputs.ToList().IndexOf(neuralNetwork.lastOutputs.Max());
        var sw = System.Diagnostics.Stopwatch.StartNew();
        DataPoint dp = new DataPoint(inputs, lastAction, 4);

		double QTarget = GetQTarget(dp, lastAction, lastStepReward);
		print("QTarget : " + QTarget);
		for (int i = 0; i < dp.expectedOutputs.Length; i++)
			dp.expectedOutputs[i] = lastAction.Equals(i) ? QTarget : 1 - QTarget;

        neuralNetwork.Learn(dp, currentLearnRate, hyperParameters.regularization, hyperParameters.momentum);
        sessionInfo.BatchCompleted();
        batchIndex++;

        //if (batchIndex >= trainingBatches.Length)
        //{
            //EpochCompleted();
        //}

        UpdateSessionInfo();
		if (QTarget == -1)
		{
			neuralNetwork.lastOutputs = new double[neuralNetwork.lastOutputs.Length];
            action = UnityEngine.Random.Range(0, 4);
		}
		return action;
    }

	private double GetQTarget(DataPoint dp, int lastAction, float reward)
	{
		double gamma = 0.8f;
		double QTarget = 0;
		double QEval = neuralNetwork.lastOutputs == default ? 0 : neuralNetwork.lastOutputs[lastAction];
		double QNext = neuralNetwork.Forward(dp,false).Max();
		QTarget = QEval + hyperParameters.initialLearningRate * (reward + gamma * QNext - QEval);

		//QTarget = QTarget/4;
        return QTarget;
	}

    void EpochCompleted()
	{
		onEpochComplete?.Invoke(epochCount);

		batchIndex = 0;
		epochCount++;
		DataSetHelper.ShuffleBatches(trainingBatches);
		currentLearnRate = (1.0 / (1.0 + hyperParameters.learnRateDecay * epochCount)) * hyperParameters.initialLearningRate;
	}

	void UpdateSessionInfo()
	{
		sessionInfo.currentLearnRate = currentLearnRate;
	}

	public void Save()
	{
		string path = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Assets", networkSaveName + ".json");
		NetworkSaveData.SaveToFile(neuralNetwork, path);
		Debug.Log("Saved network to: " + path);
	}

	public void Save(string path, string fileName)
	{
		string fullPath = System.IO.Path.Combine(path, fileName + ".json");
		NetworkSaveData.SaveToFile(neuralNetwork, fullPath);
	}

	public EvaluationData Evaluate(bool useValidationSet = true)
	{
		var data = (useValidationSet) ? validationData : trainingData;
		return NetworkEvaluator.Evaluate(neuralNetwork, data);
	}

}
