using KSP.UI.Screens;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CelestialRescale.UI
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class CRUIUpdate : MonoBehaviour
    {
        private ApplicationLauncherButton toolbarButton;

        public void Start()
        {
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIApplicationLauncherReady);
        }

        private void OnGUIApplicationLauncherReady()
        {
            if (ApplicationLauncher.Instance != null && toolbarButton == null)
            {
                toolbarButton = ApplicationLauncher.Instance.AddModApplication(
                    OnButtonClick,
                    OnButtonClick,
                    null,
                    null,
                    null,
                    null,
                    ApplicationLauncher.AppScenes.MAINMENU,
                    GameDatabase.Instance.GetTexture(Path.Combine(HighLogic.SaveFolder + "GameData/CelestialRescale/Resources/icon.png"), false)
                );
            }
        }

        private void OnButtonClick()
        {
            ToggleUI();
        }

        bool isTogglingUI;
        public void Update()
        {
            isTogglingUI = GameSettings.MODIFIER_KEY.GetKey() && Input.GetKeyDown(KeyCode.C);

            if (isTogglingUI)
            {
                ToggleUI();
            }
        }

        private void ToggleUI()
        {
            if (CR_UI.UICanvas == null)
            {
                Debug.Log("[CelestialRescale] Showing UI");
                CR_UI.ShowGUI();
            }
            else
            {
                Debug.Log("[CelestialRescale] Closing UI");
                CR_UI.Destroy();
            }
        }
    }

    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class CRUILoader : MonoBehaviour
    {
        private static GameObject panelPrefab;

        public static GameObject PanelPrefab
        {
            get { return panelPrefab; }
        }

        public void Awake()
        {
            AssetBundle prefabs = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CelestialRescale-UI.dat"));
            panelPrefab = prefabs.LoadAsset("UIPanel") as GameObject;
            Debug.Log("[CelestialRescale] Loaded UI");
        }
    }

    [KSPAddon(KSPAddon.Startup.AllGameScenes, true)]
    public class CR_UI : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        public static GameObject UICanvas = null;
        private static Vector2 dragstart;
        private static Vector2 altstart;
        private static Vector2 pos;

        private const string CloseButtonName = "CloseButton";
        private const string VersionObjectName = "Version";

        public void Awake()
        {
            GameEvents.onGameSceneSwitchRequested.Add(OnSceneChange);
        }

        void OnSceneChange(GameEvents.FromToAction<GameScenes, GameScenes> fromToScenes)
        {
            if (UICanvas != null)
            {
                Destroy();
            }
        }

        public static void Destroy()
        {
            if (UICanvas != null)
            {
                pos = UICanvas.transform.position;
                UICanvas.DestroyGameObject();
                UICanvas = null;
            }
        }

        public static void ShowGUI()
        {
            if (UICanvas != null)
            {
                Debug.LogError("[CelestialRescale] UICanvas already exists");
                return;
            }

            UICanvas = Instantiate(CRUILoader.PanelPrefab);
            UICanvas.transform.SetParent(MainCanvasUtil.MainCanvas.transform);
            UICanvas.AddComponent<CR_UI>();
            UICanvas.transform.position = pos;

            GameObject checkButton = GameObject.Find(CloseButtonName);
            GameObject versionObject = GameObject.Find(VersionObjectName);

            if (versionObject != null && checkButton != null)
            {
                Text versionText = versionObject.GetComponent<Text>();
                versionText.text = CR_Version.version;

                Button button = checkButton.GetComponent<Button>();
                button.onClick.AddListener(OnCloseButtonClick);
            }
            else
            {
                Debug.LogError("[CelestialRescale] UI Elements Null");
            }
        }

        public static void OnCloseButtonClick()
        {
            Destroy();
        }

        public void OnBeginDrag(PointerEventData data)
        {
            dragstart = new Vector2(data.position.x - Screen.width / 2, data.position.y - Screen.height / 2);
            altstart = UICanvas.transform.position;
        }

        public void OnDrag(PointerEventData data)
        {
            Vector2 dpos = new Vector2(data.position.x - Screen.width / 2, data.position.y - Screen.height / 2);
            Vector2 dragdist = dpos - dragstart;
            UICanvas.transform.position = altstart + dragdist;
        }
    }
}
