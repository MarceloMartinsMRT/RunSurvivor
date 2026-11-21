using UnityEngine;

public class ChestPointer : MonoBehaviour
{
    [Header("Pointer Settings")]
    public float orbitDistance = 1.45f;

    private GameObject pointerObj;
    private SpriteRenderer pointerRenderer;
    private static Sprite cachedPointerSprite;

    void Start()
    {
        CreatePointerObject();
    }

    void Update()
    {
        if (GlobalVariables._gameIsPaused || pointerObj == null) return;

        TreasureChest nearest = GetNearestChest();

        if (nearest == null || !nearest.gameObject.activeInHierarchy)
        {
            if (pointerRenderer.enabled) pointerRenderer.enabled = false;
            return;
        }

        Vector3 diff = nearest.transform.position - transform.position;
        float distance = diff.magnitude;

        // Se o jogador já estiver em cima do baú, esconde a seta
        if (distance < 0.7f)
        {
            if (pointerRenderer.enabled) pointerRenderer.enabled = false;
            return;
        }

        if (!pointerRenderer.enabled) pointerRenderer.enabled = true;

        Vector3 dir = diff.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Posiciona a seta orbitando o jogador e apontando para o baú
        pointerObj.transform.position = transform.position + dir * orbitDistance;
        pointerObj.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Pulso suave em escala para chamar atenção
        float pulse = 0.9f + Mathf.Sin(Time.time * 7f) * 0.15f;
        pointerObj.transform.localScale = new Vector3(pulse, pulse, 1f);
    }

    private TreasureChest GetNearestChest()
    {
        if (TreasureChest.activeChests == null || TreasureChest.activeChests.Count == 0) return null;

        TreasureChest nearest = null;
        float minDstSqr = float.MaxValue;
        Vector3 playerPos = transform.position;

        for (int i = 0; i < TreasureChest.activeChests.Count; i++)
        {
            TreasureChest chest = TreasureChest.activeChests[i];
            if (chest == null || !chest.gameObject.activeInHierarchy) continue;

            float dstSqr = (chest.transform.position - playerPos).sqrMagnitude;
            if (dstSqr < minDstSqr)
            {
                minDstSqr = dstSqr;
                nearest = chest;
            }
        }

        return nearest;
    }

    private void CreatePointerObject()
    {
        pointerObj = new GameObject("ChestPointerIndicator");
        pointerObj.transform.SetParent(transform);
        pointerObj.transform.localPosition = Vector3.zero;

        pointerRenderer = pointerObj.AddComponent<SpriteRenderer>();
        if (cachedPointerSprite == null)
        {
            cachedPointerSprite = CreateChestPointerSprite();
        }
        pointerRenderer.sprite = cachedPointerSprite;
        pointerRenderer.sortingOrder = 25; // Acima de monstros, chão e player
        pointerRenderer.enabled = false;
    }

    private static Sprite CreateChestPointerSprite()
    {
        int w = 24;
        int h = 24;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color clear = new Color(0, 0, 0, 0);
        Color gold = new Color(1f, 0.85f, 0.15f, 1f);
        Color goldLight = new Color(1f, 0.98f, 0.6f, 1f);
        Color outline = new Color(0.12f, 0.1f, 0.05f, 1f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                tex.SetPixel(x, y, clear);
            }
        }

        int centerY = h / 2;

        // Desenha uma seta / chevron dourada estilizada apontando para a direita (+X)
        for (int x = 4; x <= 20; x++)
        {
            float maxDy = (20 - x) * 0.75f;
            float innerCut = (x < 13) ? (13 - x) * 1.1f : 0f;

            for (int y = 0; y < h; y++)
            {
                float dy = Mathf.Abs(y - centerY);

                if (dy <= maxDy && dy >= innerCut)
                {
                    // Miolo dourado com fio de luz no centro
                    tex.SetPixel(x, y, (dy <= 1.0f) ? goldLight : gold);
                }
                else if (dy <= maxDy + 1.2f && dy >= innerCut - 1.2f)
                {
                    // Contorno preto grosso de alto contraste
                    if (tex.GetPixel(x, y).a == 0)
                    {
                        tex.SetPixel(x, y, outline);
                    }
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
    }
}
