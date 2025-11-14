using System.Collections;
using UnityEngine;

namespace Clase15
{
    public class Demo_SaveOnStart : MonoBehaviour
    {
        [SerializeField] private string playerName = "pepito";
        [SerializeField] private int playerHP = 10;
        [SerializeField] private int playerMana = 50;
        [SerializeField] private float playerSpeed = 3f;

        IEnumerator Start()
        {
            while (!FirebaseManager.Instance || !FirebaseManager.Instance.FirebaseReady)
            {
                yield return null;
            }

            var player = new PlayerInfo(playerName, playerHP, playerMana, playerSpeed);
            FirebaseManager.Instance.AddOrUpdatePlayerInfo(player);
        }
    }
}