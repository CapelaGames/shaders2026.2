using UnityEngine;

[ExecuteAlways]
public class HealthBar : MonoBehaviour
{
    public float _Health;

    void Update()
    {
        MaterialPropertyBlock props = new MaterialPropertyBlock();
        if (props.GetFloat("_Health") != _Health)
        {
            MeshRenderer renderer;

            props.SetFloat("_Health", _Health);

            renderer = GetComponent<MeshRenderer>();
            renderer.SetPropertyBlock(props);
        }
    }
}
