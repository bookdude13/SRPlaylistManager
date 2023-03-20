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

        private GameObject Background { get; set; }
        private GameObject ExistingHeader { get; set; }
        private GameObject ExistingItem { get; set; }
        private GameObject ExistingButtons { get; set; }
        private GameObject BackNav { get; set; }
        private Transform ItemContainer { get; set; }
        private Action OnClose;
        private List<GameObject> Items = new List<GameObject>();

        public ScrollablePanel(GameObject panel, GameObject background, GameObject buttons, GameObject backNav, GameObject header, GameObject item, Transform itemContainer, Action onClose)
        {
            Panel = panel;
            Background = background;
            ExistingHeader = header;
            ExistingItem = item;
            ExistingButtons = buttons;
            BackNav = backNav;
            ItemContainer = itemContainer;
            OnClose = onClose;

            // Set up back nav actions
            Console.Error.WriteLine("Setting up button actions");
            try
            {
                var backBtn = backNav.transform.Find("Scale Wrap/Button Wrap - Full/NavBarButton");
                if (backBtn == null)
                {
                    Console.Error.WriteLine("Failed to find NavBarButton for back");
                    return;
                }
                var button = backBtn.GetComponentInChildren<UnityEngine.UI.Button>(true);
                button.onClick.RemoveAllListeners();

                var num = button.onClick.GetPersistentEventCount();
                for (var i = 0; i < num; i++)
                {
                    var nm = button.onClick.GetPersistentMethodName(i);
                    Console.Error.WriteLine($"Persistent {nm} removed");
                    button.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);
                }

                button.onClick.AddListener(Close);
            } catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
            Console.Error.WriteLine("Created ScrollablePanel");
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

        public void AddItem(string name, string text)
        {
            GameObject newLabel = GameObject.Instantiate(ExistingItem, ExistingItem.transform.position, ExistingItem.transform.rotation, ItemContainer);
            newLabel.name = name;
            newLabel.GetComponentInChildren<Synth.Utils.LocalizationHelper>().enabled = false;
            newLabel.transform.Find("Value Area").gameObject.SetActive(false);
            newLabel.GetComponentInChildren<TMPro.TextMeshProUGUI>().SetText(text);
            newLabel.GetComponent<VRTK.UnityEventHelper.VRTK_InteractableObject_UnityEvents>().enabled = false;
            newLabel.SetActive(true);
            Items.Add(newLabel);
        }

        public static ScrollablePanel Create(string name, Action onClose)
        {
            var panel = CloneInterfacePanel();
            if (panel == null)
            {
                Console.Error.WriteLine("Failed to clone interface panel");
                return null;
            }
            panel.name = name;

            var backNav = CloneBackNavBar();
            if (backNav == null)
            {
                Console.Error.WriteLine("Failed to clone back nav");
                return null;
            }
            backNav.name = "playlist_backnav";
            backNav.transform.parent = panel.transform;

            var background = panel.transform.Find("BG");
            if (background == null)
            {
                Console.Error.WriteLine("Failed to find panel background");
                return null;
            }

            var buttons = panel.transform.Find("[Buttons Layer]");
            if (buttons == null)
            {
                Console.Error.WriteLine("Failed to find buttons");
                return null;
            }
            // Hide buttons, but keep them around for cloning
            foreach (var button in buttons.GetComponentsInChildren<Transform>())
            {
                button.gameObject.SetActive(false);
            }

            Console.Error.WriteLine("Finding headers and items");
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

            return new ScrollablePanel(panel, background.gameObject, buttons.gameObject, backNav, title, item, item.transform.parent, onClose);
        }

        private static GameObject CloneInterfacePanel()
        {
            var interfacePanel = GetInterfacePanel();
            if (interfacePanel == null)
            {
                return null;
            }

            return GameObject.Instantiate(interfacePanel, interfacePanel.transform.position, interfacePanel.transform.rotation, interfacePanel.transform.parent);
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
