using UnityEngine;

public class StickyObject : MonoBehaviour
{
    private Rigidbody rb;
    public bool isSticky = false;

    //test

    private void Start()
    {
        // Obtém o Rigidbody do objeto para manipular a gravidade
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Glue"))
        {
            // Ao colidir com um objeto que possui a tag "Glue", o objeto gruda
            isSticky = true;
            rb.useGravity = false;
            AttachToGlue(collision.gameObject);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Glue"))
        {
            // Quando sai da colisão com o objeto "Glue", reativa a gravidade
            isSticky = false;
            rb.useGravity = true;
            transform.SetParent(null); // Remove o objeto do pai
        }
    }

    private void AttachToGlue(GameObject glueObject)
    {
        // Desativa a gravidade e fixa o objeto como filho do objeto "Glue"
        rb.useGravity = false;
        transform.SetParent(glueObject.transform, true);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}
