using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Interact")] private Vector2 _firstTouchPosition;
    private Vector2 _finalTouchPosition;
    [Header("Swipe")] public float swipeAngle;
    public float swipeResist = .5f;
    
    private int carDirection = 0;
    [SerializeField] private float rotateSpeed = 1f;
    [SerializeField] private float moveSpeed = 1f;

    private int travelingDirection = 1;

    void SomeFunction()
    {
        //Cars will turn right when out of the parking spot, however if car goes out reversed, then it will turn left to go straight
        StartCoroutine(PerformTurn(travelingDirection * -1));
    }
    
    public void OnMouseDown()
    {
        Debug.Log("on mouse down");
            _firstTouchPosition = Input.mousePosition;
    }

    public void OnMouseUp()
    {
        Debug.Log("on mouse up");

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
        Vector2Int direction = Vector2Int.zero;

        if (swipeAngle > -45 && swipeAngle <= 45)
        {
            direction = Vector2Int.right;
        }
        else if (swipeAngle > 45 && swipeAngle <= 135)
        {
            direction = Vector2Int.up;
        }
        else if (swipeAngle > 135 || swipeAngle <= -135)
        {
            direction = Vector2Int.left;
        }
        else if (swipeAngle > -135 && swipeAngle <= -45)
        {
            direction = Vector2Int.down;
        }

        // move car
    }

    IEnumerator PerformTurn(int turnState)
    {
        carDirection += turnState;
        float newEuler = carDirection * 90;

        while (true)
        {
            yield return null;

            Vector3 to = new(0, newEuler, 0);
            if (Vector3.Distance(transform.eulerAngles, to) > 0.2f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, newEuler, 0),
                    rotateSpeed * moveSpeed * Time.deltaTime * 120f);
            }
            else
            {
                transform.eulerAngles = to;
                break;
            }
        }

        if (turnState == -1)
        {
            travelingDirection *= -1;
        }
    }
}