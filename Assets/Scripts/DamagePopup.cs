using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float disappearTimer;
    private Color textColor;
    private Vector3 moveVector;

    private const float DISAPPEAR_TIMER_MAX = 0.65f;

    void Awake()
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(int damageAmount, bool isCritical)
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();

        textMesh.text = damageAmount.ToString();

        if (isCritical)
        {
            // Dano Crítico: maior, dourado/laranja vibrante
            textMesh.fontSize = 6f;
            textColor = new Color(1f, 0.45f, 0.1f, 1f);
            textMesh.text += "!";
            transform.localScale = Vector3.one * 1.35f;
        }
        else
        {
            // Dano Normal: branco com tom levemente amarelado
            textMesh.fontSize = 4.5f;
            textColor = new Color(1f, 0.95f, 0.75f, 1f);
            transform.localScale = Vector3.one;
        }

        textMesh.color = textColor;
        disappearTimer = DISAPPEAR_TIMER_MAX;

        // Efeito de arco: sobe com leve dispersão lateral
        moveVector = new Vector3(Random.Range(-0.9f, 0.9f), 2.4f, 0f);
    }

    void Update()
    {
        // Movimento para cima e desaceleração
        transform.position += moveVector * Time.deltaTime;
        moveVector.y -= 3.0f * Time.deltaTime;

        // Efeito de impacto (shrink back)
        if (transform.localScale.x > 1f)
        {
            transform.localScale -= Vector3.one * 1.5f * Time.deltaTime;
        }

        disappearTimer -= Time.deltaTime;

        // Fade out na metade final
        if (disappearTimer < DISAPPEAR_TIMER_MAX * 0.5f)
        {
            float fadeSpeed = 3.5f;
            textColor.a -= fadeSpeed * Time.deltaTime;
            if (textMesh != null) textMesh.color = textColor;

            if (textColor.a <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }

    public static DamagePopup Create(Vector3 position, int damageAmount, bool isCritical = false)
    {
        GameObject popupObj = new GameObject("DamagePopup");
        Vector3 spawnPos = position + new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(0.2f, 0.45f), 0f);
        popupObj.transform.position = spawnPos;

        TextMeshPro tm = popupObj.AddComponent<TextMeshPro>();
        tm.alignment = TextAlignmentOptions.Center;

        MeshRenderer mr = popupObj.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.sortingOrder = 50; // Na frente de sprites e inimigos
        }

        DamagePopup damagePopup = popupObj.AddComponent<DamagePopup>();
        damagePopup.Setup(damageAmount, isCritical);

        return damagePopup;
    }

    public static DamagePopup CreatePlayerDamage(Vector3 position, int damageAmount)
    {
        GameObject popupObj = new GameObject("DamagePopup_Player");
        Vector3 spawnPos = position + new Vector3(Random.Range(-0.2f, 0.2f), 0.5f, 0f);
        popupObj.transform.position = spawnPos;

        TextMeshPro tm = popupObj.AddComponent<TextMeshPro>();
        tm.alignment = TextAlignmentOptions.Center;
        tm.fontSize = 5f;
        tm.text = "-" + damageAmount;
        tm.color = new Color(1f, 0.25f, 0.25f, 1f); // Vermelho

        MeshRenderer mr = popupObj.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.sortingOrder = 55;
        }

        DamagePopup damagePopup = popupObj.AddComponent<DamagePopup>();
        damagePopup.textMesh = tm;
        damagePopup.textColor = tm.color;
        damagePopup.disappearTimer = DISAPPEAR_TIMER_MAX;
        damagePopup.moveVector = new Vector3(Random.Range(-0.5f, 0.5f), 2.0f, 0f);

        return damagePopup;
    }
}
