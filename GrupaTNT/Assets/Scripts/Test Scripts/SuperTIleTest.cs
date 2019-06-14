using SuperTiled2Unity;
using UnityEngine;

public class SuperTIleTest : MonoBehaviour
{
    [SerializeField]
    public GameObject SuperTestRoom;
    
    // Start is called before the first frame update
    void Start()
    {
        SuperMap superMap = SuperTestRoom.GetComponent<SuperMap>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
