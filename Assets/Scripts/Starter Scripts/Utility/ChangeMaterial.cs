using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMaterial : MonoBehaviour
{
    // To use:
    // Attach to an object you want to change the material on.
    // When the object collides with a trigger, the material will switch to alternateMaterial.
    // When the object leaves the trigger, its material will go back to originalMaterial.

    [Tooltip("Material to switch to.")]
    public Material alternateMaterial;

    [Tooltip("The tag on the trigger collider.")]
    public string tagName;

    [Tooltip("Drag in the sprite whose material will be changed.")]
    public SpriteRenderer sprite;

    private Material originalMaterial;

    private void Start()
    {
        originalMaterial = sprite.material;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(tagName))
        {
            sprite.material = alternateMaterial;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(tagName))
        {
            sprite.material = originalMaterial;
        }
    }
}
