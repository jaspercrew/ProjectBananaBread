// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.WSA;
//
// public enum WindDirection{
//     Left, Right, Up, Down
// }
// public class WindFX : MonoBehaviour
// {
//     public WindDirection direction;
//     private ParticleSystem particleSystem;
//
//     private bool isDeactivating;
//     private bool isActivating;
//
//     private void Start()
//     {
//         particleSystem = GetComponent<ParticleSystem>();
//     }
//
//
//     void Update()
//     {
//         if (direction == WindDirection.Left || direction == WindDirection.Right)
//         {
//             transform.position = new Vector3(transform.position.x, CharController.Instance.transform.position.y, transform.position.z);
//         }
//         else if (direction == WindDirection.Up || direction == WindDirection.Down)
//         {
//             transform.position = new Vector3(CharController.Instance.transform.position.x, transform.position.y, transform.position.z);
//         }
//
//         if (isActivating)
//         {
//             Gradient g;
//             GradientColorKey[] gck;
//             GradientAlphaKey[] gak;
//             g = new Gradient();
//             gck = new GradientColorKey[2];
//             gck[0].color = Color.white;
//             gck[0].time = 1.0F;
//             gck[1].color = Color.white;
//             gck[1].time = 1.0F;
//             gak = new GradientAlphaKey[2];
//             gak[0].alpha = 1.0F;
//             gak[0].time = 0.0F;
//             gak[1].alpha = 1.0F;
//             gak[1].time = 1.0F;
//             g.SetKeys(gck, gak);
//             var col = particleSystem.colorOverLifetime;
//             col.color = g;
//         }
//
//         if (isDeactivating)
//         {
//             
//         }
//         
//     }
//
//     public void ActivateWind()
//     {
//         Gradient c = new Gradient();
//         c.SetKeys();
//         Color color = particleSystem.colorOverLifetime.color;
//     }
//
//     public void DeactivateWind()
//     {
//         
//     }
//     
// }
