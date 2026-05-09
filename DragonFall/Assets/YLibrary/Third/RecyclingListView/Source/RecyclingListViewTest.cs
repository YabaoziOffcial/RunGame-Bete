using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RecyclingListViewVertical : MonoBehaviour
{
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    [SerializeField] private List<RecyclableListItem> itemPrefabs;
    [SerializeField] private float spacing = 10f;
    
    private float viewportHeight;
    private float contentHeight;
    private Vector2 lastPosition;
    private List<RecyclableListItem> activeItems = new List<RecyclableListItem>();
    private Queue<RecyclableListItem> inactiveItems = new Queue<RecyclableListItem>();
    private List<ListItemData> dataSource = new List<ListItemData>();
    private Dictionary<int, float> itemHeights = new Dictionary<int, float>();
    private float[] itemPositions;

    // 初始化列表
    public void Initialize(List<ListItemData> data)
    {
        dataSource = data;
        viewportHeight = viewport.rect.height;
        CalculateItemHeights();
        LayoutContent();
        UpdateVisibleItems();
        lastPosition = content.anchoredPosition;
        
        // 添加滚动监听
        GetComponent<ScrollRect>().onValueChanged.AddListener(OnScroll);
    }
    
    // 计算所有子物体高度
    private void CalculateItemHeights()
    {
        itemHeights.Clear();
        itemPositions = new float[dataSource.Count];
        
        for (int i = 0; i < dataSource.Count; i++)
        {
            // 获取对应类型的预制体高度
            var prefab = itemPrefabs[(int)dataSource[i].itemType];
            float height = prefab.GetComponent<RectTransform>().sizeDelta.y;
            itemHeights[i] = height;
            
            // 计算每个子物体的位置
            itemPositions[i] = i == 0 ? 0 : itemPositions[i - 1] + itemHeights[i - 1] + spacing;
        }
        
        // 设置Content总高度
        contentHeight = dataSource.Count > 0 
            ? itemPositions[dataSource.Count - 1] + itemHeights[dataSource.Count - 1] 
            : 0;
        content.sizeDelta = new Vector2(content.sizeDelta.x, contentHeight);
    }
    
    // 布局Content并更新可见项
    private void LayoutContent()
    {
        // 回收所有活动项
        foreach (var item in activeItems)
        {
            item.gameObject.SetActive(false);
            inactiveItems.Enqueue(item);
        }
        activeItems.Clear();
        
        // 计算可见区域
        float offset = content.anchoredPosition.y;
        float visibleStart = offset;
        float visibleEnd = offset + viewportHeight;
        
        // 确定可见项范围
        int startIndex = FindFirstVisibleItemIndex(visibleStart);
        int endIndex = FindLastVisibleItemIndex(visibleEnd, startIndex);
        
        // 生成可见项
        for (int i = startIndex; i <= endIndex; i++)
        {
            CreateOrUpdateItem(i);
        }
    }
    
    // 查找第一个可见项索引
    private int FindFirstVisibleItemIndex(float visibleStart)
    {
        int low = 0;
        int high = dataSource.Count - 1;
        int result = 0;
        
        while (low <= high)
        {
            int mid = (low + high) / 2;
            float itemStart = itemPositions[mid];
            float itemEnd = itemStart + itemHeights[mid];
            
            if (itemEnd >= visibleStart)
            {
                result = mid;
                high = mid - 1;
            }
            else
            {
                low = mid + 1;
            }
        }
        
        return Mathf.Clamp(result, 0, dataSource.Count - 1);
    }
    
    // 查找最后一个可见项索引
    private int FindLastVisibleItemIndex(float visibleEnd, int startIndex)
    {
        int low = startIndex;
        int high = dataSource.Count - 1;
        int result = startIndex;
        
        while (low <= high)
        {
            int mid = (low + high) / 2;
            float itemStart = itemPositions[mid];
            
            if (itemStart <= visibleEnd)
            {
                result = mid;
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }
        
        return Mathf.Clamp(result, 0, dataSource.Count - 1);
    }
    
    // 创建或更新子物体
    private void CreateOrUpdateItem(int index)
    {
        RecyclableListItem item;
        
        // 从队列中获取或实例化新项
        if (inactiveItems.Count > 0)
        {
            item = inactiveItems.Dequeue();
            item.gameObject.SetActive(true);
        }
        else
        {
            var prefab = itemPrefabs[(int)dataSource[index].itemType];
            item = Instantiate(prefab, content);
        }
        
        // 设置位置和数据
        float yPosition = -itemPositions[index];
        item.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPosition);
        item.SetData(dataSource[index]);
        item.Index = index;
        
        activeItems.Add(item);
    }
    
    // 滚动时更新可见项
    private void OnScroll(Vector2 scrollPosition)
    {
        float scrollDelta = content.anchoredPosition.y - lastPosition.y;
        
        if (Mathf.Abs(scrollDelta) > 10f) // 设置一个阈值，避免频繁更新
        {
            UpdateVisibleItems();
            lastPosition.y = content.anchoredPosition.y;
        }
    }
    
    // 更新可见项
    private void UpdateVisibleItems()
    {
        // 计算可见区域
        float offset = content.anchoredPosition.y;
        float visibleStart = offset;
        float visibleEnd = offset + viewportHeight;
        
        // 回收不可见项
        for (int i = activeItems.Count - 1; i >= 0; i--)
        {
            var item = activeItems[i];
            float itemStart = -item.GetComponent<RectTransform>().anchoredPosition.y;
            float itemEnd = itemStart + itemHeights[item.Index];
            
            if (itemEnd < visibleStart || itemStart > visibleEnd)
            {
                item.gameObject.SetActive(false);
                inactiveItems.Enqueue(item);
                activeItems.RemoveAt(i);
            }
        }
        
        // 确定可见项范围
        int startIndex = FindFirstVisibleItemIndex(visibleStart);
        int endIndex = FindLastVisibleItemIndex(visibleEnd, startIndex);
        
        // 添加新可见项
        for (int i = startIndex; i <= endIndex; i++)
        {
            if (!IsItemActive(i))
            {
                CreateOrUpdateItem(i);
            }
        }
    }
    
    // 检查项是否已激活
    private bool IsItemActive(int index)
    {
        foreach (var item in activeItems)
        {
            if (item.Index == index)
            {
                return true;
            }
        }
        return false;
    }
}