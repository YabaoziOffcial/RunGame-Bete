using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LitJson;
using UnityEngine;
using YBZ.Design;

namespace YBZ.Core
{
    /// <summary>
    /// 游戏根物体， 程序入口
    /// </summary>
    public class GameRoot : O_MonoSingleton<GameRoot>
    {
        // 相当于Awake
        protected override void Initialize()
        {
            Application.targetFrameRate = 60;
            GameHelper.Instance.Init();                    // 游戏辅助类
            GameDataManager.Instance.Init();              // 数据控制器
            GameConfig.Init();                            // 配置 ————
            GameController.Instance.Init();
            Debug.Log("GameRoot Initialized!");

            // MVC 示例：Awake 里只 Init/写 Model；开 UI 放到 Start，等 UIManager 等 Mono 单例 Awake 完成
            // TestController.Instance.Init();
            // TestController.Instance.SetTestString("Hello World");
        }

        public void Start()
        {
            Debug.Log("GameRoot Start!");
            GameController.Instance.GameStart();

            // TestView 示例：需 Resources/Prefab/UI/TestView.prefab，正式局内可不开
            // TestController.Instance.OpenTestView();
        }

        private void Update()
        {
            GameHelper.Instance.Update();
            GameController.Instance.Update();
            EquipManager.Instance.Update();

            // TestController.Instance.Update(); // 示例
            
        }
        private void LateUpdate()
        {

        }

        private void FixedUpdate()
        {
            EquipManager.Instance.FixedUpdate();
        }


        private void OnApplicationPause(bool pause)
        {
            if (!pause) return;
            FlushGameData();
        }

        private void OnApplicationQuit()
        {
            FlushGameData();
            EventManager.Clear();
        }

        private static void FlushGameData()
        {
            try { TestController.Instance.FlushPendingSave(); } catch { }
            try { GameDataManager.Instance.SaveAll(); } catch { }
        }

        public void OnDisable()
        {

        }

        public void OnDestroy()
        {

        }

        public void OnGUI()
        {
            
        }

        [SerializeField] bool IsDrawCameraLine;
        private void OnDrawGizmos()
        {
            if (IsDrawCameraLine)
            {
                var camera = Camera.main;
                var size = camera.aspect * camera.orthographicSize;
                var pos = camera.transform.position;
                Vector2 start, end;
                Gizmos.color = Color.yellow;
                start = new Vector3(pos.x - size, pos.y - camera.orthographicSize);
                end = new Vector3(pos.x - size, pos.y + camera.orthographicSize);
                Gizmos.DrawLine(start, end);

                start = new Vector3(pos.x + size, pos.y + camera.orthographicSize);
                Gizmos.DrawLine(end, start);

                end = new Vector3(pos.x + size, pos.y - camera.orthographicSize);
                Gizmos.DrawLine(start, end);

                start = new Vector3(pos.x - size, pos.y - camera.orthographicSize);
                Gizmos.DrawLine(end, start);
            }
        }
    }
}
