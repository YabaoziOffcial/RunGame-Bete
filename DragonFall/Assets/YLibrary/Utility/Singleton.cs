using UnityEngine;
namespace YBZ.Design {

    /// <summary>
    /// 非Mono的单例模式
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> where T : new()
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                _instance ??= new T();
                return _instance;
            }
        }
    }

    /// <summary>
    /// 动态(Dynamic), 会自动在不存在的时候生成一个
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class D_MonoSingleton<T> : MonoBehaviour where T : D_MonoSingleton<T>
    {
        private static T _instance = null;

        public static T Instance
        {
            get
            {
                if(_instance == null)
                {
                    GameObject go = new GameObject();
                    DontDestroyOnLoad(go);
                    go.name = "MonoSingleton:" + typeof(T).ToString();
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localEulerAngles = Vector3.zero;
                    go.transform.localScale = Vector3.one;
                    _instance = go.AddComponent<T>();
                    _instance.Initialize();
                }
                return _instance;
            }
        }

        // private void Awake() => ;

        protected virtual void Initialize(){}

        private void OnDestroy()
        {
            _instance = null;
            Dispose();
        }

        /// <summary>
        /// 摧毁时调用
        /// </summary>
        protected virtual void Dispose(){}
    }


    /// <summary>
    /// 静态(Static)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class S_MonoSingleton<T> : MonoBehaviour where T : S_MonoSingleton<T>
    {
        private static T _instance;

        public static T Instance { 
            get {
                return _instance; 
            }
        }

        public void Awake()
        {
            if(_instance != null && _instance != (T)this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = (T)this;
            Initialize();
        }

        protected virtual void Initialize() { }

        private void OnDestroy()
        {
            _instance = null;
        }
    }

    /// <summary>
    /// 静态常驻, 需要挂在到场景中的物体，并且可以手动挂载
    /// </summary>
    public abstract class O_MonoSingleton<T> : MonoBehaviour where T : O_MonoSingleton<T> 
    {
        static T _instance = null;
        
        public static T Instance
        {
            get
            {
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        private void Awake()
        {
            // 单例不为空， 并且单例不等于自己
            if(_instance != null && _instance != (T) this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = (T)this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        protected virtual void Initialize(){}
    }
}