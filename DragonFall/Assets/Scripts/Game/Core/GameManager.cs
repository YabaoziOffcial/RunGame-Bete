using System.Collections.Generic;
using UnityEngine;
using YBZ.Design;
using Dragonfall.Data;

namespace Dragonfall.Core
{
    public enum GameState
    {
        Waiting,
        Playing,
        Paused,
        LevelUp,
        Victory,
        Defeat
    }

    public class GameManager : D_MonoSingleton<GameManager>
    {
        public GameState State { get; private set; } = GameState.Waiting;

        [Header("Config")]
        [SerializeField] private CharacterConfigSO characterConfig;
        [SerializeField] private WaveConfigSO waveConfig;
        [SerializeField] private List<WeaponConfigSO> weaponPool;
        [SerializeField] private List<PassiveItemConfigSO> passiveItemPool;

        public CharacterConfigSO CharacterConfig => characterConfig;
        public WaveConfigSO WaveConfig => waveConfig;
        public List<WeaponConfigSO> WeaponPool => weaponPool;
        public List<PassiveItemConfigSO> PassiveItemPool => passiveItemPool;

        private Player.PlayerController player;
        private Systems.LevelManager levelManager;
        private Systems.PassiveItemManager passiveItemManager;
        private Enemies.EnemySpawner enemySpawner;
        private RunDirector runDirector;

        public Player.PlayerController Player => player;
        public Systems.LevelManager LevelManager => levelManager;
        public Systems.PassiveItemManager PassiveItemManager => passiveItemManager;
        public Enemies.EnemySpawner EnemySpawner => enemySpawner;
        public Camera MainCamera { get; private set; }

        private float prePauseTimeScale = 1f;

        protected override void Initialize()
        {
            MainCamera = Camera.main;
            SpawnSubsystems();
        }

        private void SpawnSubsystems()
        {
            levelManager = gameObject.AddComponent<Systems.LevelManager>();
            passiveItemManager = gameObject.AddComponent<Systems.PassiveItemManager>();
            enemySpawner = gameObject.AddComponent<Enemies.EnemySpawner>();
            runDirector = gameObject.AddComponent<RunDirector>();

            levelManager.Init(this);
            passiveItemManager.Init(this);
            enemySpawner.Init(this);
            runDirector.Init(this);
        }

        public void StartRun()
        {
            if (characterConfig == null)
            {
                Debug.LogError("[GameManager] CharacterConfig not assigned!");
                return;
            }

            SpawnPlayer();
            runDirector.StartRun();
            EventManager.SendEvent(GameEvents.OnRunStart);
            SetState(GameState.Playing);
        }

        private void SpawnPlayer()
        {
            var playerGO = new GameObject("Player");
            player = playerGO.AddComponent<Player.PlayerController>();
            player.Init(characterConfig);

            var weaponsGO = new GameObject("Weapons");
            weaponsGO.transform.SetParent(playerGO.transform);
        }

        public void SetState(GameState newState)
        {
            if (State == newState) return;

            State = newState;
            switch (newState)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    EventManager.SendEvent(GameEvents.OnGameResume);
                    break;
                case GameState.Paused:
                case GameState.LevelUp:
                    prePauseTimeScale = Time.timeScale;
                    Time.timeScale = 0f;
                    EventManager.SendEvent(GameEvents.OnGamePause);
                    break;
                case GameState.Victory:
                    Time.timeScale = 0f;
                    EventManager.SendEvent(GameEvents.OnRunVictory);
                    break;
                case GameState.Defeat:
                    Time.timeScale = 0f;
                    EventManager.SendEvent(GameEvents.OnRunDefeat);
                    break;
            }
        }

        public void ResumeFromLevelUp()
        {
            if (State == GameState.LevelUp)
                SetState(GameState.Playing);
        }

        public void GameOver(bool victory)
        {
            SetState(victory ? GameState.Victory : GameState.Defeat);
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }
    }
}
