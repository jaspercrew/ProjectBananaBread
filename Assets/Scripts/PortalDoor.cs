// using UnityEngine;
// using UnityEngine.SceneManagement;
//
// public class PortalDoor : Interactor
// {
//     private const float RotateSpeed = 150f;
//     //public Object scene;
//     public int prevSceneIndex;
//     public override void Interact()
//     {
//         SceneManager.LoadSceneAsync(prevSceneIndex);
//         interactors.Remove(this);
//     }
//
//     protected override void Update()
//     {
//         base.Update();
//         if (IsInteractable)
//         {
//             HighlightFX.gameObject.transform.Rotate(Vector3.forward, RotateSpeed * Time.deltaTime);
//         }
//     }
//
// }
