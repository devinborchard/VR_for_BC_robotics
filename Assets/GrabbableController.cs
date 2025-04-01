using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbableController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetGrabbed(Transform location){
        transform.SetParent(location);
        GetComponent<Rigidbody>().isKinematic = true;
    }

    public void SetUnGrabbed(){
        transform.SetParent(null);
        GetComponent<Rigidbody>().isKinematic = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
