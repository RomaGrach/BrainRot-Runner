using UnityEngine;

public class Obstacle : MonoBehaviour
{

    // Если у препятствия настроен Collider с isTrigger = true,
    // раскомментируйте этот метод и закомментируйте OnCollisionEnter.
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
    
}
