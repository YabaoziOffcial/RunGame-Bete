using UnityEngine;
using Dragonfall.Data;

namespace Dragonfall.Core
{
    public class RunDirector : MonoBehaviour
    {
        private GameManager gameManager;
        private WaveConfigSO waveConfig;
        private float runStartTime;
        private bool isRunning;
        private int currentWaveIndex;
        private int currentBossIndex;

        public float RunTime => isRunning ? Time.time - runStartTime : 0f;

        public void Init(GameManager manager)
        {
            gameManager = manager;
        }

        void Update()
        {
            if (!isRunning) return;
            if (gameManager.State != GameState.Playing && gameManager.State != GameState.LevelUp)
                return;

            float elapsed = RunTime;

            if (waveConfig != null)
            {
                ProcessWaves(elapsed);
                ProcessBosses(elapsed);

                if (elapsed >= waveConfig.runDuration)
                {
                    gameManager.GameOver(true);
                    isRunning = false;
                }
            }
        }

        public void StartRun()
        {
            waveConfig = gameManager.WaveConfig;
            runStartTime = Time.time;
            isRunning = true;
            currentWaveIndex = 0;
            currentBossIndex = 0;
        }

        private void ProcessWaves(float elapsed)
        {
            if (waveConfig == null || waveConfig.waves == null) return;

            while (currentWaveIndex < waveConfig.waves.Count)
            {
                var wave = waveConfig.waves[currentWaveIndex];
                if (elapsed >= wave.startTime)
                {
                    gameManager.EnemySpawner.SpawnWave(wave);
                    currentWaveIndex++;
                }
                else
                {
                    break;
                }
            }
        }

        private void ProcessBosses(float elapsed)
        {
            if (waveConfig == null || waveConfig.bosses == null) return;

            while (currentBossIndex < waveConfig.bosses.Count)
            {
                var boss = waveConfig.bosses[currentBossIndex];
                if (elapsed >= boss.spawnTime)
                {
                    gameManager.EnemySpawner.SpawnBoss(boss.bossType);
                    EventManager.SendEvent(GameEvents.OnBossSpawned, boss.bossType);
                    currentBossIndex++;
                }
                else
                {
                    break;
                }
            }
        }
    }
}
