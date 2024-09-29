using UnityEngine;

public class Gate : MonoBehaviour
{
    public GameObject otherGate; // Reference to the other gate to teleport to

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered the trigger is the player
        if (other.CompareTag("Player"))
        {
            // Teleport the player to the other gate's position
            other.transform.position = otherGate.transform.position;

            // Optional: You can also reset the player's rotation if needed
            // other.transform.rotation = otherGate.transform.rotation;
        }
    }
}
