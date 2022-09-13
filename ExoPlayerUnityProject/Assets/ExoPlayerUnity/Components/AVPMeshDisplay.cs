using UnityEngine;

public class AVPMeshDisplay : MonoBehaviour
{
    [SerializeField] Material material;
    [SerializeField] Renderer[] meshDisplays;


    private void Start()
    {
        foreach (var renderer in meshDisplays)
        {
            renderer.material = material;
        }
    }
}
