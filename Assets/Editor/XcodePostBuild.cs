using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public class XcodePostBuild
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string buildPath)
    {
        if (target != BuildTarget.iOS) return;

        string teamId = ReadTeamId();
        if (string.IsNullOrEmpty(teamId))
        {
            UnityEngine.Debug.LogWarning("XcodePostBuild: TEAM_ID not found in .env — signing not applied.");
            return;
        }

        string projPath = PBXProject.GetPBXProjectPath(buildPath);
        PBXProject proj = new PBXProject();
        proj.ReadFromFile(projPath);

        string mainTarget = proj.GetUnityMainTargetGuid();
        string frameworkTarget = proj.GetUnityFrameworkTargetGuid();

        proj.SetTeamId(mainTarget, teamId);
        proj.SetTeamId(frameworkTarget, teamId);

        proj.SetBuildProperty(mainTarget, "CODE_SIGN_STYLE", "Automatic");
        proj.SetBuildProperty(frameworkTarget, "CODE_SIGN_STYLE", "Automatic");

        proj.WriteToFile(projPath);
    }

    static string ReadTeamId()
    {
        string envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
        if (!File.Exists(envPath)) return null;

        foreach (string line in File.ReadAllLines(envPath))
        {
            if (line.StartsWith("TEAM_ID="))
                return line["TEAM_ID=".Length..].Trim();
        }
        return null;
    }
}
