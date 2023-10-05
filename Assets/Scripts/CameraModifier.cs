using Cinemachine;
using UnityEngine;

public class CameraModifier : MonoBehaviour
{
    public bool offsetEnabled;
    public bool sizeEnabled;

    public Vector3 offset;
    public float size;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<CharController>() == null)
        {
            return;
        }

        //print(CameraManager.Instance.transform.Find("CharacterCam").GetComponent<CinemachineVirtualCamera>());

        if (offsetEnabled)
        {
            CameraManager.Instance.transform
                .Find("CharacterCam")
                .GetComponent<CinemachineVirtualCamera>()
                .GetCinemachineComponent<CinemachineFramingTransposer>()
                .m_TrackedObjectOffset = offset;
        }

        if (sizeEnabled)
        {
            CameraManager.Instance.transform
                .Find("CharacterCam")
                .GetComponent<CinemachineVirtualCamera>()
                .m_Lens.OrthographicSize *= size;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<CharController>() == null)
        {
            return;
        }

        if (offsetEnabled)
        {
            CameraManager.Instance.transform
                .Find("CharacterCam")
                .GetComponent<CinemachineVirtualCamera>()
                .GetCinemachineComponent<CinemachineFramingTransposer>()
                .m_TrackedObjectOffset = Vector3.zero;
        }
        if (sizeEnabled)
        {
            CameraManager.Instance.transform
                .Find("CharacterCam")
                .GetComponent<CinemachineVirtualCamera>()
                .m_Lens.OrthographicSize /= size;
        }
    }
}
