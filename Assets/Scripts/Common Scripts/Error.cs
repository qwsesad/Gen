using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Error : MonoBehaviour
{
    public GameObject error;
    public Transform parent;
    public GameObject ALL;


    public void Problem(GameObject er, Transform par, GameObject all, string text)
    {
        error = er;
        parent = par;
        ALL = all;
        GameObject problem;
        problem = Instantiate(error, new Vector3(960, 541, 0), error.transform.rotation, parent);
        problem.tag = "Error";
        GameObject Child = problem.transform.Find("WhatProblem").gameObject;
        Child.GetComponent<Text>().text = text;
        ALL.SetActive(false);
    }

    public void Close()
    {
        ALL.SetActive(true);
        GameObject[] Objects;

        Objects = GameObject.FindGameObjectsWithTag("Error");
        foreach (GameObject ob in Objects)
        {
            Destroy(ob);
        }
    }


}
