using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using HeavenStudio.Util;
using HeavenStudio.Common;
using HeavenStudio.Editor;
using Jukebox;
using Jukebox.Legacy;
using System;

namespace HeavenStudio
{
    public class StaticCamera : MonoBehaviour
    {
        [SerializeField] RectTransform canvas;
        [SerializeField] GameObject overlayView;

        [SerializeField] Image ambientBg;
        [SerializeField] GameObject ambientBgGO;
        [SerializeField] GameObject letterboxBgGO;

        [SerializeField] RectTransform overlayCanvas;
        [SerializeField] RectTransform letterboxMask;
        [SerializeField] RectTransform parentView;

        public static StaticCamera instance { get; private set; }
        public new Camera camera;

        public enum ViewAxis
        {
            All,
            X,
            Y,
        }

        const float AspectRatioWidth = 1;
        const float AspectRatioHeight = 1;

        private List<RiqEntity> panEvents = new();
        private List<RiqEntity> scaleEvents = new();
        private List<RiqEntity> rotationEvents = new();
        private List<RiqEntity> fitScreenEvents = new();

        static Vector3 defaultPan = new Vector3(0, 0, 0);
        static Vector3 defaultScale = new Vector3(1, 1, 1);
        static float defaultRotation = 0;

        private static Vector3 pan;
        private static Vector3 scale;
        private static float rotation;

        private static Vector3 panLast;
        private static Vector3 scaleLast;
        private static float rotationLast;

        private void Awake()
        {
            instance = this;
            camera = this.GetComponent<Camera>();
        }

        // Start is called before the first frame update
        void Start()
        {
            GameManager.instance.onBeatChanged += OnBeatChanged;

            Reset();

            panLast = defaultPan;
            scaleLast = defaultScale;
            rotationLast = defaultRotation;

            ToggleLetterboxBg(PersistentDataManager.gameSettings.letterboxBgEnable);
            ToggleLetterboxGlow(PersistentDataManager.gameSettings.letterboxFxEnable);
        }

