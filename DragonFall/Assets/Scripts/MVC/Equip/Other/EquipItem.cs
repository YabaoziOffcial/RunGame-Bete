/// <summary>
/// 通用装备（无弹幕，纯属性加成）。属性通过 WeaponConfig SO 的 levels 中 PlayerStatModifiers 生效。
/// </summary>
public class EquipItem : EquipBase
{
    public override void Enter(Player player) { }
    public override void Update(Player player) { }
    public override void FixedUpdate(Player player) { }
    public override void Exit(Player player) { }
    public override void LevelUp(Player player) { }
}
