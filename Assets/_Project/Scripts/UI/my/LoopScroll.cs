using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum E_Direction
{
    Horizontal,
    Vertical
}

// 定义RecycleView类，继承MonoBehaviour并实现拖拽接口
public class LoopScroll : MonoBehaviour
{
    // [Header("布局设置")]
    public E_Direction dir = E_Direction.Vertical; // 滑动方向，默认为垂直
    public int lines = 1; // 每行/列显示的子项数量（多行布局时使用）
    public float squareSpacing = 5f; // 统一间距（行列相同时使用）
    public Vector2 Spacing = Vector2.zero; // 独立行列间距（x:行间距，y:列间距）
    private float row = 0f; // 实际使用的行间距（根据Spacing计算）
    private float col = 0f; // 实际使用的列间距（根据Spacing计算）
    public float paddingTop = 0f; // 内容区域顶部内边距
    public float paddingLeft = 0f; // 内容区域左侧内边距
    public GameObject cell; // 子项预制体引用（必须赋值）

    // 回调函数定义
    protected Action<GameObject, int> FuncCallBackFunc; // 子项数据绑定回调
    protected Action<GameObject, int> FuncOnClickCallBack; // 子项点击回调
    protected Action<int, bool, GameObject> FuncOnButtonClickCallBack; // 子项按钮回调

    // 尺寸记录字段
    protected float planeW; // ScrollView可视区域宽度
    protected float planeH; // ScrollView可视区域高度
    protected float contentW; // Content总宽度
    protected float contentH; // Content总高度
    protected float cellW; // 子项预制体宽度
    protected float cellH; // 子项预制体高度

    // 状态标志
    private bool isInit = false; // 是否已完成初始化
    protected GameObject content; // ScrollRect的Content对象引用
    protected ScrollRect scrollRect; // ScrollRect组件缓存
    protected RectTransform rectTrans; // 自身RectTransform
    protected RectTransform contentRectTrans; // Content的RectTransform

    // 列表控制字段
    protected int maxCount = -1; // 当前列表总数量（-1表示未初始化）
    protected int minIndex = -1; // 当前显示的最小索引
    protected int maxIndex = -1; // 当前显示的最大索引

    // 子项信息结构体
    protected struct CellInfo
    {
        public Vector3 pos; // 子项本地坐标
        public GameObject obj; // 子项实例对象
    };

    protected CellInfo[] cellInfos; // 存储所有子项信息的数组
    protected bool isClearList = false; // 是否强制清空列表标志

    // 对象池系统
    protected Stack<GameObject> Pool = new Stack<GameObject>(); // 子项对象池
    protected bool isInited = false; // 是否完成首次数据加载

    //===== 初始化方法 =====//
    /// <summary>
    /// 简化版初始化（只有数据回调）
    /// </summary>
    public virtual void Init(Action<GameObject, int> callBack)
    {
        // 调用完整初始化方法，点击回调设为null
        Init(callBack, null);
    }

    /// <summary>
    /// 完整初始化方法（带按钮回调）
    /// </summary>
    public virtual void Init(Action<GameObject, int> callBack, Action<GameObject, int> onClickCallBack,
        Action<int, bool, GameObject> onButtonClickCallBack)
    {
        // 存储按钮回调（如果存在）
        if (onButtonClickCallBack != null)
        {
            FuncOnButtonClickCallBack = onButtonClickCallBack;
        }

        // 继续执行标准初始化
        Init(callBack, onClickCallBack);
    }

