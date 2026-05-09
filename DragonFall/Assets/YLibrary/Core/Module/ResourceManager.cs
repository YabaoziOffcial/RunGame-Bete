using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YBZ.Design;
using UObject = UnityEngine.Object;
using System.IO;
using UnityEngine.SceneManagement;
#if YooAsset
using YooAsset;
#endif

public class ResourceManager : D_MonoSingleton<ResourceManager>
{
    private Dictionary<string, UObject> mResDic = new Dictionary<string, UObject>();

#if YooAsset
    private ResourcePackage mMainPackage;
    static YooConfig yooConfig;
#endif
    Action IntSucCall;

    public void Init(Action action = null)
    {
        // 计划在这里完成一些异步预加载
        this.IntSucCall = action;
#if YooAsset
        yooConfig = Resources.Load<YooConfig>("YooConfig");
        YooAssets.Initialize();
        mMainPackage = YooAssets.CreatePackage(yooConfig.PackageName);
        YooAssets.SetDefaultPackage(mMainPackage);
        mResDic = new Dictionary<string, UObject>();
        YooAssets.SetCacheSystemDisableCacheOnWebGL();

#if UNITY_EDITOR
        StartCoroutine(InitializeMainEditor());
#else
        StartCoroutine(InitializeMainOffline());
#endif
        // switch (yooConfig.PlayMode)
        // {
        //     case EPlayMode.EditorSimulateMode:
        //         StartCoroutine(InitializeMainEditor());
        //         break;
        //     case EPlayMode.OfflinePlayMode:
        //         StartCoroutine(InitializeMainOffline());
        //         break;
        //     case EPlayMode.HostPlayMode:
        //         StartCoroutine(NetInitializeYooAsset());
        //         break;
        //     case EPlayMode.WebPlayMode:
        //         StartCoroutine(WebInitializeYooAsset());
        //         break;
        // }
#else
        this.IntSucCall?.Invoke();
#endif
    }

    public GameObject LoadRes(string pPath)
    {
        return LoadRes<GameObject>(pPath);
    }

    public T LoadRes<T>(string pPath) where T : UObject
    {
        if (!mResDic.ContainsKey(pPath))
        {
#if YooAsset
            bool isEnableAddressable = YooAsset.Editor.AssetBundleCollectorSettingData.Setting.Packages[0].EnableAddressable;
            string name = "";
            if (isEnableAddressable)
            {
                name = Path.GetFileNameWithoutExtension(pPath);
            }
            else
            {
                name = "Assets/GameAsset/" + pPath; // 不采用可寻址
            }
            // string name = Path.GetFileNameWithoutExtension(pPath); // 只得到名字
            AssetHandle pHandler = mMainPackage.LoadAssetSync<T>(name);
            mResDic[pPath] = pHandler.AssetObject;
            pHandler.Release();
#else
            mResDic[pPath] = Resources.Load<T>(pPath);
#endif
        }
        if (mResDic[pPath] == null)
        {
            Y_Debug.LogRed("LoadRes: " + pPath + " 不存在");
        }
        return mResDic[pPath] as T;
    }

    public void LoadScene(string pPath, Action doneCall = null, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
#if YooAsset
        bool isEnableAddressable = YooAsset.Editor.AssetBundleCollectorSettingData.Setting.Packages[0].EnableAddressable;
        string name = "";
        if (isEnableAddressable)
        {
            name = Path.GetFileNameWithoutExtension(pPath);
        }
        else
        {
            name = "Assets/GameAsset/" + pPath; // 不采用可寻址
        }
        var handle = mMainPackage.LoadSceneAsync(name, loadSceneMode);
        handle.Completed += (op) =>
        {
            Debug.Log("场景加载完成：" + pPath);
            doneCall?.Invoke();
        };
#else
        UnityEngine.SceneManagement.SceneManager.LoadScene(pPath, loadSceneMode);
#endif
    }


    public void LoadSceneAsync(string pPath, Action doneCall = null, LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool suspendLoad = false)
    {
#if YooAsset

        // YooAsset 的异步本质是协程，所以这里返回的是协程
        bool isEnableAddressable = YooAsset.Editor.AssetBundleCollectorSettingData.Setting.Packages[0].EnableAddressable;
        string name = "";
        if (isEnableAddressable)
        {
            name = Path.GetFileNameWithoutExtension(pPath);
        }
        else
        {
            name = "Assets/GameAsset/" + pPath; // 不采用可寻址
        }
        var handle = mMainPackage.LoadSceneAsync(name, loadSceneMode, suspendLoad);
        handle.Completed += (op) =>
        {
            Debug.Log("场景加载完成：" + pPath);
            doneCall?.Invoke();
        };
#else
        var op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(pPath, loadSceneMode);
        if (op != null) op.allowSceneActivation = !suspendLoad;
#endif
    }

