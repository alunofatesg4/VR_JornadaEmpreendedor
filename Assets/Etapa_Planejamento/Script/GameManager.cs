using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int totalScore = 0;
    public GlueBoard[] boards;

    [Header("UI")]
    public TextMeshProUGUI scoreText;

    [Header("Confirmação de Finalização")]
    public GameObject confirmPanel; // Canvas de confirmação
    public Button confirmButton;    // Botão "Concluir"
    public Button cancelButton;     // Botão "Voltar"
    public Button openConfirmButton; // Botão que abre o painel

    [HideInInspector] public bool interactionLocked = false;

    private StickyObject[] allPostIts; // Todos os post-its na cena

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        UpdateScore();

        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        if (openConfirmButton != null)
            openConfirmButton.onClick.AddListener(OpenConfirmationPanel);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(ConfirmEndSession);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(CancelEndSession);

        // Localiza todos os post-its existentes na cena
        allPostIts = FindObjectsOfType<StickyObject>(true);
    }

    public void UpdateScore()
    {
        totalScore = 0;
        foreach (var board in boards)
            totalScore += board.GetScore();

        if (scoreText != null)
            scoreText.text = "Pontuação: " + totalScore + " / 80";
    }

    // Quando o botão "Finalizar" é clicado
    public void OpenConfirmationPanel()
    {
        interactionLocked = true;
        confirmPanel.SetActive(true);

        LockAllPostIts(true);
    }

    // Quando o jogador confirma a conclusão
    public void ConfirmEndSession()
    {
        confirmPanel.SetActive(false);
        interactionLocked = true;

        LockAllPostIts(true); // garante que continuam bloqueados

        int finalScore = totalScore;
        Debug.Log($"Jogo concluído! Pontuação final: {finalScore}");
    }

    // Quando o jogador cancela
    public void CancelEndSession()
    {
        confirmPanel.SetActive(false);
        interactionLocked = false;

        LockAllPostIts(false); // reativa post-its
    }

    //Bloqueia ou desbloqueia todos os post-its da cena
    private void LockAllPostIts(bool locked)
    {
        if (allPostIts == null) return;

        foreach (var postit in allPostIts)
        {
            if (postit == null) continue;

            var grab = postit.GetComponent<XRGrabInteractable>();
            if (grab != null)
                grab.enabled = !locked;

            // Não altera isKinematic — apenas congela interações via script
            // Assim o post-it não "reinicia" fisicamente
        }
    }
}
