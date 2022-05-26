using System.Collections;
using System.Collections.Generic;
using HandPhysicsToolkit.Helpers.Interfaces;
using Obi;
using UnityEngine;

public class BookManager : MonoBehaviour
{
    //leftpagenum is used by pagesideTextManager
    public int leftPageNum = 0;
    int simPageNum = 1;
    public int rightPageNum = 2;
    int versoInspectorPageNum = 0;
    int rectoInspectorPageNum = 1;
    public bool pairedMode = true;
    private bool isInspectorOn = true;

    [SerializeField] GameObject rightPage;
    [SerializeField] GameObject leftPage;
    [SerializeField] GameObject simPage;
    [SerializeField] GameObject versoInspectorPage;
    [SerializeField] GameObject rectoInspectorPage;
    [SerializeField] private GameObject bookSystemObject;
    private GameObject folioMenuText;

    [SerializeField] List<Material> pageList = new List<Material>(); 

    private void Start()
    {
        rightPage.GetComponent<Renderer>().material = pageList[rightPageNum];
        leftPage.GetComponent<Renderer>().material = pageList[leftPageNum];
        simPage.GetComponent<Renderer>().material = pageList[simPageNum];
        versoInspectorPage.GetComponent<Renderer>().material = pageList[versoInspectorPageNum];
        rectoInspectorPage.GetComponent<Renderer>().material = pageList[rectoInspectorPageNum];
        folioMenuText = GameObject.Find("Advance Page Name Text");
    }

    public void Increment(GameObject page,ref int pgn)
    {
        pgn++;
        if(pgn>=pageList.Count)
        {
            pgn = 0;
            page.GetComponent<Renderer>().material = pageList[pgn];
        }
        else
        {
            page.GetComponent<Renderer>().material = pageList[pgn];
        }
    }

    public void Decrement(GameObject page,ref int pgn)
    {
        pgn--;
        if ( pgn < 0)
        {
            pgn = pageList.Count-1;
            page.GetComponent<Renderer>().material = pageList[pgn];
        }
        else
        {
            page.GetComponent<Renderer>().material = pageList[pgn];
        }
    }

    public void IncrementAll()
    {
        Increment(rightPage, ref rightPageNum);
        Increment(leftPage, ref leftPageNum);
        Increment(simPage, ref simPageNum);
        if (pairedMode)
        {
            Increment(versoInspectorPage,  ref versoInspectorPageNum);
            Increment(rectoInspectorPage,  ref rectoInspectorPageNum);
        }
        
        //increment the control panel's page number display
        folioMenuText.GetComponent<MenuText>().IncrementState();
    }

    public void decrementAll()
    {
        Decrement(rightPage,ref rightPageNum);
        Decrement(leftPage,ref leftPageNum);
        Decrement(simPage,ref simPageNum);
        if (pairedMode)
        {
            Decrement(versoInspectorPage,  ref versoInspectorPageNum);
            Decrement(rectoInspectorPage,  ref rectoInspectorPageNum);
        }
        
        //decrement the control panel's page number display
        folioMenuText.GetComponent<MenuText>().DecrementState();
    }

    //increments only the inspector
    public void IncrementInspector()
    {
        Increment(versoInspectorPage, ref versoInspectorPageNum);
        Increment(rectoInspectorPage, ref rectoInspectorPageNum);
    }
    
    //decrements only the inspector
    public void DecrementInspector()
    {
        Decrement(versoInspectorPage, ref versoInspectorPageNum);
        Decrement(rectoInspectorPage, ref rectoInspectorPageNum);
    }

    //sets inspector pages to book's currently displayed pages, then inverts attachedMode bool
    public void ToggleInspectorMode()
    {
        if (pairedMode == false)
        {
            versoInspectorPageNum = leftPageNum;
            rectoInspectorPageNum = simPageNum;
            versoInspectorPage.GetComponent<Renderer>().material = pageList[versoInspectorPageNum];
            rectoInspectorPage.GetComponent<Renderer>().material = pageList[rectoInspectorPageNum];
        }
        pairedMode = !pairedMode;
    }

    public void ToggleInspector()
    {
        isInspectorOn = !isInspectorOn;
        versoInspectorPage.GetComponent<MeshRenderer>().enabled = isInspectorOn;
        rectoInspectorPage.GetComponent<MeshRenderer>().enabled = isInspectorOn;
    }

    public void ResetExperienceBM()
    {
        leftPageNum = 0;
        simPageNum = 1;
        rightPageNum = 2;
        versoInspectorPageNum = 0;
        rectoInspectorPageNum = 1;
        leftPage.GetComponent<Renderer>().material = pageList[leftPageNum];
        simPage.GetComponent<Renderer>().material = pageList[simPageNum];
        rightPage.GetComponent<Renderer>().material = pageList[rightPageNum];
        versoInspectorPage.GetComponent<Renderer>().material = pageList[versoInspectorPageNum];
        rectoInspectorPage.GetComponent<Renderer>().material = pageList[rectoInspectorPageNum];

        ResetPageParticles(null);
        bookSystemObject.GetComponent<ParticlePositionManager>().LoadParticles("RightSideResting");
    }

    IEnumerator ResetPageRoutine(BaseInteractor interactor)
    {
        simPage.GetComponent<ObiTearableCloth>().ResetParticles();
        simPage.GetComponent<ObiTearableCloth>().RemoveFromSolver();
        simPage.GetComponent<ObiTearableCloth>().AddToSolver();

        yield return new WaitForEndOfFrame();

        yield return null;
    }

    public void ResetPageParticles(BaseInteractor interactor)
    {
        StartCoroutine(ResetPageRoutine(interactor));
    }
    
    
}
