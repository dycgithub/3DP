using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecycleViewControl : MonoBehaviour
{
    //存储数据对象
    private List<string> data;
    //信息总数
    private int ListCount => data.Count;
    //绑定具体的ScollView
    public LoopScroll VerticalScroll;

    void Start()
    {
        //获取数据信息
        data = new List<string>();
        for (int i = 1; i <= 100; i++)
        {
            data.Add("test data" + i);
        }
        StartScrollView();
    }
    public void StartScrollView()
    {
        // 1. 初始化（注册 Cell 数据回调）
        VerticalScroll.Init(NormalCallBack);
        // 2. 显示列表（传入总数量）
        VerticalScroll.ShowList(ListCount);
    }
    /// <summary>
    /// Cell 数据绑定与交互逻辑
    /// </summary>
    private void NormalCallBack(GameObject cell, int index)
    {
        // 文本内容事件（transform.Find只在当前 Transform 的子层级中查找）
        cell.transform.Find("text").GetComponent<TMP_Text>().text = data[index];

        // 按钮事件（必须先清理旧监听，避免复用导致叠加）
        Button btn = cell.transform.Find("btn").GetComponent<Button>();
        // 使用Lambad表达式 只能移除所有监听
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            Debug.Log(index);
        });
    }
}
