#if ODIN_INSPECTOR
#endif

namespace DaftAppleGames.Lighting
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering.HighDefinition;

    public class OnDemandShadowMapUpdate : MonoBehaviour
    {
        // Update on camera move variables
        [HideInInspector]
        public bool updateOnCameraMove;
        [HideInInspector]
        public Camera cameraToTrack;

        // Enums for the shadow map to refresh and the counter mode
        [HideInInspector]
        public ShadowMapToRefresh shadowMapToRefresh = ShadowMapToRefresh.EntireShadowMap;
        [HideInInspector]
        public CounterMode counterMode = CounterMode.Frames;

        // Unity GameObjects and components
        [HideInInspector]
        public Camera mainCamera;
        private HDAdditionalLightData _hdLight;

        // Full shadow map refresh counter variables
        [HideInInspector]
        public int fullShadowMapRefreshWaitFrames;
        [HideInInspector]
        public float fullShadowMapRefreshWaitSeconds;
        private int _framesCounterFullShadowMap;
        private float _secondsCounterFullShadowMap;

        // Cascades refresh counter variables
        [HideInInspector]
        public int[] cascadesRefreshWaitFrames = new int[NumberOfCascades];
        [HideInInspector]
        public float[] cascadesRefreshWaitSeconds = new float[NumberOfCascades];
        private int[] _framesCounterCascades = new int[NumberOfCascades];
        private float[] _secondsCounterCascades = new float[NumberOfCascades];
        private const int NumberOfCascades = 4;

        // Subshadows refresh counter variables
        [HideInInspector]
        public int[] subshadowsRefreshWaitFrames = new int[NumberOfSubshadows];
        [HideInInspector]
        public float[] subshadowsRefreshWaitSeconds = new float[NumberOfSubshadows];
        private int[] _framesCounterSubshadows = new int[NumberOfSubshadows];
        private float[] _secondsCounterSubshadows = new float[NumberOfSubshadows];
        private const int NumberOfSubshadows = 6;

        private void Start()
        {
            ResetCounters();

            // Null check for the HDAdditionalLightData component
            _hdLight = GetComponent<HDAdditionalLightData>();
            if (_hdLight == null)
            {
                Debug.LogError("This script requires an HDAdditionalLightData component to work!");
                return;
            }

            // Cache the main camera and do a null check
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("There's no Camera GameObject with the MainCamera tag in the scene!");
                return;
            }
        }

        private void Update()
        {
            // Check which shadow map to refresh mode is selected
            switch (shadowMapToRefresh)
            {
                case ShadowMapToRefresh.EntireShadowMap:
                    UpdateShadowMap();
                    break;
                case ShadowMapToRefresh.Cascades:
                    UpdateCascades();
                    break;
                case ShadowMapToRefresh.Subshadows:
                    UpdateSubshadows();
                    break;
            }

            // Call this method if Update On Camera Movement is enabled
            if (updateOnCameraMove)
            {
                UpdateOnCameraMove();
            }
        }

        private void ResetCounters()
        {
            // Initialize the refresh wait frames counters for shadow maps
            _framesCounterFullShadowMap = 0;
            _secondsCounterFullShadowMap = 0;

            // Initialize the refresh wait frames counters for shadow cascades
            for (int i = 0; i < NumberOfCascades; i++)
            {
                _framesCounterCascades[i] = 0;
                _secondsCounterCascades[i] = 0;
            }

            // Initialize the refresh wait frames counters for subshadows
            for (int i = 0; i < NumberOfSubshadows; i++)
            {
                _framesCounterSubshadows[i] = 0;
                _secondsCounterSubshadows[i] = 0;
            }
        }

        private void UpdateShadowMap()
        {
            // Check which counter mode (frames or seconds) is selected
            switch (counterMode)
            {
                case CounterMode.Frames:
                    _framesCounterFullShadowMap++;
                    if (fullShadowMapRefreshWaitFrames > 0 && _framesCounterFullShadowMap >= fullShadowMapRefreshWaitFrames)
                    {
                        _hdLight.RequestShadowMapRendering();
                        _framesCounterFullShadowMap = 0;
                    }
                    break;
                case CounterMode.Seconds:
                    _secondsCounterFullShadowMap += Time.deltaTime;
                    if (fullShadowMapRefreshWaitSeconds > 0 && _secondsCounterFullShadowMap >= fullShadowMapRefreshWaitSeconds)
                    {
                        _hdLight.RequestShadowMapRendering();
                        _secondsCounterFullShadowMap = 0;
                    }
                    break;
            }
        }

        private void UpdateCascades()
        {
            // Check which counter mode (frames or seconds) is selected
            switch (counterMode)
            {
                case CounterMode.Frames:
                    for (int i = 0; i < _framesCounterCascades.Length; i++)
                    {
                        _framesCounterCascades[i]++;

                        if (cascadesRefreshWaitFrames[i] > 0 && _framesCounterCascades[i] >= cascadesRefreshWaitFrames[i])
                        {
                            _hdLight.RequestSubShadowMapRendering(i);
                            _framesCounterCascades[i] = 0;
                        }
                    }
                    break;
                case CounterMode.Seconds:
                    for (int i = 0; i < _secondsCounterCascades.Length; i++)
                    {
                        _secondsCounterCascades[i] += Time.deltaTime;

                        if (cascadesRefreshWaitSeconds[i] > 0 && _secondsCounterCascades[i] >= cascadesRefreshWaitSeconds[i])
                        {
                            _hdLight.RequestSubShadowMapRendering(i);
                            _secondsCounterCascades[i] = 0;
                        }
                    }
                    break;
            }
        }

        private void UpdateSubshadows()
        {
            // Check which counter mode (frames or seconds) is selected
            switch (counterMode)
            {
                case CounterMode.Frames:
                    for (int i = 0; i < _framesCounterSubshadows.Length; i++)
                    {
                        _framesCounterSubshadows[i]++;

                        if (subshadowsRefreshWaitFrames[i] > 0 && _framesCounterSubshadows[i] >= subshadowsRefreshWaitFrames[i])
                        {
                            _hdLight.RequestSubShadowMapRendering(i);
                            _framesCounterSubshadows[i] = 0;
                        }
                    }
                    break;
                case CounterMode.Seconds:
                    for (int i = 0; i < _secondsCounterSubshadows.Length; i++)
                    {
                        _secondsCounterSubshadows[i] += Time.deltaTime;

                        if (subshadowsRefreshWaitSeconds[i] > 0 && _secondsCounterSubshadows[i] >= subshadowsRefreshWaitSeconds[i])
                        {
                            _hdLight.RequestSubShadowMapRendering(i);
                            _secondsCounterSubshadows[i] = 0;
                        }
                    }
                    break;
            }
        }

        private void UpdateOnCameraMove()
        {
            if (cameraToTrack != null)
            {
                if (cameraToTrack.transform.hasChanged)
                {
                    _hdLight.RequestShadowMapRendering();
                    cameraToTrack.transform.hasChanged = false;
                }
            }
        }

        [CustomEditor(typeof(OnDemandShadowMapUpdate))]
        public class ShadowMapRefreshEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                OnDemandShadowMapUpdate myScript = (OnDemandShadowMapUpdate)target;

                // Update on camera move UI toggle
                myScript.updateOnCameraMove = EditorGUILayout.Toggle("Update On Camera Movement", myScript.updateOnCameraMove);

                if (myScript.updateOnCameraMove)
                {
                    DrawCameraField("Camera To Track", ref myScript.cameraToTrack);

                    // If the camera to track is null, display a help box and a assign the MainCamera automatically
                    if (myScript.cameraToTrack == null)
                    {
                        EditorGUILayout.HelpBox("The MainCamera GameObject will be automatically assigned if none is specified.", MessageType.Info);
                        myScript.cameraToTrack = myScript.mainCamera;
                    }
                }

                // Counter mode UI
                myScript.counterMode = (CounterMode)EditorGUILayout.EnumPopup("Counter Mode", myScript.counterMode);

                // Shadow map to refresh UI
                myScript.shadowMapToRefresh = (ShadowMapToRefresh)EditorGUILayout.EnumPopup("Shadow Map To Update", myScript.shadowMapToRefresh);

                // Draw the appropriate UI based on the shadow map to refresh mode
                switch (myScript.shadowMapToRefresh)
                {
                    case ShadowMapToRefresh.EntireShadowMap:
                        if (myScript.counterMode == CounterMode.Frames)
                        {
                            DrawShadowMapGUIIntField("Update Interval", ref myScript.fullShadowMapRefreshWaitFrames);
                        }
                        else if (myScript.counterMode == CounterMode.Seconds)
                        {
                            DrawShadowMapGUIFloatField("Update Interval", ref myScript.fullShadowMapRefreshWaitSeconds);
                        }
                        break;
                    case ShadowMapToRefresh.Cascades:
                        if (myScript.counterMode == CounterMode.Frames)
                        {
                            DrawShadowMapGUIIntField("Cascade 1 Update Interval", ref myScript.cascadesRefreshWaitFrames[0]);
                            DrawShadowMapGUIIntField("Cascade 2 Update Interval", ref myScript.cascadesRefreshWaitFrames[1]);
                            DrawShadowMapGUIIntField("Cascade 3 Update Interval", ref myScript.cascadesRefreshWaitFrames[2]);
                            DrawShadowMapGUIIntField("Cascade 4 Update Interval", ref myScript.cascadesRefreshWaitFrames[3]);
                        }
                        else if (myScript.counterMode == CounterMode.Seconds)
                        {
                            DrawShadowMapGUIFloatField("Cascade 1 Update Interval", ref myScript.cascadesRefreshWaitSeconds[0]);
                            DrawShadowMapGUIFloatField("Cascade 2 Update Interval", ref myScript.cascadesRefreshWaitSeconds[1]);
                            DrawShadowMapGUIFloatField("Cascade 3 Update Interval", ref myScript.cascadesRefreshWaitSeconds[2]);
                            DrawShadowMapGUIFloatField("Cascade 4 Update Interval", ref myScript.cascadesRefreshWaitSeconds[3]);
                        }
                        break;
                    case ShadowMapToRefresh.Subshadows:
                        if (myScript.counterMode == CounterMode.Frames)
                        {
                            DrawShadowMapGUIIntField("Subshadow 1 Update Interval", ref myScript.subshadowsRefreshWaitFrames[0]);
                            DrawShadowMapGUIIntField("Subshadow 2 Update Interval", ref myScript.subshadowsRefreshWaitFrames[1]);
                            DrawShadowMapGUIIntField("Subshadow 3 Update Interval", ref myScript.subshadowsRefreshWaitFrames[2]);
                            DrawShadowMapGUIIntField("Subshadow 4 Update Interval", ref myScript.subshadowsRefreshWaitFrames[3]);
                            DrawShadowMapGUIIntField("Subshadow 5 Update Interval", ref myScript.subshadowsRefreshWaitFrames[4]);
                            DrawShadowMapGUIIntField("Subshadow 6 Update Interval", ref myScript.subshadowsRefreshWaitFrames[5]);
                        }
                        else if (myScript.counterMode == CounterMode.Seconds)
                        {
                            DrawShadowMapGUIFloatField("Subshadow 1 Update Interval", ref myScript.subshadowsRefreshWaitSeconds[0]);
                            DrawShadowMapGUIFloatField("Subshadow 2 Update Interval", ref myScript.subshadowsRefreshWaitSeconds[1]);
                            DrawShadowMapGUIFloatField("Subshadow 3 Update Interval", ref myScript.subshadowsRefreshWaitSeconds[2]);
                            DrawShadowMapGUIFloatField("Subshadow 4 Update Interval", ref myScript.subshadowsRefreshWaitSeconds[3]);
                            DrawShadowMapGUIFloatField("Subshadow 5 Update Interval", ref myScript.subshadowsRefreshWaitSeconds[4]);
                            DrawShadowMapGUIFloatField("Subshadow 6 Update Interval", ref myScript.subshadowsRefreshWaitSeconds[5]);
                        }
                        break;
                }
            }

            // Helper method for drawing int fields
            private void DrawShadowMapGUIIntField(string label, ref int refreshWaitFrames)
            {
                OnDemandShadowMapUpdate myScript = (OnDemandShadowMapUpdate)target;

                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                refreshWaitFrames = Mathf.Max(0, EditorGUILayout.IntField(label, refreshWaitFrames));
                EditorGUI.indentLevel--;
                EditorGUILayout.LabelField("Frames", GUILayout.MaxWidth(43));
                EditorGUILayout.EndHorizontal();
            }

            // Helper method for drawing float fields
            private void DrawShadowMapGUIFloatField(string label, ref float refreshWaitSeconds)
            {
                OnDemandShadowMapUpdate myScript = (OnDemandShadowMapUpdate)target;

                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                refreshWaitSeconds = Mathf.Max(0, EditorGUILayout.FloatField(label, refreshWaitSeconds));
                EditorGUI.indentLevel--;
                EditorGUILayout.LabelField("Seconds", GUILayout.MaxWidth(52));
                EditorGUILayout.EndHorizontal();
            }

            // Draw a Camera GameObject field
            private void DrawCameraField(string label, ref Camera gameObject)
            {
                OnDemandShadowMapUpdate myScript = (OnDemandShadowMapUpdate)target;

                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                myScript.cameraToTrack = (Camera)EditorGUILayout.ObjectField(label, gameObject, typeof(Camera), true);
                EditorGUI.indentLevel--;
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    public enum ShadowMapToRefresh
    {
        EntireShadowMap,
        Cascades,
        Subshadows
    }

    public enum CounterMode
    {
        Frames,
        Seconds
    }
}