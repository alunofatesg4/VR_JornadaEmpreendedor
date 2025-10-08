using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ButtonTextSetter : MonoBehaviour
{
    [Header("Destino do texto (TMP)")]
    public TMP_Text targetText;

    [Header("Texto que este bot�o define")]
    public string textoDefinido;

    public void SetTexto()
    {
        if (targetText != null)
            targetText.text = textoDefinido;
        else
            Debug.LogWarning("TargetText n�o atribu�do no bot�o: " + gameObject.name);
    }
}
