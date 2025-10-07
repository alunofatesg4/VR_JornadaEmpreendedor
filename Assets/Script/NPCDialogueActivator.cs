using UnityEngine;
using System.Collections;

public class NPCDialogueActivator : MonoBehaviour
{
    [Header("Configurações do NPC")]
    public Transform player;                 // Referência ao jogador
    public float activationRange = 3f;       // Distância máxima para iniciar o diálogo
    public AudioSource audioSource;          // Fonte de áudio do NPC
    public AudioClip[] dialogueClips;        // Lista de falas em sequência
    public GameObject objectToActivate;      // Objeto que será ativado após o diálogo

    private bool dialogueStarted = false;    // Evita repetir o diálogo

    private void Update()
    {
        if (dialogueStarted) return;

        // Verifica distância do jogador
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

        // Toca todas as falas em sequência
        foreach (AudioClip clip in dialogueClips)
        {
            audioSource.clip = clip;
            audioSource.Play();
            yield return new WaitForSeconds(clip.length + 0.2f); // pequeno intervalo entre falas
        }

        // Após finalizar todas as falas, ativa o objeto
        if (objectToActivate != null)
            objectToActivate.SetActive(true);
    }

    private void OnDrawGizmosSelected()
    {
        // Gizmo visual no editor para o range de ativação
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, activationRange);
    }
}
