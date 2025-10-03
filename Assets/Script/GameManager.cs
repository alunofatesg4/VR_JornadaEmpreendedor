using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int totalScore = 0;
    private GlueBoard[] boards;

    [Header("UI")]
    public TextMeshProUGUI scoreText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        boards = FindObjectsOfType<GlueBoard>();
        UpdateScore();
    }

    public void UpdateScore()
    {
        totalScore = 0;
        foreach (var board in boards)
        {
            totalScore += board.GetScore();
        }

        if (scoreText != null)
            scoreText.text = "Pontuação: " + totalScore + " / 80";

        Debug.Log("Pontuação Atual: " + totalScore);
    }
}
