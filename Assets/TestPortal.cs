using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TestPortal : MonoBehaviour
{
    [SerializeField]
    private SceneChangeSkeleton skeleton;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            skeleton.PlayCloseAnimation("8_SlothCutScene 1");
        }
    }
}
