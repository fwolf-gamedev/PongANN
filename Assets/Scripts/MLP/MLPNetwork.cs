using UnityEngine;
using System.IO;
using System.Collections.Generic;
using WyrmTale;

/// <summary>
/// Multi Layer Perceptron Network
/// </summary>

public class MLPNetwork : MonoBehaviour
{
    public float Gain = 0.9f;
    public float InitWeightRange = 0.2f;

    public bool ComputWeights = false;
    public bool LoadWeights = false;
    public bool DoSave = false;
    public bool DoLoad = false;

    public int NbInputPerceptron = 4;
    public int NbHiddenPerceptron = 6;
    public int NbOutputPerceptron = 1;

    public List<Perceptron> InputPerceptronList = new List<Perceptron>();
    public List<Perceptron> HiddenPerceptronList = new List<Perceptron>();
    public List<Perceptron> OutputPerceptronList = new List<Perceptron>();

    // debug info
    public List<float> HiddenLayerWeights = new List<float>();
    public List<float> OutputLayerWeights = new List<float>();

    void Start()
    {
        for (int i = 0; i < NbInputPerceptron; i++)
        {
            Perceptron perceptron = new Perceptron(this);
            InputPerceptronList.Add(perceptron);
        }

        for (int i = 0; i < NbHiddenPerceptron; i++)
        {
            Perceptron perceptron = new Perceptron(this);
 
            for (int j = 0; j < NbInputPerceptron; j++)
            {
                Perceptron.Input input = new Perceptron.Input();
                input.InputPerceptron = InputPerceptronList[j];
                input.Weight = Random.Range(-InitWeightRange, InitWeightRange);
                perceptron.inputList.Add(input);
            }

            HiddenPerceptronList.Add(perceptron);
        }

        for (int i = 0; i < NbOutputPerceptron; i++)
        {
            Perceptron perceptron = new Perceptron(this);

            for (int j = 0; j < NbHiddenPerceptron; j++)
            {
                Perceptron.Input input = new Perceptron.Input();
                input.InputPerceptron = HiddenPerceptronList[j];
                input.Weight = Random.Range(-InitWeightRange, InitWeightRange);
                perceptron.inputList.Add(input);
            }

            OutputPerceptronList.Add(perceptron);
        }

        Debug.Log("layers init done");
    }

    void Update()
    {
        // debug
        if (ComputWeights)
        {
            ComputWeights = false;
            GenerateWeights();
        }
        if (LoadWeights)
        {
            LoadWeights = false;
            LoadFromWeight();
        }
        if (DoSave)
        {
            DoSave = false;
            Save();
        }
        if (DoLoad)
        {
            DoLoad = false;
            Load();
        }
    }

    public void LearnPattern(List<float> input, List<float> output)
    {
        GenerateOutput(input);
        Backpropagation(output);
    }

    public void GenerateOutput(List<float> inputs)
    {
        for(int i = 0; i < InputPerceptronList.Count; i++)
            InputPerceptronList[i].State = inputs[i];

        for (int i = 0; i < HiddenPerceptronList.Count; i++)
            HiddenPerceptronList[i].FeedForward();

        for (int i = 0; i < OutputPerceptronList.Count; i++)
            OutputPerceptronList[i].FeedForward();
    }

    // generate debug info
    public void GenerateWeights()
    {
        HiddenLayerWeights.Clear();
        for (int i = 0; i < NbHiddenPerceptron; i++)
        {
            for (int j = 0; j < HiddenPerceptronList[i].inputList.Count; j++)
            {
                Perceptron.Input input = HiddenPerceptronList[i].inputList[j];
                HiddenLayerWeights.Add(input.Weight);
            }
        }
        OutputLayerWeights.Clear();
        for (int i = 0; i < NbOutputPerceptron; i++)
        {
            for (int j = 0; j < OutputPerceptronList[i].inputList.Count; j++)
            {
                Perceptron.Input input = OutputPerceptronList[i].inputList[j];
                OutputLayerWeights.Add(input.Weight);
            }
        }
    }

