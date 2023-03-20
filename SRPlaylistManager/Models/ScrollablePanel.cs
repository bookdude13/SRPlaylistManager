using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SRPlaylistManager.Models
{
    internal class ScrollablePanel
    {
        public GameObject Panel { get; private set; }
        public GameObject Background { get; private set; }
        public GameObject ExistingHeader { get; private set; }
        public GameObject ExistingItem { get; private set; }
        public Transform ItemContainer { get; private set; }

        public ScrollablePanel(GameObject panel, GameObject background, GameObject header, GameObject item, Transform itemContainer)
        {
            Panel = panel;
            Background = background;
            ExistingHeader = header;
            ExistingItem = item;
            ItemContainer = itemContainer;
        }

        public void SetVisibility(bool visible)
        {
            Background.SetActive(visible);
            Panel.gameObject.SetActive(visible);
        }

        public void AddHeader(string name, string text)
        {
            GameObject newHeader = GameObject.Instantiate(ExistingHeader, ExistingHeader.transform.position, ExistingHeader.transform.rotation, ItemContainer);
            newHeader.name = name;
            newHeader.GetComponentInChildren<Synth.Utils.LocalizationHelper>().enabled = false;
            newHeader.GetComponentInChildren<TMPro.TextMeshProUGUI>().SetText(text);
            newHeader.SetActive(true);
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
        }

        public static ScrollablePanel Create(string name)
        {
            var panel = CloneInterfacePanel();
            panel.name = name;

            var background = panel.transform.Find("BG");
            if (background == null)
            {
                Console.Error.WriteLine("Failed to find panel background");
                return null;
            }

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

            return new ScrollablePanel(panel, background.gameObject, title, item, item.transform.parent);
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
