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
            EquipManager.Instance.Init();
            GameController.Instance.Init();
            Debug.Log("GameRoot Initialized!");

            MapManager mapManager = FindObjectOfType<MapManager>();
            if (mapManager != null)
            {
                mapManager.Init();
            }
        }

        public void Start()
        {
            Debug.Log("GameRoot Start!");
            // 先打开主菜单，游戏由 MainPanel 的"开始"按钮驱动
            UIManager.Instance.OpenUI<MainPanel>();
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
        [SerializeField] int m_TestLevelUpCount = 1; // Inspector 测试 PlayerLevel 时连续升几级

        // Play 模式下测试升级选装：补足当前级所需经验并走完整事件链
        [InspectorButton]
        public void PlayerLevel()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[Test] PlayerLevel 请在 Play 模式下使用。");
                return;
            }

            GameController controller = GameController.Instance;
            if (controller.Model == null)
            {
                Debug.LogWarning("[Test] GameController 尚未 Init / GameStart。");
                return;
            }

            if (controller.IsGameOver)
            {
                Debug.LogWarning("[Test] 本局已结束，无法测试升级。");
                return;
            }

            int grantTimes = Mathf.Max(1, m_TestLevelUpCount);
            for (int i = 0; i < grantTimes; i++)
            {
                GameModel model = controller.Model;
                int expToLevel = Mathf.Max(1, model.LevelUpExp - model.Exp);
                controller.GrantExpForTest(expToLevel);
            }

            Debug.Log($"[Test] PlayerLevel x{grantTimes} -> Lv.{controller.Model.Level}, 待选装: {controller.HasPendingLevelUp}");
        }

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
