using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Collider2D))]
public class MonsterController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Untick to make this monster stay still (e.g. a stationary/turret-type monster). Tick to make it walk left like normal.")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private float moveSpeed = 3f;
    [Tooltip("If the monster's x position goes below this value, it is destroyed (assumed off-screen). Only applies while it can move.")]
    [SerializeField] private float destroyXThreshold = -12f;

    [Header("Player Collision")]
    [Tooltip("Tag used on the Player GameObject.")]
    [TagSelector]
    [SerializeField] private string playerTag = "Player";

    [Header("Components")]
    [Tooltip("Optional. Used to play animations (e.g. a 'Death' trigger when the monster is destroyed).")]
    [SerializeField] private Animator animator;
    [Tooltip("Optional. Used for things like flipping the sprite or hiding it while a death animation plays.")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Death Animation")]
    [Tooltip("Name of the Animator trigger played when this monster dies. Leave blank if you don't have a death animation.")]
    [SerializeField] private string deathTrigger = "Death";
    [Tooltip("How long to wait after triggering the death animation before actually destroying the GameObject, so the animation has time to play.")]
    [SerializeField] private float deathAnimationDuration = 0.3f;

    private bool isDead = false;

    private void Reset()
    {
        // Make sure the collider is set up as a trigger by default,
        // since PlayerController uses Physics2D.OverlapCircle (non-trigger check works fine too,
        // but we use OnTriggerEnter2D here for the player-hits-monster case).
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (isDead) return;
        if (!canMove) return;

        // Move from right to left along the x-axis.
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        // Clean up if it goes off-screen without being hit.
        if (transform.position.x < destroyXThreshold)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag(playerTag))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage();
            }

            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Call this instead of Destroy(gameObject) whenever the monster is
    /// defeated by the player (e.g. from PlayerController.PerformHit),
    /// so the death animation gets a chance to play before it disappears.
    /// </summary>
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // Stop moving and stop registering further hits/collisions.
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (animator != null && !string.IsNullOrEmpty(deathTrigger))
        {
            animator.SetTrigger(deathTrigger);
            Destroy(gameObject, deathAnimationDuration);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

/// <summary>
/// Shows a Tag dropdown in the Inspector for a string field, exactly like
/// the built-in "Tag" field on a GameObject. Usage: [TagSelector] above a
/// [SerializeField] private string field.
/// Kept in this same file so everything lives in MonsterController.cs.
/// </summary>
public class TagSelectorAttribute : PropertyAttribute
{
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(TagSelectorAttribute))]
public class TagSelectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.String)
        {
            EditorGUI.BeginProperty(position, label, property);

            string currentTag = property.stringValue;
            string newTag = EditorGUI.TagField(position, label, currentTag);

            if (newTag != currentTag)
            {
                property.stringValue = newTag;
            }

            EditorGUI.EndProperty();
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }
}
#endif