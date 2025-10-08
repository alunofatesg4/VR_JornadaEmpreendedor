using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class NPCDialogueActivator : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    public AudioSource audioSource;

    [Header("Falas e Configurações")]
    public float firstDialogueRange = 3f;
    public float thirdDialogueRange = 3f;
    public float fifthDialogueRange = 3f;
    public float tvDialogueRange = 3f;

    public AudioClip[] firstDialogueClips;
    public AudioClip[] secondDialogueClips;
    public AudioClip[] thirdDialogueClips;
    public AudioClip[] fourthDialogueClips;
    public AudioClip[] fifthDialogueClips;

    [Header("Falas Especiais dos Botões (escolhas)")]
    public AudioClip[] impactoSocialClips;
    public AudioClip[] equilibrioFinanceiroClips;
    public AudioClip[] praticasSustentaveisClips;

    [Header("Diálogo Final (quando jogador clicar em concluir - será o diálogo 7)")]
    public AudioClip[] finalDialogueClips;

    [Header("Objetos e Eventos Gerais")]
    public GameObject objectToActivateAfterFirst;
    public Animator doorAnimator;
    public string doorOpenTrigger = "Open";
    public Transform newPositionAfterDialogue2;
    public Transform tvObject;
    public GameObject finalObjectToActivate;

    [Header("Objetos Específicos por Escolha")]
    public GameObject objetoImpactoSocial;
    public GameObject objetoEquilibrioFinanceiro;
    public GameObject objetoPraticasSustentaveis;

    [Header("Temporizador de Lembrete")]
    public float reminderDelay = 60f;
    public AudioClip[] reminderClips;

    [Header("Finalização")]
    public float timeBeforeEnd = 20f;
    public Image fadeImage;
    public float fadeDuration = 2f;
    public string menuSceneName = "MenuPrincipal";
    public bool quitGameInstead = false;

    // estados internos
    private bool firstDialoguePlayed = false;
    private bool secondDialogueTriggered = false;
    private bool npcTeleported = false;
    private bool thirdDialogueStarted = false;
    private bool fourthDialogueStarted = false;
    private bool fifthDialogueStarted = false;

    private Coroutine currentDialogueCoroutine = null;
    private Coroutine reminderCoroutine = null;
    private bool isSpecialDialogueActive = false;
    private int selectedOption = 0;
    private bool isDialoguePlaying = false; // impede diálogos automáticos simultâneos

    private void Update()
    {
        float distanceToNPC = Vector3.Distance(player.position, transform.position);

        // --- FALA 1 ---
        if (!firstDialoguePlayed && !isDialoguePlaying && distanceToNPC <= firstDialogueRange)
        {
            firstDialoguePlayed = true;
            StartCoroutine(PlayDialogueSequence(firstDialogueClips, () =>
            {
                if (objectToActivateAfterFirst != null)
                    objectToActivateAfterFirst.SetActive(true);

                StartReminderLoop();
            }));
        }

        // --- FALA 3 ---
        if (npcTeleported && !thirdDialogueStarted && !isDialoguePlaying && distanceToNPC <= thirdDialogueRange)
        {
            thirdDialogueStarted = true;
            StartCoroutine(PlayDialogueSequence(thirdDialogueClips));
        }

        // --- FALA 4 (TV) ---
        if (npcTeleported && !fourthDialogueStarted && !isDialoguePlaying && tvObject != null)
        {
            float distanceToTV = Vector3.Distance(player.position, tvObject.position);
            if (distanceToTV <= tvDialogueRange)
            {
                fourthDialogueStarted = true;
                StartCoroutine(PlayDialogueSequence(fourthDialogueClips));
            }
        }

        // --- FALA 5 ---
        if (fourthDialogueStarted && !fifthDialogueStarted && !isDialoguePlaying && distanceToNPC <= fifthDialogueRange)
        {
            fifthDialogueStarted = true;
            StartCoroutine(PlayDialogueSequence(fifthDialogueClips, () =>
            {
                if (finalObjectToActivate != null)
                    finalObjectToActivate.SetActive(true);

                StartReminderLoop();
            }));
        }

        // Faz o NPC olhar para o jogador se estiver falando
        if (audioSource != null && audioSource.isPlaying)
        {
            Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.LookAt(lookPos);
        }
    }

    public void TriggerNextDialogue()
    {
        if (secondDialogueTriggered) return;
        secondDialogueTriggered = true;

        StopReminderLoop();
        StopCurrentDialogueImmediate();

        StartCoroutine(PlayDialogueSequence(secondDialogueClips, () =>
        {
            if (doorAnimator != null)
                doorAnimator.SetTrigger(doorOpenTrigger);

            if (newPositionAfterDialogue2 != null)
            {
                transform.position = newPositionAfterDialogue2.position;
                transform.rotation = newPositionAfterDialogue2.rotation;
                npcTeleported = true;
            }

            StartReminderLoop();
        }));
    }

    // --- ESCOLHAS DOS BOTÕES ---
    public void SelectImpactoSocial()
    {
        selectedOption = 1;
        StartSpecialDialogue(impactoSocialClips);
    }

    public void SelectEquilibrioFinanceiro()
    {
        selectedOption = 2;
        StartSpecialDialogue(equilibrioFinanceiroClips);
    }

    public void SelectPraticasSustentaveis()
    {
        selectedOption = 3;
        StartSpecialDialogue(praticasSustentaveisClips);
    }

    private void StartSpecialDialogue(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
            return;

        StopReminderLoop();
        StopCurrentDialogueImmediate();

        if (objetoImpactoSocial != null) objetoImpactoSocial.SetActive(false);
        if (objetoEquilibrioFinanceiro != null) objetoEquilibrioFinanceiro.SetActive(false);
        if (objetoPraticasSustentaveis != null) objetoPraticasSustentaveis.SetActive(false);

        isSpecialDialogueActive = true;
        currentDialogueCoroutine = StartCoroutine(PlayDialogueSequence(clips, () =>
        {
            isSpecialDialogueActive = false;
            StartReminderLoop();
        }));
    }

    // --- BOTÃO CONCLUIR FINAL ---
    public void TriggerFinalizeActivity()
    {
        StopReminderLoop();
        StopCurrentDialogueImmediate();

        StartCoroutine(PlayFinalSequenceAndActivateSelected());
    }

    private IEnumerator PlayFinalSequenceAndActivateSelected()
    {
        if (finalDialogueClips == null || finalDialogueClips.Length == 0)
        {
            ActivateSelectedObject();
            yield return new WaitForSeconds(timeBeforeEnd);
            FindObjectOfType<VRScreenFade>()?.StartCoroutine("FadeAndQuitRoutine");
            yield break;
        }

        currentDialogueCoroutine = StartCoroutine(PlayDialogueSequence(finalDialogueClips, () =>
        {
            currentDialogueCoroutine = null;
        }));

        yield return new WaitForSeconds(2f);
        ActivateSelectedObject();

        yield return new WaitForSeconds(timeBeforeEnd);
        FindObjectOfType<VRScreenFade>()?.StartCoroutine("FadeAndQuitRoutine");
    }

    private void ActivateSelectedObject()
    {
        if (objetoImpactoSocial != null) objetoImpactoSocial.SetActive(false);
        if (objetoEquilibrioFinanceiro != null) objetoEquilibrioFinanceiro.SetActive(false);
        if (objetoPraticasSustentaveis != null) objetoPraticasSustentaveis.SetActive(false);

        switch (selectedOption)
        {
            case 1: if (objetoImpactoSocial != null) objetoImpactoSocial.SetActive(true); break;
            case 2: if (objetoEquilibrioFinanceiro != null) objetoEquilibrioFinanceiro.SetActive(true); break;
            case 3: if (objetoPraticasSustentaveis != null) objetoPraticasSustentaveis.SetActive(true); break;
        }
    }

    // --- UTILITÁRIOS ---
    private void StopCurrentDialogueImmediate()
    {
        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
            currentDialogueCoroutine = null;
        }

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        isDialoguePlaying = false;
        isSpecialDialogueActive = false;
    }

    private IEnumerator PlayDialogueSequence(AudioClip[] clips, System.Action onComplete = null)
    {
        if (audioSource == null || clips == null || clips.Length == 0)
        {
            onComplete?.Invoke();
            yield break;
        }

        isDialoguePlaying = true;

        foreach (AudioClip clip in clips)
        {
            if (!isActiveAndEnabled) yield break;

            audioSource.clip = clip;
            audioSource.Play();
            yield return new WaitForSeconds(clip.length + 0.2f);
        }

        isDialoguePlaying = false;
        onComplete?.Invoke();
    }

    private void StartReminderLoop()
    {
        StopReminderLoop();
        reminderCoroutine = StartCoroutine(ReminderRoutine());
    }

    private void StopReminderLoop()
    {
        if (reminderCoroutine != null)
        {
            StopCoroutine(reminderCoroutine);
            reminderCoroutine = null;
        }
    }

    private IEnumerator ReminderRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(reminderDelay);

            if (reminderClips != null && reminderClips.Length > 0 && (audioSource != null && !audioSource.isPlaying))
            {
                AudioClip randomClip = reminderClips[Random.Range(0, reminderClips.Length)];
                audioSource.PlayOneShot(randomClip);
                yield return new WaitForSeconds(randomClip.length + 0.1f);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, firstDialogueRange);

        if (newPositionAfterDialogue2 != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(newPositionAfterDialogue2.position, thirdDialogueRange);
        }

        if (tvObject != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(tvObject.position, tvDialogueRange);
        }
    }
}
