using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DeathZone : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out PlayerRespawn player))
            player.Respawn(5f);

        else if(other.TryGetComponent(out ObjectRespawn objectRespawn))
            objectRespawn.Respawn();

    }
}
