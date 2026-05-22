using UnityEngine;
using UnityEngine.SocialPlatforms;

public static class GameCenter
{
    public const string SwipeSprint   = "com.thetouchgames.swiper.sprint";
    public const string SwipeMiddle   = "com.thetouchgames.swiper.middle";
    public const string SwipeMarathon = "com.thetouchgames.swiper.marathon";
    public const string Tapper        = "com.thetouchgames.tapper";
    public const string Beeper        = "com.thetouchgames.beeper";
    public const string Rotator       = "com.thetouchgames.rotator";
    public const string Timer         = "com.thetouchgames.timer";
    public const string Balancer      = "com.thetouchgames.balancer";
    public const string Tracer        = "com.thetouchgames.tracer";

    private static bool authenticated = false;

    public static void Authenticate()
    {
        if (authenticated) return;

        Social.localUser.Authenticate(success =>
        {
            authenticated = success;
            if (!success)
                Debug.Log("GameCenter: authentication failed or unavailable.");
        });
    }

    // Submit a raw score. For time-based games (lower = better), pass the
    // inverted value: ReportScore(999999 - timeInMs, leaderboardId)
    public static void ReportScore(long score, string leaderboardId)
    {
        if (!authenticated) return;
        Social.ReportScore(score, leaderboardId, success =>
        {
            if (!success)
                Debug.LogWarning($"GameCenter: failed to report score to {leaderboardId}");
        });
    }

    public static void ShowLeaderboard(string leaderboardId)
    {
        Social.ShowLeaderboardUI();
    }
}
