using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class StickyObject : MonoBehaviour
{
    private Rigidbody rb;
    private FixedJoint currentJoint;

    [Header("Configurações do Post-it")]
    public int[] noteIDs;
    public bool isSticky = false;
    public int timerToReturn;
    public float returnDistanceThreshold = 0.05f; // distância mínima para considerar "fora do lugar"

    private XRGrabInteractable grabInteractable;
    private GlueBoard currentBoard;
    private XRBaseControllerInteractor lastInteractor; // Guarda o último interactor usado

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 originalScale;

    private float lostTimer = 0f;
    private bool isGrabbed = false;

    [Header("Feedbacks")]
    public AudioSource audioSource;
    public AudioClip stickSound;
    public Material normalMaterial;
    public Material highlightMaterial;
    public MeshRenderer meshRenderer;
    public float timeToWaitFeedback = 0.5f;

    private bool canBeGrabbedAgain = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        initialPosition = transform.position;
        initialRotation = transform.rotation;
        originalScale = transform.localScale;

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.interactionLocked)
            return;

        if (transform.localScale != originalScale)
            transform.localScale = originalScale;

        if (!isGrabbed && !isSticky)
        {
            if (Vector3.Distance(transform.position, initialPosition) > returnDistanceThreshold)
            {
                lostTimer += Time.deltaTime;
                if (lostTimer >= timerToReturn)
                    ReturnToInitialPosition();
            }
            else
            {
                lostTimer = 0f;
            }
        }
        else
        {
            lostTimer = 0f;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (GameManager.Instance != null && GameManager.Instance.interactionLocked)
            return;

        if (collision.gameObject.CompareTag("Glue") && currentJoint == null && canBeGrabbedAgain)
        {
            GlueBoard board = collision.gameObject.GetComponent<GlueBoard>();
            if (board == null) return;

            if (!board.CanAcceptPostIt(this))
                return;

            isSticky = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Corrige profundidade
            ContactPoint contact = collision.contacts[0];
            Vector3 normal = contact.normal;
            float halfDepth = transform.localScale.z / 2f;
            Vector3 correctedPos = transform.position;
            float distToSurface = Vector3.Dot((transform.position - contact.point), normal);
            correctedPos -= normal * distToSurface;
            correctedPos += normal * halfDepth;
            transform.position = correctedPos;

            // Força rotação X=0, Y=180, mantém o Z atual
            Vector3 currentEuler = transform.eulerAngles;
            transform.rotation = Quaternion.Euler(0f, 180f, currentEuler.z);

            // Cria o joint
            currentJoint = gameObject.AddComponent<FixedJoint>();
            currentJoint.connectedBody = collision.rigidbody;
            currentJoint.breakForce = Mathf.Infinity;
            currentJoint.breakTorque = Mathf.Infinity;

            currentBoard = board;
            board.AddPostIt(this);

            // Vibração imediata ao colar (usa o último interactor)
            if (lastInteractor != null)
                lastInteractor.SendHapticImpulse(0.4f, 0.25f);

            // Inicia feedback visual/sonoro
            StartCoroutine(FeedbackColado());
        }
    }

    private IEnumerator FeedbackColado()
    {
        if (grabInteractable != null)
            grabInteractable.enabled = false;

        canBeGrabbedAgain = false;

        yield return new WaitForSeconds(0.05f);

        // Troca de material (feedback visual)
        if (meshRenderer != null && highlightMaterial != null)
        {
            meshRenderer.material = highlightMaterial;
            yield return new WaitForSeconds(0.2f);
            meshRenderer.material = normalMaterial;
        }

        // Som
        if (audioSource != null && stickSound != null)
            audioSource.PlayOneShot(stickSound);

        // Escala de feedback
        yield return StartCoroutine(ScalePulse(1.5f, 0.1f));

        yield return new WaitForSeconds(timeToWaitFeedback);

        if (grabInteractable != null)
            grabInteractable.enabled = true;

        canBeGrabbedAgain = true;
    }

    private IEnumerator ScalePulse(float multiplier, float duration)
    {
        Vector3 startScale = originalScale;
        Vector3 targetScale = originalScale * multiplier;
        float t = 0f;

        while (t < duration)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;

        t = 0f;
        while (t < duration)
        {
            transform.localScale = Vector3.Lerp(targetScale, startScale, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = startScale;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (GameManager.Instance != null && GameManager.Instance.interactionLocked)
            return;

        if (collision.gameObject.CompareTag("Glue") && currentBoard != null)
            ReleaseFromBoard();
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

        transform.localScale = originalScale;
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        if (GameManager.Instance != null && GameManager.Instance.interactionLocked)
            return;

        if (!canBeGrabbedAgain)
        {
            args.interactorObject.transform.GetComponent<XRBaseControllerInteractor>()?.SendHapticImpulse(0.2f, 0.1f);
            return;
        }

        isGrabbed = true;
        lostTimer = 0f;

        // Guarda o interactor ativo
        lastInteractor = args.interactorObject as XRBaseControllerInteractor;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        if (GameManager.Instance != null && GameManager.Instance.interactionLocked)
            return;

        isGrabbed = false;

        // Guarda o último interactor antes de perder a referência
        lastInteractor = args.interactorObject as XRBaseControllerInteractor;

        ReleaseFromBoard();
    }

    private void ReturnToInitialPosition()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        transform.localScale = originalScale;
        lostTimer = 0f;
    }
}
