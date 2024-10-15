using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineOnHover : MonoBehaviour
{
    [SerializeField]
    private Material hoverMaterial;

    private Renderer currentRenderer;   
    private Renderer previousRenderer;  

    void Start()
    {
        if (hoverMaterial == null)
        {
            Debug.LogWarning("Hover material not assigned.");
            return;
        }
    }

    void Update()
    {
        if (hoverMaterial == null) return;

        // Cast a ray from the mouse position into the scene
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                currentRenderer = hit.collider.GetComponent<Renderer>();

                ResetMaterial();    

                ApplyHoverMaterial(currentRenderer);

                previousRenderer = currentRenderer;
         
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
                if (!AreMaterialsEqual(mat, hoverMaterial))
                {
                    newMaterials.Add(mat);
                }
            }

            previousRenderer.materials = newMaterials.ToArray();
        }
    }

    private void ApplyHoverMaterial(Renderer renderer)
    {
        if (!IsHoverMaterialApplied(renderer))
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

    private bool IsHoverMaterialApplied(Renderer renderer)
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
