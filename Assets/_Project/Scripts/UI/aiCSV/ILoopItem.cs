using UnityEngine;

namespace UI.Loops
{
    /// <summary>
    /// 可在LoopingScrollController中使用的项的接口
    /// </summary>
    /// <typeparam name="T">此项显示的数据类型</typeparam>
    public interface ILoopItem<T>
    {
        /// <summary>
        /// 使用新数据更新项的内容
        /// </summary>
        /// <param name="data">要显示的数据</param>
        void UpdateContent(T data);

        /// <summary>
        /// 当项被回收且应被重置时调用
        /// </summary>
        void ResetItem();

        /// <summary>
        /// 获取项的RectTransform
        /// </summary>
        RectTransform RectTransform { get; }

        /// <summary>
        /// 获取此项正在显示的当前数据索引
        /// </summary>
        int DataIndex { get; set; }
    }
}