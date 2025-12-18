using UnityEngine;

namespace UI.Loops
{
    /// <summary>
    /// Interface for items that can be used in a LoopingScrollController
    /// </summary>
    /// <typeparam name="T">The type of data this item displays</typeparam>
    public interface ILoopItem<T>
    {
        /// <summary>
        /// Updates the item's content with new data
        /// </summary>
        /// <param name="data">The data to display</param>
        void UpdateContent(T data);

        /// <summary>
        /// Called when the item is recycled and should be reset
        /// </summary>
        void ResetItem();

        /// <summary>
        /// Gets the RectTransform of the item
        /// </summary>
        RectTransform RectTransform { get; }

        /// <summary>
        /// Gets the current data index this item is displaying
        /// </summary>
        int DataIndex { get; set; }
    }
}