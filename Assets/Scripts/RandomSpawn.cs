using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawn : MonoBehaviour
{
    public float maxDistance = 0.1f;
    public int seedOffset;

    void Start()
    {
        Random.InitState(GameData.randomSeed + (seedOffset * 10)); // seed is any int
        Vector2 randomOffset = Random.insideUnitCircle * maxDistance;
        Vector3 newPosition = transform.position + new Vector3(randomOffset.x, 0f, randomOffset.y);
        transform.position = newPosition;

        // Random Y-axis rotation
        float randomYRotation = Random.Range(0f, 360f);
        transform.rotation = Quaternion.Euler(0f, randomYRotation, 0f);
    }
}