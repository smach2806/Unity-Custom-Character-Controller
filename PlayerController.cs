using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    [Header("Important")]
    public CapsuleCollider scol;

    [Header("Settings")]
    public float speed;
    public float gravity;
    public float smoothSpeed;
    public float playerHeight;
    public float jumpPower;

    [Header("Advanced Settings")]
    public Vector3 liftPoint;
    public LayerMask discludePlayer;
    public float CapsuleCastRadius;
    public float CapsuleCastMaxDistance;
    public Vector3 groundCheckPoint;
    public float groundConfirmRadius;

    Vector3 velocity;
    Vector3 move;
    Vector3 vel;

    float jumpHeight = 0;

    bool isGrounded;
    bool inputJump;

    RaycastHit groundHit;

    void Update()
    {
        Gravity();
        Move();
        jump();
        finalMove();
        groundCheck();
        collisionCheck();
    }

    void Move()
    {
        move = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")) * speed;
        velocity += move;
    }

    void finalMove()
    {
        Vector3 vel = new Vector3(velocity.x, velocity.y, velocity.z);
        vel = transform.TransformDirection(vel);
        transform.position += vel * Time.deltaTime;

        velocity = Vector3.zero;
    }

    void Gravity()
    {
        if (isGrounded == false)
        {
            velocity.y -= gravity;
        }
    }

    Ray debugRay1;
    RaycastHit gHit;
    void groundCheck()
    {
        Ray ray = new Ray(transform.TransformPoint(liftPoint), Vector3.down);
        RaycastHit tempHit = new RaycastHit();
        debugRay1 = new Ray(transform.TransformPoint(liftPoint), Vector3.down);

        Debug.DrawRay(ray.origin, ray.direction, Color.red, Time.deltaTime, false);

        if (Physics.CapsuleCast(ray.origin, CapsuleCastRadius, ray.direction, out tempHit, CapsuleCastMaxDistance, discludePlayer))
        {
            groundConfirm(tempHit);
            gHit = tempHit;
        } else
        {
            isGrounded = false;
        }
    }

    void groundConfirm(RaycastHit tempHit)
    {
        Collider[] col = new Collider[3];
        int num = Physics.OverlapCapsuleNonAlloc(transform.TransformPoint(groundCheckPoint), groundConfirmRadius, col, discludePlayer);

        isGrounded = false;

        for (int i = 0; i < num; i++)
        {
            if (col[i].transform == tempHit.transform)
            {
                groundHit = tempHit;
                isGrounded = true;

                if (!inputJump)
                {
                    transform.position = new Vector3(transform.position.x, (groundHit.point.y + playerHeight / 2), transform.position.z);
                }

                break;
            }
        }

        if (num <= 1 && tempHit.distance <= 3.1f && inputJump == false)
        {
            if (col[0] != null)
            {
                Ray ray = new Ray(transform.TransformPoint(liftPoint), Vector3.down);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 3.1f, discludePlayer))
                {
                    if (hit.transform != col[0].transform)
                    {
                        isGrounded = false;
                        return;
                    }
                }
            }
        }
    }

    void collisionCheck()
    { 
        Collider[] overlaps = new Collider[4];
        int num = Physics.OverlapCapsuleNonAlloc(transform.TransformPoint(scol.center), scol.radius, overlaps, discludePlayer, QueryTriggerInteraction.UseGlobal);

        for (int i = 0; i < num; i++)
        {
            Transform t = overlaps[i].transform;
            Vector3 dir;
            float dist;

            if (Physics.ComputePenetration(scol, transform.position, transform.rotation, overlaps[i], t.position, t.rotation, out dir, out dist))
            {
                Vector3 pVector = dir * dist;
                Vector3 pVelocity = Vector3.Project(velocity, -dir);
                transform.position += pVector;
                vel -= pVelocity;
                
            }
        }
    }

    void jump()
    {
        bool canJump;
        canJump = !Physics.Raycast(new Ray(transform.position, Vector3.up), playerHeight, discludePlayer);

        if(isGrounded && jumpHeight > 0.2f  || jumpHeight <= 0.2f && isGrounded)
        {
            jumpHeight = 0;
            inputJump = false;
        }

        if (isGrounded && canJump)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                inputJump = true;
                transform.position += Vector3.up * .6f * 2;
                jumpHeight += jumpPower;
            }
        } else if (!isGrounded)
        {
            jumpHeight -= (jumpHeight * .7f * Time.deltaTime);
        }

        velocity.y += jumpHeight;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(debugRay1.origin + debugRay1.direction * gHit.distance, CapsuleCastRadius);
    }
}
