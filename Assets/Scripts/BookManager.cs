using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookManager : MonoBehaviour
{
    int rightPageNum = 2;
    int leftPageNum = 0;
    int simPageNum = 1;

    [SerializeField] GameObject rightPage;
    [SerializeField] GameObject leftPage;
    [SerializeField] GameObject simPage;
    
    [SerializeField] Material testMat;
    [SerializeField] List<Material> pageList = new List<Material>(); 

    private void Start()
    {

        rightPage.GetComponent<Renderer>().material = pageList[rightPageNum];
        leftPage.GetComponent<Renderer>().material = pageList[leftPageNum];
        simPage.GetComponent<Renderer>().material = pageList[simPageNum];
    }
    
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            Increment(rightPage, ref rightPageNum);
            Increment(leftPage, ref leftPageNum);
            Increment(simPage, ref simPageNum);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            Decrement(rightPage,ref rightPageNum);
            Decrement(leftPage,ref leftPageNum);
            Decrement(simPage,ref simPageNum);
        }
    }

    public void Increment(GameObject page,ref int pgn)
    {
        pgn++;
        Debug.Log(page.name + pgn + " was incremented");
        if(pgn>pageList.Count || pgn < 0)
        {
            page.GetComponent<Renderer>().material = testMat;
        }
        else
        {
            page.GetComponent<Renderer>().material = pageList[pgn];
        }
    }

    public void Decrement(GameObject page,ref int pgn)
    {
        pgn--;
        if (pgn > pageList.Count || pgn < 0)
        {
            page.GetComponent<Renderer>().material = testMat;
        }
        else
        {
            page.GetComponent<Renderer>().material = pageList[pgn];
        }
    }
}
