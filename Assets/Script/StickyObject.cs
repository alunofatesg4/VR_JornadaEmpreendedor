using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class StickyObject : MonoBehaviour
{
    private Rigidbody rb;
    private FixedJoint currentJoint;

    [Header("Configura��es do Post-it")]
    public int[] noteIDs;
    public bool isSticky = false;
    public int timerToReturn;
    public float returnDistanceThreshold = 0.05f; // dist�ncia m�nima para considerar "fora do lugar"

    private XRGrabInteractable grabInteractable;
    private GlueBoard currentBoard;

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
        // pausa se intera��o bloqueada
        if (GameManager.Instance != null && GameManager.Instance.interactionLocked)
            return;

        if (transform.localScale != originalScale)
            transform.localScale = originalScale;

        if (!isGrabbed && !isSticky)
        {
            // S� come�a a contar o tempo se o post-it estiver distante do ponto original
            if (Vector3.Distance(transform.position, initialPosition) > returnDistanceThreshold)
            {
                lostTimer += Time.deltaTime;
                if (lostTimer >= timerToReturn)
                    ReturnToInitialPosition();
            }
            else
            {
                lostTimer = 0f; // Est� perto, ent�o reseta o contador
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

            // Cria o joint
            currentJoint = gameObject.AddComponent<FixedJoint>();
            currentJoint.connectedBody = collision.rigidbody;
            currentJoint.breakForce = Mathf.Infinity;
            currentJoint.breakTorque = Mathf.Infinity;

            currentBoard = board;
            board.AddPostIt(this);

            StartCoroutine(FeedbackColado());
        }
    }

    private IEnumerator FeedbackColado()
    {
        // Desativa intera��o temporariamente
        if (grabInteractable != null)
            grabInteractable.enabled = false;

        canBeGrabbedAgain = false;

        yield return new WaitForSeconds(0.05f);

        //Vibra��o com seguran�a � procura o interactor ativo
        XRBaseControllerInteractor controllerInteractor = null;
        if (grabInteractable != null && grabInteractable.interactorsSelecting.Count > 0)
        {
            controllerInteractor = grabInteractable.interactorsSelecting[0] as XRBaseControllerInteractor;
        }

        if (controllerInteractor != null)
            controllerInteractor.SendHapticImpulse(0.4f, 0.25f);

        // Troca de material
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

        // Espera feedback antes de liberar intera��o
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
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        if (GameManager.Instance != null && GameManager.Instance.interactionLocked)
            return;

        isGrabbed = false;
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
