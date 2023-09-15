using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuInput
{
    public int houseNum;
    public int treeNum;
    public int detailsNum;
    public int roadSize;
}

public class menuController : MonoBehaviour
{
    public TMP_InputField fbxInputFileName;
    private MenuInput menuInput;
    private GameObject[] menuArr;
    private bool menuStatus;
    GameObject[] allCams;
    private playerMovement pmScript;
    private townGeneration tgScript;
    private loadTown ltScript;

    public GameObject currentMenu;
    public Button new_button;
    public Button load_button;
    public Button save_button;
    public Dropdown saveOptions;

    void Start()
    {
        if (PlayerPrefs.HasKey("lastFbxFile"))
        {
            fbxInputFileName.text = PlayerPrefs.GetString("lastFbxFile");
        }
        else
        {
            PlayerPrefs.SetString("lastFbxFile", "default.fbx");
        }
        currentMenu = null;
        menuArr = GameObject.FindGameObjectsWithTag("Menu");
        foreach(GameObject menu in menuArr)
        {
            menu.SetActive(false);
        }
        pmScript = gameObject.GetComponent<playerMovement>();
        tgScript = gameObject.GetComponent<townGeneration>();
        ltScript = gameObject.GetComponent<loadTown>();
        setupMenuDefaults();
        setMenu("WelcomeCanvas");
    }

    public MenuInput getMenuInput()
    {
        return menuInput;
    }

    private void setupMenuDefaults()
    {
        saveOptions.AddOptions(new List<string>() { "Save 1", "Save 2", "Save 3" });
        saveOptions.value = 0;
    }

    public void altMenuStatus()
    {
        menuStatus = !menuStatus;
    }

    public void setMenuStatus(bool status)
    {
        menuStatus = status;
    }

    public void setMenu(string menuChoice)
    {
        if (!menuStatus)
        {
            altMenuStatus();
        }

        foreach(GameObject ob in menuArr)
        {
            if (ob.name == menuChoice)
            {
                if (currentMenu != null)
                {
                    currentMenu.SetActive(false);
                }
                currentMenu = ob;
                currentMenu.SetActive(true);
            }
        }
    }


    public bool menuExists(string menuChoice)
    {
        foreach(GameObject ob in menuArr)
        {
            if (ob.name == menuChoice)
            {
                return true;
            }
        }
        return false;
    }
}
