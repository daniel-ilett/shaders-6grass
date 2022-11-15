using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class ImpostorGenerator : MonoBehaviour
{
    [Header("Camera settings")]

    [Tooltip("Temporary camera to capture impostor. Remember to disable the camera's GameObject" +
        " to avoid rendering it to the screen.")]
    public Camera impostorCamera;
    [Tooltip("The distance of the camera from the object.")]
    public float distance = 1.0f;
    [Tooltip("The orthographic size of the camera viewport. Make it just large enough to capture" +
        " the target object.")]
    public float cameraSize = 1.0f;
    [Tooltip("The name of the layer used for the target object. This makes sure that other objects" +
        " are culled from the captures.")]
    public string captureLayerName;

    [Space(10)]

    [Header("Target object")]

    [Tooltip("The object that will be captured by the impostor generator." +
        " Currently, the generator does not support objects with children of children.")]
    public Transform targetObject;
   
    [Space(10)]

    [Header("Capture settings")]

    [Tooltip("Resolution of each frame of the impostor texture atlas.")]
    public int textureResolution = 256;
    [Tooltip("Number of captures along each axis. Total number of captures is this number squared.")]
    public int captureResolution = 6;
    [Tooltip("Default filename for new textures. Don't include the file extension.")]
    public string filename = "ImpostorAtlas";
    
    // This object isn't needed in Play Mode or at runtime.
    private void Awake()
    {
        Destroy(gameObject);
    }

    public void CaptureImpostor()
    {
        int capturelayer = LayerMask.NameToLayer(captureLayerName);

        // Set up camera.
        impostorCamera.clearFlags = CameraClearFlags.Color | CameraClearFlags.Depth;
        impostorCamera.orthographicSize = cameraSize;
        impostorCamera.orthographic = true;
        impostorCamera.cullingMask = 1 << capturelayer;

        var originalLayer = targetObject.gameObject.layer;

        // Prepare target object and any children (note: this doesn't recursively
        // change the layers of children of children).
        targetObject.gameObject.layer = capturelayer;
        foreach(Transform child in targetObject)
        {
            child.gameObject.layer = capturelayer;
        }

        // Set up texture array.
        RenderTexture[,] textures = new RenderTexture[captureResolution,captureResolution];
        var fullRect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);

        // Set up composite texture.
        int compositeResolution = captureResolution * textureResolution;
        RenderTexture compositeTexture = new RenderTexture(compositeResolution, compositeResolution, 0);

        // Generate textures.
        for (int x = 0; x < captureResolution; ++x)
        {
            for(int z = 0; z < captureResolution; ++z)
            {
                // Code for mapping 2D grid => 3D "hemi-octahedron" here: 
                // https://gamedev.stackexchange.com/questions/169508/octahedral-impostors-octahedral-mapping
                float divisor = captureResolution - 1.0f;
                Vector3 pos = new Vector3(x / divisor, 0.0f, z / divisor);
                pos = new Vector3(pos.x - pos.z, 0.0f, -1.0f + pos.x + pos.z);
                pos.y = 1.0f - Mathf.Abs(pos.x) - Mathf.Abs(pos.z);

                pos = pos.normalized * distance;

                // Move camera to positions and capture render texture.
                impostorCamera.transform.position = targetObject.position + pos;
                impostorCamera.transform.LookAt(targetObject, Vector3.up);

                // Render into texture array.
                var rt = new RenderTexture(textureResolution, textureResolution, 0);
                impostorCamera.rect = fullRect;
                impostorCamera.targetTexture = rt;
                impostorCamera.Render();

                // Render into texture atlas.
                var tileSize = 1.0f / captureResolution;
                impostorCamera.rect = new Rect(tileSize * x, tileSize * z, tileSize, tileSize);
                impostorCamera.targetTexture = compositeTexture;
                impostorCamera.Render();

                impostorCamera.targetTexture = null;

                textures[x,z] = rt;
            }
        }

        // Change the target layer back to normal.
        // Child objects are set to the same layer as the root, which is probably fine in most cases.
        targetObject.gameObject.layer = originalLayer;
        foreach (Transform child in targetObject)
        {
            child.gameObject.layer = originalLayer;
        }

        // Code to save new texture to file somewhere. 
        // See https://answers.unity.com/questions/37134/is-it-possible-to-save-rendertextures-into-png-fil.html
        RenderTexture.active = compositeTexture;
        Texture2D impostorAtlas = new Texture2D(compositeResolution, compositeResolution, TextureFormat.ARGB32, false);
        impostorAtlas.ReadPixels(new Rect(0, 0, compositeResolution, compositeResolution), 0, 0);
        RenderTexture.active = null;

        byte[] bytes = impostorAtlas.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/Impostors/" + filename + ".png", bytes);

        AssetDatabase.Refresh();
    }
}
