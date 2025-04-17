using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    // Start is called before the first frame update
    public float gripSpeed = 0.075f;
    public float handSpeed = 0.25f;
    public float handLiftSpeed = 0.25f;

    public float rotateSpeed = 0.25f; // degrees per second
    public GameObject tableTop;
    private float tableTopY;
    public GameObject finger1;
    public GameObject finger2;
    public GameObject hand;
    public GameObject handRotateRef;
    public GameObject effLocation;
    public bool gripping;
    private bool DL;
    private bool DR;
    private bool DU;
    private bool DD;
    private float moveX;
    private float moveY;
    private float moveZ;

    private float RotateX;

    public bool gripped; //set elsewhere in DetectGripped

    public bool simulation = false;

    private BasicContoller controls;
    private Vector2 moveInput;
    private Vector2 lookInput;

    void Start()
    {
        gripping = false;
        DD = false;
        DU = false;
        DL = false;
        DR = false;
        moveX = 0;
        moveY = 0;
        moveZ = 0;
        RotateX = 0;
        gripped = false;

        tableTopY = tableTop.transform.position.y;
    }

    private void Awake()
    {

        controls = new BasicContoller();

        controls.Gameplay.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Gameplay.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Gameplay.Look.canceled += ctx => lookInput = Vector2.zero;
        
        controls.Gameplay.RT.performed += ctx => gripping = true;
        controls.Gameplay.RT.canceled += ctx => gripping = false;

        controls.Gameplay.DPadLeft.performed += ctx => DL = true;
        controls.Gameplay.DPadLeft.canceled += ctx => DL = false;
        
        controls.Gameplay.DPadRight.performed += ctx => DR = true;
        controls.Gameplay.DPadRight.canceled += ctx => DR = false;

        controls.Gameplay.DPadUp.performed += ctx => DU = true;
        controls.Gameplay.DPadUp.canceled += ctx => DU = false;

        controls.Gameplay.DPadDown.performed += ctx => DD = true;
        controls.Gameplay.DPadDown.canceled += ctx => DD = false;
    }

    void OnEnable() => controls.Gameplay.Enable();
    void OnDisable() => controls.Gameplay.Disable();


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

        moveX = moveInput.y * handSpeed;
        moveY = moveInput.x * handSpeed;
        moveZ = lookInput.y * handLiftSpeed;

        RotateX = lookInput.x * rotateSpeed;

        Vector3 last = hand.transform.position;

        Vector3 movement = new Vector3(moveX*-1, moveY*-1, moveZ) * Time.deltaTime;
        hand.transform.Translate(movement);
        
        if(handRotateRef.transform.position.y < tableTopY){
            hand.transform.position = last;
        }

    // Rotation (just visual/spin)
        float yaw = RotateX;
        
        float pitch = 0;
        if(DR){
            pitch = rotateSpeed;
        }
        if(DL){
            pitch = -rotateSpeed;
        }

        float roll = 0;
        if(DU){
            roll = rotateSpeed;
        }
        if(DD){
            roll = -rotateSpeed;
        }

        handRotateRef.transform.RotateAround(effLocation.transform.position, Vector3.up, yaw);
        handRotateRef.transform.RotateAround(effLocation.transform.position, Vector3.right, pitch);
        handRotateRef.transform.RotateAround(effLocation.transform.position, Vector3.forward, roll);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateFingers();
        UpdateHand();
    }
}