    public async System.Threading.Tasks.Task<T> LoadResAsync<T>(string pPath) where T : UObject
    {
        if (mResDic.ContainsKey(pPath))
        {
            return mResDic[pPath] as T;
        }
        ResourceRequest request = Resources.LoadAsync<T>(pPath);
        while (!request.isDone)
        {
            await System.Threading.Tasks.Task.Yield();
        }

        mResDic[pPath] = request.asset;
        return request.asset as T;
    }

    public void UnloadRes(string pPath)
    {
        if (mResDic.ContainsKey(pPath))
        {
            Resources.UnloadAsset(mResDic[pPath]);
            mResDic.Remove(pPath);
        }
    }

    public void Release()
    {
        mResDic.Clear();
#if YooAsset
        mMainPackage.UnloadUnusedAssets();
#endif
    }

    #region YooAsset相关
#if YooAsset
    private IEnumerator InitializeMainEditor()
    {
        var initParameters = new EditorSimulateModeParameters();
        var simulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(EDefaultBuildPipeline.BuiltinBuildPipeline, yooConfig.PackageName);
        initParameters.SimulateManifestFilePath = simulateManifestFilePath;
        var initOperation = mMainPackage.InitializeAsync(initParameters);
        yield return initOperation;
        IntSucCall?.Invoke();

        if (initOperation.Status == EOperationStatus.Succeed)
            Debug.Log("资源包初始化成功！");
        else
            Debug.LogError($"资源包初始化失败：{initOperation.Error}");
    }

    // 离线模式
    private IEnumerator InitializeMainOffline()
    {
        var initParameters = new OfflinePlayModeParameters();
        initParameters.DecryptionServices = new TKResDecryption();
        var initOperation = mMainPackage.InitializeAsync(initParameters);
        yield return initOperation;
        IntSucCall?.Invoke();

        if (initOperation.Status == EOperationStatus.Succeed)
            Debug.Log("资源包初始化成功！");
        else
            Debug.LogError($"资源包初始化失败：{initOperation.Error}");
    }

    public static void DealData(byte[] pData)
    {
        int tIndex = 0;
        byte[] tCodes = new byte[] { 11, 2, 1, 5, 8, 50 };
        for (int i = 0; i < pData.Length; i += 500)
        {
            pData[i] = (byte)(pData[i] ^ tCodes[tIndex]);
            tIndex++;
            tIndex = tIndex % tCodes.Length;
        }
    }

    public class TKResEncryption : IEncryptionServices
    {
        public EncryptResult Encrypt(EncryptFileInfo pFileInfo)
        {
            byte[] tFileData = File.ReadAllBytes(pFileInfo.FilePath);
            ResourceManager.DealData(tFileData);

            EncryptResult result = new EncryptResult();
            result.Encrypted = true;
            result.EncryptedData = tFileData;
            return result;
        }
    }

    public class TKResDecryption : IDecryptionServices
    {
        public AssetBundle LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
        {
            Debug.Log("LoadAssetBundle");
            byte[] encryptedData = File.ReadAllBytes(fileInfo.FileLoadPath);
            ResourceManager.DealData(encryptedData); // 解密数据，使用相同的 DealData 方法

            managedStream = new MemoryStream(encryptedData);
            return AssetBundle.LoadFromMemory(encryptedData);
        }

        public AssetBundleCreateRequest LoadAssetBundleAsync(DecryptFileInfo fileInfo, out Stream managedStream)
        {
            Debug.Log("LoadAssetBundleAsync");
            byte[] encryptedData = File.ReadAllBytes(fileInfo.FileLoadPath);
            ResourceManager.DealData(encryptedData); // 解密数据，使用相同的 DealData 方法

            managedStream = new MemoryStream(encryptedData);
            return AssetBundle.LoadFromMemoryAsync(encryptedData);
        }
    }


