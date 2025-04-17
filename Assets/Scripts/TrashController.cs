using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashController : MonoBehaviour
{
    public GameObject episodeController;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("grabbable")){
            episodeController.GetComponent<StateRecorder>().done = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
