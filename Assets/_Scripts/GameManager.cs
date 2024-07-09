using UnityEngine;

namespace _Scripts
{
    public class GameManager : MonoBehaviour
    {
        public GameObject carPrefab;

        public void Reset()
        {
            Instantiate(carPrefab, new Vector3(-3.75f, 0, 3.5f), Quaternion.identity);
            Instantiate(carPrefab, new Vector3(-2.25f, 0, 3.5f), Quaternion.identity);
        }
    }
}