using UnityEngine;
using System;
using System.Collections;

public class Ball : MonoBehaviour {

    public class BallData
    {
        public float posY = 0f;
        public float angle = 0.5f;
    }
    private BallData ballData = new BallData();
    public BallData GetBallData()
    {
        return ballData;
    }

    private void SetBallData(Vector2 dir, float posY)
    {
        if (dir.magnitude == 0)
            dir = Vector2.right;
        SetBallData(GetAngleInt(dir), posY);
    }

    public void SetBallData(float angle, float posY)
    {
        ballData.angle = angle;
        ballData.posY = posY;
    }

    public void SaveBallData()
    {
        SetBallData(rigidBody.velocity.normalized, GetBallPos0To1Rounded(transform.position.y));
    }

    public float InitialSpeed = 10f;
    public float MaxSpeed = 30f;
    public float HitAcceleration = 1f;
    float currentSpeed;
    Rigidbody2D rigidBody;
    public Rigidbody2D Rigidbody { get { return GetComponent<Rigidbody2D>(); } }

	// Use this for initialization
	void Awake () {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    static public float GetAngleInt(Vector2 dir)
    {
        int angle = Mathf.RoundToInt(Mathf.Acos(dir.x) * Mathf.Sign(dir.y) * Mathf.Rad2Deg);
        return GetAngle0To1(angle);
    }

    static public float GetAngle0To1(int angle)
    {
        float angle0To1 = (angle + 90) / 180;
        return GetRoundedValue(angle0To1);
    }

    static public int GetAngleDegree(float angle)
    {
        int angleDegree = Mathf.RoundToInt((angle * 180f) - 90f);
        return angleDegree;
    }

    static public float GetRoundedValue(float val, int nbDecimal = 1)
    {
        return (float)Math.Round(Convert.ToDecimal(val), nbDecimal);
    }

    static public float GetBallPos0To1(float posY)
    {
        int courtHeight = GameMgr.Instance.CourtHeight;
        float output = posY / courtHeight + 0.5f;
        return output = Mathf.Max(Mathf.Min(output, 1f), 0f);
    }

    static public float GetBallPos0To1Rounded(float posY)
    {
        return GetRoundedValue(GetBallPos0To1(posY), 2);
    }

    static public float ComputeBallPosCourt(float pos0To1)
    {
        return (pos0To1 - 0.5f) * GameMgr.Instance.CourtHeight;
    }

    public void Launch(bool useRandomDir = false, bool repeatLastLaunch = false)
    {
        float angle = 0.5f;
        if (repeatLastLaunch)
        {
            Vector3 pos = transform.position;
            pos.y = ComputeBallPosCourt(ballData.posY);
            transform.position = pos;
            angle = ballData.angle;
        }
        else if (useRandomDir)
        {
            angle = UnityEngine.Random.Range(0.2f, 0.8f);
        }

        int degAngle = GetAngleDegree(angle);
        Vector2 dir = new Vector2(Mathf.Cos(degAngle * Mathf.Deg2Rad), Mathf.Sin(degAngle * Mathf.Deg2Rad));
        rigidBody.velocity = dir * InitialSpeed;
        currentSpeed = InitialSpeed;

        // store ball trajectory data
        SetBallData(angle, GetBallPos0To1Rounded(transform.position.y));

        // $$$ Q&D
        GameMgr.Instance.AI.OnBallThrown();
    }

    void Update()
    {
        if (IsBallStuck())
            ForceBallBounceBack();
        else if (IsBallOut())
            transform.position = Vector2.zero;
    }

    float ComputeHitFactor(Vector2 racketPos, float racketHeight)
    {
        return (transform.position.y - racketPos.y) / racketHeight;
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (col.gameObject.tag != "Player")
            return;

        if (IsBallStuck())
            ForceBallBounceBack();
    }

    private bool IsBallStuck()
    {
        return GameMgr.Instance.IsBallLaunched() && rigidBody.velocity.magnitude <= 0.001f;
    }

    private bool IsBallOut()
    {
        return Mathf.Abs(transform.position.x) > 10f || Mathf.Abs(transform.position.y) > 10f;
    }

    private void ForceBallBounceBack()
    {
        rigidBody.velocity = Vector2.left * currentSpeed;
        Vector3 newPos = transform.position;
        newPos.x -= transform.localScale.x;
        transform.position = newPos;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag != "Player")
            return;

        // deal collision with upper or lower part of bracket
        Vector3 colNormal = col.contacts[0].normal;
        if (colNormal.x == 0f)
        {
            float x = ComputeHitFactor(col.transform.position, col.collider.bounds.size.x);
            float sign = colNormal.y;
            Vector2 dir = new Vector2(x, sign).normalized;
            if (dir.magnitude > 0f)
            {
                currentSpeed = Mathf.Min(MaxSpeed, currentSpeed + HitAcceleration);
                rigidBody.velocity = dir * currentSpeed;
            }
            else
            {
                Debug.LogWarning("magnitude <= 0 " + dir.magnitude);
            }
        }
        // collision with front part of the bracket
        else
        {
            float y = ComputeHitFactor(col.transform.position, col.collider.bounds.size.y);
            float sign = colNormal.x;
            Vector2 dir = new Vector2(sign, y).normalized;
            if (dir.magnitude > 0f)
            {
                currentSpeed = Mathf.Min(MaxSpeed, currentSpeed + HitAcceleration);
                rigidBody.velocity = dir * currentSpeed;
            }
            else
            {
                Debug.LogWarning("magnitude <= 0 " + dir.magnitude);
            }
        }

        // learn from ball collision
        AIController ai = col.gameObject.GetComponent<AIController>();
        if (ai != null)
            ai.OnBallCollideAIPaddle(transform.position);
        else
        {
            Vector2 dir = rigidBody.velocity.normalized;
            SetBallData(dir, GetBallPos0To1(transform.position.y));
            // $$$ Q&D
            GameMgr.Instance.AI.OnBallThrown();
        }
    }

}