    // TODO refactor
    public void LoadFromWeight()
    {
        InputPerceptronList.Clear();
        HiddenPerceptronList.Clear();
        OutputPerceptronList.Clear();

        int weightCount = 0;

        for (int i = 0; i < NbInputPerceptron; i++)
        {
            Perceptron perceptron = new Perceptron(this);
            InputPerceptronList.Add(perceptron);
        }

        for (int i = 0; i < NbHiddenPerceptron; i++)
        {
            Perceptron perceptron = new Perceptron(this);

            for (int j = 0; j < NbInputPerceptron; j++)
            {
                Perceptron.Input input = new Perceptron.Input();
                input.InputPerceptron = InputPerceptronList[j];
                input.Weight = HiddenLayerWeights[weightCount++];
                perceptron.inputList.Add(input);
            }

            HiddenPerceptronList.Add(perceptron);
        }

        weightCount = 0;

        for (int i = 0; i < NbOutputPerceptron; i++)
        {
            Perceptron perceptron = new Perceptron(this);

            for (int j = 0; j < NbHiddenPerceptron; j++)
            {
                Perceptron.Input input = new Perceptron.Input();
                input.InputPerceptron = HiddenPerceptronList[j];
                input.Weight = OutputLayerWeights[weightCount++];
                perceptron.inputList.Add(input);
            }

            OutputPerceptronList.Add(perceptron);
        }
    }

    public List<float> GetOutputs()
    {
        List<float> outputs = new List<float>();
        for (int i = 0; i < OutputPerceptronList.Count; i++)
            outputs.Add(OutputPerceptronList[i].State);

        return outputs;
    }

    private void Backpropagation(List<float> outputs)
    {
        for (int i = 0; i < OutputPerceptronList.Count; i++)
        {
            Perceptron perceptron = OutputPerceptronList[i];
            float state = perceptron.State;

            // calculate error term (gradient)
            float error = state * (1 - state) * (outputs[i] - state);
            // adjust output perceptron weight with error
            perceptron.AdjustWeight(error);
        }

        for (int i = 0; i < HiddenPerceptronList.Count; i++)
        {
            Perceptron perceptron = HiddenPerceptronList[i];
            float state = perceptron.State;

            // calculate error term
            float sum = 0;
            for (int j = 0; j < OutputPerceptronList.Count; j++)
            {
                Perceptron outputPerceptron = OutputPerceptronList[j];
                sum += outputPerceptron.GetIncomingWeight(perceptron) * outputPerceptron.Error;
            }
            float error = state * (1 - state) * sum;

            perceptron.AdjustWeight(error);
        }
    }

    public JSON Serialize()
    {
        JSON result = new JSON();

        result["NbInputPerceptron"] = NbInputPerceptron;
        result["NbHiddenPerceptron"] = NbHiddenPerceptron;
        result["NbOutputPerceptron"] = NbOutputPerceptron;

        for (int i = 0; i < HiddenLayerWeights.Count; i++ )
        {
            result[i.ToString()] = HiddenLayerWeights[i];
        }
        for (int i = 0; i < OutputLayerWeights.Count; i++)
        {
            result[i.ToString()] = OutputLayerWeights[i];
        }

        return result;
    }

    public void Deserialize(JSON source)
    {
        HiddenLayerWeights.Clear();
        OutputLayerWeights.Clear();

        NbInputPerceptron = System.Convert.ToInt32(source["NbInputPerceptron"]);
        NbHiddenPerceptron = System.Convert.ToInt32(source["NbHiddenPerceptron"]);
        NbOutputPerceptron = System.Convert.ToInt32(source["NbOutputPerceptron"]);

        int nbHiddenLayerWeights = NbInputPerceptron * NbHiddenPerceptron;
        for (int i = 0; i < nbHiddenLayerWeights; i++)
        {
            HiddenLayerWeights.Add(System.Convert.ToSingle(source[i.ToString()]));
        }
        int nbOutputLayerWeights = NbHiddenPerceptron * NbOutputPerceptron;
        for (int i = 0; i < nbOutputLayerWeights; i++)
        {
            OutputLayerWeights.Add(System.Convert.ToSingle(source[i.ToString()]));
        }
    }

    public void Save()
    {
        GenerateWeights();

        string serializedData = Serialize().serialized;
        byte[] dataArray = System.Text.Encoding.ASCII.GetBytes(serializedData);
        File.WriteAllBytes("save", dataArray);
    }

    public void Load()
    {
        JSON saveData = new JSON();
        saveData.serialized = File.ReadAllText("save");
        Deserialize(saveData);

        LoadFromWeight();
    }
}
