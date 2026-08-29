using UnityEngine;

/// <summary>
/// Infinite parallax scrolling background, similar to endless-runner games
/// (e.g. "Mush Dash" style). Attach this to EACH background layer
/// (sky, far mountains, near ground, etc.) separately, with a different
/// scrollSpeed per layer — slower for far-away layers, faster for close ones.
///
/// SETUP (single image, easiest way):
/// 1. Put your background sprite on a GameObject with a SpriteRenderer.
/// 2. Add this script to that SAME GameObject.
/// 3. Leave "pieces" EMPTY in the Inspector — the script will automatically
///    clone this GameObject at Start() to create enough copies to loop
///    seamlessly. You only need ONE image in the scene.
///
/// (Optional, manual way): if you already prepared 2-3 copies yourself,
/// drag them into "pieces" and the script will use those instead of
/// auto-cloning.
///
/// LOOPING: only "autoCopyCount" objects ever exist — they are recycled
/// (repositioned) forever rather than creating new copies each time, so
/// this scrolls infinitely without ever spawning extra objects or leaking
/// memory. Recycling is measured against the Camera's left edge (not this
/// object's own position), so it stays accurate no matter how long the
/// game runs.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Header("Scroll Settings")]
    [Tooltip("How fast this layer scrolls left. Use smaller values for background (far) layers, larger for foreground (near) layers.")]
    [SerializeField] private float scrollSpeed = 2f;

    [Header("Tiling Pieces (leave empty to auto-generate from this GameObject's own sprite)")]
    [Tooltip("Optional. If left empty, the script clones this GameObject automatically using its own SpriteRenderer.")]
    [SerializeField] private Transform[] pieces;

    [Tooltip("Only used for auto-generation: how many total copies to create (including the original). 2 is usually enough, use 3 if the camera moves very fast or the image is narrow.")]
    [SerializeField] private int autoCopyCount = 2;

    [Header("Recycle Reference")]
    [Tooltip("Camera used to detect when a piece has scrolled fully off-screen. Defaults to Camera.main if left empty.")]
    [SerializeField] private Camera referenceCamera;

    [Tooltip("Extra margin (world units) added past the camera's left edge before a piece is recycled, so it never visibly pops.")]
    [SerializeField] private float recycleMargin = 1f;

    private float pieceWidth;
    private float leftBound; // camera left edge, cached each frame

    private void Start()
    {
        if (referenceCamera == null)
        {
            referenceCamera = Camera.main;
        }

        if (pieces == null || pieces.Length == 0)
        {
            AutoGeneratePieces();
        }
        else
        {
            SpriteRenderer sr = pieces[0].GetComponent<SpriteRenderer>();
            pieceWidth = sr != null ? sr.bounds.size.x : 10f;
        }
    }

    private void AutoGeneratePieces()
    {
        SpriteRenderer ownSprite = GetComponent<SpriteRenderer>();
        if (ownSprite == null)
        {
            Debug.LogWarning($"[ParallaxBackground] No SpriteRenderer found on {name}. Assign one, or fill the 'pieces' array manually.");
            return;
        }

        pieceWidth = ownSprite.bounds.size.x;

        int copyCount = Mathf.Max(2, autoCopyCount);
        pieces = new Transform[copyCount];
        pieces[0] = transform; // the original counts as piece 0

        for (int i = 1; i < copyCount; i++)
        {
            GameObject clone = Instantiate(gameObject, transform.parent);
            clone.name = $"{name}_clone{i}";

            // Remove the cloned ParallaxBackground component so only
            // the original one drives movement for the whole group.
            ParallaxBackground cloneScript = clone.GetComponent<ParallaxBackground>();
            if (cloneScript != null) Destroy(cloneScript);

            clone.transform.position = transform.position + new Vector3(pieceWidth * i, 0f, 0f);
            pieces[i] = clone.transform;
        }
    }

    private void Update()
    {
        if (pieces == null || pieces.Length == 0) return;

        UpdateLeftBound();

        // Move every piece to the left.
        foreach (Transform piece in pieces)
        {
            piece.position += Vector3.left * scrollSpeed * Time.deltaTime;
        }

        // Recycle: once a piece is fully past the camera's left edge,
        // send it to the right end of the chain. This repeats forever —
        // no new objects are ever created after Start().
        foreach (Transform piece in pieces)
        {
            if (piece.position.x + pieceWidth * 0.5f < leftBound - recycleMargin)
            {
                float rightmostX = GetRightmostX();
                piece.position = new Vector3(rightmostX + pieceWidth, piece.position.y, piece.position.z);
            }
        }
    }

    private void UpdateLeftBound()
    {
        if (referenceCamera == null)
        {
            // Fallback: no camera assigned/found, just use world x = 0 as a rough edge.
            leftBound = 0f;
            return;
        }

        // Left edge of the camera's view in world space.
        leftBound = referenceCamera.ViewportToWorldPoint(new Vector3(0f, 0.5f, referenceCamera.nearClipPlane)).x;
    }

    private float GetRightmostX()
    {
        float maxX = float.MinValue;
        foreach (Transform piece in pieces)
        {
            if (piece.position.x > maxX)
            {
                maxX = piece.position.x;
            }
        }
        return maxX;
    }
}
