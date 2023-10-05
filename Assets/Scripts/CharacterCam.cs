using Cinemachine;
using UnityEngine;

public class CharacterCam : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        transform.GetComponent<CinemachineVirtualCamera>().Follow =
            FindObjectOfType<CharController>().transform;
        var bounds = SceneInformation.instance.transform.Find("CameraBounds");
        if (bounds! is null && transform.GetComponent<CinemachineConfiner>().enabled)
            transform.GetComponent<CinemachineConfiner>().m_BoundingShape2D =
                bounds.GetComponent<PolygonCollider2D>();
    }

    // Update is called once per frame
    // void Update()
    // {
    //
    // }
}