using UnityEngine;
using System.Collections;

public class NPCDialogueActivator : MonoBehaviour
{
    [Header("Configura��es do NPC")]
    public Transform player;                 // Refer�ncia ao jogador
    public float activationRange = 3f;       // Dist�ncia m�xima para iniciar o di�logo
    public AudioSource audioSource;          // Fonte de �udio do NPC
    public AudioClip[] dialogueClips;        // Lista de falas em sequ�ncia
    public GameObject objectToActivate;      // Objeto que ser� ativado ap�s o di�logo

    private bool dialogueStarted = false;    // Evita repetir o di�logo

    private void Update()
    {
        if (dialogueStarted) return;

        // Verifica dist�ncia do jogador
        float distance = Vector3.Distance(player.position, transform.position);
        if (distance <= activationRange)
        {
            StartCoroutine(PlayDialogueSequence());
            dialogueStarted = true;
        }
    }

    private IEnumerator PlayDialogueSequence()
    {
        if (audioSource == null || dialogueClips.Length == 0)
            yield break;

        // Toca todas as falas em sequ�ncia
        foreach (AudioClip clip in dialogueClips)
        {
            audioSource.clip = clip;
            audioSource.Play();
            yield return new WaitForSeconds(clip.length + 0.2f); // pequeno intervalo entre falas
        }

        // Ap�s finalizar todas as falas, ativa o objeto
        if (objectToActivate != null)
            objectToActivate.SetActive(true);
    }

    private void OnDrawGizmosSelected()
    {
        // Gizmo visual no editor para o range de ativa��o
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, activationRange);
    }
}
