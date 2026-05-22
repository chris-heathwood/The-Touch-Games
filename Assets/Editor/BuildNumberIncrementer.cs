using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildNumberIncrementer : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.iOS) return;

        string current = PlayerSettings.iOS.buildNumber;
        if (int.TryParse(current, out int number))
        {
            string next = (number + 1).ToString();
            PlayerSettings.iOS.buildNumber = next;
            AssetDatabase.SaveAssets();
            Debug.Log($"Build number incremented: {current} → {next}");
        }
        else
        {
            Debug.LogWarning($"BuildNumberIncrementer: could not parse build number '{current}', skipping.");
        }
    }
}
