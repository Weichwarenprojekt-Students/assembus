using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainScreen
{
    /// <summary>
    ///     Manage the behaviour of a hierarchy view item
    /// </summary>
    public class HierarchyItemController : MonoBehaviour
    {
        /// <summary>
        ///     True if the child elements are expanded in the hierarchy view
        /// </summary>
        private bool _isExpanded = true;

        /// <summary>
        ///     The name of the item
        /// </summary>
        private string _name;

        /// <summary>
        ///     True if the hierarchy view needs to be updated
        /// </summary>
        private bool _updateHierarchy;

        /// <summary>
        ///     The Element part of the hierarchy item
        /// </summary>
        private Transform Element => gameObject.transform.Find("Background").Find("Element");

        /// <summary>
        ///     The container of the item which contains all children
        /// </summary>
        public GameObject ChildrenContainer => gameObject.transform.Find("Content").gameObject;

        /// <summary>
        ///     The root element of the hierarchy view
        /// </summary>
        private GameObject MainHierarchyView { get; set; }

        /// <summary>
        ///     True if the item has children
        /// </summary>
        private bool HasChildren => ChildrenContainer.transform.childCount > 0;

        /// <summary>
        ///     Setup the item the first time the script gets executed
        /// </summary>
        private void Start()
        {
            // Add a click listener to the expand button
            Element.Find("Expand").GetComponent<Button>().onClick.AddListener(
                () =>
                {
                    if (!HasChildren) return;

                    ChildrenContainer.SetActive(!_isExpanded);

                    _isExpanded = !_isExpanded;
                    _updateHierarchy = true;

                    UpdateButton();
                }
            );

            // Update the expand button to display the correct icon
            UpdateButton();
        }

        /// <summary>
        ///     Late update of the UI
        /// </summary>
        private void LateUpdate()
        {
            // force update of the hierarchy view if the item expansion changed
            if (_updateHierarchy)
                LayoutRebuilder.ForceRebuildLayoutImmediate(MainHierarchyView.GetComponent<RectTransform>());
        }

        /// <summary>
        ///     Update the expand button to display the correct icons
        /// </summary>
        private void UpdateButton()
        {
            if (HasChildren)
            {
                // enable the expand button
                Element.Find("Expand").gameObject.SetActive(true);
                if (_isExpanded)
                {
                    // show the down arrow
                    Element.Find("Expand").Find("ExpandDown").gameObject.SetActive(true);
                    Element.Find("Expand").Find("ExpandRight").gameObject.SetActive(false);
                }
                else
                {
                    // show the right arrow
                    Element.Find("Expand").Find("ExpandDown").gameObject.SetActive(false);
                    Element.Find("Expand").Find("ExpandRight").gameObject.SetActive(true);
                }
            }
            else
            {
                // disable the expand button
                Element.Find("Expand").gameObject.SetActive(false);
            }
        }

        /// <summary>
        ///     Initialize the hierarchy item
        /// </summary>
        /// <param name="itemName">Name of corresponding GameObject</param>
        /// <param name="indentionDepth">Depth of indentation inside the listview</param>
        /// <param name="mainHierarchyView">Reference to the root of the hierarchy view</param>
        public void Initialize(string itemName, int indentionDepth, GameObject mainHierarchyView)
        {
            // set the name of the item
            Element.Find("Name").GetComponent<TextMeshProUGUI>().text = itemName;

            // indent the item
            Element.GetComponent<RectTransform>().position += new Vector3(indentionDepth, 0);

            // set the root hierarchy view
            MainHierarchyView = mainHierarchyView;
        }
    }
}