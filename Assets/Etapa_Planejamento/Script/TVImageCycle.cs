using UnityEngine;

public class TVImageCycle : MonoBehaviour
{
    [Header("Imagens que vão aparecer na TV")]
    public Texture[] imagens;

    [Header("Tempo entre trocas (segundos)")]
    public float delay = 5f;

    private Renderer tvRenderer;
    private int imagemAtual = 0;
    private float timer = 0f;

    void Start()
    {
        tvRenderer = GetComponent<Renderer>();

        if (imagens.Length > 0 && tvRenderer != null)
        {
            tvRenderer.material.mainTexture = imagens[0];
        }
    }

    void Update()
    {
        if (imagens.Length == 0 || tvRenderer == null)
            return;

        timer += Time.deltaTime;

        if (timer >= delay)
        {
            timer = 0f;
            imagemAtual = (imagemAtual + 1) % imagens.Length;
            tvRenderer.material.mainTexture = imagens[imagemAtual];
        }
    }
}
