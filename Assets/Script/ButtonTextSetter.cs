using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ButtonTextSetter : MonoBehaviour
{
    [Header("Destino do texto (TMP)")]
    public TMP_Text targetText;

    [Header("Texto que este bot�o define")]
    public string textoDefinido;

    // Chame este m�todo no OnClick do bot�o
    public void SetTexto()
    {
        if (targetText != null)
            targetText.text = textoDefinido;
        else
            Debug.LogWarning("TargetText n�o atribu�do no bot�o: " + gameObject.name);
    }
}
