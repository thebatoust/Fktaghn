﻿using UnityEngine;
using DG.Tweening;
using System.Collections;

public class PriestBehaviour : MonoBehaviour
{

    enum PriestState
    {
        Patrol, Attack
    }

    PriestState state;
    public LayerMask playerMask;
    private Weapon[] weapons;
    public Vector3 movement;
    public float movementDuration;
    public float rotationDuration;
    private Vector3 initPosition;
    private float diffVector;
    public int distanceDetec;

    public Transform avatar;

    public bool convertable = false;

    private bool readyLaunch = false;

    bool facingRight = true;

    void Awake()
    {
        // Retrieve the weapon only once
        weapons = GetComponentsInChildren<Weapon>();
    }

    // Use this for initialization
    void Start()
    {
        initPosition = transform.position;
        Vector3 targetPosition = initPosition + movement;
        state = PriestState.Patrol;

        Sequence patrolSequence = DOTween.Sequence();
        patrolSequence
           .Append(transform.DOMove(movement + initPosition, movementDuration))
           .Append(transform.DORotate(new Vector3(0, 0, 0), rotationDuration))
           .AppendCallback(() => { facingRight = true; })
           .Append(transform.DOMove(initPosition, movementDuration))
           .Append(transform.DORotate(new Vector3(0, 180, 0), rotationDuration))
           .AppendCallback(() => { facingRight = false; });

        patrolSequence.SetLoops(-1, LoopType.Restart);

        patrolSequence.SetTarget(transform);
        transform.DOPlay();

        Debug.Log(transform.GetComponent<Collider2D>().bounds.max.y);
    }

    void SwitchFaceDir(int index)
    {
        transform.DORotate(new Vector3(0, 0, index * 180), 2.0f, RotateMode.Fast);
    }

    void FlipX(bool x)
    {
        x = !x;
    }

    void Update()
    {
        Vector3 raycastDirection = (facingRight) ? Vector3.right : -Vector3.right;
        float maxY = gameObject.GetComponent<Collider2D>().bounds.max.y - 1;
        float minY = gameObject.GetComponent<Collider2D>().bounds.min.y + 1;

      
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), raycastDirection, distanceDetec, playerMask);
            if (hit == null || hit.collider == null)
            {
                hit = Physics2D.Raycast(new Vector2(transform.position.x, maxY), raycastDirection, distanceDetec, playerMask);
                if (hit == null || hit.collider == null)
                {
                    hit = Physics2D.Raycast(new Vector2(transform.position.x, minY), raycastDirection, distanceDetec, playerMask);
                    if (hit == null || hit.collider == null)
                    {

                    }
                }
            }

            if (hit != null && hit.collider != null)
            {
                transform.DOPause();
                StartCoroutine(Attack());
            }
            else
            {
                transform.DOPlay();
            }
    }

    IEnumerator Attack()
    {
        if (readyLaunch == false)
        {
            readyLaunch = true;
            foreach (Weapon weapon in weapons)
            {
                // Auto-fire
                if (weapon != null && weapon.CanAttack)
                {
                    Vector3 raycastDirection = (facingRight) ? Vector3.right : -Vector3.right;
                    weapon.Attack(true, facingRight);
                    yield return new WaitForSeconds(1);
                    weapon.Attack(true, facingRight);
                    yield return new WaitForSeconds(1);
                    weapon.Attack(true, facingRight);
                    convertable = true;
                    yield return new WaitForSeconds(3);
                    convertable = false;
                }
            }
            readyLaunch = false;
        }
    }

}