        public void OnBeatChanged(double beat)
        { 
            Reset();

            panEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "pan view" });
            scaleEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "scale view" });
            rotationEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "rotate view" });
            fitScreenEvents = EventCaller.GetAllInGameManagerList("vfx", new string[] { "fitScreen" });

            panLast = defaultPan;
            scaleLast = defaultScale;
            rotationLast = defaultRotation;

            UpdatePan();
            UpdateRotation();
            UpdateScale();
            UpdateGameScreenFit();

            canvas.localPosition = pan;
            canvas.eulerAngles = new Vector3(0, 0, rotation);
            canvas.localScale = scale;
        }

        // Update is called once per frame
        void Update()
        {
            UpdatePan();
            UpdateRotation();
            UpdateScale();
            UpdateGameScreenFit();

            canvas.localPosition = pan;
            canvas.eulerAngles = new Vector3(0, 0, rotation);
            canvas.localScale = scale;
        }

        private void UpdateGameScreenFit()
        {
            var curBeat = Conductor.instance.songPositionInBeatsAsDouble;
            letterboxMask.localScale = new Vector3(1, 1, 1);
            overlayCanvas.localScale = new Vector3(1, 1, 1);
            foreach (var e in fitScreenEvents)
            {
                if (curBeat < e.beat) break;
                if (e["enable"])
                {
                    letterboxMask.localScale = new Vector3(parentView.sizeDelta.x / 16, parentView.sizeDelta.y / 9, 1);
                    overlayCanvas.localScale = new Vector3(parentView.sizeDelta.x / 16, parentView.sizeDelta.y / 9, 1);
                }
                else
                {
                    letterboxMask.localScale = new Vector3(1, 1, 1);
                    overlayCanvas.localScale = new Vector3(1, 1, 1);
                }
            }
        }

        private void UpdatePan()
        {
            foreach (var e in panEvents)
            {
                float prog = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (prog >= 0f)
                {
                    Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease) e["ease"]);
                    switch (e["axis"])
                    {
                        case (int) ViewAxis.X:
                            pan.x = func(panLast.x, e["valA"], Mathf.Min(prog, 1f));
                            break;
                        case (int) ViewAxis.Y:
                            pan.y = func(panLast.y, e["valB"], Mathf.Min(prog, 1f));
                            break;
                        default:
                            float dx = func(panLast.x, e["valA"], Mathf.Min(prog, 1f));
                            float dy = func(panLast.y, e["valB"], Mathf.Min(prog, 1f));
                            pan = new Vector3(dx, dy, 0);
                            break;
                    }
                }
                if (prog > 1f)
                {
                    switch (e["axis"])
                    {
                        case (int) ViewAxis.X:
                            panLast.x = e["valA"];
                            break;
                        case (int) ViewAxis.Y:
                            panLast.y = e["valB"];
                            break;
                        default:
                            panLast = new Vector3(e["valA"], e["valB"], 0);
                            break;
                    }
                }
            }
        }

        private void UpdateRotation()
        {
            foreach (var e in rotationEvents)
            {
                float prog = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (prog >= 0f)
                {
                    Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease) e["ease"]);
                    rotation = func(rotationLast, -e["valA"], Mathf.Min(prog, 1f));
                }
                if (prog > 1f)
                {
                    rotationLast = -e["valA"];
                }
            }
        }

        private void UpdateScale()
        {
            foreach (var e in scaleEvents)
            {
                float prog = Conductor.instance.GetPositionFromBeat(e.beat, e.length);
                if (prog >= 0f)
                {
                    Util.EasingFunction.Function func = Util.EasingFunction.GetEasingFunction((Util.EasingFunction.Ease) e["ease"]);
                    switch (e["axis"])
                    {
                        case (int) ViewAxis.X:
                            scale.x = func(scaleLast.x, e["valA"], Mathf.Min(prog, 1f)) * AspectRatioWidth;
                            break;
                        case (int) ViewAxis.Y:
                            scale.y = func(scaleLast.y, e["valB"], Mathf.Min(prog, 1f)) * AspectRatioHeight;
                            break;
                        default:
                            float dx = func(scaleLast.x, e["valA"], Mathf.Min(prog, 1f)) * AspectRatioWidth;
                            float dy = func(scaleLast.y, e["valB"], Mathf.Min(prog, 1f)) * AspectRatioHeight;
                            scale = new Vector3(dx, dy, 1);
                            break;
                    }
                }
                if (prog > 1f)
                {
                    switch (e["axis"])
                    {
                        case (int) ViewAxis.X:
                            scaleLast.x = e["valA"] * AspectRatioWidth;
                            break;
                        case (int) ViewAxis.Y:
                            scaleLast.y = e["valB"] * AspectRatioHeight;
                            break;
                        default:
                            scaleLast = new Vector3(e["valA"] * AspectRatioWidth, e["valB"] * AspectRatioHeight, 1);
                            break;
                    }
                }
            }
        }

        public static void Reset()
        {
            pan = defaultPan;
            scale = defaultScale;
            rotation = defaultRotation;
        }

        public void ToggleOverlayView(bool toggle)
        {
            overlayView.SetActive(toggle);
        }

        [NonSerialized] public bool usingMinigameAmbientColor;

        public void SetAmbientGlowColour(Color colour, bool minigameColor, bool overrideMinigameColor = true)
        {
            if (overrideMinigameColor) usingMinigameAmbientColor = minigameColor;
            ambientBg.color = colour;
            GameSettings.UpdatePreviewAmbient(colour);
        }

        public void ToggleLetterboxBg(bool toggle)
        {
            letterboxBgGO.SetActive(toggle);
        }

        public void ToggleLetterboxGlow(bool toggle)
        {
            ambientBgGO.SetActive(toggle);
        }

        public void ToggleCanvasVisibility(bool toggle)
        {
            canvas.gameObject.SetActive(toggle);
        }
    }
}