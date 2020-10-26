using Shared;
using UnityEngine;

namespace MainScreen
{
    public class UIController : MonoBehaviour
    {
        /// <summary>
        ///     The main camera
        /// </summary>
        public Camera mainCamera;

        /// <summary>
        ///     The layout components
        /// </summary>
        public RectTransform toolbar, sidebar;

        /// <summary>
        ///     The canvas for the main screen
        /// </summary>
        public Canvas mainCanvas;

        /// <summary>
        ///     The panels
        /// </summary>
        public GameObject editPanel, settingsPanel;
        
        /// <summary>
        ///     The two screens
        /// </summary>
        public GameObject startScreen, mainScreen;

        /// <summary>
        ///     The dialog
        /// </summary>
        public DialogController dialog;
        
        /// <summary>
        ///     The current width of the screen
        /// </summary>
        private int _width;

        /// <summary>
        ///      Check if the screen was resized
        /// </summary>
        private void Update()
        {
            var width = Screen.width;
            if (width == _width) return;
            
            // Get the actual layout sizes
            var localScale = mainCanvas.transform.localScale;
            var toolbarHeight = localScale.y * toolbar.rect.height;
            var sidebarWidth = localScale.x * sidebar.rect.width;
            
            // Calculate the bounds
            var x = sidebarWidth / width;
            var y = toolbarHeight / Screen.height;
            
            // Set the new bounds
            mainCamera.rect = new Rect(x, 0, 1 - x, 1 - y);
            _width = width;
        }

        /// <summary>
        ///     Show the edit panel
        /// </summary>
        public void ShowEditor()
        {
            editPanel.SetActive(true);
            settingsPanel.SetActive(false);
        }
        
        /// <summary>
        ///     Show the settings panel
        /// </summary>
        public void ShowSettings()
        {
            editPanel.SetActive(false);
            settingsPanel.SetActive(true);
        }

        /// <summary>
        ///     Close a project
        /// </summary>
        public void CloseProject()
        {
            dialog.Show("Close Project", "Are you sure?", () =>
            {
                // Show the start screen
                mainScreen.SetActive(false);
                startScreen.SetActive(true);
                
                // Reset camera
                _width = 0;
                mainCamera.rect = new Rect(0, 0, 1, 1);
            });
        }
    }
}
