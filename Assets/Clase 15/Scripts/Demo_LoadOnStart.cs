using System.Collections;
using UnityEngine;

namespace Clase15
{
    public class Demo_LoadOnStart : MonoBehaviour
    {
        [SerializeField] private string playerName = "pepito";

        IEnumerator Start()
        {
            while (!FirebaseManager.Instance || !FirebaseManager.Instance.FirebaseReady)
            {
                yield return null;
            }

            FirebaseManager.Instance.GetPlayerInfo(playerName, DebugPlayerInfo);
        }

        private void DebugPlayerInfo(PlayerInfo? info)
        {
            if (info.HasValue)
            {
                var debug = info.Value;
                Debug.Log($"Cargamos el player {debug.playerName} HP: {debug.playerHP} Mana: {debug.playerMana} Speed: {debug.playerSpeed}");
            }
            else
            {
                Debug.LogWarning("Jugador no encontrado");
            }
        }
    }
}