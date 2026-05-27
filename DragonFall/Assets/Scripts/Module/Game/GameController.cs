using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : YBZ.Design.Singleton<GameController>
{
    private GameModel m_Model;
    public GameModel Model => m_Model;
    public Player Player { get; private set; }

    private float m_EnemySpawnTimer;
    private GameObject m_EnemyPrefab;


    public void Init()
    {
        m_Model = new GameModel();
        m_Model.SetStartGameTime(Time.time);
        m_EnemyPrefab = ResourceManager.Instance.LoadRes<GameObject>(GameConst.EnemyPrefabPath);
        m_EnemySpawnTimer = 0f;
        Player = GameObject.FindObjectOfType<Player>();
    }


    public void Update()
    {
        m_EnemySpawnTimer -= Time.deltaTime;
        if (m_EnemySpawnTimer > 0f) return;
        SpawnEnemy();
        m_EnemySpawnTimer = GameConst.EnemySpawnInterval;
    }

    private void SpawnEnemy()
    {
        if (m_EnemyPrefab == null || Camera.main == null) return;

        GameObject enemy = ObjectPool.GetObj(m_EnemyPrefab);
        enemy.transform.position = GetRandomSpawnPosition();
    }

    public void GameOver()
    {
        Debug.Log("GameOver");
        UIManager.Instance.OpenUI<GameOverView>(); 
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Camera mainCamera = Camera.main;
        float halfHeight = mainCamera.orthographicSize;
        float halfWidth = halfHeight * mainCamera.aspect;
        Vector3 cameraPosition = mainCamera.transform.position;

        int side = Random.Range(0, 4);
        float x = Random.Range(cameraPosition.x - halfWidth, cameraPosition.x + halfWidth);
        float y = Random.Range(cameraPosition.y - halfHeight, cameraPosition.y + halfHeight);

        switch (side)
        {
            case 0:
                y = cameraPosition.y + halfHeight + GameConst.EnemySpawnPadding;
                break;
            case 1:
                y = cameraPosition.y - halfHeight - GameConst.EnemySpawnPadding;
                break;
            case 2:
                x = cameraPosition.x - halfWidth - GameConst.EnemySpawnPadding;
                break;
            default:
                x = cameraPosition.x + halfWidth + GameConst.EnemySpawnPadding;
                break;
        }

        return new Vector3(x, y, 0f);
    }


}
