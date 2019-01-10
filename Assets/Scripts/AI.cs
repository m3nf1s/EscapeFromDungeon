using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour
{
    public MeshRenderer meshRenderer; // meshrenderer FoV
    public GameManager gm; // GameManager для проверки возможности передвижения объекта
    public AudioClip alertSound; //звук тревоги, что увидел противник
    public AudioClip stepSound; //звук шагов
    public AudioSource stepSource; // источник для шагов
    public AudioSource soundSource; // источник для звуков

    private Vector3 bugPosition; // коордиты объекта для проверки, если он бежит на одном месте
    private float timeBug; // время, для проверки, если он бежит на одном месте
    private int alertCount; // количество раз для проигрывания звука тревоги
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
        stepSource = GetComponent<AudioSource>();
        soundSource = GetComponent<AudioSource>();
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

                PlayStepSound();

                if (Vector3.Distance(transform.position, _lvlgen.wallPositions[randomPosition]) < 0.3f)// проверка достиг объект своей цели или нет
                {
                    anim.SetBool("Walk", false);
                    timeWait = Time.time;
                    moving = false;
                }

                if (Vector3.Distance(transform.position, bugPosition) < 0.4f && Time.time - timeBug > 1.2f)
                {
                    StartWalkToPostion();
                }
            }
            else //задержка перед движением к следующей точке
            {
                if (Time.time - timeWait > 2.0f)
                {
                    StartWalkToPostion();
                    moving = true;
                    bugPosition = transform.position;
                    timeBug = Time.time;
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
            PlayStepSound();
        }

        if (alertCount < 1 && fow.isSeeing)
        {
            alertCount++;
            soundSource.clip = alertSound;
            soundSource.Play();
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

    /// <summary>
    /// Проигрывание звуков шагов
    /// </summary>
    void PlayStepSound()
    {
        if (!stepSource.isPlaying) // проигрывание звуков шагов
        {
            stepSource.clip = stepSound;
            stepSource.Play();
        }
    }

    /// <summary>
    /// Назначение новой случаной точки для патрулирования
    /// </summary>
    void StartWalkToPostion()
    {
        randomPosition = Random.Range(0, _lvlgen.wallPositions.Count);
        _agent.SetDestination(_lvlgen.wallPositions[randomPosition]);
        anim.SetBool("Walk", true);
    }
}
