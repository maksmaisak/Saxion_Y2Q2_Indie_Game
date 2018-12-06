using UnityEngine;
using System.Linq;
using UnityEngine.Assertions;

public static class RendererExtensions
{
    public static Vector3[] GetBoundsCorners(this Renderer renderer)
    {
        Vector3 min = renderer.bounds.min;
        Vector3 max = renderer.bounds.max;
        return new[]
        {
            new Vector3(min.x, min.y, min.z),
            new Vector3(min.x, min.y, max.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(max.x, min.y, max.z),
            new Vector3(max.x, max.y, min.z),
            new Vector3(max.x, max.y, max.z)
        };
    }
    
    public static Rect GetViewportBounds(this Renderer renderer, Camera camera)
    {
        Vector2[] viewportBorders = renderer
            .GetBoundsCorners()
            .Select(b => (Vector2)camera.WorldToViewportPoint(b))
            .ToArray();
		
        Vector2 minViewportPosition = new Vector2(viewportBorders.Min(b => b.x), viewportBorders.Min(b => b.y));
        Vector2 maxViewportPosition = new Vector2(viewportBorders.Max(b => b.x), viewportBorders.Max(b => b.y));
        
        return Rect.MinMaxRect(
            minViewportPosition.x, minViewportPosition.y, 
            maxViewportPosition.x, maxViewportPosition.y
        );
    }

    public static Rect GetViewportBounds(this Renderer renderer)
    {
        var camera = Camera.main;
        Assert.IsNotNull(camera);
        return renderer.GetViewportBounds(camera);
    }
}