using UnityEngine;
using System;
using System.Collections;

public class Ball : MonoBehaviour {

    public class BallData
    {
        public float posY;
        public int angle;
    }
    private BallData ballData = new BallData();
    public BallData GetBallData { get { return ballData; } }

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

    public void Launch(bool useRandomDir = false, bool repeatLastLaunch = false)
    {
        int angle = 0;
        if (repeatLastLaunch)
        {
            Vector3 pos = transform.position;
            pos.y = ballData.posY;
            transform.position = pos;
            angle = ballData.angle;
        }
        else if (useRandomDir)
        {
            angle = UnityEngine.Random.Range(0, 60);
            if (UnityEngine.Random.Range(0, 2) == 0)
                angle *= -1;
        }

        Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        rigidBody.velocity = dir * InitialSpeed;
        currentSpeed = InitialSpeed;

        // store ball trajectory data
        ballData.posY = (float)Math.Round(Convert.ToDecimal(transform.position.y), 1);
        ballData.angle = angle;
        //ballData.angle = Mathf.Acos(dir.x) * Mathf.Sign(dir.y) * Mathf.Rad2Deg;

        // $$$ Q&D
        GameMgr.Instance.AI.OnBallThrown();
    }

    void Update()
    {
        //if (rigidBody.velocity.magnitude == 0)
        //{
        //    ballData.posY = transform.position.y;
        //    ballData.direction = Vector2.right;
        //}
    }

    float ComputeHitFactor(Vector2 racketPos, float racketHeight)
    {
        return (transform.position.y - racketPos.y) / racketHeight;
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
                GetComponent<Rigidbody2D>().velocity = dir * currentSpeed;
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
                GetComponent<Rigidbody2D>().velocity = dir * currentSpeed;
            }
        }

        // learn from ball collision
        AIController ai = col.gameObject.GetComponent<AIController>();
        if (ai != null)
            ai.OnBallCollide(transform.position);
        else
        {
            ballData.posY = (float)Math.Round(Convert.ToDecimal(transform.position.y), 1);
            Vector2 dir = GetComponent<Rigidbody2D>().velocity.normalized;
            ballData.angle = Mathf.RoundToInt(Mathf.Acos(dir.x) * Mathf.Sign(dir.y) * Mathf.Rad2Deg);
            // $$$ Q&D
            GameMgr.Instance.AI.OnBallThrown();
        }
    }

}
