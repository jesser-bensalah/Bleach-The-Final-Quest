using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayer : MonoBehaviour
{
    private void Awake()
    {
       GameObject.FindGameObjectsWithTag("Player")[0].transform.position = transform.position;
    }
}
