using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Bson;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    public static DialogManager instance { get; private set; }

    [Header("Dialog References")]
    [SerializeField] private DialogDatabaseSO dialogDatabase;

    [Header("UI References")]
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Button NextButton;

    private DialogSO currentDialog;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        if (dialogDatabase != null)
        {
            dialogDatabase.Initailize();           //초기화
        }
        else
        {
            Debug.Log("Dialog Database is not assigned to Dialog Manager");
        }
        if (NextButton != null)
        {
            //NextButton.onClick.AddListener(NextDialog);        //버튼 리스너 등록
        }
        else
        {
            Debug.LogError("Next Button is not assigned!");
        }
    }

    private void Start()
    {
        //UI초기화 후 대화 시작
        CloseDialog();        //자동으로 첫번째 대화시작
        StartDialog(1);
    }

    //ID로 대화 시작
    public void StartDialog(int dialogId)
    {
        DialogSO dialog = dialogDatabase.GetDialogById(dialogId);
        if (dialog != null)
        {
            StartDialog(dialog);
        }
        else
        {
            Debug.LogError($"Dialog with ID {dialogId} not found!");
        }
    }

    //DialogSO로 대화 시작
    public void StartDialog(DialogSO dialog)
    {
        if (dialog == null) return;

        currentDialog = dialog;
        ShowDialog();
        dialogPanel.SetActive(true);
    }

    public void ShowDialog()
    {
        if (currentDialog == null) return; 
        characterNameText.text = currentDialog.characterName;     //캐릭터 이름 설정
        dialogText.text = currentDialog.text;                     //대화 텍스트 설정
    }

    public void NextDialog()          //다음 대화로 진행
    {
        if (currentDialog != null && currentDialog.nextld > 0)
        {
            DialogSO nextDialog = dialogDatabase.GetDialogById(currentDialog.nextld);
            if (nextDialog != null)
            {
                currentDialog = nextDialog;
                ShowDialog();
            }
            else
            {
                CloseDialog();
            }
        }
        else
        {
            CloseDialog();
        }
    }

    public void CloseDialog()         //대화종료
    {
        dialogPanel.SetActive(false);
        currentDialog = null;
    }
}
