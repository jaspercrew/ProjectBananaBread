using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class CapeController : MonoBehaviour
{
    public const float lerpSpeed = 20f;
    public Vector2 partOffset = Vector2.zero;

    private Transform[] capeParts;

    private Transform capeAnchor;

    private void Awake()
    {
        capeAnchor = GetComponent<Transform>();
        capeParts = GetComponentsInChildren<Transform>();
    }

    private void Update()
    {
        Transform pieceToFollow = capeAnchor;

        foreach (Transform capePart in capeParts)
        {
            if (!capePart.Equals(capeAnchor))
            {
                Vector2 targetPosition = (Vector2) pieceToFollow.position + partOffset;
                Vector2 newPositionLerped = Vector2.Lerp(capePart.position, 
                    targetPosition, Time.deltaTime * lerpSpeed);

                capePart.position = newPositionLerped;
                pieceToFollow = capePart;
            }
        }


    }
    // Start is called before the first frame update
    void Start()
    {
        
    }


}