    /// <summary>
    /// 核心初始化逻辑
    /// </summary>
    public virtual void Init(Action<GameObject, int> callBack, Action<GameObject, int> onClickCallBack)
    {
        // 清理旧数据（防止重复初始化问题）
        DisposeAll();

        // 存储数据绑定回调
        FuncCallBackFunc = callBack;

        // 存储点击回调（如果存在）
        if (onClickCallBack != null)
        {
            FuncOnClickCallBack = onClickCallBack;
        }

        // 如果已经初始化过，直接返回
        if (isInit) return;

        // 获取ScrollRect的content对象
        content = this.GetComponent<ScrollRect>().content.gameObject;

        // 如果未手动指定子项预制体，自动获取Content下第一个子对象
        if (cell == null)
        {
            cell = content.transform.GetChild(0).gameObject;
        }

        // 将子项模板放入对象池（初始隐藏）
        SetPoolsObj(cell);

        // 配置子项RectTransform
        RectTransform cellRectTrans = cell.GetComponent<RectTransform>();
        cellRectTrans.pivot = new Vector2(0f, 1f); // 设置轴心点为左上角
        CheckAnchor(cellRectTrans); // 验证锚点设置
        //cellRectTrans.anchoredPosition = Vector2.zero; // 重置位置（相对与锚点的位置，设为0）感觉和上面设置轴心、锚点位置重复

        // 记录子项原始尺寸
        cellH = cellRectTrans.rect.height; //rect表示物体矩形区域
        cellW = cellRectTrans.rect.width;

        // 记录ScrollView可视区域尺，这里默认Viewport矩形大小等于ScrollView矩形大小
        rectTrans = GetComponent<RectTransform>();
        Rect planeRect = rectTrans.rect;
        planeH = planeRect.height;
        planeW = planeRect.width;

        // 记录Content原始尺寸
        contentRectTrans = content.GetComponent<RectTransform>();
        //Rect contentRect = contentRectTrans.rect;
        //contentH = contentRect.height;
        //contentW = contentRect.width;//感觉没用

        // 计算实际使用的间距值
        row = Spacing.x; // 从Vector2获取行间距
        col = Spacing.y; // 从Vector2获取列间距
        if (row == 0 && col == 0)
        {
            // 如果未设置独立间距，使用统一间距
            row = col = squareSpacing;
        }
        else
        {
            // 如果使用独立间距，清空统一间距
            squareSpacing = 0;
        }

        // 配置Content的RectTransform
        contentRectTrans.pivot = new Vector2(0f, 1f); // 左上角轴心
        CheckAnchor(contentRectTrans); // 验证锚点

        // 获取ScrollRect组件并清除旧监听
        scrollRect = this.GetComponent<ScrollRect>();
        scrollRect.onValueChanged.RemoveAllListeners();

        // 添加滑动事件监听
        scrollRect.onValueChanged.AddListener(delegate(Vector2 value)
        {
            ScrollRectListener(value); // 监听滑动位置变化
        });

        // 标记初始化完成
        isInit = true;
    }

    /// <summary>
    /// 检查RectTransform的锚点设置是否符合当前滑动方向要求
    /// </summary>
    /// <param name="rectTrans">需要检查的RectTransform</param>
    private void CheckAnchor(RectTransform rectTrans)
    {
        // 垂直滑动模式的锚点验证
        if (dir == E_Direction.Vertical)
        {
            // 允许的锚点配置：
            // 1. 左上角固定锚点（min(0,1), max(0,1)）
            // 2. 顶部横向拉伸锚点（min(0,1), max(1,1)）
            if (!((rectTrans.anchorMin == new Vector2(0, 1) && rectTrans.anchorMax == new Vector2(0, 1)) ||
                  (rectTrans.anchorMin == new Vector2(0, 1) && rectTrans.anchorMax == new Vector2(1, 1))))
            {
                // 自动修正为顶部横向拉伸锚点
                rectTrans.anchorMin = new Vector2(0, 1);
                rectTrans.anchorMax = new Vector2(1, 1);
            }
        }
        // 水平滑动模式的锚点验证
        else
        {
            // 允许的锚点配置：
            // 1. 左上角固定锚点（min(0,1), max(0,1)）
            // 2. 左侧纵向拉伸锚点（min(0,0), max(0,1)）
            if (!((rectTrans.anchorMin == new Vector2(0, 1) && rectTrans.anchorMax == new Vector2(0, 1)) ||
                  (rectTrans.anchorMin == new Vector2(0, 0) && rectTrans.anchorMax == new Vector2(0, 1))))
            {
                // 自动修正为左侧纵向拉伸锚点
                rectTrans.anchorMin = new Vector2(0, 0);
                rectTrans.anchorMax = new Vector2(0, 1);
            }
        }
    }

