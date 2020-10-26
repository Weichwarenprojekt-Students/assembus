using Services;
using UnityEngine;

namespace Shared
{
    public class MouseController : MonoBehaviour
    {
        public Texture2D cursor;
        
        /// <summary>
        ///     Change the cursor to the hand
        /// </summary>
        public void SetHand()
        {
            Cursor.SetCursor(cursor, Vector2.up, CursorMode.Auto);
        }

        /// <summary>
        ///     Change to default cursor
        /// </summary>
        public void SetDefault()
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}