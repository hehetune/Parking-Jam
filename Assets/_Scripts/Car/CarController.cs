using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts;
using UnityEngine;

public enum Direction
{
    None = 0,
    Up = 1,
    Right = 2,
    Down = 3,
    Left = 4,
}

public class CarController : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Interact")]
    private Vector2 _firstTouchPosition;
    private Vector2 _finalTouchPosition;
    
    [Header("Swipe")]
    public float swipeAngle;
    public float swipeResist = .5f;

    [SerializeField] private int carDirection = 0;
    [SerializeField] private float rotateSpeed = 10f;
    [SerializeField] private float moveSpeed = 10f;

    [SerializeField] private bool isMovingForward = true;
    [SerializeField] private bool canMove = false;
    [SerializeField] private Vector3 moveDirection = Vector3.zero;
    public float targetX;
    public float targetZ;
    public bool isFollowXAxis;
    public bool aimToTarget = false;

    private Waypoint curWaypoint;
    public bool readyToGo = false;

    private bool cacheTurnDirection;

    private Coroutine waitToMoveCoroutine;
    
    private float TurnDistance = 0.1f;

    private static readonly Dictionary<(int, Direction), bool> MovementLogic = new()
    {
        { (0, Direction.Down), false },
        { (0, Direction.Up), true },
        { (1, Direction.Left), false },
        { (1, Direction.Right), true },
        { (2, Direction.Up), false },
        { (2, Direction.Down), true },
        { (3, Direction.Right), false },
        { (3, Direction.Left), true }
    };

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        GetCarDirection();
        isFollowXAxis = carDirection == 1 || carDirection == 3;
    }

    private void GetCarDirection()
    {
        int yRot = Mathf.RoundToInt(transform.eulerAngles.y) % 360;
        switch (yRot)
        {
            case 0:
                carDirection = 0;
                break;
            case 90:
                carDirection = 1;
                break;
            case 180:
                carDirection = 2;
                break;
            case -90:
            case 270:
                carDirection = 3;
                break;
        }
    }

    private void Update()
    {
        if (!canMove)
        {
            if (rb.velocity != Vector3.zero) rb.velocity = Vector3.zero;
            return;
        }

        if (aimToTarget)
        {
            float distance = isFollowXAxis ? (transform.position.x - targetX) : (transform.position.z - targetZ);
            if (Mathf.Abs(distance) <= TurnDistance)
            {
                curWaypoint.SetCarMovementAxis(this);
                // moveDirection.x = isFollowXAxis ? targetX : 0f;
                // moveDirection.z = isFollowXAxis ? 0f : targetZ;

                StartCoroutine(PerformTurn(isMovingForward ? cacheTurnDirection : !cacheTurnDirection));
            }
        }

        Vector3 dir = moveDirection * moveSpeed;
        rb.velocity = new Vector3(dir.x, rb.velocity.y, dir.z);
    }

    public void FollowXAxis(bool follow, float multiply, float targetX = 0, float targetZ = 0)
    {
        Debug.Log("FollowXAxis " + follow + " " + multiply);
        isFollowXAxis = follow;
        moveDirection.x = follow ? multiply : 0;
        moveDirection.z = follow ? 0 : multiply;

        this.targetX = targetX;
        this.targetZ = targetZ;
    }

    public void Turn(bool turnRight)
    {
        cacheTurnDirection = turnRight;
    }

    public void OnMouseDown()
    {
        _firstTouchPosition = Input.mousePosition;
    }

    public void OnMouseUp()
    {
        _finalTouchPosition = Input.mousePosition;
        CalculateAngle();
    }

    private void CalculateAngle()
    {
        if (Mathf.Abs(_finalTouchPosition.y - _firstTouchPosition.y) > swipeResist ||
            Mathf.Abs(_finalTouchPosition.x - _firstTouchPosition.x) > swipeResist)
        {
            swipeAngle = Mathf.Atan2(_finalTouchPosition.y - _firstTouchPosition.y,
                _finalTouchPosition.x - _firstTouchPosition.x) * 180 / Mathf.PI;

            HandleSwipe();
        }
    }

    private void HandleSwipe()
    {
        Direction direction = Direction.None;

        if (swipeAngle > -45 && swipeAngle <= 45)
        {
            direction = Direction.Right;
        }
        else if (swipeAngle > 45 && swipeAngle <= 135)
        {
            direction = Direction.Up;
        }
        else if (swipeAngle > 135 || swipeAngle <= -135)
        {
            direction = Direction.Left;
        }
        else if (swipeAngle > -135 && swipeAngle <= -45)
        {
            direction = Direction.Down;
        }

        if (MovementLogic.TryGetValue((carDirection, direction), out var moveForward))
        {
            isMovingForward = moveForward;
            moveDirection = isMovingForward ? transform.forward : -transform.forward;
            canMove = true;
        }
    }

    private IEnumerator PerformTurn(bool turnRight)
    {
        ChangeCarDirection(turnRight);
        float newEuler = carDirection * 90;
        Vector3 targetEulerAngles = new(0, newEuler, 0);

        while (Vector3.Distance(transform.eulerAngles, targetEulerAngles) > 0.2f)
        {
            yield return null;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(targetEulerAngles),
                rotateSpeed * moveSpeed * Time.deltaTime * 120f);
        }

        transform.eulerAngles = targetEulerAngles;
        isMovingForward = true;
    }

    private void ChangeCarDirection(bool turnRight)
    {
        carDirection = (carDirection + (turnRight ? 1 : -1) + 4) % 4;
    }

    public void WaitToMove(float secs)
    {
        if (waitToMoveCoroutine != null) StopCoroutine(waitToMoveCoroutine);
        waitToMoveCoroutine = StartCoroutine(WaitToMoveCoroutine(secs));
    }

    private IEnumerator WaitToMoveCoroutine(float secs)
    {
        canMove = false;
        yield return new WaitForSeconds(secs);
        if (curWaypoint.CheckCarCanMoveInto(this))
            canMove = true;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Obstacle") || other.gameObject.CompareTag("Car"))
        {
            canMove = false;
        }

        if (other.gameObject.CompareTag($"Exit"))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Waypoint"))
        {
            Debug.Log("Enter waypoint");
            curWaypoint = other.GetComponent<Waypoint>();
            curWaypoint.CarEnter(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Exit waypoint");
        if (other.gameObject.CompareTag("Waypoint"))
        {
            Waypoint wp = other.GetComponent<Waypoint>();
            wp.CarExit(this);
        }
    }
}
