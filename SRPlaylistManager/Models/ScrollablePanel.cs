using SRModCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace SRPlaylistManager.Models
{
    internal class ScrollablePanel
    {
        public GameObject Panel { get; private set; }

        private SRLogger _logger;
        private GameObject Background { get; set; }
        private GameObject ExistingHeader { get; set; }
        private GameObject ExistingItem { get; set; }
        private GameObject ExistingButtons { get; set; }
        private GameObject BackNav { get; set; }
        private Transform ItemContainer { get; set; }
        private Action OnClose;
        private List<GameObject> Items = new List<GameObject>();

        public ScrollablePanel(SRLogger logger, GameObject panel, GameObject background, GameObject buttons, GameObject backNav, GameObject header, GameObject item, Transform itemContainer, Action onClose)
        {
            _logger = logger;
            Panel = panel;
            Background = background;
            ExistingHeader = header;
            ExistingItem = item;
            ExistingButtons = buttons;
            BackNav = backNav;
            ItemContainer = itemContainer;
            OnClose = onClose;

            SetUpBackNav();
            
            _logger.Debug("Created ScrollablePanel");
        }

        private void SetUpBackNav()
        {
            _logger.Debug("Setting up button actions");
            try
            {
                var backBtn = BackNav.transform.Find("Scale Wrap/Button Wrap - Full/NavBarButton");
                if (backBtn == null)
                {
                    _logger.Error("Failed to find NavBarButton for back");
                    return;
                }
                var button = backBtn.GetComponentInChildren<UnityEngine.UI.Button>(true);
                button.onClick.RemoveAllListeners();

                var num = button.onClick.GetPersistentEventCount();
                for (var i = 0; i < num; i++)
                {
                    var nm = button.onClick.GetPersistentMethodName(i);
                    _logger.Debug($"Persistent {nm} removed");
                    button.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);
                }

                button.onClick.AddListener(Close);
            }
            catch (Exception e)
            {
                _logger.Error("Failed to set up back nav: " + e.ToString());
            }
        }

        public void SetVisibility(bool visible)
        {
            BackNav?.SetActive(visible);
            Background?.SetActive(visible);
            Panel?.gameObject?.SetActive(visible);
        }

        private void Close()
        {
            SetVisibility(false);
            OnClose?.Invoke();
        }

        public void AddHeader(string name, string text)
        {
            GameObject newHeader = GameObject.Instantiate(ExistingHeader, ExistingHeader.transform.position, ExistingHeader.transform.rotation, ItemContainer);
            newHeader.name = name;
            newHeader.GetComponentInChildren<Synth.Utils.LocalizationHelper>().enabled = false;
            newHeader.GetComponentInChildren<TMPro.TextMeshProUGUI>().SetText(text);
            newHeader.SetActive(true);
        }

        public void ClearItems()
        {
            foreach (var go in Items)
            {
                GameObject.Destroy(go);
            }
            Items.Clear();
        }

        public void AddItem(PanelItem panelItem)
        {
            GameObject newItem = GameObject.Instantiate(ExistingItem, ExistingItem.transform.position, ExistingItem.transform.rotation, ItemContainer);
            var setUpItem = panelItem.Setup(newItem);
            Items.Add(setUpItem);
        }

        public static ScrollablePanel Create(string name, Action onClose, SRLogger logger)
        {
            var panel = CloneInterfacePanel();
            if (panel == null)
            {
                logger.Error("Failed to clone interface panel");
                return null;
            }
            panel.name = name;

            var backNav = CloneBackNavBar();
            if (backNav == null)
            {
                logger.Error("Failed to clone back nav");
                return null;
            }
            backNav.name = "playlist_backnav";
            backNav.transform.parent = panel.transform;

            var background = panel.transform.Find("BG");
            if (background == null)
            {
                logger.Error("Failed to find panel background");
                return null;
            }

            var buttons = panel.transform.Find("[Buttons Layer]");
            if (buttons == null)
            {
                logger.Error("Failed to find buttons");
                return null;
            }
            // Hide buttons, but keep them around for cloning
            foreach (var button in buttons.GetComponentsInChildren<Transform>())
            {
                button.gameObject.SetActive(false);
            }

            logger.Debug("Finding headers and items");
            GameObject title = null;
            GameObject item = null;
            foreach (var go in panel.GetComponentsInChildren<RectTransform>())
            {
                // Keep one header and one item for cloning, but hide them.
                // Delete the rest

                if (go.name.StartsWith("Setting Header"))
                {
                    if (title == null)
                    {
                        title = go.gameObject;
                        title.SetActive(false);
                    }
                    else
                    {
                        GameObject.Destroy(go.gameObject);
                    }
                }

                if (go.name.StartsWith("Setting Item"))
                {
                    if (item == null)
                    {
                        item = go.gameObject;
                        item.SetActive(false);
                    }
                    else
                    {
                        GameObject.Destroy(go.gameObject);
                    }
                }
            }
            if (title == null) return null;
            if (item == null) return null;

            return new ScrollablePanel(logger, panel, background.gameObject, buttons.gameObject, backNav, title, item, item.transform.parent, onClose);
        }

        private static GameObject CloneInterfacePanel()
        {
            var interfacePanel = GetInterfacePanel();
            if (interfacePanel == null)
            {
                return null;
            }

            var newPanel = GameObject.Instantiate(interfacePanel, interfacePanel.transform.position, interfacePanel.transform.rotation, interfacePanel.transform.parent);
            
            // The settings tab on the left offsets the main content - bring it back center
            //var settingsTab = panel.Panel.transform.parent.Find("[Settings Tabs Layer]");
            newPanel.transform.localPosition -= new Vector3(newPanel.transform.localPosition.x, 0, 0);

            return newPanel;
        }

        private static GameObject CloneBackNavBar()
        {
            GameObject navBar = GameObject.Find("Z-Wrap/[Pause  Menu Room]/[Wrapper]/Nav Bar");
            if (navBar == null)
            {
                return null;
            }

            return GameObject.Instantiate(navBar, navBar.transform.position, navBar.transform.rotation, navBar.transform.parent);
        }

        private static GameObject GetInterfacePanel()
        {
            foreach (var go in GameObject.Find("Z-Wrap").transform.GetComponentsInChildren<Game_PauseMenuPanel>(true))
            {
                if (go.gameObject.name == "[Interface Panel]")
                {
                    return go.gameObject;
                }
            }
            return null;
        }
    }
}
