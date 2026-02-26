using UnityEngine;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEngine.XR.Management;
using UnityEngine.XR.ARFoundation;
using System.Reflection;

namespace Editor
{
    [InitializeOnLoad]
    public class EnableSimulationLoader
    {
        static EnableSimulationLoader() 
        {
            EditorApplication.delayCall += EnableLoader;
        }

        static void EnableLoader()
        {
            // Only run if we are in the Editor and not playing
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            BuildTargetGroup buildTargetGroup = BuildTargetGroup.Standalone;
            
            // XRGeneralSettingsForBuildTarget returns UnityEngine.XR.Management.XRGeneralSettings
            XRGeneralSettings buildTargetSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(buildTargetGroup);
            
            if (buildTargetSettings == null)
            {
                Debug.LogWarning("Could not find XR General Settings for Standalone.");
                return;
            }

            XRManagerSettings managerSettings = buildTargetSettings.Manager;
            if (managerSettings == null)
            {
                Debug.LogWarning("Could not find XR Manager Settings.");
                return;
            }
            
            // Check if Simulation Loader is already active
            // We use reflection to find the type "Unity.XR.Simulation.SimulationLoader"
            // This avoids compilation errors if package is missing or not fully imported yet
            string loaderTypeString = "Unity.XR.Simulation.SimulationLoader";
            bool isLoaded = false;
            
            foreach (var loader in managerSettings.activeLoaders)
            {
                if (loader != null && loader.GetType().FullName == loaderTypeString)
                {
                    isLoaded = true;
                    break;
                }
            }

            if (!isLoaded)
            {
                Debug.Log($"[{nameof(EnableSimulationLoader)}] XR Simulation Loader not active. Attempting to enable...");
                
                // Find the type in assemblies
                System.Type simulationLoaderType = null;
                foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    simulationLoaderType = assembly.GetType(loaderTypeString);
                    if (simulationLoaderType != null)
                        break;
                }

                if (simulationLoaderType != null)
                {
                    // Create instance of XRLoader
                    XRLoader loaderInstance = ScriptableObject.CreateInstance(simulationLoaderType) as XRLoader;
                    if (loaderInstance != null)
                    {
                        if (managerSettings.TryAddLoader(loaderInstance))
                        {
                            Debug.Log($"[{nameof(EnableSimulationLoader)}] Successfully enabled {loaderTypeString}!");
                            EditorUtility.SetDirty(managerSettings);
                            AssetDatabase.SaveAssets();
                        }
                        else
                        {
                             Debug.LogError($"[{nameof(EnableSimulationLoader)}] Failed to add loader instance.");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[{nameof(EnableSimulationLoader)}] Could not find type {loaderTypeString}. Ensure com.unity.xr.simulation is installed.");
                }
            }
        }
    }
}
