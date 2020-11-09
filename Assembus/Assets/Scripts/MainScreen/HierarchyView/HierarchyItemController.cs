using MainScreen.HierarchyView;
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
        public HierarchyViewController hierarchyViewController;

        /// <summary>
        ///     The text view in which the name is shown
        /// </summary>
        public TextMeshProUGUI nameText;

        /// <summary>
        ///     The rect transform of the name
        /// </summary>
        public RectTransform nameRect;

        /// <summary>
        ///     The expand button
        /// </summary>
        public GameObject expandButton, expandDown, expandRight;

        /// <summary>
        ///     The container of the item which contains all children
        /// </summary>
        public GameObject childrenContainer;

        /// <summary>
        ///     True if the child elements are expanded in the hierarchy view
        /// </summary>
        private bool _isExpanded = true;

        /// <summary>
        ///     The name of the item
        /// </summary>
        private string _name;

        /// <summary>
        ///     The root element of the hierarchy view
        /// </summary>
        private RectTransform _rectTransform;

        /// <summary>
        ///     True if the hierarchy view needs to be updated
        /// </summary>
        private bool _updateHierarchy;

        /// <summary>
        ///     True if the item has children
        /// </summary>
        private bool HasChildren => childrenContainer.transform.childCount > 0;

        /// <summary>
        ///     Update the expand button to display the correct icon
        /// </summary>
        private void Start()
        {
            UpdateButton();
        }

        /// <summary>
        ///     Late update of the UI
        /// </summary>
        private void LateUpdate()
        {
            // force update of the hierarchy view if the item expansion changed
            if (_updateHierarchy)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
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
            nameText.text = itemName;

            // indent the item
            nameRect.offsetMin += new Vector2(indentionDepth, 0);
            expandButton.GetComponent<RectTransform>().anchoredPosition += new Vector2(indentionDepth, 0);

            // set the root hierarchy view
            _rectTransform = mainHierarchyView.GetComponent<RectTransform>();
        }

        /// <summary>
        ///     Expand the item's content
        /// </summary>
        public void ExpandItem()
        {
            if (!HasChildren) return;
            childrenContainer.SetActive(!_isExpanded);
            _isExpanded = !_isExpanded;
            _updateHierarchy = true;
            UpdateButton();
        }

        /// <summary>
        ///     OnClick Method for the Selection of an item
        /// </summary>
        public void SelectItem()
        {
            // Item Selection if left control is used
            if (Input.GetKey(KeyCode.LeftControl))
                hierarchyViewController.ClickItem(gameObject, KeyCode.LeftControl);

            // Item selection if the left shift key is used
            else if (Input.GetKey(KeyCode.LeftShift))
                hierarchyViewController.ClickItem(gameObject, KeyCode.LeftShift);

            // Item Selection if No modifier is used
            else
                hierarchyViewController.ClickItem(gameObject, KeyCode.None);
        }

        /// <summary>
        ///     Update the expand button to display the correct icons
        /// </summary>
        private void UpdateButton()
        {
            // Enable/Disable the button
            expandButton.SetActive(HasChildren);
            if (!HasChildren) return;

            // Update the logos if necessary
            expandDown.SetActive(_isExpanded);
            expandRight.SetActive(!_isExpanded);
        }
    }
}