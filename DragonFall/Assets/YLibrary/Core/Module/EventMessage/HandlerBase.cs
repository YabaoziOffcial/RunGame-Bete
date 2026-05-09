using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 消息处理
/// </summary>
public abstract class HandlerBase
{
    public abstract void HandlerMsg(IMessage pMsg);
}
