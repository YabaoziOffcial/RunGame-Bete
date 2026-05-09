using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using YBZ.Design;


// 消息分发器类
// 暂时能用
public class MessageDispatcher : D_MonoSingleton<MessageDispatcher>
{
    /// <summary>
    /// 消息处理类字典
    /// </summary>
    private Dictionary<CmdType, List<HandlerBase>> m_MsgHandlerDict = new();

    public Dictionary<CmdType, List<HandlerBase>> MsgHandlerDict { get => m_MsgHandlerDict; set => m_MsgHandlerDict = value; }


    /// <summary>
    /// 消息处理类缓存队列
    /// </summary>
    private readonly Queue<HandlerBase> m_MsgHandlerQueue = new Queue<HandlerBase>();
    /// <summary>
    /// 消息包缓存队列
    /// </summary>
    private readonly Queue<CmdData> m_DataQueue = new Queue<CmdData>();


    protected override void Initialize()
    {
        Clear();
    }

    public void Clear()
    {
        m_MsgHandlerQueue.Clear();
        m_DataQueue.Clear();
        MsgHandlerDict.Clear();
    }


    // 注册消息监听
    public  void Register(CmdType messageType, HandlerBase handler)
    {
        if(!MsgHandlerDict.ContainsKey(messageType))
        {
            MsgHandlerDict[messageType] = new List<HandlerBase>();
        }
        MsgHandlerDict[messageType].Add(handler);
    }

    public void Unregister(CmdType messageType, HandlerBase handler)
    {
        if(MsgHandlerDict.ContainsKey(messageType))
        {
            MsgHandlerDict[messageType].Remove(handler);
        }
    }

    // 移除消息监听
    public  void Unregister(CmdType messageType)
    {
        if(MsgHandlerDict.ContainsKey(messageType))
        {
            MsgHandlerDict.Remove(messageType);
        }
    }

    // 发送消息
    public void Dispatch(CmdData data)
    {
        if(MsgHandlerDict.ContainsKey(data.cmdType))
        {
            int handlerCount = MsgHandlerDict[data.cmdType].Count;
            for(int i = 0; i < handlerCount; i ++)
            {
                m_MsgHandlerQueue.Enqueue(MsgHandlerDict[data.cmdType][i]);
                m_DataQueue.Enqueue(data);
            }
        } else
        {
            Debug.LogError("消息处理类未注册：" + data.cmdType);
        }
    }


    public void Update()
    {
        if(m_MsgHandlerQueue.Count <= 0 || m_DataQueue.Count <= 0)
        {
            return;
        }

        while(true)
        {
            if(m_MsgHandlerQueue.Count <= 0 || m_DataQueue.Count <= 0) return;
            var handler = m_MsgHandlerQueue.Dequeue();
            var data = m_DataQueue.Dequeue();

            // Debug.Log($"接受消息 type = {data.cmdType}, message = {JsonUtility.ToJson(data.message.ToString())}");
            handler?.HandlerMsg(data.message);
        }
    }
}

public enum CmdType

{
    None,
    SendMsg,        // 发送信息
    ReceiveMsg,     // 接受信息
    History,        // 历史消息

    LastLoginTime,  // 最后登录时间
    
    PetActionMsg,   // 宠物行为
    RPC,            // 文字语音相互转化
}


[System.Serializable]
public struct CmdData
{
    public CmdType cmdType;         // 消息类型
    public IMessage message;        // 消息内容
}