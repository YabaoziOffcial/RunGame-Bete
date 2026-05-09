using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// ViewBase
// -> Y_PanelBase
// -> Y_PopupBase
public class ViewBase : UIBehaviour
{
    #region View
    public virtual void Show() { } // 显示UI

    public virtual void Load() { } // 可以在此处请求资源

    public virtual void Close() { } // 关闭UI

    public virtual void UnLoad() { } // 可以在此处回收资源
    #endregion
}
