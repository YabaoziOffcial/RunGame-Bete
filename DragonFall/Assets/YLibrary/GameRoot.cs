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
            Debug.Log("GameRoot Initialized!");
        }

        public void Start()
        {
            Debug.Log("GameRoot Start!");
        }

        private void Update()
        {
            GameHelper.Instance.Update();

            if (Input.GetKeyDown(KeyCode.R))
            {
                Time.timeScale = 1f;
            }
        }
        private void LateUpdate()
        {

        }

        private void FixedUpdate()
        {
        }


        private void OnApplicationPause(bool pause)
        {

        }

        private void OnApplicationQuit()
        {
            // MaterialLoad.Instance.candy_mat.SetColor("_Color", new Color(1, 1, 1, 1));
            // 退出时主动反注册事件，避免静态事件残留导致“内存泄漏”（尤其是编辑器关闭域重载时）
            EventManager.Clear();
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
