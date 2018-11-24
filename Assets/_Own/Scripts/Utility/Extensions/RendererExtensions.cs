using UnityEngine;
using System.Linq;

public static class RendererExtensions
{
    public static Rect GetViewportBounds(this Renderer renderer, Camera camera)
    {
        Vector3 min = renderer.bounds.min;
        Vector3 max = renderer.bounds.max;
        Vector2[] viewportBorders = new []{
            new Vector3(min.x, min.y, min.z),
            new Vector3(min.x, min.y, max.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(max.x, min.y, max.z),
            new Vector3(max.x, max.y, min.z),
            new Vector3(max.x, max.y, max.z)
        }.Select(b => (Vector2)camera.WorldToViewportPoint(b)).ToArray();
		
        Vector2 minViewportPosition = new Vector2(viewportBorders.Min(b => b.x), viewportBorders.Min(b => b.y));
        Vector2 maxViewportPosition = new Vector2(viewportBorders.Max(b => b.x), viewportBorders.Max(b => b.y));
        
        return Rect.MinMaxRect(
            minViewportPosition.x, minViewportPosition.y, 
            maxViewportPosition.x, maxViewportPosition.y
        );
    }
}