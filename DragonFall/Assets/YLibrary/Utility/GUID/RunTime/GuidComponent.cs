using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
#endif

// 这个组件为 GameObject 提供一个稳定的、不可复制的全局唯一标识符。
// 它可以用于引用对象的特定实例，无论它位于何处。
// 这也可以用于其他系统，例如保存/加载游戏
[ExecuteInEditMode, DisallowMultipleComponent]
public class GuidComponent : MonoBehaviour, ISerializationCallbackReceiver // 序列化回调接口
{
    // 使用的是System.Guid 
    // Guid是一种128位的唯一标识符，由32位的UUID和64位的Clock序列组成
    System.Guid guid = System.Guid.Empty;
   
    // Unity 序列化无法识别System.Guid，因此我们将其转换为字节数组
    // 趣事：我们一开始使用字符串，但这会分配内存并且速度较慢两倍
    [SerializeField]
    private byte[] serializedGuid;


    void Awake()
    {
        CreateGuid();
    }

    /// <summary>
    ///  检查GUID是否已分配
    /// </summary>
    /// <returns></returns>
    public bool IsGuidAssigned()
    {
        return guid != System.Guid.Empty;
    }
    
    // 创建GUID
    void CreateGuid()
    {
        // if our serialized data is invalid, then we are a new object and need a new GUID
        if(serializedGuid == null || serializedGuid.Length != 16)
        {
#if UNITY_EDITOR
            // if in editor, make sure we aren't a prefab of some kind
            // 编辑器状态需要确保不是Prefab
            if(IsAssetOnDisk())
            {
                return;
            }
            Undo.RecordObject(this, "Added GUID");
#endif
            guid = System.Guid.NewGuid();           // System 的Guid
            serializedGuid = guid.ToByteArray();    // 序列化为byte数组

#if UNITY_EDITOR 
            // If we are creating a new GUID for a prefab instance of a prefab, but we have somehow lost our prefab connection
            // force a save of the modified prefab instance properties
            // 需要检查当前资产是否是预制体
            if(PrefabUtility.IsPartOfNonAssetPrefabInstance(this))
            {
                // 记录预制体事例属性的修改
                PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            }
#endif
        } else if(guid == System.Guid.Empty)
        {
            // otherwise, we should set our system guid to our serialized guid
            guid = new System.Guid(serializedGuid);
        }

        // 注册到GUID管理器
        if(guid != System.Guid.Empty)
        {
            if(!GuidManager.Add(this))
            {
                // if registration fails, we probably have a duplicate or invalid GUID, get us a new one.
                serializedGuid = null;
                guid = System.Guid.Empty;
                CreateGuid();
            }
        }
    }

#if UNITY_EDITOR

    /// <summary>
    ///  检查当前资产是否是预制体
    /// </summary>
    /// <returns></returns>
    private bool IsEditingInPrefabMode()
    {
        if(EditorUtility.IsPersistent(this))
        {
            // if the game object is stored on disk, it is a prefab of some kind, despite not returning true for IsPartOfPrefabAsset =/
            return true;
        } else
        {
            // If the GameObject is not persistent let's determine which stage we are in first because getting Prefab info depends on it
            var mainStage = StageUtility.GetMainStageHandle();
            var currentStage = StageUtility.GetStageHandle(gameObject);
            if(currentStage != mainStage)
            {
                var prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);
                if(prefabStage != null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsAssetOnDisk()
    {
        return PrefabUtility.IsPartOfPrefabAsset(this) || IsEditingInPrefabMode();
    }
#endif

    // We cannot allow a GUID to be saved into a prefab, and we need to convert to byte[]
    /// <summary>
    /// 在序列化之后，我们需要将GUID转换为字节数组，以便在加载时恢复
    /// </summary>
    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        // This lets us detect if we are a prefab instance or a prefab asset.
        // A prefab asset cannot contain a GUID since it would then be duplicated when instanced.
        if(IsAssetOnDisk())
        {
            serializedGuid = null;
            guid = System.Guid.Empty;
        } else
#endif
        {
            if(guid != System.Guid.Empty)
            {
                serializedGuid = guid.ToByteArray();
            }
        }
    }

    // On load, we can go head a restore our system guid for later use
    /// <summary>
    /// 在反序列化之后，我们可以恢复系统GUID
    /// </summary>
    public void OnAfterDeserialize()
    {
        if(serializedGuid != null && serializedGuid.Length == 16)
        {
            guid = new System.Guid(serializedGuid);
        }
    }

    


    /// <summary>
    ///  验证组件是否有效(检视组件)
    /// </summary>
    void OnValidate()
    {
#if UNITY_EDITOR
        // similar to on Serialize, but gets called on Copying a Component or Applying a Prefab
        // at a time that lets us detect what we are
        if(IsAssetOnDisk())
        {
            serializedGuid = null;
            guid = System.Guid.Empty;
        } else
#endif
        {
            CreateGuid();
        }
    }

    // Never return an invalid GUID
    public System.Guid GetGuid()
    {
        if(guid == System.Guid.Empty && serializedGuid != null && serializedGuid.Length == 16)
        {
            guid = new System.Guid(serializedGuid);
        }

        return guid;
    }

    // let the manager know we are gone, so other objects no longer find this
    public void OnDestroy()
    {
        GuidManager.Remove(guid);
    }
}
