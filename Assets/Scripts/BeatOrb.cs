using System.Collections;
using UnityEngine;

public class BeatOrb : MonoBehaviour
{
    public Vector3 beatScale;
    public Vector3 restScale;
    public float timeToBeat;
    public float restSmoothTime;

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            restScale,
            restSmoothTime * Time.deltaTime
        );
    }

    private IEnumerator MoveToScale(Vector3 target)
    {
        var curr = transform.localScale;
        var initial = curr;
        float timer = 0;

        while (curr != target)
        {
            curr = Vector3.Lerp(initial, target, timer / timeToBeat);
            timer += Time.deltaTime;

            transform.localScale = curr;

            yield return null;
        }
    }

    public void Trigger()
    {
        StopCoroutine("MoveToScale");
        StartCoroutine("MoveToScale", beatScale);
    }
}