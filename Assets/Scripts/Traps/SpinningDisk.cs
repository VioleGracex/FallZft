using UnityEngine;

public class SpinningDisk : MonoBehaviour
{
    public float rotationSpeed = 200f; // Speed of the disk's rotation
    public float moveSpeed = 5f; // Speed at which the player moves with the disk
   
 
    private void Update()
    {
        // Rotate the disk continuously
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

       
    }

}
