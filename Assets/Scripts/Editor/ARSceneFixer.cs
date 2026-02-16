using UnityEngine;
using UnityEditor;
using Unity.XR.CoreUtils;
using UnityEngine.XR.ARFoundation;

namespace FaceFilter.Editor
{
    [InitializeOnLoad]
    public class ARSceneFixer
    {
        static ARSceneFixer()
        {
            EditorApplication.delayCall += CheckAndFixScene;
        }

        [MenuItem("AR/Fix Scene Hierarchy")]
        public static void CheckAndFixScene()
        {
            Debug.Log("[ARSceneFixer] Starting scene validation...");

            // 1. Check ARSession
            var session = Object.FindObjectOfType<ARSession>();
            if (session == null)
            {
                if (EditorUtility.DisplayDialog("AR Fix", "AR Session is missing. Create one?", "create", "skip"))
                {
                    EditorApplication.ExecuteMenuItem("GameObject/XR/AR Session");
                }
            }

            // 2. Check XROrigin / ARSessionOrigin
            var sessionOrigin = Object.FindObjectOfType<ARSessionOrigin>();
            var xrOrigin = Object.FindObjectOfType<XROrigin>();

            if (xrOrigin == null && sessionOrigin != null)
            {
                if (EditorUtility.DisplayDialog("AR Fix", "Deprecated ARSessionOrigin found. Upgrade to XROrigin?", "Upgrade", "Ignore"))
                {
                    UpgradeOrigin(sessionOrigin);
                    xrOrigin = Object.FindObjectOfType<XROrigin>(); // Refresh
                }
            }
            else if (xrOrigin == null)
            {
                if (EditorUtility.DisplayDialog("AR Fix", "No XR Origin found. Create default?", "Create", "Cancel"))
                {
                    EditorApplication.ExecuteMenuItem("GameObject/XR/XR Origin (Mobile AR)");
                    xrOrigin = Object.FindObjectOfType<XROrigin>();
                }
            }

            if (xrOrigin != null)
            {
                ValidateXROriginConfig(xrOrigin);
            }
            
            Debug.Log("[ARSceneFixer] Validation complete.");
        }

        static void UpgradeOrigin(ARSessionOrigin oldOrigin)
        {
            GameObject go = oldOrigin.gameObject;
            var newOrigin = Undo.AddComponent<XROrigin>(go);
            
            if (oldOrigin.camera != null)
            {
                newOrigin.Camera = oldOrigin.camera;
                if (oldOrigin.camera.transform.parent != null)
                    newOrigin.CameraFloorOffsetObject = oldOrigin.camera.transform.parent.gameObject;
            }
            else
            {
                var main = Camera.main;
                if (main != null)
                {
                    newOrigin.Camera = main;
                    if (main.transform.parent != null)
                        newOrigin.CameraFloorOffsetObject = main.transform.parent.gameObject;
                }
            }
            Undo.DestroyObjectImmediate(oldOrigin);
            Debug.Log("[ARSceneFixer] Upgraded ARSessionOrigin -> XROrigin");
        }

        static void ValidateXROriginConfig(XROrigin origin)
        {
            // 1. Fix Camera Reference
            if (origin.Camera == null)
            {
                var main = Camera.main;
                if (main == null)
                {
                    // Fallback: Find ANY camera
                    main = Object.FindObjectOfType<Camera>();
                    if (main != null)
                    {
                         Debug.LogWarning("[ARSceneFixer] 'MainCamera' tag missing. Found a Camera and using it.");
                    }
                }

                if (main != null)
                {
                    origin.Camera = main;
                    EditorUtility.SetDirty(origin);
                    Debug.Log("[ARSceneFixer] Linked Camera to XR Origin");
                }
                else
                {
                    Debug.LogError("[ARSceneFixer] No Camera found in scene! Converting to XR requires a Camera.");
                    // Check if we should create one
                    if (EditorUtility.DisplayDialog("AR Fix", "No Camera found. Create AR Camera?", "Yes", "No"))
                    {
                        var camObj = new GameObject("AR Camera");
                        var cam = camObj.AddComponent<Camera>();
                        camObj.tag = "MainCamera";
                        origin.Camera = cam;
                        main = cam;
                    }
                }
            }
            
            // 2. Fix Camera Floor Offset Object
            if (origin.CameraFloorOffsetObject == null)
            {
                // Unparent usage: default to itself or camera parent
                if (origin.Camera != null && origin.Camera.transform.parent != null)
                {
                     origin.CameraFloorOffsetObject = origin.Camera.transform.parent.gameObject;
                     EditorUtility.SetDirty(origin);
                     Debug.Log("[ARSceneFixer] Linked Camera Floor Offset to Camera Parent");
                }
                else
                {
                     // Fallback if camera causes issues or is root
                     origin.CameraFloorOffsetObject = origin.gameObject;
                     EditorUtility.SetDirty(origin);
                     Debug.Log("[ARSceneFixer] Linked Camera Floor Offset to XR Origin (Fallback)");
                }
            }

            if (origin.Camera != null)
            {
                GameObject camObj = origin.Camera.gameObject;

                // 3. Ensure Camera Background
                if (camObj.GetComponent<ARCameraBackground>() == null)
                {
                    Debug.Log("[ARSceneFixer] ARCameraBackground missing on camera. Adding it.");
                    Undo.AddComponent<ARCameraBackground>(camObj);
                }

                // 4. Ensure Camera Manager
                if (camObj.GetComponent<ARCameraManager>() == null)
                {
                    Debug.Log("[ARSceneFixer] ARCameraManager missing on camera. Adding it.");
                    Undo.AddComponent<ARCameraManager>(camObj);
                }

                // 5. Ensure Tracked Pose Driver
                var driver = camObj.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
                if (driver == null)
                {
                    Debug.Log("[ARSceneFixer] TrackedPoseDriver missing. Adding default.");
                    Undo.AddComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>(camObj);
                }
            }

            // 6. Check Face Manager
            var faceManager = Object.FindObjectOfType<ARFaceManager>();
            if (faceManager != null)
            {
                 if (!faceManager.enabled) faceManager.enabled = true;
            }
            else
            {
                // Verify if user wants face tracking
                if (EditorUtility.DisplayDialog("AR Fix", "AR Face Manager not found. Add to XR Origin?", "Yes", "No"))
                {
                    Undo.AddComponent<ARFaceManager>(origin.gameObject);
                }
            }
        }
    }
}
