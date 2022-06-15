using Cinemachine;
using UnityEngine;

public class CharacterCam : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        transform.GetComponent<CinemachineVirtualCamera>().Follow = FindObjectOfType<CharController>().transform;
        transform.GetComponent<CinemachineConfiner>().m_BoundingShape2D = SceneInformation.Instance.transform
            .Find("CameraBounds").GetComponent<PolygonCollider2D>();
    }

    // Update is called once per frame
    // void Update()
    // {
    //     
    // }
}
