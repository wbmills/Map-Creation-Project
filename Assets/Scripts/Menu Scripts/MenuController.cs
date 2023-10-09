using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public List<GameObject> allMenuCanvases;
    public GameObject curMenu;

    // Start is called before the first frame update
    void Start()
    {
        allMenuCanvases = new List<GameObject>();
        UpdateMenuCanvases();
        SetCurMenu("Welcome Canvas");
        SetCurMenu(menuName:"Welcome Canvas");
    }

    private void UpdateMenuCanvases()
    {
        allMenuCanvases.Clear();
        GameObject[] allCans = GameObject.FindGameObjectsWithTag("Menu");
        foreach (GameObject c in allCans)
        {
            allMenuCanvases.Add(c);
        }
    }

    public void HideMenus()
    {
        foreach(GameObject m in allMenuCanvases)
        {
            m.SetActive(false);
        }
    }

    public void ShowCurMenu()
    {
        HideMenus();
        curMenu.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SetCurMenu(string menuName=null)
    {
        if (menuName != null)
        {
            foreach(GameObject m in allMenuCanvases)
            {
                if (m.name == menuName)
                {
                    curMenu = m;
                }
            }
        }

        if (curMenu == null | menuName == null)
        {
            curMenu = null;
        }
        ShowCurMenu();
/*        else if (menu != null && allMenuCanvases.Contains(menu))
        {
            curMenu = menu;
        }*/
    }
}
