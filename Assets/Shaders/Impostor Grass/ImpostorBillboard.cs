using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpostorBillboard : MonoBehaviour
{
    public new Renderer renderer;

    private void Update()
    {
        // Billboards that look at camera, with X-rotation.
        Quaternion newRotation = Quaternion.LookRotation(-Camera.main.transform.forward, Vector3.up);
        transform.rotation = newRotation;

        // Send view vector to shader for calculations there.
        Vector3 viewVector = (transform.position - Camera.main.transform.position).normalized;
        renderer.material.SetVector("_ViewVector", viewVector);
    }
}
