using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// GamePanel 上的装备栏显示组件
public class EquipUnit : MonoBehaviour
{
    [Header("Template")]
    // 装备模板，通常命名为 EquipTemplate，运行时会被隐藏并用于克隆
    [SerializeField] private Transform m_EquipTemplate;
    // 装备模板生成到的父节点，未配置时使用模板父节点
    [SerializeField] private Transform m_EquipRoot;
    // 模板内图标节点名称；找不到时会自动使用模板下第一个 Image
    [SerializeField] private string m_IconChildName = "Icon";

    private readonly List<Transform> m_ItemList = new List<Transform>();
    private EquipManager m_BoundEquipManager;

    private void Awake()
    {
        AutoResolveTemplate();
        if (m_EquipTemplate != null)
        {
            if (m_EquipRoot == null)
            {
                m_EquipRoot = m_EquipTemplate.parent;
            }

            m_EquipTemplate.SetActive(false);
        }
    }

    private void OnEnable()
    {
        BindEquipManager();
        Refresh();
    }

    private void OnDisable()
    {
        UnbindEquipManager();
    }

    // 外部主动刷新入口，例如 GamePanel.Show 时调用
    public void Refresh()
    {
        BindEquipManager();
        Player player = GameController.Instance != null ? GameController.Instance.Player : null;
        EquipManager equipManager = player != null ? player.EquipManager : null;
        Refresh(equipManager != null ? equipManager.CurrentEquips : null);
    }

    // 按当前装备列表刷新 UI
    public void Refresh(IReadOnlyList<EquipBase> currentEquips)
    {
        if (m_EquipTemplate == null || m_EquipRoot == null)
        {
            return;
        }

        int equipCount = currentEquips != null ? currentEquips.Count : 0;
        EnsureItemCount(equipCount);

        for (int i = 0; i < m_ItemList.Count; i++)
        {
            bool active = i < equipCount;
            Transform item = m_ItemList[i];
            item.SetActive(active);
            if (!active) continue;

            SetItem(item, currentEquips[i]);
        }
    }

    private void BindEquipManager()
    {
        Player player = GameController.Instance != null ? GameController.Instance.Player : null;
        EquipManager equipManager = player != null ? player.EquipManager : null;
        if (m_BoundEquipManager == equipManager) return;

        UnbindEquipManager();
        m_BoundEquipManager = equipManager;
        if (m_BoundEquipManager != null)
        {
            m_BoundEquipManager.CurrentEquipsChanged += Refresh;
        }
    }

    private void UnbindEquipManager()
    {
        if (m_BoundEquipManager == null) return;

        m_BoundEquipManager.CurrentEquipsChanged -= Refresh;
        m_BoundEquipManager = null;
    }

    private void AutoResolveTemplate()
    {
        if (m_EquipTemplate != null) return;

        Transform equipTemplate = transform.Find("EquipTemplate");
        if (equipTemplate != null)
        {
            m_EquipTemplate = equipTemplate;
            return;
        }

        if (transform.childCount > 0)
        {
            m_EquipTemplate = transform.GetChild(0);
        }
    }

    private void EnsureItemCount(int count)
    {
        while (m_ItemList.Count < count)
        {
            Transform item = Instantiate(m_EquipTemplate, m_EquipRoot);
            item.name = $"{m_EquipTemplate.name}_{m_ItemList.Count + 1}";
            item.SetActive(true);
            m_ItemList.Add(item);
        }
    }

    private void SetItem(Transform item, EquipBase equip)
    {
        Image iconImage = GetIconImage(item);
        if (iconImage == null) return;

        Sprite iconSprite = GetIconSprite(equip);
        iconImage.sprite = iconSprite;
        iconImage.enabled = iconSprite != null;
        iconImage.preserveAspect = true;
    }

    private Image GetIconImage(Transform item)
    {
        Transform iconTransform = string.IsNullOrEmpty(m_IconChildName) ? null : item.Find(m_IconChildName);
        if (iconTransform != null)
        {
            Image iconImage = iconTransform.GetComponent<Image>();
            if (iconImage != null) return iconImage;
        }

        return item.GetComponentInChildren<Image>(true);
    }

    private Sprite GetIconSprite(EquipBase equip)
    {
        EquipData equipData = equip != null ? equip.EquipData : null;
        if (equipData == null) return null;
        if (equipData.iconSprite != null) return equipData.iconSprite;
        if (string.IsNullOrEmpty(equipData.iconPath)) return null;

        return ResourceManager.Instance.LoadRes<Sprite>(equipData.iconPath);
    }
}
