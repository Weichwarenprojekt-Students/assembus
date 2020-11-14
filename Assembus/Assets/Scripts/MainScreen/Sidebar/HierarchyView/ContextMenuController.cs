using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MainScreen.Sidebar.HierarchyView
{
    public class ContextMenuController : MonoBehaviour, IPointerDownHandler
    {
        /// <summary>
        ///     The default item
        /// </summary>
        public GameObject defaultItem;

        /// <summary>
        ///     The list view of the context menu
        /// </summary>
        public Transform listView;

        /// <summary>
        ///     Hide the context menu
        /// </summary>
        private void OnMouseDown()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        ///     Hide the context menu if the user clicked outside of the panel
        /// </summary>
        /// <param name="eventData">The event data</param>
        public void OnPointerDown(PointerEventData eventData)
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        ///     Show the context menu
        /// </summary>
        /// <param name="names">The names of the items</param>
        /// <param name="actions">The on click actions for the items</param>
        public void Show(string[] names, Action[] actions)
        {
            listView.position = Input.mousePosition;
            // Hide the panel
            gameObject.SetActive(false);

            // Remove old items
            for (var i = 1; i < listView.childCount; i++) Destroy(listView.GetChild(i).gameObject);

            // Show the default item
            defaultItem.SetActive(true);

            // Apply data to listview. Iterate oldProjects list backwards
            for (var i = 0; i < names.Length; i++)
            {
                // Create new listview item by instantiating a new prefab
                var newListViewItem = Instantiate(defaultItem, listView, true);

                // Set the new values
                var nameText = newListViewItem.transform.Find("Name").GetComponent<TextMeshProUGUI>();
                nameText.text = names[i];
                var index = i;
                newListViewItem.GetComponent<Button>().onClick.AddListener(
                    () =>
                    {
                        actions[index]();
                        gameObject.SetActive(false);
                    }
                );
            }

            // Hide the default item
            defaultItem.SetActive(false);

            // Show the context menu
            gameObject.SetActive(true);
        }
    }
}