using UnityEngine;

public class move : MonoBehaviour
{

    public float velocidad;
    public Vector3 direccion; // (x, y, z)

    CharacterController controller; //Se comunca con el character controller

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = gameObject.GetComponent<CharacterController>(); //Se toma el componente Character y lo asignamos a la variable controller
    }

    // Update is called once per frame
    void Update()
    {
        direccion = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")); //Lee las entradas de direccion horizontal y vertical -1, 0 ó 1

        controller.Move(direccion * Time.deltaTime * velocidad); //Se aplica el movimiento al controller

        if (direccion != Vector3.zero) //Detecta si se mueve el personaje
        {
            transform.forward = direccion; //Apunta al personaje en la direccion
        }
    }
}
