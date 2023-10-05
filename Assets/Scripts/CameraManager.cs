using System.Collections;
using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private const int NumSlices = 8;
    private const float InitialDelay = .3f;
    private const float ArppegioTime = .55f;
    private const float DisappearDelay = .5f;
    public const float SliceFadeTime = .2f;
    public static CameraManager instance;
    public CinemachineVirtualCamera currentCam;
    public GameObject slicePrefab;
    public float totalDelayToSpawn;
    private TransitionSlice[] slices;
    private Camera unityCam;

    private void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
            instance = this;
    }

    private void Start()
    {
        unityCam = GetComponent<Camera>();

        if (slicePrefab is null) return;

        totalDelayToSpawn = InitialDelay + ArppegioTime + DisappearDelay / 2;
        slices = new TransitionSlice[NumSlices];
        var distanceFromCamera = unityCam.nearClipPlane; // Change this value if you want
        var midLeft = unityCam.ViewportToWorldPoint(new Vector3(0, .5f, distanceFromCamera));
        var midRight = unityCam.ViewportToWorldPoint(new Vector3(1, .5f, distanceFromCamera));
        var topMid = unityCam.ViewportToWorldPoint(new Vector3(.5f, 1f, distanceFromCamera));
        var botMid = unityCam.ViewportToWorldPoint(new Vector3(.5f, 0f, distanceFromCamera));

        var sliceGap = new Vector3((midRight.x - midLeft.x) / (NumSlices + 1), 0, 0);
        var camHeight = topMid.y - botMid.y;
        for (var i = 0; i < NumSlices; i++)
        {
            var spawnPoint = midLeft + sliceGap * (i + 1);
            var clone = Instantiate(slicePrefab, spawnPoint, Quaternion.identity, transform);
            clone.transform.localScale = new Vector3(sliceGap.x * 2, camHeight, 1f);
            slices[i] = clone.GetComponent<TransitionSlice>();
        }
    }

    private void Update()
    {
        if (GetComponent<CinemachineBrain>().ActiveVirtualCamera is null) return;
        currentCam =
            GetComponent<CinemachineBrain>().ActiveVirtualCamera.VirtualCameraGameObject
                .GetComponent<CinemachineVirtualCamera>();
    }

    public void DoTransition(bool invertDirection = false)
    {
        StartCoroutine(TransitionCoroutine(invertDirection));
    }

    private IEnumerator TransitionCoroutine(bool invertDirection)
    {
        CharController.instance.disabledMovement = true;
        yield return new WaitForSeconds(InitialDelay);
        var sliceInterval = ArppegioTime / NumSlices;

        for (var i = 0; i < NumSlices; i++)
        {
            yield return new WaitForSeconds(sliceInterval);
            slices[i].Appear();
        }

        yield return new WaitForSeconds(DisappearDelay);

        if (!invertDirection)
            for (var i = 0; i < NumSlices; i++)
            {
                yield return new WaitForSeconds(sliceInterval);
                slices[i].Disappear();
            }
        else
            for (var i = NumSlices - 1; i >= 0; i--)
            {
                yield return new WaitForSeconds(sliceInterval);
                slices[i].Disappear();
            }

        CharController.instance.disabledMovement = false;
    }

    // public void SwitchCam(CinemachineVirtualCamera cam) {
    //     currentCam.Priority = 5;
    //     cam.Priority = 15;
    //     currentCam = cam;
    // }
}