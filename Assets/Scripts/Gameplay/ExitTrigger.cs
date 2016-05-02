using UnityEngine;
using System.Collections;

public class ExitTrigger : MonoBehaviour {

    public bool IsLeftSide = true;

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("Ball"))
        {
            GameMgr.Instance.OnBallExit(IsLeftSide);
        }
    }
}