    //联机模式的代码
    private IEnumerator NetInitializeYooAsset()
    {
        var initParameters = new HostPlayModeParameters();
        initParameters.BuildinQueryServices = new GameQueryServices();
        initParameters.DecryptionServices = new TKResDecryption();
#if UNITY_ANDROID
        initParameters.RemoteServices = new RemoteServices(yooConfig.AndroidNetPath, yooConfig.AndroidNetPath);
#elif UNITY_IOS
		initParameters.RemoteServices = new RemoteServices(yooConfig.IOSNetPath, yooConfig.IOSNetPath);
#endif


        var initOperation = mMainPackage.InitializeAsync(initParameters);
        yield return initOperation;

        if (initOperation.Status == EOperationStatus.Succeed)
        {
            Debug.Log("资源包初始化成功！");
        }
        else
        {
            Debug.LogError($"资源包初始化失败：{initOperation.Error}");
        }

        IntSucCall?.Invoke();
    }


    //web模式初始化
    private IEnumerator WebInitializeYooAsset()
    {
        var initParameters = new WebPlayModeParameters();
        initParameters.BuildinQueryServices = new GameQueryServices();
        initParameters.RemoteServices = new RemoteServices(yooConfig.WebNetPath, yooConfig.WebNetPath);
        var initOperation = mMainPackage.InitializeAsync(initParameters);
        yield return initOperation;

        if (initOperation.Status == EOperationStatus.Succeed)
        {
            Debug.Log("资源包初始化成功！");
        }
        else
        {
            Debug.LogError($"资源包初始化失败：{initOperation.Error}");
        }
        IntSucCall?.Invoke();
    }

    /// <summary>
    /// 远端资源地址查询服务类
    /// </summary>
    private class RemoteServices : IRemoteServices
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
        }
        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{_defaultHostServer}/{fileName}";
        }
        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{_fallbackHostServer}/{fileName}";
        }
    }

    string packageVersion = "1.0.0";
    public IEnumerator UpdatePackageVersion(Action<bool> call)
    {
        //2.获取资源版本
        var operation = mMainPackage.UpdatePackageVersionAsync();
        yield return operation;
        if (operation.Status != EOperationStatus.Succeed)
        {
            Debug.LogError("版本号更新失败，可能是找不到服务器");
            call?.Invoke(false);
            yield break;
        }
        //这是获取到的版本号，在下一个步骤要用
        packageVersion = operation.PackageVersion;
        Debug.Log("获取到了线上版本号：" + packageVersion);


        //3.获取补丁清单
        var op = mMainPackage.UpdatePackageManifestAsync(packageVersion);
        yield return op;
        if (op.Status != EOperationStatus.Succeed)
        {
            call?.Invoke(false);
            Debug.LogError("Mainfest更新失败！");
        }
        else
        {
            call?.Invoke(true);
        }

    }

    int downloadingMaxNum = 10;
    int failedTryAgain = 3;
    int timeout = 60;

    public IEnumerator Download(Action<bool> stateCall, Action<float> downloadCall)
    {
        var downloader = mMainPackage.CreateResourceDownloader(downloadingMaxNum, failedTryAgain, timeout);
        //下载数量是0，直接就完成了
        if (downloader.TotalDownloadCount == 0)
        {
            Debug.Log("没有资源要下载");
            stateCall?.Invoke(true);
            yield break;
        }

        //注册一些回调
        downloader.OnDownloadErrorCallback += (string fileName, string error) =>
        {
            Debug.Log("下载失败:" + fileName + "错误信息:" + error);
        };
        downloader.OnDownloadProgressCallback += (int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes) =>
        {

            float val = ((float)currentDownloadBytes / (float)totalDownloadBytes);
            Debug.Log("下载进度:" + val);
            downloadCall?.Invoke(val);
        };
        downloader.OnDownloadOverCallback += (bool suc) =>
        {
            Debug.Log("下载结束：" + suc);
        };
        downloader.OnStartDownloadFileCallback += (string fileName, long sizeBytes) =>
        {
            Debug.Log("开始下载：" + fileName + " 文件大小：" + sizeBytes);
        };

        //开始下载
        downloader.BeginDownload();
        //等待下载完成
        yield return downloader;
        //检查状态
        if (downloader.Status == EOperationStatus.Succeed)
        {
            Debug.Log("下载完成");
            stateCall?.Invoke(true);
        }
        else
        {
            Debug.Log("下载失败");
            stateCall?.Invoke(false);
        }
    }
    //联机模式结束


    /// <summary>
	/// 资源文件查询服务类
	/// </summary>
	public class GameQueryServices : IBuildinQueryServices
    {
        /// <summary>
        /// 查询内置文件的时候，是否比对文件哈希值
        /// </summary>
        public static bool CompareFileCRC = false;

        public bool Query(string packageName, string fileName, string fileCRC)
        {
            // 注意：fileName包含文件格式
            return StreamingAssetsHelper.FileExists(packageName, fileName, fileCRC);
        }
    }

