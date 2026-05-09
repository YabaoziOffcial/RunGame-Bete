using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dragonfall.Data
{
    [Serializable]
    public class WaveEntry
    {
        [Tooltip("Seconds from run start")]
        public float startTime;
        public EnemyConfigSO enemyType;
        public int count;
        public float spawnInterval = 1f;
    }

    [Serializable]
    public class BossEntry
    {
        public float spawnTime;
        public EnemyConfigSO bossType;
    }

    [CreateAssetMenu(menuName = "Dragonfall/Wave Config", fileName = "WaveConfig")]
    public class WaveConfigSO : ScriptableObject
    {
        public string levelName = "Emerald Forest";
        public float runDuration = 300f; // 5 minutes for MVP
        public float mapWidth = 20f;
        public float mapHeight = 15f;
        public List<WaveEntry> waves = new();
        public List<BossEntry> bosses = new();
    }
}
