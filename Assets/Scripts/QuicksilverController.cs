using UnityEngine;

public class QuicksilverController : MonoBehaviour
{

    public float speed = 100f;
    public float sensitivity = 1f;
    public float gravity = 9.81f;
    public float initialJumpForce = 10f;

    [Range(0f, 1f)]
    public float slowDownTime = 0f;
    public Transform head;

    private Rigidbody rb;
    private CharacterController characterController;
    private float jumpForce = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        characterController = GetComponent<CharacterController>();

        ToggleTime();
    }

    // Update is called once per frame
    void Update()
    {
        Movement();

        if (Input.GetButtonDown("Fire1"))
        {
            ToggleTime();
        }
    }

    private void Movement()
    {
        float forwardMovement = Input.GetAxisRaw("Vertical");
        float sidewaysMovement = Input.GetAxisRaw("Horizontal");

        float horizontalRotation = Input.GetAxisRaw("Mouse X");
        float verticalRotation = Input.GetAxisRaw("Mouse Y");

        Vector3 movement = ((transform.forward * forwardMovement) + (transform.right * sidewaysMovement)) * speed * Time.unscaledDeltaTime;
        if (characterController.isGrounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                jumpForce = initialJumpForce;
            }
        }
        else
        {
            jumpForce -= gravity * Time.unscaledDeltaTime;
        }
        movement.y -= (gravity * Time.unscaledDeltaTime) - (jumpForce * Time.unscaledDeltaTime);
        characterController.Move(movement);

        transform.Rotate(0f, sensitivity * horizontalRotation, 0f);
        head.Rotate(-verticalRotation * sensitivity, 0f, 0f);
    }

    void ToggleTime()
    {
        Time.timeScale = (Time.timeScale < 1) ? 1f : slowDownTime;
    }
}