    /// <summary>
    /// 核心列表显示方法
    /// </summary>
    /// <param name="num">要显示的子项总数</param>
    public virtual void ShowList(int num)
    {
        // 重置显示范围标记
        minIndex = -1;
        maxIndex = -1;

        //========== 计算Content尺寸 ==========//
        if (dir == E_Direction.Vertical)
        {
            // 垂直方向计算：
            // 总高度 = (单元格高度+列间距) * 行数 + 顶部内边距
            float contentSize = (col + cellH) * Mathf.CeilToInt((float)num / lines) + paddingTop;
            contentH = contentSize; // 记录内容高度
            contentW = contentRectTrans.sizeDelta.x + paddingLeft; // 宽度保持原有+左边距

            // 如果内容高度小于可视区域，则使用可视区域高度
            contentSize = contentSize < rectTrans.rect.height ? rectTrans.rect.height : contentSize;

            // 应用新尺寸
            contentRectTrans.sizeDelta = new Vector2(contentW, contentSize);

            // 如果列表数量变化，重置滚动位置到顶部
            if (num != maxCount)
            {
                contentRectTrans.anchoredPosition = new Vector2(contentRectTrans.anchoredPosition.x, 0);
            }
        }
        else
        {
            // 水平方向计算：
            // 总宽度 = (单元格宽度+行间距) * 列数 + 左侧内边距
            float contentSize = (row + cellW) * Mathf.CeilToInt((float)num / lines) + paddingLeft;
            contentW = contentSize;
            contentH = contentRectTrans.sizeDelta.x + paddingLeft;

            // 如果内容宽度小于可视区域，则使用可视区域宽度
            contentSize = contentSize < rectTrans.rect.width ? rectTrans.rect.width : contentSize;

            // 应用新尺寸
            contentRectTrans.sizeDelta = new Vector2(contentSize, contentH);

            // 如果列表数量变化，重置滚动位置到起始
            if (num != maxCount)
            {
                contentRectTrans.anchoredPosition = new Vector2(0, contentRectTrans.anchoredPosition.y);
            }
        }

        //========== 处理已有子项 ==========//
        int lastEndIndex = 0; // 旧数据的有效截止索引
        // 如果不是首次加载
        if (isInited)
        {
            // 计算需要保留的旧数据量
            lastEndIndex = num - maxCount > 0 ? maxCount : num;
            // 如果要求清空列表，则从0开始
            lastEndIndex = isClearList ? 0 : lastEndIndex;

            // 回收多余子项到对象池
            int count = isClearList ? cellInfos.Length : maxCount;
            for (int i = lastEndIndex; i < count; i++)
            {
                if (cellInfos[i].obj != null)
                {
                    SetPoolsObj(cellInfos[i].obj); // 回收到对象池
                    cellInfos[i].obj = null; // 清空引用
                }
            }
        }

        //========== 创建新数据数组 ==========//
        CellInfo[] tempCellInfos = cellInfos; // 临时保存旧数据
        cellInfos = new CellInfo[num]; // 创建新数组

        //========== 计算子项布局 ==========//
        for (int i = 0; i < num; i++)
        {
            //--> 复用已有数据
            if (maxCount != -1 && i < lastEndIndex)
            {
                CellInfo tempCellInfo = tempCellInfos[i];
                // 计算子项是否在可见范围内
                float rPos = dir == E_Direction.Vertical ? tempCellInfo.pos.y : tempCellInfo.pos.x;
                if (!IsOutRange(rPos))
                {
                    // 更新显示范围标记
                    minIndex = minIndex == -1 ? i : minIndex;
                    maxIndex = i;

                    // 如果对象不存在则从池中获取
                    if (tempCellInfo.obj == null)
                    {
                        tempCellInfo.obj = GetPoolsObj();
                    }

                    // 修正位置（使用localPosition避免z轴问题）
                    tempCellInfo.obj.transform.GetComponent<RectTransform>().localPosition = tempCellInfo.pos;
                    tempCellInfo.obj.name = i.ToString(); // 用索引命名便于调试
                    tempCellInfo.obj.SetActive(true);

                    // 执行数据绑定
                    Func(FuncCallBackFunc, tempCellInfo.obj);
                }
                else
                {
                    // 不可见的子项回收到对象池
                    SetPoolsObj(tempCellInfo.obj);
                    tempCellInfo.obj = null;
                }

                cellInfos[i] = tempCellInfo;
                continue;
            }

            //--> 创建新子项数据
            CellInfo cellInfo = new CellInfo();
            float pos = 0; // 主轴方向坐标
            float rowPos = 0; // 副轴方向坐标

            // 计算子项坐标
            if (dir == E_Direction.Vertical)
            {
                // 垂直布局计算：
                // Y坐标 = (单元格高度+列间距) * 行索引
                pos = cellH * Mathf.FloorToInt(i / lines) + col * Mathf.FloorToInt(i / lines);
                // X坐标 = (单元格宽度+行间距) * 行内索引
                rowPos = cellW * (i % lines) + row * (i % lines);
                // 最终位置 = (X+左内边距, -Y-顶内边距)
                cellInfo.pos = new Vector3(rowPos + paddingLeft, -pos - paddingTop, 0); //相对于左上角锚点位置
            }
            else
            {
                // 水平布局计算：
                // X坐标 = (单元格宽度+行间距) * 列索引
                pos = cellW * Mathf.FloorToInt(i / lines) + row * Mathf.FloorToInt(i / lines);
                // Y坐标 = (单元格高度+列间距) * 列内索引
                rowPos = cellH * (i % lines) + col * (i % lines);
                // 最终位置 = (X+左内边距, -Y-顶内边距)
                cellInfo.pos = new Vector3(pos + paddingLeft, -rowPos - paddingTop, 0); //相对于左上角锚点位置
            }

            // 检查是否在可见范围内
            float cellPos = dir == E_Direction.Vertical ? cellInfo.pos.y : cellInfo.pos.x;
            if (IsOutRange(cellPos))
            {
                cellInfo.obj = null;
                cellInfos[i] = cellInfo;
                continue;
            }

            // 更新显示范围标记
            minIndex = minIndex == -1 ? i : minIndex;
            maxIndex = i;

            // 从对象池获取子项实例
            GameObject cellObj = GetPoolsObj();
            cellObj.transform.GetComponent<RectTransform>().localPosition = cellInfo.pos;
            cellObj.name = i.ToString();

            // 存储到数组
            cellInfo.obj = cellObj;
            cellInfos[i] = cellInfo;

            // 执行数据绑定回调
            Func(FuncCallBackFunc, cellObj);
        }

        // 更新列表状态
        maxCount = num;
        isInited = true;

    }

