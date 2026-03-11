using UnityEngine;

public class PlayerController : MonoBehaviour
{


    public float walkSpeed = 5.0f;
    public float mouseSensitivity = 2.0f;
    public CharacterController controller;
    public GameObject Camera;
    private float mouseY;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }
        if (Camera == null)
        {
            Camera = GameObject.Find("Main Camera");
        }
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        MouseLook();
    }


    void Movement()
    {
        float verInput = Input.GetAxis("Vertical");
        float horInput = Input.GetAxis("Horizontal");
        float verSpeed = verInput * walkSpeed;
        float horSpeed = horInput * walkSpeed;

        Vector3 horizontalMovement = new Vector3(horSpeed, 0, verSpeed);
        horizontalMovement = transform.TransformDirection(horizontalMovement);

        controller.Move(horizontalMovement * Time.deltaTime);
    }


    void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseXSpeed = mouseX * mouseSensitivity;

        transform.Rotate(0, mouseXSpeed, 0);

        mouseY -= Input.GetAxis("Mouse Y");
        float mouseYSpeed = mouseY * mouseSensitivity;

        Camera.transform.localRotation = Quaternion.Euler(-mouseYSpeed, 0, 0);


    }
}
