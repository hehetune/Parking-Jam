using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts
{
    public class Waypoint : MonoBehaviour
    {
        public Transform startPoint;
        public Transform endPoint;

        [SerializeField] private bool turnRight;
        [SerializeField] private Waypoint prevWaypoint;

        private readonly List<CarController> currentCars = new();
        private const float SafeDistance = 4f;

        private bool roadFollowXAxis;

        private float TurnDistance = 0.1f;

        private void Awake()
        {
            roadFollowXAxis = Mathf.Abs(startPoint.position.z - endPoint.position.z) < Mathf.Epsilon;
        }

        public void CarEnter(CarController car)
        {
            if (car.readyToGo)
            {
                currentCars.Add(car);
                car.Turn(turnRight);
                SetCarMovementAxis(car);
                return;
            }

            CheckCarCanMoveInto(car);
        }

        public void CarExit(CarController car)
        {
            currentCars.Remove(car);
        }

        public void SetCarMovementAxis(CarController car)
        {
            if (roadFollowXAxis)
            {
                float diff = endPoint.position.z - car.transform.position.z;
                if (Mathf.Abs(diff) <= TurnDistance)
                {
                    car.FollowXAxis(true, endPoint.position.x - startPoint.position.x >= 0 ? 1 : -1, targetX: endPoint.position.x);
                }
                else car.FollowXAxis(false, diff >= 0 ? 1 : -1, targetZ: endPoint.position.z);
            }
            else
            {
                float diff = endPoint.position.x - car.transform.position.x;
                if (Mathf.Abs(diff) <= TurnDistance)
                {
                    car.FollowXAxis(false, endPoint.position.z - startPoint.position.z >= 0 ? 1 : -1, targetZ: endPoint.position.z);
                }
                else car.FollowXAxis(true, diff >= 0 ? 1 : -1, targetX: endPoint.position.x);
            }
        }

        public bool CheckCarCanMoveInto(CarController car)
        {
            if (!CanMove(car))
            {
                car.WaitToMove(1f);
                return false;
            }

            car.readyToGo = true;
            currentCars.Add(car);
            SetCarMovementAxis(car);
            car.aimToTarget = true;
            car.Turn(turnRight);

            return true;
        }

        private float GetCarsDistance(Vector3 car1, Vector3 car2)
        {
            if (!roadFollowXAxis)
                return Mathf.Abs(car1.z - car2.z);

            return Mathf.Abs(car1.x - car2.x);
        }

        private float GetCarDistanceFromStartPoint(CarController car)
        {
            if (!roadFollowXAxis)
                return Mathf.Abs(car.transform.position.z - startPoint.position.z);

            return Mathf.Abs(car.transform.position.x - startPoint.position.x);
        }

        public bool HaveCarNearby(CarController curCar, Vector3 carPos, float distance)
        {
            foreach (CarController car in currentCars)
            {
                if (car == curCar) continue;
                if (GetCarsDistance(carPos, car.transform.position) < distance)
                    return true;
            }

            return false;
        }

        private bool CanMove(CarController car)
        {
            bool result = !HaveCarNearby(car, car.transform.position, SafeDistance);
            if (prevWaypoint != null)
                result &= !prevWaypoint.HaveCarNearby(car, startPoint.position,
                    SafeDistance - GetCarDistanceFromStartPoint(car));
            return result;
        }
    }
}