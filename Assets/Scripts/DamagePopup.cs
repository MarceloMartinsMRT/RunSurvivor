using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float disappearTimer;
    private Color textColor;
    private Color outlineColor = Color.black;
    private Vector3 moveVector;

    private const float DISAPPEAR_TIMER_MAX = 0.65f;
    private const float OUTLINE_WIDTH = 0.32f;

    // POOL DE OBJETOS DE ALTA PERFORMANCE (Evita Instantiate/Destroy constantes)
    private static List<DamagePopup> pool = new List<DamagePopup>();
    private const int MAX_POOL_SIZE = 60;

    void Awake()
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(int damageAmount, bool isCritical)
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();

        // Texto em negrito para dar volume aos números
        textMesh.fontStyle = FontStyles.Bold;
        textMesh.text = damageAmount.ToString();

        if (isCritical)
        {
            // Dano Crítico: maior, laranja/ouro vibrante com borda preta grossa
            textMesh.fontSize = 6.5f;
            textColor = new Color(1f, 0.5f, 0.1f, 1f); // Laranja incandescente
            textMesh.text += "!";
            transform.localScale = Vector3.one * 1.4f;
        }
        else
        {
            // Dano Normal: branco puro com leve tom amarelo para alto contraste
            textMesh.fontSize = 4.8f;
            textColor = new Color(1f, 0.98f, 0.8f, 1f);
            transform.localScale = Vector3.one;
        }

        textMesh.color = textColor;
        ApplyOutline(Color.black, OUTLINE_WIDTH);

        disappearTimer = DISAPPEAR_TIMER_MAX;

        // Efeito de arco: sobe com leve dispersão lateral
        moveVector = new Vector3(Random.Range(-0.85f, 0.85f), 2.5f, 0f);
    }

    public void SetupPlayerDamage(int damageAmount)
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();

        textMesh.fontStyle = FontStyles.Bold;
        textMesh.fontSize = 5.2f;
        textMesh.text = "-" + damageAmount;
        textColor = new Color(1f, 0.25f, 0.25f, 1f); // Vermelho vivo
        textMesh.color = textColor;

        ApplyOutline(Color.black, OUTLINE_WIDTH);

        disappearTimer = DISAPPEAR_TIMER_MAX;
        moveVector = new Vector3(Random.Range(-0.5f, 0.5f), 2.0f, 0f);
    }

    private void ApplyOutline(Color color, float width)
    {
        outlineColor = color;
        textMesh.outlineColor = outlineColor;
        textMesh.outlineWidth = width;

        if (textMesh.fontMaterial != null)
        {
            textMesh.fontMaterial.EnableKeyword("OUTLINE_ON");
            textMesh.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, outlineColor);
            textMesh.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, width);
        }
    }

    void Update()
    {
        // Movimento para cima e desaceleração vertical
        transform.position += moveVector * Time.deltaTime;
        moveVector.y -= 3.2f * Time.deltaTime;

        // Efeito de "punch" elástico na escala
        if (transform.localScale.x > 1f)
        {
            transform.localScale -= Vector3.one * 1.8f * Time.deltaTime;
        }

        disappearTimer -= Time.deltaTime;

        // Fade out na metade final
        if (disappearTimer < DISAPPEAR_TIMER_MAX * 0.5f)
        {
            float fadeSpeed = 3.5f;
            textColor.a -= fadeSpeed * Time.deltaTime;
            outlineColor.a = textColor.a;

            if (textMesh != null)
            {
                textMesh.color = textColor;
                textMesh.outlineColor = outlineColor;
                if (textMesh.fontMaterial != null)
                {
                    textMesh.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, outlineColor);
                }
            }

            if (textColor.a <= 0f)
            {
                gameObject.SetActive(false); // Retorna ao pool sem destruir o GameObject
            }
        }
    }

    public static DamagePopup Create(Vector3 position, int damageAmount, bool isCritical = false)
    {
        Vector3 spawnPos = position + new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(0.2f, 0.45f), 0f);
        DamagePopup popup = GetFromPool();
        popup.transform.position = spawnPos;
        popup.gameObject.SetActive(true);
        popup.Setup(damageAmount, isCritical);

        return popup;
    }

    public static DamagePopup CreatePlayerDamage(Vector3 position, int damageAmount)
    {
        Vector3 spawnPos = position + new Vector3(Random.Range(-0.2f, 0.2f), 0.5f, 0f);
        DamagePopup popup = GetFromPool();
        popup.transform.position = spawnPos;
        popup.gameObject.SetActive(true);
        popup.SetupPlayerDamage(damageAmount);

        return popup;
    }

    private static DamagePopup GetFromPool()
    {
        // 1. Procura um popup inativo na lista do pool
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i] != null && !pool[i].gameObject.activeInHierarchy)
            {
                return pool[i];
            }
        }

        // 2. Se não houver inativos e o pool ainda não atingiu o tamanho máximo, cria um novo
        if (pool.Count < MAX_POOL_SIZE)
        {
            GameObject popupObj = new GameObject("DamagePopup_Pooled");
            TextMeshPro tm = popupObj.AddComponent<TextMeshPro>();
            tm.alignment = TextAlignmentOptions.Center;

            MeshRenderer mr = popupObj.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sortingOrder = 50;
            }

            DamagePopup damagePopup = popupObj.AddComponent<DamagePopup>();
            pool.Add(damagePopup);
            return damagePopup;
        }

        // 3. Se o pool estiver esgotado no limite, recicla o popup ativo mais antigo (pool[0])
        DamagePopup recycled = pool[0];
        pool.RemoveAt(0);
        pool.Add(recycled);
        return recycled;
    }
}
