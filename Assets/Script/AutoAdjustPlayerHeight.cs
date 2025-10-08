using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

[RequireComponent(typeof(XROrigin))]
[RequireComponent(typeof(CharacterController))]
public class AutoAdjustPlayerHeight : MonoBehaviour
{
    private XROrigin xrOrigin;
    private CharacterController characterController;

    [Header("Configurações de Altura")]
    public float minHeight = 1.0f;  // altura mínima (sentado)
    public float maxHeight = 2.0f;  // altura máxima (em pé)
    public float skinWidth = 0.05f;

    void Start()
    {
        xrOrigin = GetComponent<XROrigin>();
        characterController = GetComponent<CharacterController>();
    }

    void FixedUpdate()
    {
        if (xrOrigin == null || characterController == null)
            return;

        Transform cameraTransform = xrOrigin.Camera.transform;
        Vector3 cameraPos = xrOrigin.CameraInOriginSpacePos;

        float headHeight = Mathf.Clamp(cameraPos.y, minHeight, maxHeight);
        characterController.height = headHeight;

        Vector3 newCenter = Vector3.zero;
        newCenter.y = characterController.height / 2f + characterController.skinWidth;
        newCenter.x = cameraPos.x;
        newCenter.z = cameraPos.z;

        characterController.center = newCenter;
    }
}
