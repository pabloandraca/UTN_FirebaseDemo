using System.Collections;
using UnityEngine;

public class Demo_LoadOnStart : MonoBehaviour
{
    IEnumerator Start()
    {
        while (!FirebaseManager.Instance || !FirebaseManager.Instance.FirebaseReady)
        {
            yield return null;
        }

        FirebaseManager.Instance.GetPlayerInfo("pepito", DebugPlayerInfo);
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