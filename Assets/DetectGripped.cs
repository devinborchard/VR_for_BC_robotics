using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectGripped : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject grippedObject;
    public bool gripped;
    void Start()
    {
        gripped = false;
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("grabbable")){
            grippedObject = collision.gameObject;
            gripped = true;
        }
    }

    void OnTriggerExit()
    {
        gripped = false;
        grippedObject = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
