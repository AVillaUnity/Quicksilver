using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuicksilverCharacterController : MonoBehaviour
{
 
 /// <summary>
 /// C# translation from http://answers.unity3d.com/questions/155907/basic-movement-walking-on-walls.html
 /// Author: UA @aldonaletto 
 /// </summary>
 
 // Prequisites: create an empty GameObject, attach to it a Rigidbody w/ UseGravity unchecked
 // To empty GO also add BoxCollider and this script. Makes this the parent of the Player
 // Size BoxCollider to fit around Player model.

    public float moveSpeed = 6; // move speed
    public float turnSpeed = 90; // turning speed (degrees/second)
    public float jumpSpeed = 10; // vertical jump initial speed
    public float jumpRange = 10; // range to detect target wall
    public float gravity = 10; // gravity acceleration
    public LayerMask layerMask;


    private float lerpSpeed = 10; // smoothing speed
    private bool isGrounded;
    private float deltaGround = 0.2f; // character is grounded up to this distance
    private Vector3 surfaceNormal; // current surface normal
    private Vector3 myNormal; // character normal
    private float distGround; // distance from character position to ground
    private bool jumping = false; // flag &quot;I'm jumping to wall&quot;
    private float vertSpeed = 0; // vertical jump current speed

    private Transform myTransform;
    private BoxCollider boxCollider; // drag BoxCollider ref in editor
    private Rigidbody rb;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        myNormal = transform.up; // normal starts as character up direction
        myTransform = transform;
        rb.freezeRotation = true; // disable physics rotation
        
        // distance from transform.position to ground
        distGround = boxCollider.bounds.extents.y - boxCollider.center.y;
        print(distGround);

    }

    private void FixedUpdate()
    {
        // apply constant weight force according to character normal:
        rb.AddForce(-gravity * rb.mass * myNormal);
    }

    private void Update()
    {
        // jump code - jump to wall or simple jump
        if (jumping) return; // abort Update while jumping to a wall

        Ray ray;
        RaycastHit hit;

        if (Input.GetButtonDown("Jump"))
        { // jump pressed:
            ray = new Ray(myTransform.position, myTransform.forward);
            if (Physics.Raycast(ray, out hit, jumpRange, layerMask))
            { // wall ahead?
                JumpToWall(hit.point, hit.normal); // yes: jump to the wall
            }
            else if (isGrounded)
            { // no: if grounded, jump up
                rb.velocity += jumpSpeed * myNormal;
            }
        }

        // movement code - turn left/right with Horizontal axis:
        myTransform.Rotate(0, Input.GetAxisRaw("Horizontal") * turnSpeed * Time.unscaledDeltaTime, 0);
        // update surface normal and isGrounded:
        ray = new Ray(myTransform.position, -myNormal); // cast ray downwards
        if (Physics.Raycast(ray, out hit))
        { // use it to update myNormal and isGrounded
            isGrounded = hit.distance <= distGround + deltaGround;
            surfaceNormal = hit.normal;
        }
        else
        {
            isGrounded = false;
            // assume usual ground normal to avoid "falling forever"
            surfaceNormal = Vector3.up;
        }
        myNormal = Vector3.Lerp(myNormal, surfaceNormal, lerpSpeed * Time.unscaledDeltaTime);
        // find forward direction with new myNormal:
        Vector3 myForward = Vector3.Cross(myTransform.right, myNormal);
        // align character to the new myNormal while keeping the forward direction:
        Quaternion targetRot = Quaternion.LookRotation(myForward, myNormal);
        myTransform.rotation = Quaternion.Lerp(myTransform.rotation, targetRot, lerpSpeed * Time.unscaledDeltaTime);
        // move the character forth/back with Vertical axis:
        myTransform.Translate(0, 0, Input.GetAxisRaw("Vertical") * moveSpeed * Time.unscaledDeltaTime);
    }

    private void JumpToWall(Vector3 point, Vector3 normal)
    {
        // jump to wall
        jumping = true; // signal it's jumping to wall
        rb.isKinematic = true; // disable physics while jumping
        Vector3 orgPos = myTransform.position;
        Quaternion orgRot = myTransform.rotation;
        Vector3 dstPos = point + normal * (distGround + 0.5f); // will jump to 0.5 above wall
        Vector3 myForward = Vector3.Cross(myTransform.right, normal);
        Quaternion dstRot = Quaternion.LookRotation(myForward, normal);

        StartCoroutine(jumpTime(orgPos, orgRot, dstPos, dstRot, normal));
        //jumptime
    }

    private IEnumerator jumpTime(Vector3 orgPos, Quaternion orgRot, Vector3 dstPos, Quaternion dstRot, Vector3 normal)
    {
        float t = 0f;
        while (t < 1f)
        { 
            myTransform.position = Vector3.Lerp(orgPos, dstPos, t);
            myTransform.rotation = Quaternion.Slerp(orgRot, dstRot, t);
            t += Time.unscaledDeltaTime;
            yield return null; // return here next frame
        }
        myNormal = normal; // update myNormal
        rb.isKinematic = false; // enable physics
        jumping = false; // jumping to wall finished
    }
}
