using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FieldOfView))]
public class FieldOfViewEditor : Editor
{
    void OnSceneGUI()
    {
        //Помещаем в fow игровой объект, на котором висит скрипт 
        FieldOfView fow = (FieldOfView) target;
        //Устанавливает цвет отрисовки FOW белым
        Handles.color = Color.white;
        //Отрисовка окружности
        /*
         * fow.transform.position - центр нашего круга
         * Vector3.up - Ось(нормаль) вокруг которой будет отрисован наш круг
         * Vector3.forward - Направление движения от центра к окружности
         * 360  - угол отрисовки окружности
         * fow.viewRadius - радиус окружности
         */
        Handles.DrawWireArc(fow.transform.position, Vector3.up, Vector3.forward, 360, fow.viewRadius);

        //
        Vector3 viewAngleA = fow.DirectionFromAngle(-fow.viewAngle / 2, false);
        Vector3 viewAngleB = fow.DirectionFromAngle(fow.viewAngle / 2, false);

        /*
         *fow.transform.position - координата, откуда будет рисоваться линия
         *fow.transform.position + viewAngleA/B * fow.viewRadius - конечная координата, куда будет рисоваться линия
         */
        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleA * fow.viewRadius);
        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleB * fow.viewRadius);

        Handles.color = Color.red;
        foreach (Transform visibleTarget in fow.visibleTargets)
        {
            Handles.DrawLine(fow.transform.position, visibleTarget.transform.position);
        }
    }
}
