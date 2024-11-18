using System.Collections.Generic;
using UnityEngine;

public class OutlineOnHover : MonoBehaviour
{
    [SerializeField]
    private Material hoverMaterialInRange;

    [SerializeField]
    private Material hoverMaterialOutOfRange;

    [SerializeField]
    private float interactionRange = 3f;

    private Renderer currentRenderer;
    private Renderer previousRenderer;

    private InteractableObject currentInteractable;
    private InteractableObject previousInteractable;

    private GameObject player;

    void Start()
    {
        if (hoverMaterialInRange == null || hoverMaterialOutOfRange == null)
        {
            Debug.LogWarning("Hover materials not assigned.");
            return;
        }

        player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found. Ensure the player has the 'Player' tag.");
        }
    }

    void Update()
    {
        if (hoverMaterialInRange == null || hoverMaterialOutOfRange == null || player == null) return;

        // Cast a ray from the mouse position into the scene
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                currentRenderer = hit.collider.GetComponent<Renderer>();
                currentInteractable = hit.collider.GetComponent<InteractableObject>();

                // Check proximity
                bool isInRange = Vector3.Distance(player.transform.position, hit.collider.transform.position) <= interactionRange;

                ResetMaterial();
                ApplyHoverMaterial(currentRenderer, isInRange);

                if (currentInteractable != null)
                {
                    currentInteractable.isHovered = true;  // Set the isHovered property
                }

                previousRenderer = currentRenderer;
                previousInteractable = currentInteractable;
            }
            else
            {
                ResetMaterial();
            }
        }
        else
        {
            ResetMaterial();
            previousRenderer = null;
        }
    }

    private void ResetMaterial()
    {
        if (previousRenderer != null)
        {
            List<Material> newMaterials = new List<Material>();

            foreach (var mat in previousRenderer.materials)
            {
                if (!AreMaterialsEqual(mat, hoverMaterialInRange) && !AreMaterialsEqual(mat, hoverMaterialOutOfRange))
                {
                    newMaterials.Add(mat);
                }
            }

            previousRenderer.materials = newMaterials.ToArray();
        }

        if (previousInteractable != null)
        {
            previousInteractable.isHovered = false;  // Reset isHovered when not hovering anymore
        }
    }

    private void ApplyHoverMaterial(Renderer renderer, bool isInRange)
    {
        Material hoverMaterial = isInRange ? hoverMaterialInRange : hoverMaterialOutOfRange;

        if (!IsHoverMaterialApplied(renderer, hoverMaterial))
        {
            List<Material> materials = new List<Material>(renderer.materials);
            materials.Add(hoverMaterial);
            renderer.materials = materials.ToArray();
        }
    }

    private bool AreMaterialsEqual(Material mat1, Material mat2)
    {
        return mat1.shader == mat2.shader && mat1.color == mat2.color;
    }

    private bool IsHoverMaterialApplied(Renderer renderer, Material hoverMaterial)
    {
        foreach (Material mat in renderer.materials)
        {
            if (AreMaterialsEqual(mat, hoverMaterial))
            {
                return true;
            }
        }
        return false;
    }
}
