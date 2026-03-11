using UnityEngine;

public class PlayerController : MonoBehaviour
{


    public float walkSpeed = 5.0f;
    public CharacterController controller;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float verInput = Input.GetAxis("Vertical");
        float horInput = Input.GetAxis("Horizontal");
        float verSpeed = verInput * walkSpeed;
        float horSpeed = horInput * walkSpeed;

        Vector3 horizontalMovement = new Vector3(horSpeed, 0, verSpeed);

        controller.Move(horizontalMovement * Time.deltaTime);
    }
}