#if UNITY_EDITOR
    public sealed class StreamingAssetsHelper
    {
        public static void Init() { }
        public static bool FileExists(string packageName, string fileName, string fileCRC)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, yooConfig.PackageName, packageName, fileName);
            if (File.Exists(filePath))
            {
                if (GameQueryServices.CompareFileCRC)
                {
                    string crc32 = YooAsset.Editor.EditorTools.GetFileCRC32(filePath);
                    return crc32 == fileCRC;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
    }
#else
    public sealed class StreamingAssetsHelper
    {
        private class PackageQuery
        {
            public readonly Dictionary<string, BuildinFileManifest.Element> Elements = new Dictionary<string, BuildinFileManifest.Element>(1000);
        }

        private static bool _isInit = false;
        private static readonly Dictionary<string, PackageQuery> _packages = new Dictionary<string, PackageQuery>(10);

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            if (_isInit == false)
            {
                _isInit = true;

                var manifest = Resources.Load<BuildinFileManifest>("BuildinFileManifest");
                if (manifest != null)
                {
                    foreach (var element in manifest.BuildinFiles)
                    {
                        if (_packages.TryGetValue(element.PackageName, out PackageQuery package) == false)
                        {
                            package = new PackageQuery();
                            _packages.Add(element.PackageName, package);
                        }
                        package.Elements.Add(element.FileName, element);
                    }
                }
            }
        }

        /// <summary>
        /// 内置文件查询方法
        /// </summary>
        public static bool FileExists(string packageName, string fileName, string fileCRC32)
        {
            if (_isInit == false)
                Init();

            if (_packages.TryGetValue(packageName, out PackageQuery package) == false)
                return false;

            if (package.Elements.TryGetValue(fileName, out var element) == false)
                return false;

            if (GameQueryServices.CompareFileCRC)
            {
                return element.FileCRC32 == fileCRC32;
            }
            else
            {
                return true;
            }
        }
}
#endif
#endif

    #endregion


    #region 场景加载（统一句柄）
    /// <summary>
    /// 统一的“场景异步加载句柄”，用于屏蔽 Unity AsyncOperation 与 YooAsset SceneHandle 的差异。
    /// 你可以用它在 LoadingView 里读取 Progress、监听 Completed，并决定是否允许激活场景。
    /// </summary>
    public interface ISceneLoadHandle
    {
        /// <summary>0~1 的进度（对 0~0.9 做了归一化）</summary>
        float Progress01 { get; }
        /// <summary>是否完成（完成通常意味着：资源加载+场景激活都结束）</summary>
        bool IsDone { get; }
        /// <summary>是否成功（Unity 原生没有失败态，这里一般恒 true；YooAsset 失败会返回 false）</summary>
        bool IsSucceeded { get; }
        /// <summary>失败信息（Unity 原生一般为空；YooAsset 失败会给 LastError）</summary>
        string Error { get; }

        /// <summary>
        /// 是否允许激活场景：
        /// - Unity：对应 AsyncOperation.allowSceneActivation
        /// - YooAsset：如果创建时 suspendLoad=true，则 Progress 到 90% 会挂起，设置为 true 时会 UnSuspend()
        /// </summary>
        bool AllowSceneActivation { get; set; }

        /// <summary>完成回调</summary>
        event Action<ISceneLoadHandle> Completed;

        /// <summary>
        /// 可选：对于 Additive 场景，你可能希望在加载完成后主动设置为 ActiveScene。
        /// - Unity：SetActiveScene
        /// - YooAsset：handle.ActivateScene()
        /// </summary>
        bool ActivateScene();
    }

