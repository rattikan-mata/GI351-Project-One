using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Collider2D))]
public class MonsterController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [Tooltip("If the monster's x position goes below this value, it is destroyed (assumed off-screen).")]
    [SerializeField] private float destroyXThreshold = -12f;

    [Header("Player Collision")]
    [Tooltip("Tag used on the Player GameObject.")]
    [TagSelector]
    [SerializeField] private string playerTag = "Player";

    private void Reset()
    {
        // Make sure the collider is set up as a trigger by default,
        // since PlayerController uses Physics2D.OverlapCircle (non-trigger check works fine too,
        // but we use OnTriggerEnter2D here for the player-hits-monster case).
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Update()
    {
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