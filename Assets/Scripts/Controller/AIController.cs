using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MLPNetwork))]
public class AIController : PaddleController
{
    public int UpdateFrenquency = 1;
    public float output = 0f;
    public float ErrorAcceptation = 0.1f;

    float expectedOutput;
    public float ExpectedOutput { get { return expectedOutput; } }
    float deltaError;
    public float DeltaError { get { return deltaError; } }
    public bool IsErrorAcceptable { get { return deltaError <= ErrorAcceptation; } }

    public bool IsLearning = false;

    private int frameCount = 0;

    MLPNetwork MLPNet = null;

    List<float> outputList = null;
    List<float> inputList = new List<float>();

	// Use this for initialization
	protected override void Start ()
    {
        base.Start();
        MLPNet = GetComponent<MLPNetwork>();
    }

    float wantedPosY;

    void FixedUpdate ()
    {
        if (++frameCount % UpdateFrenquency != 0)
            return;

        ProcessOutput();

        // deduce paddle move from output
        wantedPosY = Ball.ComputeBallPosCourt(output);
    }

    void ProcessOutput()
    {
        inputList.Clear();
        Ball ball = GameMgr.Instance.GetBall();
        //inputList.Add(Ball.GetBallPos0To1Rounded(ball.transform.position.y));
        inputList.Add(ball.transform.position.y);
        //Rigidbody2D rb = ball.Rigidbody;
        //inputList.Add(Ball.GetAngleInt(rb.velocity.normalized));
        // BIAS
        //inputList.Add(1f);

        if (outputList != null)
            outputList.Clear();

        MLPNet.GenerateOutput(inputList);
        outputList = MLPNet.GetOutputs();
        output = outputList[0];
    }


    void Update()
    {
        MoveToPos(wantedPosY);
    }

    public void OnPointLost(Vector3 ballPos)
    {
        if (IsLearning)
            LearnFromBallPos(ballPos);
    }

    public void OnBallCollideAIPaddle(Vector3 ballPos)
    {
        if (IsLearning)
            LearnFromBallPos(ballPos);
    }

    public void OnBallThrown()
    {
        ProcessOutput();
    }

    private void LearnFromBallPos(Vector3 ballPos)
    {
        expectedOutput = Ball.GetBallPos0To1Rounded(ballPos.y);
        Debug.Log(expectedOutput);

        deltaError = Mathf.Abs(expectedOutput - Ball.GetRoundedValue(output, 2));

        // do learning via backpropagation
        List<float> outputs = new List<float>();
        outputs.Add(expectedOutput);
        MLPNet.LearnPattern(inputList, outputs);
    }
}
