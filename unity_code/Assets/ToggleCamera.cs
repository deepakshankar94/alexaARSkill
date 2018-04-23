using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleCamera : MonoBehaviour {

    public GameObject ARCamera;
    public GameObject NonARCamera;
    public GameObject ImageTarget;
    public bool isAR = true;
    
    public void Toggle()
    {
        isAR = !isAR;
        ARCamera.SetActive(isAR);
        NonARCamera.SetActive(!isAR);

        var rendererComponents = ImageTarget.GetComponentsInChildren<Renderer>(true);
        var colliderComponents = ImageTarget.GetComponentsInChildren<Collider>(true);
        var canvasComponents = ImageTarget.GetComponentsInChildren<Canvas>(true);

        // Enable rendering:
        foreach (var component in rendererComponents)
            component.enabled = !isAR;

        // Enable colliders:
        foreach (var component in colliderComponents)
            component.enabled = !isAR;

        // Enable canvas':
        foreach (var component in canvasComponents)
            component.enabled = !isAR;

    }
}
