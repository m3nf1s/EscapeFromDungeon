using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float viewRadius; //Радиус обзора

    [Range(0,360)]
    public float viewAngle; //Угол Обзора

    public LayerMask targetMask; // Маска с целями

    public LayerMask ObstacleLayerMask; // Маска с препятствиями

    public List<Transform> visibleTargets = new List<Transform>(); // список видимых объектов

    public float meshResolution; // Множитель лучей

    public int edgeResolveIterations; //итерация разбиения грани

    public float edgeDistanceThreshold;

    public MeshFilter viewMeshFilter; 

    private Mesh viewMesh;

    public bool isSeeing = false;
    /// <summary>
    /// Сбор информации о столкновении
    /// </summary>
    public struct ViewCastInfo
    {
        public bool hit; // произошло столкновение или нет
        public Vector3 point; // позиция цели
        public float distance; // расстояние
        public float angle; // угол
        
        public ViewCastInfo(bool hit, Vector3 point, float distance, float angle)
        {
            this.hit = hit;
            this.point = point;
            this.distance = distance;
            this.angle = angle;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 pointA, Vector3 pointB)
        {
            this.pointA = pointA;
            this.pointB = pointB;
        }
    }

    void Start()
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;
        StartCoroutine("FindTargetsWithDelay", 0.2f);
    }

    void Update()
    {
        DrawFieldOfView();
    }

    /// <summary>
    /// Корутин, поиска целей с задержкой по времени
    /// </summary>
    /// <param name="delay">Задержка</param>
    /// <returns>Приостановка корутина и возобновление ее с задержкой</returns>
    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    /// <summary>
    /// Поиск видимых целей
    /// </summary>
    void FindVisibleTargets()
    {
        visibleTargets.Clear();

        /* Заполняем массив колайдерами игровых объектов, которые находятся или касаются сферы и удовлетворяют маске
         * transform.position - центр сферы
         * viewRadius - радиус сферы
         * targetMask - маска с которой будет взаимодействовать сфера при пусканиях лучей
         */
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform; //получаем координаты цели в пространстве
            Vector3 dirToTarget = (target.position - transform.position).normalized; // в каком направление находится цель от объекта
            //Если цель попадает попадает в угол обзора объекта
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position); //расстояние между объектом и целью
                /*
                 * transform.position - начальая координата луча в мировом пространстве
                 * dirToTarget - направление луча
                 * dstToTarget - максимальная растояние, которое луч должен проверить на столкновение 
                 * ObstacleLayerMask - маска с которой будет взаимодействовать луч
                 */
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, ObstacleLayerMask))
                {
                    visibleTargets.Add(target);
                    isSeeing = true;
                }
            }
        }
    }

    /// <summary>
    /// Отрисовка поля зрения
    /// </summary>
    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution); //количество лучей

        float stepAngleSize = viewAngle / stepCount; // угол между лучами

        List<Vector3> viewPoints = new List<Vector3>(); //список точек столкновений

        ViewCastInfo oldViewCast = new ViewCastInfo();

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            if (i > 0)
            {
                bool edgeDistanceThresholdExceeded = Mathf.Abs(oldViewCast.distance - newViewCast.distance) > edgeDistanceThreshold;

                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDistanceThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero)
                        viewPoints.Add(edge.pointA);
                    if (edge.pointB != Vector3.zero)
                        viewPoints.Add(edge.pointB);
                }

            }
            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1; //Количество вершин
        Vector3[] vertices = new Vector3[vertexCount]; // массив координат вершин
        int[] triangles = new int[(vertexCount - 2) * 3]; //массив вершин треугольников

        vertices[0] = Vector3.zero;
        
        for (int i = 0; i < vertexCount - 1; i++) // заполняем массив координатами вершин
        {
            vertices[i + 1] = transform .InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2) //заполняем массив множеством вершин
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear(); //очищаем меш
        viewMesh.vertices = vertices; // координты вершин меша берем из массива
        viewMesh.triangles = triangles; // множество вершин берем из массива
        viewMesh.RecalculateNormals(); //перерасчитываем нормали
    }

    /// <summary>
    /// Нахождение граний между двучами точками Cast
    /// </summary>
    /// <param name="minViewCast"></param>
    /// <param name="maxViewCast"></param>
    /// <returns></returns>
    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDistanceThresholdExceeded = Mathf.Abs(minViewCast.distance - newViewCast.distance) > edgeDistanceThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDistanceThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }
        return new EdgeInfo(minPoint, maxPoint);
    }
    /// <summary>
    /// Отображение лучей
    /// </summary>
    /// <param name="globalAngle">угол</param>
    /// <returns></returns>
    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 direction = DirectionFromAngle(globalAngle, true);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, direction, out hit, viewRadius, ObstacleLayerMask))
        {
            return new ViewCastInfo(true, hit.point, hit.distance,  globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + direction * viewRadius, viewRadius, globalAngle); ;
        }
    }

    /// <summary>
    /// Направление от угла
    /// </summary>
    /// <param name="angleInDegrees">Угол в градусах</param>
    /// <param name="angleIsGlobal">Является ли угол глобальным или нет</param>
    /// <returns>Вектор направления</returns>
    public Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        //Если угол не глобальный, то добавляем ему вращения как угла Эйлера
        if (!angleIsGlobal)
            angleInDegrees += transform.eulerAngles.y;

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
