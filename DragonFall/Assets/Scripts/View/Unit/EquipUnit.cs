using UnityEngine;
using UnityEngine.UI;

// 升级选卡单项：展示武器图标、名称、等级预览与效果描述
public class EquipUnit : MonoBehaviour
{
    public Image equipIcon;           // 武器图标
    public Text equipName;            // 武器名称
    public Text equipLevel;           // 新装备 / 升级等级预览
    public Text equipDescription;     // 选中后将生效的等级描述

    public EquipData equipData;

    // 按 WeaponConfigSO 刷新卡片（升级三选一主入口）
    public void Refresh(WeaponConfigSO config)
    {
        if (config == null) return;

        EquipManager equipManager = EquipManager.Instance;

        equipData = config.CreateEquipData();
        equipIcon.sprite = config.iconSprite;
        equipName.text = config.weaponName;
        equipDescription.text = equipManager.GetEquipChoiceDescription(config);

        if (equipManager.HasEquip(config))
        {
            equipLevel.text = equipManager.GetEquipLevelPreview(config);
        }
        else
        {
            equipLevel.text = "新装备";
        }
    }

    // 按 EquipData 刷新卡片（兼容已有调用）
    public void Refresh(EquipData equipData)
    {
        if (equipData == null) return;

        this.equipData = equipData;

        if (equipData.weaponConfig != null)
        {
            Refresh(equipData.weaponConfig);
            return;
        }

        EquipManager equipManager = EquipManager.Instance;

        equipIcon.sprite = equipData.iconSprite;
        equipName.text = equipData.name;
        equipDescription.text = equipData.description;

        if (equipManager.HasEquip(equipData.className))
        {
            equipLevel.text = equipManager.GetEquipLevelPreview(equipData.className);
        }
        else
        {
            equipLevel.text = "新装备";
        }
    }

    public void OnEquipUnitClick()
    {
        if (equipData == null) return;
        if (EquipManager.Instance.HasEquip(equipData.className))
        {
            EquipManager.Instance.UpgradeEquip(equipData.className);
        }
        else
        {
            EquipManager.Instance.AddEquip(equipData.className);
        }
        GameController.Instance.CompleteCurrentLevelUpSelection();
    }
}
