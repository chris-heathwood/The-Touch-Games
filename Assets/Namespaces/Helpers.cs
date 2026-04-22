using System;
using UnityEngine;

namespace Helpers
{
  public class Timing
  {
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    static void Log(string msg) => Debug.Log(msg);

    /// <summary>
    /// Takes some bounds, a point outside the bounds and a point inside the bounds and the time taken and then
    /// estimates the time that the bounds would have been crossed.
    ///
    /// This is because on devices we are stuck at 30fps (or near to it so delta will always be 0.033...) so this gives
    /// us a more accurate final delta.
    /// </summary>
    public static float CalculateFinalDelta(Bounds bounds, Vector2 outside, Vector2 inside, float delta)
    {
      // Calculate the total distance traveled (using Pythagorean theorem)
      double totalDistance = Math.Sqrt(Math.Pow(outside.x - inside.x, 2) + Math.Pow(outside.y - inside.y, 2));

      Log("Total distance: " + totalDistance);

      // Speed = Distance / Time
      double speed = totalDistance / delta;

      Vector2 direction = inside - outside;
      Ray ray = new(outside, direction);

      Log("Outside point: " + outside);
      Log("Inside point: " + inside);
      Log("Direction: " + direction);

      if (bounds.IntersectRay(ray, out float distance))
      {
          Log("Collide Distance: " + distance);

          // Time = Distance / Speed
          double time = distance / speed;

          Log("Time to collision: " + time);

          return (float)time;
      }

      // The ray does not intersect the bounds
      Log("No intersection, so just return delta");

      return delta;
    }
  }
}
