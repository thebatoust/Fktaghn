﻿using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

	public float maxJumpHeight = 4;
	public float minJumpHeight = 1;
	public float timeToJumpApex = .4f;
    public LayerMask enemyMask;
	float accelerationTimeAirborne = .2f;
	float accelerationTimeGrounded = .1f;
    float moveSpeed = 6;

    int timeJumped = 0;
    
    // Wall walking
    float accelerationTimeClimibingWall = .1f;
    float wallClimbSpeed = 5;

    /*
	public Vector2 wallJumpClimb;
	public Vector2 wallJumpOff;
	public Vector2 wallLeap;

    public float wallSlideSpeedMax = 3;
	public float wallStickTime = .25f;
	float timeToWallUnstick;
    */

    float gravity;
	float maxJumpVelocity;
	float minJumpVelocity;
	Vector3 velocity;
	float velocityXSmoothing;
    float velocityYSmoothing;

    Controller2D controller;

    GameObject avatar;

    public Animator anim;
    int jumpedHash = Animator.StringToHash("Jumped");
    int doubleJumpedHash = Animator.StringToHash("DoubleJumped");
    int isGroundedHash = Animator.StringToHash("isGrounded");
    int isWalkingHash = Animator.StringToHash("isWalking");

    void Start() {

		controller = GetComponent<Controller2D> ();

		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);

        avatar = transform.FindChild("Avatar").gameObject;
	}

	void Update() {
		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		int wallDirX = (controller.collisions.left) ? -1 : 1;

        if (controller.collisions.faceDir == -1) {
            avatar.transform.localEulerAngles = (new Vector3(0, 180));
        } else {
            avatar.transform.localEulerAngles = new Vector3(0, 0);
        }

		float targetVelocityX = input.x * moveSpeed;
        if(targetVelocityX != 0)
        {
            anim.SetBool(isWalkingHash, true);
        } else
        {
            anim.SetBool(isWalkingHash, false);
        }
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below)?accelerationTimeGrounded:accelerationTimeAirborne);

		/*bool wallSliding = false;
		if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0) {
			wallSliding = true;

			if (velocity.y < -wallSlideSpeedMax) {
				velocity.y = -wallSlideSpeedMax;
			}

			if (timeToWallUnstick > 0) {
				velocityXSmoothing = 0;
				velocity.x = 0;

				if (input.x != wallDirX && input.x != 0) {
					timeToWallUnstick -= Time.deltaTime;
				}
				else {
					timeToWallUnstick = wallStickTime;
				}
			}
			else {
				timeToWallUnstick = wallStickTime;
			}

		}*/
        if (Input.GetButtonDown("Fire1"))
        {
            Debug.DrawRay(gameObject.transform.position, gameObject.transform.position, Color.green);
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), transform.right, 20, enemyMask);
            if (hit != null && hit.collider != null)
            {
                if (hit.collider.gameObject.GetComponent<PriestBehaviour>().convertable == true)
                {
                    Destroy(hit.collider.gameObject);
                }
            }
        }


		if (Input.GetButtonDown ("Jump")) {
			/*if (wallSliding) {
				if (wallDirX == input.x) {
					velocity.x = -wallDirX * wallJumpClimb.x;
					velocity.y = wallJumpClimb.y;
				}
				else if (input.x == 0) {
					velocity.x = -wallDirX * wallJumpOff.x;
					velocity.y = wallJumpOff.y;
				}
				else {
					velocity.x = -wallDirX * wallLeap.x;
					velocity.y = wallLeap.y;
				}
			}*/

			if (controller.collisions.below || (timeJumped == 1)) {
				velocity.y = maxJumpVelocity;
                // +1 JUMP
                timeJumped++;

                anim.SetBool(isGroundedHash, false);
                if (timeJumped == 1)
                {
                    anim.SetTrigger(jumpedHash);
                } else
                {
                    anim.SetTrigger(doubleJumpedHash);
                }
            }
		}

		if (Input.GetButtonUp ("Jump")) {
			if (velocity.y > minJumpVelocity) {
				velocity.y = minJumpVelocity;
			}
		}

        if (controller.collisions.climbingWall)
        {
            float targetVelocityY = input.y * wallClimbSpeed;
            velocity.y = Mathf.SmoothDamp(velocity.y, targetVelocityY, ref velocityYSmoothing, accelerationTimeClimibingWall);
        }
        else if (controller.collisions.stuckToTheCeiling)
        {
            if(input.y < 0 ) {
                velocity.y = -maxJumpVelocity;
            }
        } else { 
            velocity.y += gravity * Time.deltaTime;
        }

		controller.Move (velocity * Time.deltaTime, input);

		if (controller.collisions.below || controller.collisions.above) {
			velocity.y = 0;
            // REMET LE COMPTEUR DE SAUT A 0
            timeJumped = 0;
            anim.SetBool(isGroundedHash, true);
		}

	}
}