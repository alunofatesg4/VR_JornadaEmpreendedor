using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GlueBoard : MonoBehaviour
{
    [Header("Configuração do Quadro")]
    public int boardID;
    public int maxPostIts = 1;

    [Header("UI de Aviso (Canvas do aviso)")]
    public GameObject avisoUI;
    public float avisoDuration = 2.5f;

    [Header("Som de Aviso")]
    public AudioSource audioSource;
    public AudioClip avisoSom;

    [Header("Botão de voltar que será desativado quando um post-it for colado")]
    public GameObject objetoParaDesativar;

    private List<StickyObject> postIts = new List<StickyObject>();
    private Coroutine avisoRoutine;
    private Vector3 avisoEscalaOriginal;

    private void Start()
    {
        if (avisoUI != null)
        {
            avisoUI.SetActive(false);
            avisoEscalaOriginal = avisoUI.transform.localScale;
        }
    }

    public bool CanAcceptPostIt(StickyObject postIt)
    {
        if (GameManager.Instance != null && GameManager.Instance.interactionLocked)
            return false;

        bool canAccept = postIts.Count < maxPostIts;

        if (!canAccept)
        {
            MostrarAviso();
            TocarSomAviso();
        }

        return canAccept;
    }

    public void AddPostIt(StickyObject postIt)
    {
        if (!postIts.Contains(postIt))
        {
            postIts.Add(postIt);
            GameManager.Instance.UpdateScore();

            // Desativa o botão vinculado assim que o primeiro post-it for colado
            if (objetoParaDesativar != null && objetoParaDesativar.activeSelf)
            {
                objetoParaDesativar.SetActive(false);
            }
        }
    }

    public void RemovePostIt(StickyObject postIt)
    {
        if (postIts.Contains(postIt))
        {
            postIts.Remove(postIt);
            GameManager.Instance.UpdateScore();
        }
    }

    // Calcula a pontuação do quadro
    public int GetScore()
    {
        if (postIts.Count == 0)
            return 0;

        int points = 0;

        foreach (var p in postIts)
        {
            // Verifica se o post-it possui o boardID na sua lista de IDs
            bool match = false;
            foreach (var id in p.noteIDs)
            {
                if (id == boardID)
                {
                    match = true;
                    break;
                }
            }

            if (match)
            {
                // Mantém a lógica anterior de pontuação
                if (maxPostIts == 1)
                    points += 25;
                else
                    points += 10;
            }
        }

        return points;
    }


    // Mostra o aviso visual
    private void MostrarAviso()
    {
        if (avisoUI == null) return;

        if (avisoRoutine != null)
            StopCoroutine(avisoRoutine);

        avisoRoutine = StartCoroutine(AvisoTemporario());
    }

    private IEnumerator AvisoTemporario()
    {
        avisoUI.SetActive(true);
        StartCoroutine(PulsarAviso());

        yield return new WaitForSeconds(avisoDuration);

        avisoUI.SetActive(false);
        avisoUI.transform.localScale = avisoEscalaOriginal;
        avisoRoutine = null;
    }

    // Faz o aviso "pulsar" (aumentar e reduzir)
    private IEnumerator PulsarAviso()
    {
        float duracao = 0.2f;
        float escalaMax = 1.25f;

        Vector3 alvo = avisoEscalaOriginal * escalaMax;
        float tempo = 0f;

        // Cresce
        while (tempo < duracao)
        {
            avisoUI.transform.localScale = Vector3.Lerp(avisoEscalaOriginal, alvo, tempo / duracao);
            tempo += Time.deltaTime;
            yield return null;
        }
        avisoUI.transform.localScale = alvo;

        // Volta
        tempo = 0f;
        while (tempo < duracao)
        {
            avisoUI.transform.localScale = Vector3.Lerp(alvo, avisoEscalaOriginal, tempo / duracao);
            tempo += Time.deltaTime;
            yield return null;
        }
        avisoUI.transform.localScale = avisoEscalaOriginal;
    }

    private void TocarSomAviso()
    {
        if (audioSource != null && avisoSom != null)
        {
            audioSource.PlayOneShot(avisoSom);
        }
    }
}
