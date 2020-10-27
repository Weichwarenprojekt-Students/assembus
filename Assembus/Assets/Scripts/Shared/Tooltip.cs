using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Shared
{
    public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        ///     The text to be shown
        /// </summary>
        public string text;

        /// <summary>
        ///     The offset of the tooltip
        /// </summary>
        public int offsetX, offsetY;
        
        /// <summary>
        ///     The controller for the tooltip
        /// </summary>
        public TooltipController tooltip;
        
        /// <summary>
        ///     Show the tooltip
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            // Calculate the position
            var position = gameObject.GetComponent<RectTransform>().position;
            var scale = tooltip.mainCanvas.transform.localScale;
            var x = position.x + offsetX * scale.x;
            var y = position.y + offsetY * scale.y;
            
            Debug.Log(position);
            // Show the tooltip
            tooltip.ShowTooltip(x, y, text);
        }

        /// <summary>
        ///     Hide the tooltip
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            tooltip.HideTooltip();
        }
    }
}