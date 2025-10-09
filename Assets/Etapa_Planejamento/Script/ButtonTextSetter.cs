using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ButtonTextSetter : MonoBehaviour
{
    [Header("Destino do texto (TMP)")]
    public TMP_Text targetText;

    [Header("Texto que este botão define")]
    public string textoDefinido;

    public void SetTexto()
    {
        if (targetText != null)
            targetText.text = textoDefinido;
        else
            Debug.LogWarning("TargetText não atribuído no botão: " + gameObject.name);
    }
}
