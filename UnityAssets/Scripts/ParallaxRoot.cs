using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxRoot : MonoBehaviour
{
    [Header("Camera (si és null, farà servir Camera.main)")]
    public Camera cam;

    [Header("Nom de la capa de cel que ha de quedar fixa")]
    public string skyName = "SKY";

    [Header("Parallax Config")]
    [Tooltip("BACK factors (0..1) — valor ALT a capes MÉS LLUNYANES (mouen poc), valor MÉS BAIX a capes prop del midground.")]
    public float backParallaxMin = 0.1f; // per capes més properes al midground
    public float backParallaxMax = 0.8f; // per capes més llunyanes (gairebé càmera)

    [Tooltip("FORE intensitat (0..1). El factor final és 1 + abs, i es desplaça en SENTIT CONTRARI a la càmera.")]
    public float foreParallaxAbsMin = 0.40f;   // Fore_0 → factor efectiu = -(0.40)
    public float foreParallaxAbsMax = 0.50f;   // Fore_N → factor efectiu = -(0.50)

    private const string PrefixBack = "Back_";
    private const string PrefixFore = "Fore_";

    [System.Serializable]
    private class LayerTilingData
    {
        // duplicació horitzontal
        public Transform layer;      // tile central
        public Transform left;       // duplicat esquerra (local -offset)
        public Transform right;      // duplicat dreta (local +offset)

        // orígens per a deltes (X i Y)
        public float x0_cam;
        public float y0_cam;
        public float x0_layer;
        public float y0_layer;

        // mesures
        public float widthWorld;     // amplada efectiva del sprite en món (inclou escales)
        public float localOffsetX;   // desplaçament local per col·locar _L/_R (widthWorld / lossyScale.x)

        // parallax (valor base: 0..1 per Back/Mid, >1 per Fore)
        public float parallaxFactor;
    }

    private readonly List<LayerTilingData> layers = new();

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) Debug.LogWarning("[ParallaxRoot] No s'ha trobat cap càmera!");
    }

    void Start()
    {
        int maxBackIdx = DetectMaxIndex(PrefixBack);
        int maxForeIdx = DetectMaxIndex(PrefixFore);

        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var layer = transform.GetChild(i);

            // --- SKY: ordre i factor 0 (fix) ---
            if (layer.name == skyName)
            {
                SetSortingOrderRecursive(layer, -100);
                layers.Add(new LayerTilingData
                {
                    layer = layer,
                    left = null,
                    right = null,
                    x0_cam = cam ? cam.transform.position.x : 0f,
                    y0_cam = cam ? cam.transform.position.y : 0f,
                    x0_layer = layer.position.x,
                    y0_layer = layer.position.y,
                    widthWorld = 0f,
                    localOffsetX = 0f,
                    parallaxFactor = 0f
                });
                continue;
            }

            var sr = layer.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                Debug.LogWarning($"[ParallaxRoot] El fill '{layer.name}' no té SpriteRenderer. S'omet.");
                continue;
            }

            // --- Assignació d'ordre segons prefix ---
            int bidx, fidx;
            if (TryParseIndexedName(layer.name, PrefixBack, out bidx))
            {
                SetSortingOrderRecursive(layer, -99 + bidx);
            }
            else if (TryParseIndexedName(layer.name, PrefixFore, out fidx))
            {
                SetSortingOrderRecursive(layer, 100 + fidx);
            }

            // --- Preparar duplicats _L/_R amb offset LOCAL ---
            Transform left = layer.Find(layer.name + "_L");
            Transform right = layer.Find(layer.name + "_R");

            float worldWidth = Mathf.Abs(sr.bounds.size.x); // en món (inclou escales/ancestres)
            float sx = Mathf.Abs(layer.lossyScale.x);
            if (sx < 1e-6f) sx = 1e-6f;
            float localOffset = worldWidth / sx; // perquè en món la separació sigui = worldWidth

            if (left == null)  left  = CreateClone(layer, -localOffset, "_L", sr);
            if (right == null) right = CreateClone(layer, +localOffset, "_R", sr);

            // --- Factor de parallax per capa ---
            float factor;
            if (TryParseIndexedName(layer.name, PrefixBack, out bidx))
            {
                // Back_0 (molt llunya) → factor alt (proper a 1, mou poc relatiu)
                float t = (maxBackIdx <= 0) ? 0f : Mathf.Clamp01((float)bidx / (float)maxBackIdx);
                factor = Mathf.Lerp(backParallaxMax, backParallaxMin, t); // 0→max, 1→min
                factor = Mathf.Clamp01(factor);                          // 0..1
            }
            else if (TryParseIndexedName(layer.name, PrefixFore, out fidx))
            {
                // Fore_0..N → factor base > 1 (després s'invertirà el sentit a LateUpdate)
                float t = (maxForeIdx <= 0) ? 0f : Mathf.Clamp01((float)fidx / (float)maxForeIdx);
                float abs = Mathf.Lerp(foreParallaxAbsMin, foreParallaxAbsMax, t); // 0.40..0.50 (per ex.)
                factor = 1f + Mathf.Max(0f, abs); // 1.40..1.50 (base)
            }
            else
            {
                // Sense prefix → midground (1.0)
                factor = 1f;
            }

            // --- Desa dades d'aquesta capa ---
            layers.Add(new LayerTilingData
            {
                layer = layer,
                left = left,
                right = right,
                x0_cam = cam ? cam.transform.position.x : 0f,
                y0_cam = cam ? cam.transform.position.y : 0f,
                x0_layer = layer.position.x,
                y0_layer = layer.position.y,
                widthWorld = worldWidth,
                localOffsetX = localOffset,
                parallaxFactor = factor
            });
        }
    }

    void LateUpdate()
    {
        if (cam == null) return;

        float cx = cam.transform.position.x;
        float cy = cam.transform.position.y;

        foreach (var d in layers)
        {
            if (d.parallaxFactor == 0f)
            {
                // SKY: enganxat del tot a la càmera (X i Y)
                Vector3 p = d.layer.position;
                d.layer.position = new Vector3(cx, cy, p.z);
                continue;
            }

            if (d.widthWorld < 1e-6f) continue;

            // --------- FACTOR EFECTIU (sentit + magnitud) ---------
            // Back/Mid: 0..1 → mateix sentit (mou menys que la càmera)
            // Fore: >1   → sentit contrari amb magnitud (factor - 1)
            float eff;
            if (d.parallaxFactor <= 1f)
                eff = d.parallaxFactor;                  // 0..1 (mateix sentit)
            else
                eff = -(d.parallaxFactor - 1f);          // -(0..∞) (sentit contrari)

            // --------- HORITZONTAL (parallax + wrap ancorat a CÀMERA) ---------
            float dx = (cx - d.x0_cam) * eff;            // desplaçament desenvolupat del layer (X)
            float W  = d.widthWorld;

            float xUnwrapped = d.x0_layer + dx;          // centre sense wrap
            int   n         = Mathf.RoundToInt((cx - xUnwrapped) / W); // múltiple més proper al centre de càmera
            float xCentral  = xUnwrapped + n * W;        // centre del tile central

            // --------- VERTICAL (parallax sense replicar) ---------
            float dy      = (cy - d.y0_cam) * eff;       // desplaçament desenvolupat del layer (Y)
            float yTarget = d.y0_layer + dy;

            // Mou NOMÉS el tile central; _L/_R romanen a ±offset local
            var pos = d.layer.position;
            d.layer.position = new Vector3(xCentral, yTarget, pos.z);
        }
    }

    // ===== Helpers =====

    private int DetectMaxIndex(string prefix)
    {
        int maxIdx = -1;
        var rx = new Regex("^" + Regex.Escape(prefix) + @"(\d+)$");
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var layer = transform.GetChild(i);
            var m = rx.Match(layer.name);
            if (m.Success && int.TryParse(m.Groups[1].Value, out int idx))
                maxIdx = Mathf.Max(maxIdx, idx);
        }
        return maxIdx;
    }

    private bool TryParseIndexedName(string name, string prefix, out int index)
    {
        index = 0;
        if (!name.StartsWith(prefix)) return false;
        var tail = name.Substring(prefix.Length);
        return int.TryParse(tail, out index);
    }

    private void SetSortingOrderRecursive(Transform root, int order)
    {
        var srs = root.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        foreach (var s in srs) s.sortingOrder = order;
    }

    private Transform CreateClone(Transform parentLayer, float offsetLocalX, string suffix, SpriteRenderer srcSr)
    {
        string cloneName = parentLayer.name + suffix;
        GameObject go = new GameObject(cloneName);
        go.transform.SetParent(parentLayer, worldPositionStays: false);

        // Posició LOCAL fixa (no es tocarà més)
        go.transform.localPosition = new Vector3(offsetLocalX, 0f, 0f);
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one; // hereta escala del pare

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = srcSr.sprite;
        sr.sharedMaterial = srcSr.sharedMaterial;
        sr.color = srcSr.color;
        sr.flipX = srcSr.flipX;
        sr.flipY = srcSr.flipY;
        sr.drawMode = srcSr.drawMode;
        sr.sortingLayerID = srcSr.sortingLayerID;
        sr.sortingOrder = srcSr.sortingOrder;
        sr.maskInteraction = srcSr.maskInteraction;

        return go.transform;
    }
}