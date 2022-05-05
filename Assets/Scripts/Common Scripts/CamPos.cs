using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CamPos : MonoBehaviour
{
    public Camera cam;
    public GameObject frame;
    bool mousbuttondown = false;
    bool getkeyWdown = false;
    bool getkeySdown = false;
    Vector3 mousestart;
    float MinX;
    float MaxX;
    float MinY;
    float MaxY;
    float lcs;
    float rast;
    float izmen;
    void Start()
    {
        MinX = frame.transform.position.x - frame.transform.localScale.x / 512;
        MaxX = frame.transform.position.x + frame.transform.localScale.x / 512;
        lcs = cam.orthographicSize;
        MinY = frame.transform.position.y - frame.transform.localScale.y / 2;
        MaxY = frame.transform.position.y + frame.transform.localScale.y / 2;
    }

    void Update()
    {
        CamSize();
        Pos();
    }

    private void Pos()
    {
        if (Input.GetMouseButtonDown(1))
        {
            mousestart = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.transform.position.z));
            mousbuttondown = true;
        }

        if (mousbuttondown)
        {
            float ort = cam.orthographicSize;
            izmen = lcs - ort;
            float minx = MinX - izmen*(float)1.7;
            float maxx = MaxX + izmen*(float)1.7;
            float miny = MinY + ort;
            float maxy = MaxY - ort;
            Vector3 newMousePosition = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.transform.position.z));
            float newx = newMousePosition.x - mousestart.x;
            float newy = newMousePosition.y - mousestart.y;
            if (Math.Abs(newx) > 1)
                cam.transform.position = new Vector3(Mathf.Clamp(cam.transform.position.x - newx / 2, minx, maxx), cam.transform.position.y, cam.transform.position.z);
            if (Math.Abs(newy) > 1)
                cam.transform.position = new Vector3(cam.transform.position.x, Mathf.Clamp(cam.transform.position.y - newy / 2,miny, maxy), cam.transform.position.z);
            mousestart = newMousePosition;
        }

        if (Input.GetMouseButtonUp(1))
            mousbuttondown = false;
    }

    void CamSize()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            getkeyWdown = true;
        }
        if (getkeyWdown)
        {
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - 1, 200, (float)539.1098);
        }
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            getkeyWdown = false;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            getkeySdown = true;
        }
        if (getkeySdown)
        {
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize + 1, 200, (float)539.1098);
            float ort = cam.orthographicSize;
            izmen = lcs - ort;
            float minx = MinX - izmen * (float)1.7;
            float maxx = MaxX + izmen * (float)1.7;
            float miny = MinY + ort;
            float maxy = MaxY - ort;
            cam.transform.position = new Vector3(Mathf.Clamp(cam.transform.position.x, minx, maxx), cam.transform.position.y, cam.transform.position.z);
            cam.transform.position = new Vector3(cam.transform.position.x, Mathf.Clamp(cam.transform.position.y, miny, maxy), cam.transform.position.z);

        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            getkeySdown = false;
            float ort = cam.orthographicSize;
            izmen = lcs - ort;
            float minx = MinX - izmen * (float)1.7;
            float maxx = MaxX + izmen * (float)1.7;
            float miny = MinY + ort;
            float maxy = MaxY - ort;
            cam.transform.position = new Vector3(Mathf.Clamp(cam.transform.position.x, minx, maxx), cam.transform.position.y, cam.transform.position.z);
            cam.transform.position = new Vector3(cam.transform.position.x, Mathf.Clamp(cam.transform.position.y, miny, maxy), cam.transform.position.z);
        }

    }

}
