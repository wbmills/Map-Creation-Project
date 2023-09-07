using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class menuController : MonoBehaviour
{
    private GameObject sceneController;
    private playerMovement pmScript;
    private townGeneration tgScript;
    private loadTown ltScript;
    public bool isMenuOpen;
    public Button new_button;
    public Button load_button;
    public Button save_button;
    public Dropdown saveOptions;

    void Start()
    {
        sceneController = GameObject.Find("SceneManager");
        pmScript = sceneController.GetComponent<playerMovement>();
        tgScript = sceneController.GetComponent<townGeneration>();
        ltScript = sceneController.GetComponent<loadTown>();

        load_button.onClick.AddListener(ltScript.loadObjects);
        save_button.onClick.AddListener(ltScript.saveObjects);
        new_button.onClick.AddListener(tgScript.generate);
        saveOptions.AddOptions(new List<string>() {"Save 1", "Save 2", "Save 3" });
        saveOptions.value = 0;
    }

    // Update is called once per frame
    void Update()
    {
        isMenuOpen = pmScript.isMenuActive;
        load_button.enabled = isMenuOpen;
        save_button.enabled = isMenuOpen;
        new_button.enabled = isMenuOpen;
        saveOptions.enabled = isMenuOpen;
    }
}