    /// <summary>
    /// 滑动事件监听回调
    /// </summary>
    /// <param name="value">当前滑动位置归一化坐标</param>
    protected virtual void ScrollRectListener(Vector2 value)
    {
        // 触发可见性检查
        UpdateCheck();
    }

    /// <summary>
    /// 更新所有子项的可见状态
    /// </summary>
    private void UpdateCheck()
    {
        // 安全检查
        if (cellInfos == null) return;

        // 遍历所有子项
        for (int i = 0, length = cellInfos.Length; i < length; i++)
        {
            CellInfo cellInfo = cellInfos[i];
            GameObject obj = cellInfo.obj;
            Vector3 pos = cellInfo.pos;

            // 根据滑动方向获取关键坐标
            float rangePos = dir == E_Direction.Vertical ? pos.y : pos.x;

            // 判断是否超出显示范围
            if (IsOutRange(rangePos))
            {
                // 回收不可见的子项
                if (obj != null)
                {
                    SetPoolsObj(obj);
                    cellInfos[i].obj = null;
                }
            }
            else
            {
                // 处理需要显示的子项
                if (obj == null)
                {
                    // 从对象池获取或创建新子项
                    GameObject cell = GetPoolsObj();
                    // 设置位置和名称
                    cell.transform.localPosition = pos;
                    cell.gameObject.name = i.ToString();
                    // 更新引用
                    cellInfos[i].obj = cell;
                    // 执行数据绑定回调
                    Func(FuncCallBackFunc, cell);
                }
            }
        }
    }

