using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalDoor : Interactor
{
    private const float rotateSpeed = 150f;
    //public Object scene;
    public int sceneIndex;
    public override void Interact()
    {
        SceneManager.LoadSceneAsync(sceneIndex);
        interactors.Remove(this);
    }

    protected override void Update()
    {
        base.Update();
        if (isInteractable)
        {
            highlightFX.gameObject.transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
        }
    }

}
