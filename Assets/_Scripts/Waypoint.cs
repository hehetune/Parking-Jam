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

        public void CarEnter(CarController car)
        {
            if (car.readyToGo)
            {
                currentCars.Add(car);
                car.Turn(turnRight);
                car.targetPosition = GetCarTargetPosition(car);
                return;
            }

            CheckCarCanMoveInto(car);
        }

        public void CarExit(CarController car)
        {
            currentCars.Remove(car);
        }

        public Vector3 GetCarTargetPosition(CarController car)
        {
            if (Mathf.Abs(startPoint.position.x - endPoint.position.x) < Mathf.Epsilon)
            {
                if (Mathf.Abs(car.transform.position.x - endPoint.position.x) < 0.1f)
                    return endPoint.position;

                return new Vector3(startPoint.position.x, 0, car.transform.position.z);
            }

            if (Mathf.Abs(car.transform.position.z - endPoint.position.z) < 0.1f)
                return endPoint.position;

            return new Vector3(car.transform.position.x, 0, startPoint.position.z);
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
            car.targetPosition = GetCarTargetPosition(car);
            car.aimToTarget = true;
            car.Turn(turnRight);

            return true;
        }

        private float GetCarsDistance(Vector3 car1, Vector3 car2)
        {
            if (Mathf.Abs(startPoint.position.x - endPoint.position.x) < Mathf.Epsilon)
                return Mathf.Abs(car1.z - car2.z);

            return Mathf.Abs(car1.x - car2.x);
        }

        private float GetCarDistanceFromStartPoint(CarController car)
        {
            if (Mathf.Abs(startPoint.position.x - endPoint.position.x) < Mathf.Epsilon)
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
