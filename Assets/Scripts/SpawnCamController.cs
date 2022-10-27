// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// public class SpawnCamController : MonoBehaviour
// {
//     public static SpawnCamController Instance;
//     private Camera mainCamera;
//     private Camera spawnCamera;
//     public int numSlices = 8;
//     //private bool doBeats;
//
//     //private int transitionProgression;
//     // Start is called before the first frame update
//     private void Awake()
//     {
//         Instance = this;
//     }
//     private void Start()
//     {
//         mainCamera = CameraManager.Instance.transform.GetComponent<Camera>();
//         spawnCamera = transform.GetComponent<Camera>();
//         spawnCamera.rect = new Rect(-1, 0, 1, 1);
//         ResetTransition();
//         //StartCoroutine(TestTransition());
//
//     }
//
//     private void ResetTransition()
//     {
//         
//         //mainCamera.rect = new Rect(0, 0, 1, 1);
//         if (CharController.Instance.currentArea == null || CharController.Instance.currentArea.spawnLocation == null)
//         {
//             transform.position = SceneInformation.Instance.GetInitialSpawnPosition();
//         }
//         else
//         {
//             transform.position = CharController.Instance.currentArea.spawnLocation.position;
//         }
//
//         transform.position = new Vector3(transform.position.x, transform.position.y, -10);
//     }
//
//     public IEnumerator ProgressTransition()
//     {
//         spawnCamera.rect = new Rect(-1, 0, 1, 1);
//         //float timeToMove = 4;
//         //Vector2 lowerDestination = Vector2.zero;
//         float elapsedTime = 0f;
//         float moveTime = 1f;
//
//         while (elapsedTime < moveTime)
//         {
//             //transform.position = Vector3.Lerp(transform.position, destination, (elapsedTime / moveTime));
//             Vector2 toSet = Vector2.Lerp(Vector2.zero,  Vector2.right, elapsedTime / moveTime);
//             //mainCamera.rect = new Rect(toSet, Vector2.one);
//             spawnCamera.rect = new Rect(toSet - Vector2.right, Vector2.one);
//             elapsedTime += Time.deltaTime;
//             
//             // Yield here
//             yield return null;
//         }  
//         // Make sure we got there
//         //mainCamera.rect = new Rect(Vector2.right, Vector2.one);
//         spawnCamera.rect = new Rect(Vector2.zero, Vector2.one);
//         yield return null;
//         ResetTransition();
//         // transitionProgression++;
//         // float sliceWidth = (1f / (float) numSlices);
//         // mainCamera.rect = new Rect(mainCamera.rect.x + sliceWidth / 2, 0, mainCamera.rect.width - sliceWidth, 1f);
//         // spawnCamera.rect = new Rect(spawnCamera.rect.x - sliceWidth, 0, spawnCamera.rect.width + sliceWidth, 1f);
//         // if (transitionProgression == 8) //reset transition assets
//         // {
//         //     ResetTransition();
//         //     doBeats = false;
//         // }
//
//     }
//
//
//     public void DoTransition()
//     {
//         StartCoroutine(ProgressTransition());
//     }
//     //
//     // private IEnumerator TestTransition()
//     // {
//     //     
//     //     yield return new WaitForSeconds(5f);
//     //     doBeats = true;
//     //
//     // }
//
//     // Update is called once per frame
//     void Update()
//     {
//         
//     }
//     
// }
