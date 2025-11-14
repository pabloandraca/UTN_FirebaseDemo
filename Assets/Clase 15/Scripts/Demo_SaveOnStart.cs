using System.Collections;
using UnityEngine;

public class Demo_SaveOnStart : MonoBehaviour
{
    IEnumerator Start()
    {
        while (!FirebaseManager.Instance || !FirebaseManager.Instance.FirebaseReady)
        {
            yield return null;
        }

        var player = new PlayerInfo("pepito", 10, 50, 3);
        FirebaseManager.Instance.AddOrUpdatePlayerInfo(player);
    }
}