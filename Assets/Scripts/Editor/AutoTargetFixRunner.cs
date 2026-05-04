using UnityEngine;
using UnityEditor;

namespace TowerDefence.Editor
{
    [InitializeOnLoad]
    public class AutoTargetFixRunner
    {
        static AutoTargetFixRunner()
        {
            EditorApplication.delayCall += RunFix;
        }

        static void RunFix()
        {
            if (SessionState.GetBool("AutoTargetFixDone", false)) return;
            SessionState.SetBool("AutoTargetFixDone", true);

            Debug.Log(">>> UPDATING TOWER UPGRADE UI PREFAB <<<");
            TowerDefence.Editor.UIMasterPrefabCreator.CreateTowerUpgradeUIPrefab();
            Debug.Log(">>> TOWER UPGRADE UI UPDATED WITH TARGETING BUTTONS! <<<");
        }
    }
}