    /// <summary>
    /// 判断坐标是否超出可见范围
    /// </summary>
    /// <param name="pos">待检查的坐标值</param>
    /// <returns>true表示不可见，false表示可见</returns>
    protected bool IsOutRange(float pos)
    {
        // 获取当前content的偏移量
        Vector3 listP = contentRectTrans.anchoredPosition;

        // 根据滑动方向分别判断
        if (dir == E_Direction.Vertical)
        {
            // 垂直方向判断：
            // - 超过一个单元格高度
            // - 完全滚出可视区域底部
            if (pos + listP.y > cellH || pos + listP.y < -rectTrans.rect.height) //listP.y为竖向滑动值，向下为负，向上为正
            {
                return true;
            }
        }
        else
        {
            // 水平方向判断：
            // - 超过一个单元格宽度
            // - 完全滚出可视区域右侧
            if (pos + listP.x < -cellW || pos + listP.x > rectTrans.rect.width)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 从对象池获取子项（优先复用）
    /// </summary>
    /// <returns>可用的子项对象</returns>
    protected virtual GameObject GetPoolsObj()
    {
        GameObject cell = null;

        // 1. 尝试从池中获取
        if (Pool != null && Pool.Count > 0)
        {
            cell = Pool.Pop();
        }

        // 2. 严谨性检查：如果 cell 为空，或者它意外地指向了预制体资产
        // cell.scene.IsValid() 为 false 说明该对象不在任何场景中，即它是资产
        if (cell == null || !cell.scene.IsValid())
        {
            if (cell != null && !cell.scene.IsValid())
            {
                Debug.LogWarning("检测到池中包含了预制体资产引用，已自动重新实例化副本。");
            }

            // 实例化新对象，并直接指定父物体（性能最优）
            cell = Instantiate<GameObject>(this.cell, content.transform);
        }
        else
        {
            // 3. 如果是从池中取出的有效实例，重新设置父子关系
            cell.transform.SetParent(content.transform, false);
        }

        // 4. 重置基础变换信息
        cell.transform.localScale = Vector3.one;
        cell.transform.localRotation = Quaternion.identity;

        // 5. 激活对象
        SetActive(cell, true);

        return cell;
    }

    /// <summary>
    /// 将子项回收到对象池
    /// </summary>
    /// <param name="cell">要回收的子项</param>
    protected virtual void SetPoolsObj(GameObject cell)
    {
        // 安全检查
        if (cell != null)
        {
            // 压入对象池
            Pool.Push(cell);
            // 禁用对象
            SetActive(cell, false);
        }
    }

    /// <summary>
    /// 执行回调函数的封装方法
    /// </summary>
    /// <param name="func">回调函数</param>
    /// <param name="selectObject">目标子项</param>
    /// <param name="isUpdate">是否为更新操作</param>
    protected void Func(Action<GameObject, int> func, GameObject selectObject, bool isUpdate = false)
    {
        // 从子项名称解析索引
        int index = int.Parse(selectObject.name);
        // 安全执行回调
        if (func != null)
        {
            func(selectObject, index);
        }
    }

    /// <summary>
    /// 清理所有回调引用（防止内存泄漏）
    /// </summary>
    public void DisposeAll()
    {
        if (FuncCallBackFunc != null) FuncCallBackFunc = null;
        if (FuncOnClickCallBack != null) FuncOnClickCallBack = null;
        if (FuncOnButtonClickCallBack != null) FuncOnButtonClickCallBack = null;
    }

    /// <summary>
    /// Unity销毁对象时自动调用
    /// </summary>
    protected void OnDestroy()
    {
        DisposeAll();
    }

    /// <summary>
    /// 安全设置对象激活状态
    /// </summary>
    /// <param name="obj">目标对象</param>
    /// <param name="isActive">要设置的状态</param>
    protected void SetActive(GameObject obj, bool isActive)
    {
        // 空安全检查
        if (obj != null)
        {
            obj.SetActive(isActive);
        }
    }
}