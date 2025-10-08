using UnityEngine;
using System.Collections;

public class VRScreenFade : MonoBehaviour
{
    [Header("Fade Settings")]
    public Renderer fadeRenderer;
    public float fadeDuration = 2f;
    public float delayBeforeFade = 20f;
    private Material fadeMaterial;

    private void Start()
    {
        if (fadeRenderer == null)
        {
            Debug.LogError("Nenhum Renderer atribuído ao Fade.");
            return;
        }

        // Garante que o material comece transparente
        fadeMaterial = fadeRenderer.material;
        Color color = fadeMaterial.color;
        color.a = 0f;
        fadeMaterial.color = color;

        // Inicia o processo automático (ou pode ser chamado manualmente)
        //StartCoroutine(FadeAndQuitRoutine());
    }

    public IEnumerator FadeAndQuitRoutine()
    {
        // Espera o tempo antes do fade
        yield return new WaitForSeconds(delayBeforeFade);

        // Executa o fade-out suave
        float t = 0f;
        Color color = fadeMaterial.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            fadeMaterial.color = color;
            yield return null;
        }

        // Garante preto total
        color.a = 1f;
        fadeMaterial.color = color;

        // Espera um pouco e fecha o jogo
        yield return new WaitForSeconds(1f);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
