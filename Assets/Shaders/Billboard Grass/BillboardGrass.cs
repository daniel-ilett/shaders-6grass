using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardGrass : MonoBehaviour
{
    private void Update()
    {
        // Billboards that look at camera, without rotating around X.
        /*
        Vector3 targetPosition = Camera.main.transform.position;
        targetPosition.y = transform.position.y;
        transform.LookAt(targetPosition, Vector3.up);
        */

        // Billboards that look at camera, with X-rotation.
        /*
        Quaternion newRotation = Quaternion.LookRotation(-Camera.main.transform.forward, Vector3.up);
        transform.rotation = newRotation;
        */

        // Billboards that align with camera plane, without rotating around X.
        Vector3 targetForward = Camera.main.transform.forward;
        targetForward.y = 0.01f;   // Not zero to avoid issues.
        transform.rotation = Quaternion.LookRotation(targetForward.normalized, Vector3.up);
    }
}
