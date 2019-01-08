using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class LevelGeneration : MonoBehaviour
{
    [Header("Mash")]
    public NavMeshSurface Surfaces; //Навигационная сетка

    [Header("Lvl Generation")]
    public int columns = 10; // столбцы
    public int rows = 10; // строки
    public int wallCount = 22; // количество стен
    public int enemyCount = 2; // количество врагов
    public List<Vector3> wallPositions = new List<Vector3>(); // список векторов, в которых разместим стены и врагов

    [Header("Prefabs")]
    public GameObject exit; // префаб выход
    public GameObject[] floorPrefabs; // массив префабов плиток
    public GameObject[] wallPrefabs; // массив префабов стенн
    public GameObject[] enemyPrefabs; // массив перфабов врагов
    public GameObject outerWallPrefab; // массив префабов наружных стен

    private Transform _outerWalls; 
    private Transform _floor;
    private Transform _walls;
    private Transform _enemies;
    private Transform _points;

    /// <summary>
    /// Очистка и заполнение списка новыми коодринатими перед каждым раундом
    /// </summary>
    void InitialiseList()
    {
        wallPositions.Clear();
        for (int x = 1; x < columns - 1; x++)
        {
            for (int z = 1; z < rows - 1; z++)
            {
                wallPositions.Add(new Vector3(x, 0, z));
            }
        }
    }

    /// <summary>
    /// Генератор игрового пространства
    /// </summary>
    void BoardGeneration()
    {
        _floor = new GameObject("Board").transform;
        _outerWalls = new GameObject("OuterWalls").transform;

        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                GameObject toInstantiate = floorPrefabs[Random.Range(0, floorPrefabs.Length)];
                if (x == 0 || x == columns - 1 || z == 0 || z == rows -1)
                {
                    if(z == 0)
                        Instantiate(outerWallPrefab, new Vector3(x - 0.5f, 0, z - 0.5f), Quaternion.AngleAxis(180, new Vector3(0,1,0))).transform.SetParent(_outerWalls);
                    if (z == rows - 1)
                        Instantiate(outerWallPrefab, new Vector3(x - 0.5f, 0, z + 0.5f), Quaternion.AngleAxis(180, new Vector3(0, 1, 0))).transform.SetParent(_outerWalls);
                    if (x == 0)
                        Instantiate(outerWallPrefab, new Vector3(x - 0.5f, 0, z - 0.5f), Quaternion.AngleAxis(90, new Vector3(0, 1, 0))).transform.SetParent(_outerWalls);
                    if (x == columns - 1)
                        Instantiate(outerWallPrefab, new Vector3(x + 0.5f, 0, z + 0.5f), Quaternion.AngleAxis(-90, new Vector3(0, 1, 0))).transform.SetParent(_outerWalls);

                }

                Instantiate(toInstantiate, new Vector3(x,0,z), Quaternion.identity).transform.SetParent(_floor);
            }
        }
    }

    /// <summary>
    /// Получение случайной координаты из списка и удалением ее, чтобы не повторялось
    /// </summary>
    /// <returns>Слачайная коородината</returns>
    Vector3 RandomPosition()
    {
        int randomIndex = Random.Range(0, wallPositions.Count);

        Vector3 randomPosition = wallPositions[randomIndex];

        wallPositions.RemoveAt(randomIndex);

        return randomPosition;
    }

    /// <summary>
    /// Генерация объектов в случайных координатах из списка
    /// </summary>
    /// <param name="Prefabs">Массыв игровых перфабов</param>
    /// <param name="count">Количество объектов, которые надо сгенерировать</param>
    /// <param name="parentTransform">Родительский Transform для соблюдения иерархии в проекте</param>
    /// <param name="parentName">Имя родительского Transform</param>
    void LayoutObjectAtRandom(GameObject[] Prefabs, int count, Transform parentTransform, string parentName)
    {
        parentTransform = new GameObject(parentName).transform;
        for (int i = 0; i < count; i++)
        {
            Vector3 randomPosition = RandomPosition();

            GameObject tileChoice = Prefabs[Random.Range(0, Prefabs.Length)];

            Instantiate(tileChoice, randomPosition, Quaternion.identity).transform.SetParent(parentTransform);
        }
    }

    /// <summary>
    /// Генератор игровой сцены
    /// </summary>
    public void SceneSetup()
    {
        BoardGeneration();

        InitialiseList();

        LayoutObjectAtRandom(wallPrefabs, wallCount , _walls, "Walls");

        Surfaces = GameObject.FindGameObjectWithTag("NavMesh").GetComponent<NavMeshSurface>();
        Surfaces.BuildNavMesh();

        LayoutObjectAtRandom(enemyPrefabs, enemyCount, _enemies, "Enemies");

        Instantiate(exit, new Vector3(columns - 1, 0.05f, rows - 1), Quaternion.identity);
    }
}
