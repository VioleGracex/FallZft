using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCharacter : MonoBehaviour {

    public float speed = 30f;
    public GameObject rotatedobject;
    private bool isRotating;

    private void Update()
    {

        if (isRotating)
        {
           
            rotatedobject.transform.Rotate(0, -Input.GetAxis("Mouse X")* speed, 0);
        }
      
    }

    public void RotateObject()
    {

        isRotating = true;

    }

    public void StopRotating()
    {

        isRotating = false;

    }
	
}
