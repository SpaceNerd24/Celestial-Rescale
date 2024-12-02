using CelestialRescale.API;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CelestialRescale.UI
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class CRUIUpdate : MonoBehaviour
    {
        bool isTogglingUI;
        public void Update()
        {
            Debug.Log("sdfsf");
            isTogglingUI = GameSettings.MODIFIER_KEY.GetKey() && Input.GetKeyDown(KeyCode.C);

            if (isTogglingUI)
            {
                Debug.Log("[CelestialRescale] Toggling UI");
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
            pos = UICanvas.transform.position;
            UICanvas.DestroyGameObject();
            UICanvas = null;
        }

        public static void ShowGUI()
        {
            if (UICanvas != null)
            {
                return;
            }
            else
            {
                UICanvas = Instantiate(CRUILoader.PanelPrefab);
                UICanvas.transform.SetParent(MainCanvasUtil.MainCanvas.transform);
                UICanvas.AddComponent<CR_UI>();
                UICanvas.transform.position = pos;

                GameObject checkButton = (GameObject)GameObject.Find("CloseButton");
                Button button = checkButton.GetComponent<Button>();

                button.onClick.AddListener(onButtonClick);
            }
        }

        public static void onButtonClick()
        { 
            if (UICanvas != null)
            {
                Destroy();
            }
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
