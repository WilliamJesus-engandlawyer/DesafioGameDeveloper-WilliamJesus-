using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2D : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidadeBase = 4f;

    [HideInInspector] public float multiplicadorClima = 1f;

    private Rigidbody2D rb;
    private Vector2 inputMovimento;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    void Update()
    {
        if (Keyboard.current == null) { inputMovimento = Vector2.zero; return; }

        float x = 0f, y = 0f;
        if (Keyboard.current.aKey.isPressed) x = -1f;
        if (Keyboard.current.dKey.isPressed) x =  1f;
        if (Keyboard.current.sKey.isPressed) y = -1f;
        if (Keyboard.current.wKey.isPressed) y =  1f;

        inputMovimento = new Vector2(x, y).normalized;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = inputMovimento * velocidadeBase * multiplicadorClima;
    }

    public void Morrer()
    {
        inputMovimento = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        gameObject.SetActive(false);
    }

    public void Resetar(Vector3 posicaoInicial)
    {
        transform.position = posicaoInicial;
        gameObject.SetActive(true);
        multiplicadorClima = 1f;
    }
}