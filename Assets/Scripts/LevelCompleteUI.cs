using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LevelCompleteUI : MonoBehaviour
{
    [SerializeField] private GameObject level_complete_UI;
    [SerializeField] private Button level_complete_button;

    // Start is called before the first frame update
    void Start()
    {
        level_complete_UI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PopupLevelCompleteUI()
    {
        level_complete_UI.GetComponent<RectTransform>().localScale = Vector3.zero;
        level_complete_UI.SetActive(true);
        level_complete_UI.GetComponent<RectTransform>().DOScale(1, 0.4f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            level_complete_button.onClick.AddListener(NextLevel);
        });
    }


    public void NextLevel()
    {
        int scenindex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        scenindex = scenindex + 1;
        if (scenindex >= UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
        {
            scenindex = 0;
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(scenindex);
    }
}
