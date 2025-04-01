using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    // Start is called before the first frame update
    public float gripSpeed = 0.025f;
    public float handSpeed = 0.5f;
    public float handLiftSpeed = 0.25f;
    public GameObject finger1;
    public GameObject finger2;
    public GameObject hand;
    public bool gripping;
    private float moveX;
    private float moveY;
    private float moveZ;

    public bool gripped; //set elsewhere in DetectGripped

    public bool simulation = false;


    void Start()
    {
        gripping = false;
        moveX = 0;
        moveY = 0;
        moveZ = 0;
        gripped = false;
    }


    void DetectGripperInput(){
        gripping = Input.GetKey(KeyCode.Space);
    }

    void DetectMovementInput(){
        moveX = Input.GetAxis("Vertical"); // A (-1) / D (+1)
        moveY = Input.GetAxis("Horizontal");   // W (+1) / S (-1)

        if(Input.GetMouseButton(0)){
            moveZ = -1;
        }
        else if(Input.GetMouseButton(1)){
            moveZ = 1;
        }
        else{
            moveZ = 0;
        }

        moveX = moveX * handSpeed;
        moveY = moveY * handSpeed;
        moveZ = moveZ * handLiftSpeed;
    }

    void UpdateFingers(){
        bool closing = gripping;
        bool opening = !gripping;
        if((closing && finger1.transform.localPosition.y > 0 && !gripped) || (opening && finger1.transform.localPosition.y < 0.03f)){

            // Debug.Log($"Opening: {opening}, Closing: {closing}");
            int direction = 0;

            Vector3 gripperAxis = new Vector3(0, 1, 0);

            if(opening) direction = 1;
            if(closing) direction = -1;
            if(opening || closing){
                finger1.transform.localPosition += gripperAxis * gripSpeed * Time.deltaTime * direction;
                finger2.transform.localPosition += gripperAxis * gripSpeed * Time.deltaTime * direction * -1;
            }

        }

        
    }

    void UpdateHand(){
        Vector3 movement = new Vector3(moveX, moveY, moveZ) * Time.deltaTime;
        hand.transform.Translate(movement);
    }

    // Update is called once per frame
    void Update()
    {
        if(!simulation){
            DetectGripperInput();
            DetectMovementInput();
        }
       
        UpdateFingers();
        UpdateHand();
    }
}
