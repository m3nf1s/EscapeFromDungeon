﻿using UnityEngine;
using UnityEngine.AI;


public class AI : MonoBehaviour
{
    public MeshRenderer meshRenderer; // meshrenderer FoV
    public GameManager gm; // GameManager для проверки возможности передвижения объекта
    public AudioClip alert; //звук тревоги, что увидел противник

    private int alertCount;
    private NavMeshAgent _agent; // для доступ к навигационной сетке
    private LevelGeneration _lvlgen; // для доступа к списку координат для патрулирования
    private bool patrul; // проверка патрулирует объект или нет
    private bool moving; // проверка движется объект или стоит на месте
    private int randomPosition; // для получения случайно координаты из списка _lvlgen
    private float timeWait; // для проверки сколько персонаж стоит на одном месте
    private Animator anim; // доступ к конструктору анимаций объекта
    private PlayerController player; // для доступа к проверке уровню шуму главного героя
    private FieldOfView fow; // для проверки попал персонаж в прямое поле видимости
    
    void Start()
    {
        anim = GetComponent<Animator>();
        _lvlgen = GameObject.FindWithTag("GameManager").GetComponent<LevelGeneration>();
        _agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        fow = GetComponent<FieldOfView>();
        meshRenderer.material.color = new Color(0f, 1f, 0f, 0.2f);
        gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        patrul = true;
        moving = false;
    }

    void Update()
    {
        if (gm.canMove)//проверка может ли персонаж двигаться или нет
        {
            Patrol();
            HuntDown();
            StopHuntDown();
        }
    }

    /// <summary>
    /// Патрулирование NPC
    /// </summary>
    void Patrol()
    {
        if (patrul)
        {
            if (moving) // если истина, то объект движется к точке
            {
                _agent.SetDestination(_lvlgen.wallPositions[randomPosition]);
                if (Vector3.Distance(transform.position, _lvlgen.wallPositions[randomPosition]) < 0.3f)// проверка достиг объект своей цели или нет
                {
                    anim.SetBool("Walk", false);
                    timeWait = Time.time;
                    moving = false;
                }

            }
            else //задержка перед движением к следующей точке
            {
                if (Time.time - timeWait > 2f)
                {
                    anim.SetBool("Walk", true);
                    randomPosition = Random.Range(0, _lvlgen.wallPositions.Count);
                    _agent.SetDestination(_lvlgen.wallPositions[randomPosition]);
                    moving = true;
                }
            }
        }
    }

    /// <summary>
    /// Преследование NPC
    /// </summary>
    void HuntDown()
    {
        if ((player.noiseSlider.value >= 10 || fow.isSeeing)) // проверка уровня шума главного героя и попал ли он в зону прямой видимости
        {
            meshRenderer.material.color = new Color(1f, 0f, 0f, 0.2f);
            patrul = false;
            anim.SetBool("Walk", true);
            _agent.speed = 3;
            _agent.SetDestination(player.transform.position);
        }

        if (alertCount < 1 && fow.isSeeing)
        {
            alertCount++;
            SoundManager.instance.PlaySound(alert);
        }
        else if (player.noiseSlider.value >= 10)
            alertCount++;
    }


    /// <summary>
    /// Остановка преследования NPC
    /// </summary>
    void StopHuntDown()
    {
        if (!player.enabled)
        {
            _agent.SetDestination(transform.position);
            anim.SetBool("Walk", false);
        }
    }
}
