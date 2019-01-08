using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float speed = 2; // скорость персонажа
    public float turnSpeed = 50; // скорость поворота персонажа
    public GameManager gm; // доступ к GameManager
    public Slider noiseSlider; // слайдер
    public AudioClip alert; // звук тревоги

    private int soundCount; // сколько раз должна прозвучать тревога
    private Vector3 moveDirection; // вектор направления движения
    private Rigidbody rb; // доступ к Rigidbody
    private Animator anim; // доступ к аниматору
    private Vector3 position; // координата объекта в пространстве для шума
    private float time; // время для сброса уровня шума
    private float restartLevelDelay = 1f; // задержка перед созданием нового уровня

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        moveDirection = Vector3.zero;
        anim = GetComponent<Animator>();
        position = rb.position;
        time = Time.time;
        noiseSlider = GameObject.FindGameObjectWithTag("Slider").GetComponent<Slider>();
        noiseSlider.value = 0;
    }

    void Update()
    {
        if (gm.canMove)
        {
            Move();
            Noise();
        }

    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveDirection * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Передвижение объекта в пространстве
    /// </summary>
    void Move()
    {
        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * speed;
        if (moveDirection != Vector3.zero)
            anim.SetBool("Walk", true);
        else
            anim.SetBool("Walk", false);

        Vector3 direction = Vector3.RotateTowards(transform.forward, moveDirection, turnSpeed, 0.0f);
        transform.localRotation = Quaternion.LookRotation(direction);
    }

    /// <summary>
    /// Создание шума объектом при перемещении в пространстве
    /// </summary>
    void Noise()
    {
        if (noiseSlider.value< 10)
        {
            if (Vector3.Distance(position, rb.transform.position) > 0.2f)
            {
                noiseSlider.value += 0.5f;
                position = rb.transform.position;

            }

            if (Time.time - time > 0.1f && noiseSlider.value > 0)
            {
                noiseSlider.value -= 0.2f;
                time = Time.time;
            }
        }
        else
        {
            if (soundCount < 1)
            {
                SoundManager.instance.PlaySound(alert);
                soundCount++;
            }
            noiseSlider.value = 10;
        }

    }

    /// <summary>
    /// Перезагрузка уровня
    /// </summary>
    void RestartLevel()
    {
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Триггер перезапуска уровня
    /// </summary>
    /// <param name="exit">Коллайдер выхода</param>
    void OnTriggerEnter(Collider exit)
    {
        if (exit.tag == "Exit")
        {
            Invoke("RestartLevel", restartLevelDelay);
            enabled = false;
        }
    }

    /// <summary>
    /// Конец игры при коллизии с врагом
    /// </summary>
    /// <param name="enemy">Враг</param>
    void OnCollisionEnter(Collision enemy)
    {
        if (enemy.gameObject.tag == "Enemy")
        {
            GameManager.instance.GameOver();
        }
    }
}
