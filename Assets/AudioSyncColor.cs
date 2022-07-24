// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
//
// //[RequireComponent(typeof(Image))]
// public class AudioSyncColor : AudioSyncer {
//     
//     
//     public Color[] beatColors;
//     public Color restColor;
//
//     private int m_randomIndx;
//     private SpriteRenderer spriteRenderer;
//
//     private IEnumerator MoveToColor(Color _target)
//     {
//         Color _curr = spriteRenderer.color;
//         Color _initial = _curr;
//         float _timer = 0;
// 		
//         while (_curr != _target)
//         {
//             _curr = Color.Lerp(_initial, _target, _timer / timeToBeat);
//             _timer += Time.deltaTime;
//
//             spriteRenderer.color = _curr;
//
//             yield return null;
//         }
//
//         m_isBeat = false;
//     }
//
//     private Color RandomColor()
//     {
//         if (beatColors == null || beatColors.Length == 0) return Color.white;
//         m_randomIndx = Random.Range(0, beatColors.Length);
//         return beatColors[m_randomIndx];
//     }
//
//     public override void OnUpdate()
//     {
//         base.OnUpdate();
//
//         if (m_isBeat) return;
//
//         spriteRenderer.color = Color.Lerp(spriteRenderer.color, restColor, restSmoothTime * Time.deltaTime);
//     }
//
//     public override void OnBeat()
//     {
//         base.OnBeat();
//
//         Color _c = RandomColor();
//
//         StopCoroutine("MoveToColor");
//         StartCoroutine("MoveToColor", _c);
//     }
//
//     private void Start()
//     {
//         spriteRenderer = GetComponent<SpriteRenderer>();
//     }
//
// }