using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpriteRenderer : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rBody;
    private EntityScript es;
    public bool dead { get; set; } = false;
    public bool attack { get; set; } = false;
    // Start is called before the first frame update
    void Start()
    {
        anim = gameObject.GetComponentInChildren<Animator>();
        rBody = gameObject.GetComponent<Rigidbody2D>();
        es = gameObject.GetComponent<EntityScript>();
    }

    // Update is called once per frame
    void Update()
    {
        var mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var my_pos = rBody.position;
        var flip = -Mathf.Sign(my_pos.x - mouse.x);
        var scale = gameObject.transform.localScale;
        if (flip != Mathf.Sign(scale.x))
        {
            gameObject.transform.localScale = new Vector3(-scale.x, scale.y, scale.z);
        }
        anim.SetBool("Attacked", attack);
        if (attack)
        {
            attack = false;
        }
        anim.SetFloat("Velocity", Mathf.Abs(rBody.velocity.magnitude));
        anim.SetBool("Dead", dead);
    }
}
