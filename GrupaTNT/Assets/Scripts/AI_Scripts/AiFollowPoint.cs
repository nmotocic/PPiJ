using UnityEngine;

public class AiFollowPoint : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Diff between 2 world vectors.
    public static Vector2 simpleFollow(Vector3 myPos, Vector3 enemyPos, bool Normalise) {
        Vector2 res;
        res = Camera.main.ScreenToWorldPoint(Input.mousePosition) - myPos;
        if (Normalise) res.Normalize();
        return res;
    }

}
