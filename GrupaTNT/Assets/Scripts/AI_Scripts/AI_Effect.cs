using UnityEngine;
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class AI_Effect : MonoBehaviour
{
    public float yOffset = 0;
    public float xOffset = 0;
    public bool isActive = false;
    private GameObject parent = null;
    private SpriteRenderer rend;
    private Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        rend = gameObject.GetComponent<SpriteRenderer>();
        anim = gameObject.GetComponent<Animator>();
        if (isActive) {
            anim.SetBool("isActive", true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (parent != null) {
            transform.position = parent.transform.position + new Vector3(xOffset, yOffset,0);
        }
    }

    public void activate() {
        isActive = true;
        anim.SetBool("isActive", true);
    }
    public void deActivate() {
        isActive = false;
        anim.SetBool("isActive", false);
    }
    public void setParent(GameObject parent) {
        this.parent = parent;
    }

}
