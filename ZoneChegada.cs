using UnityEngine;

// =============================================================
// ZoneChegada.cs
// Coloque este script no GameObject "Calcada_Chegada".
// Precisa de um Collider2D com "Is Trigger" marcado.
// =============================================================

public class ZoneChegada : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D outro)
    {
        if (outro.CompareTag("Player"))
        {
            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null) gm.OnPlayerChegou();
        }
    }
}
