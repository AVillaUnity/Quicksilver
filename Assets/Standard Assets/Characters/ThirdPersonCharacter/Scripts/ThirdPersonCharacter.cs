using System.Collections;
using UnityEngine;

namespace UnityStandardAssets.Characters.ThirdPerson
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CapsuleCollider))]
	[RequireComponent(typeof(Animator))]
	public class ThirdPersonCharacter : MonoBehaviour
	{
		[SerializeField] float m_MovingTurnSpeed = 360;
		[SerializeField] float m_StationaryTurnSpeed = 180;
		[SerializeField] float m_InitialJumpPower = 12f;
		[Range(1f, 4f)][SerializeField] float m_InitialGravityMultiplier = 2f;
		[SerializeField] float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
		[SerializeField] float m_MoveSpeedMultiplier = 1f;
		[SerializeField] float m_AnimSpeedMultiplier = 1f;
		[SerializeField] float m_GroundCheckDistance = 0.1f;
        [SerializeField] float timeScale = 0.03f;
        [SerializeField] float animSlowDown = 0.5f;

        public float jumpRange = 15f;

        Rigidbody m_Rigidbody;
		Animator m_Animator;
		bool m_IsGrounded;
		float m_OrigGroundCheckDistance;
		const float k_Half = 0.5f;
		float m_TurnAmount;
		float m_ForwardAmount;
		Vector3 m_GroundNormal;
		float m_CapsuleHeight;
		Vector3 m_CapsuleCenter;
		CapsuleCollider m_Capsule;
		bool m_Crouching;
        float m_InitialGravity;
        Vector3 extraGravity;
        private Vector3 currentNormal;

        public LayerMask layerMask;


        void Start()
		{
			m_Animator = GetComponent<Animator>();
			m_Rigidbody = GetComponent<Rigidbody>();
			m_Capsule = GetComponent<CapsuleCollider>();
			m_CapsuleHeight = m_Capsule.height;
			m_CapsuleCenter = m_Capsule.center;
            m_InitialGravity = Physics.gravity.y;


            m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
			m_OrigGroundCheckDistance = m_GroundCheckDistance;

            m_Animator.updateMode = AnimatorUpdateMode.UnscaledTime;

            currentNormal = transform.up;
		}


		public void Move(Vector3 move, bool crouch, bool jump)
		{

			// convert the world relative moveInput vector into a local-relative
			// turn amount and forward amount required to head in the desired
			// direction.
            if(Time.timeScale <= timeScale)
            {
                m_AnimSpeedMultiplier = animSlowDown;
                m_MoveSpeedMultiplier = 1 / m_AnimSpeedMultiplier;
            }
            //print("move: " + move + "magnitude: " + move.magnitude);
			if (move.magnitude > 1f)
            {
                move.Normalize();
               // print("move after normalize: " + move);
            }

            move = transform.InverseTransformDirection(move);
            //print("move after inverseTransformDirection: " + move);
			CheckGroundStatus();
			//move = Vector3.ProjectOnPlane(move, currentNormal);
            //print("move after project on plane: " + move);
			m_TurnAmount = Mathf.Atan2(move.x, move.z);
            //print("turnAmount: " + m_TurnAmount);
			m_ForwardAmount = move.z;
            //print("forwardAmound: " + m_ForwardAmount);
            //print("----------------------------------------------------");

			ApplyExtraTurnRotation();

			// control and velocity handling is different when grounded and airborne:
			if (m_IsGrounded)
			{
				HandleGroundedMovement(crouch, jump);
			}
			else
			{
				HandleAirborneMovement();
			}

			ScaleCapsuleForCrouching(crouch);
			PreventStandingInLowHeadroom();


			// send input and other state parameters to the animator
			UpdateAnimator(move);
		}


		void ScaleCapsuleForCrouching(bool crouch)
		{
			if (m_IsGrounded && crouch)
			{
				if (m_Crouching) return;
				m_Capsule.height = m_Capsule.height / 2f;
				m_Capsule.center = m_Capsule.center / 2f;
				m_Crouching = true;
			}
			else
			{
				Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
				float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
				if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
				{
					m_Crouching = true;
					return;
				}
				m_Capsule.height = m_CapsuleHeight;
				m_Capsule.center = m_CapsuleCenter;
				m_Crouching = false;
			}
		}

		void PreventStandingInLowHeadroom()
		{
			// prevent standing up in crouch-only zones
			if (!m_Crouching)
			{
				Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
				float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
				if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
				{
					m_Crouching = true;
				}
			}
		}


		void UpdateAnimator(Vector3 move)
		{
			// update the animator parameters
			m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.unscaledDeltaTime);
			m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.unscaledDeltaTime);
			m_Animator.SetBool("Crouch", m_Crouching);
			m_Animator.SetBool("OnGround", m_IsGrounded);
			if (!m_IsGrounded)
			{
                Vector3 locVel = transform.InverseTransformDirection(m_Rigidbody.velocity);
                locVel.Normalize();
                //float jumpAnim = Fit(locVel.y / 100f, m_InitialJumpPower / Time.timeScale, -9f, 5f, m_Rigidbody.velocity.y);
				m_Animator.SetFloat("Jump", locVel.y);
                //print(m_Rigidbody.velocity.y + " to " + jumpAnim);
			}

			// calculate which leg is behind, so as to leave that leg trailing in the jump animation
			// (This code is reliant on the specific run cycle offset in our animations,
			// and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
			float runCycle =
				Mathf.Repeat(
					m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
			float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_ForwardAmount;
			if (m_IsGrounded)
			{
				m_Animator.SetFloat("JumpLeg", jumpLeg);
            }

            // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
            // which affects the movement speed because of the root motion.
            if (m_IsGrounded && move.magnitude > 0)
			{
				m_Animator.speed = m_AnimSpeedMultiplier;
			}
			else
			{
				// don't use that while airborne
				m_Animator.speed = 1;
			}
		}


		void HandleAirborneMovement()
		{
            // apply extra gravity from multiplier:
            float gravityMultiplier = m_InitialGravityMultiplier / Time.timeScale;
            Vector3 myGravity = (m_InitialGravity * currentNormal) / Time.timeScale;
            extraGravity = (myGravity * gravityMultiplier) - myGravity;
			m_Rigidbody.AddForce(extraGravity / (1 / Mathf.Pow(Time.timeScale, .2f)));

            Vector3 locVel = transform.InverseTransformDirection(m_Rigidbody.velocity);

            m_GroundCheckDistance = locVel.y < 0 ? m_OrigGroundCheckDistance : 0.01f;
            //print("rb: " + m_Rigidbody.velocity + " | gc: " + m_GroundCheckDistance + " | locVel: " + locVel);
		}


		void HandleGroundedMovement(bool crouch, bool jump)
		{
			// check whether conditions are right to allow a jump:
			if (jump && !crouch && m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
			{
                // jump!
                float jumpForce = m_InitialJumpPower / Time.timeScale;

                Vector3 locVel = transform.InverseTransformDirection(m_Rigidbody.velocity);
                locVel.y = jumpForce;
                m_Rigidbody.velocity = transform.TransformDirection(locVel);

                //m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, jumpForce, m_Rigidbody.velocity.z);
				m_IsGrounded = false;
				m_Animator.applyRootMotion = false;
				m_GroundCheckDistance = 0.1f;
                CheckForWall();
            }
		}

		void ApplyExtraTurnRotation()
		{
			// help the character turn faster (this is in addition to root rotation in the animation)
			float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
			transform.Rotate(0, m_TurnAmount * turnSpeed * Time.unscaledDeltaTime, 0);
		}


        public void OnAnimatorMove()
        {

            float moveSpeed = m_MoveSpeedMultiplier / Time.timeScale;
            // we implement this function to override the default root motion.
            // this allows us to modify the positional speed before it's applied.
            if (m_IsGrounded && Time.unscaledDeltaTime > 0)
            {
                Vector3 v = (m_Animator.deltaPosition * moveSpeed) / Time.unscaledDeltaTime;

                // we preserve the existing y part of the current velocity.
                //v.y = m_Rigidbody.velocity.y;
                m_Rigidbody.velocity = v;
            }
        }


        void CheckGroundStatus()
		{
			RaycastHit hitInfo;
#if UNITY_EDITOR
			// helper to visualise the ground check ray in the scene view
			Debug.DrawLine(transform.position + (transform.up * 0.1f), transform.position + (transform.up * 0.1f) + (-transform.up * m_GroundCheckDistance));
#endif
			// 0.1f is a small offset to start the ray from inside the character
			// it is also good to note that the transform position in the sample assets is at the base of the character
			if (Physics.Raycast(transform.position + (transform.up * 0.1f), -transform.up, out hitInfo, m_GroundCheckDistance))
			{
				m_GroundNormal = hitInfo.normal;
				m_IsGrounded = true;
				m_Animator.applyRootMotion = true;
			}
			else
			{
				m_IsGrounded = false;
				m_GroundNormal = transform.up;
				m_Animator.applyRootMotion = false;
			}
		}

        float Fit(float oldMin, float oldMax, float newMin, float newMax, float value)
        {

            float OldRange = (oldMax - oldMin);
            float NewRange = (newMax - newMin);
            float NewValue = (((value - oldMin) * NewRange) / OldRange) + newMin;

            return (NewValue);
        }

        void CheckForWall()
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hitInfo, jumpRange, layerMask))
            { // wall ahead?
                if(hitInfo.normal != currentNormal)
                    JumpToWall(hitInfo.normal); // yes: jump to the wall
            }
        }

        private void JumpToWall(Vector3 normal)
        {
            Quaternion orgRot = transform.rotation;
            Vector3 myForward = Vector3.Cross(transform.right, normal);
            Quaternion dstRot = Quaternion.LookRotation(myForward, normal);

            StartCoroutine(Jump(orgRot, dstRot, normal));
            //jumptime
        }

        private IEnumerator Jump(Quaternion orgRot, Quaternion dstRot, Vector3 normal)
        {
            float t = 0f;
            Vector3 originalNormal = currentNormal;
            while (t < 1f)
            {
                transform.rotation = Quaternion.Slerp(orgRot, dstRot, t);
                currentNormal = Vector3.Lerp(originalNormal, normal, t);
                t += Time.unscaledDeltaTime * 2;
                yield return null; // return here next frame
            }
            currentNormal = normal; // update myNormal
            m_Rigidbody.velocity = Vector3.zero;
            m_GroundNormal = normal;
            m_IsGrounded = true;
        }

    }
}