#if YooAsset
    private sealed class YooSceneLoadHandleAdapter : ISceneLoadHandle
    {
        private readonly SceneHandle _handle;
        private readonly bool _suspendLoad;
        private bool _allowSceneActivation;

        public YooSceneLoadHandleAdapter(SceneHandle handle, bool suspendLoad)
        {
            _handle = handle;
            _suspendLoad = suspendLoad;
            _allowSceneActivation = !suspendLoad;
        }

        public float Progress01
        {
            get
            {
                if (_handle == null) return 0f;
                float p = Mathf.Clamp01(_handle.Progress);
                // YooAsset 的 suspendLoad 会在 90% 挂起；这里统一把 0~0.9 映射到 0~1
                if (_suspendLoad && !_handle.IsDone)
                    return Mathf.Clamp01(p / 0.9f);
                return p;
            }
        }

        public bool IsDone => _handle != null && _handle.IsDone;
        public bool IsSucceeded => _handle != null && _handle.Status == EOperationStatus.Succeed;
        public string Error => _handle != null ? _handle.LastError : "SceneHandle is null";

        public bool AllowSceneActivation
        {
            get => _allowSceneActivation;
            set
            {
                // YooAsset 没有 allowSceneActivation 属性；通过 suspendLoad + UnSuspend() 达到类似效果
                _allowSceneActivation = value;
                if (value && _suspendLoad && _handle != null && !_handle.IsDone)
                {
                    _handle.UnSuspend();
                }
            }
        }

        public event Action<ISceneLoadHandle> Completed;

        public void BindCompleted()
        {
            if (_handle == null) return;
            _handle.Completed += _ => Completed?.Invoke(this);
        }

        public bool ActivateScene()
        {
            if (_handle == null) return false;
            return _handle.ActivateScene();
        }
    }
#endif

    private sealed class UnitySceneLoadHandleAdapter : ISceneLoadHandle
    {
        private readonly AsyncOperation _op;
        private bool _allowSceneActivation;

        public UnitySceneLoadHandleAdapter(AsyncOperation op, bool suspendLoad)
        {
            _op = op;
            _allowSceneActivation = !suspendLoad;
            if (_op != null) _op.allowSceneActivation = _allowSceneActivation;
        }

        public float Progress01
        {
            get
            {
                if (_op == null) return 0f;
                float p = Mathf.Clamp01(_op.progress);
                // Unity 的 progress 在 allowSceneActivation=false 时通常只到 0.9
                if (!_op.allowSceneActivation && !_op.isDone)
                    return Mathf.Clamp01(p / 0.9f);
                return p;
            }
        }

        public bool IsDone => _op != null && _op.isDone;
        public bool IsSucceeded => true;
        public string Error => string.Empty;

        public bool AllowSceneActivation
        {
            get => _op != null ? _op.allowSceneActivation : _allowSceneActivation;
            set
            {
                _allowSceneActivation = value;
                if (_op != null) _op.allowSceneActivation = value;
            }
        }

        public event Action<ISceneLoadHandle> Completed;

        public void BindCompleted()
        {
            if (_op == null) return;
            _op.completed += _ => Completed?.Invoke(this);
        }

        public bool ActivateScene()
        {
            // Unity 原生：加载完成后可选设置 ActiveScene（通常用于 Additive）
            // 注意：Single 模式下通常没必要手动设置
            return true;
        }
    }

    /// <summary>
    /// 统一的异步加载场景（推荐新用法）：
    /// - YooAsset：返回 SceneHandle 适配器（可拿 Progress/Status/LastError，支持 suspendLoad->UnSuspend）
    /// - Unity：返回 AsyncOperation 适配器（支持 allowSceneActivation）
    /// </summary>
    public ISceneLoadHandle LoadSceneHandle(string pPath, Action doneCall = null, LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool suspendLoad = false)
    {
#if YooAsset
        string name = Path.GetFileNameWithoutExtension(pPath);
        var handle = mMainPackage.LoadSceneAsync(name, loadSceneMode, suspendLoad);
        var adapter = new YooSceneLoadHandleAdapter(handle, suspendLoad);
        adapter.Completed += _ => doneCall?.Invoke();
        adapter.BindCompleted();
        return adapter;
#else
        var op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(pPath, loadSceneMode);
        if (op == null)
        {
            doneCall?.Invoke();
            return null;
        }
        var adapter = new UnitySceneLoadHandleAdapter(op, suspendLoad);
        adapter.Completed += _ => doneCall?.Invoke();
        adapter.BindCompleted();
        return adapter;
#endif
    }
    #endregion
}