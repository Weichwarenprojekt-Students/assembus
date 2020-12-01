using MainScreen;
using UnityEngine;

namespace CinemaScreen
{
    public class CinemaController : MonoBehaviour
    {
        /// <summary>
        ///     References to main and cinema screen
        /// </summary>
        public GameObject mainScreen, cinemaScreen;

        /// <summary>
        ///     Reference to the component highlighting which must be disabled
        /// </summary>
        public ComponentHighlighting componentHighlighting;

        private void OnEnable()
        {
            // Disable component highlighting
            componentHighlighting.isActive = false;
        }

        /// <summary>
        ///     Exit the cinema mode
        /// </summary>
        public void CloseCinemaMode()
        {
            // Activate component highlighting again
            componentHighlighting.isActive = true;

            // Switch screens
            cinemaScreen.SetActive(false);
            mainScreen.SetActive(true);
        }
    }
}