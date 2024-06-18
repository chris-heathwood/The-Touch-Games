using System;
using UnityEngine;

namespace Helpers
{
  public class Timing
  {
    /// <summary>
    /// Takes some bounds, a point outside the bounds and a point inside the bounds and the time taken and then
    /// estimates the time that the bounds would have been crossed.
    /// </summary>
    public static float CalculateFinalDelta(Bounds bounds, Vector2 outside, Vector2 inside, float delta)
    {
      // Calculate the total distance traveled (using Pythagorean theorem)
      double totalDistance = Math.Sqrt(Math.Pow(outside.x - inside.x, 2) + Math.Pow(outside.y - inside.y, 2));

      Debug.Log("Total distance: " + totalDistance);

      // Speed = Distance / Time
      double speed = totalDistance / delta;

      Vector2 direction = inside - outside;
      Ray ray = new(outside, direction);

      Debug.Log("Outside point: " + outside);
      Debug.Log("Inside point: " + inside);
      Debug.Log("Direction: " + direction);

      if (bounds.IntersectRay(ray, out float distance))
      {
          Debug.Log("Collide Distance: " + distance);

          // Time = Distance / Speed
          double time = distance / speed;

          Debug.Log("Time to collision: " + time);

          return (float)time;
      }

      // The ray does not intersect the bounds
      Debug.Log("No intersection, so just return delta");

      return delta;
    }
  }
}
