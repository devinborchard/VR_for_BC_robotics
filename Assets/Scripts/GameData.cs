using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameData
{
    public static int randomSeed = Random.Range(1, 100000);
    public static void SetNewSeed(){
        randomSeed = Random.Range(1, 100000);
    }
}
