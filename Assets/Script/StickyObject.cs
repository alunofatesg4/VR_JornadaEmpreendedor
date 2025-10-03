using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class StickyObject : MonoBehaviour
{
    private Rigidbody rb;
    private FixedJoint currentJoint;

    [Header("Configurações do Post-it")]
    public int noteID;          // ID do post-it
    public bool isSticky = false;
    public int timerToReturn;   // segundos para voltar à posição inicial

    private XRGrabInteractable grabInteractable;
    private GlueBoard currentBoard; // referência ao quadro colado

    // Posição inicial na mesa
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    // Controle de retorno automático
    private float lostTimer = 0f;
    private bool isGrabbed = false;

    // Último interactor que segurou o post-it (para vibrar o controle certo)
    private XRBaseControllerInteractor lastInteractor;

    [Header("Feedback Haptics")]
    [Range(0f, 1f)] public float hapticIntensity = 0.5f;
    public float hapticDuration = 0.2f;

    [Header("Feedback Visual")]
    public float popScaleMultiplier = 1.5f;
    public float popDuration = 0.2f;
    private Vector3 originalScale;
    private Coroutine popCoroutine;

    [Header("Feedback Sonoro")]
    public AudioSource audioSource;
    public AudioClip stickSound;

    [Header("Feedback Cor")]
    public Material highlightMaterial;
    private Material originalMaterial;
    private MeshRenderer meshRenderer;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Salva posição inicial
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        originalScale = transform.localScale;

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalMaterial = meshRenderer.material;
        }

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void Update()
    {
        if (!isGrabbed && !isSticky)
        {
            lostTimer += Time.deltaTime;

            if (lostTimer >= timerToReturn)
            {
                ReturnToInitialPosition();
            }
        }
        else
        {
            lostTimer = 0f;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Glue") && currentJoint == null)
        {
            GlueBoard board = collision.gameObject.GetComponent<GlueBoard>();
            if (board == null) return;

            if (!board.CanAcceptPostIt(this))
                return;

            isSticky = true;
            rb.useGravity = false;

            ContactPoint contact = collision.contacts[0];
            Vector3 normal = contact.normal;
            float halfDepth = transform.localScale.z / 2f;

            Vector3 correctedPos = transform.position;
            float distToSurface = Vector3.Dot((transform.position - contact.point), normal);
            correctedPos -= normal * distToSurface;
            correctedPos += normal * halfDepth;

            transform.position = correctedPos;

            currentJoint = gameObject.AddComponent<FixedJoint>();
            currentJoint.connectedBody = collision.rigidbody;
            currentJoint.breakForce = Mathf.Infinity;
            currentJoint.breakTorque = Mathf.Infinity;

            currentBoard = board;
            board.AddPostIt(this);

            StartCoroutine(DelayedFeedback());
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Glue") && currentBoard != null)
        {
            ReleaseFromBoard();
        }
    }

    private void ReleaseFromBoard()
    {
        isSticky = false;
        rb.useGravity = true;

        if (currentJoint != null)
        {
            Destroy(currentJoint);
            currentJoint = null;
        }

        if (currentBoard != null)
        {
            currentBoard.RemovePostIt(this);
            currentBoard = null;
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        lostTimer = 0f;

        if (args.interactorObject is XRBaseControllerInteractor controllerInteractor)
        {
            lastInteractor = controllerInteractor;
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;
        ReleaseFromBoard();
    }

    private void ReturnToInitialPosition()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        lostTimer = 0f;
    }

    private IEnumerator DelayedFeedback()
    {
        yield return new WaitForEndOfFrame();

        PlayHapticFeedback();
        PlayPopEffect();
        PlaySoundFeedback();
        PlayColorFeedback();
    }

    private void PlayHapticFeedback()
    {
        if (lastInteractor != null && lastInteractor.xrController != null)
        {
            lastInteractor.xrController.SendHapticImpulse(hapticIntensity, hapticDuration);
        }
    }

    private void PlayPopEffect()
    {
        if (popCoroutine != null)
            StopCoroutine(popCoroutine);

        popCoroutine = StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        Vector3 targetScale = originalScale * popScaleMultiplier;
        float halfDuration = popDuration / 2f;
        float t = 0f;

        while (t < halfDuration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t / halfDuration);
            yield return null;
        }

        t = 0f;
        while (t < halfDuration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t / halfDuration);
            yield return null;
        }

        transform.localScale = originalScale;
        popCoroutine = null;
    }

    private void PlaySoundFeedback()
    {
        if (audioSource != null && stickSound != null)
        {
            audioSource.PlayOneShot(stickSound);
        }
    }

    private void PlayColorFeedback()
    {
        if (meshRenderer != null && highlightMaterial != null)
        {
            StartCoroutine(ColorFlashRoutine());
        }
    }

    private IEnumerator ColorFlashRoutine()
    {
        meshRenderer.material = highlightMaterial;
        yield return new WaitForSeconds(0.3f);
        meshRenderer.material = originalMaterial;
    }
}
