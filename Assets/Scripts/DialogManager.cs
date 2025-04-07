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
            dialogDatabase.Initailize();           //�ʱ�ȭ
        }
        else
        {
            Debug.Log("Dialog Database is not assigned to Dialog Manager");
        }
        if (NextButton != null)
        {
            //NextButton.onClick.AddListener(NextDialog);        //��ư ������ ���
        }
        else
        {
            Debug.LogError("Next Button is not assigned!");
        }
    }

    private void Start()
    {
        //UI�ʱ�ȭ �� ��ȭ ����
        CloseDialog();        //�ڵ����� ù��° ��ȭ����
        StartDialog(1);
    }

    //ID�� ��ȭ ����
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

    //DialogSO�� ��ȭ ����
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
        characterNameText.text = currentDialog.characterName;     //ĳ���� �̸� ����
        dialogText.text = currentDialog.text;                     //��ȭ �ؽ�Ʈ ����
    }

    public void NextDialog()          //���� ��ȭ�� ����
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

    public void CloseDialog()         //��ȭ����
    {
        dialogPanel.SetActive(false);
        currentDialog = null;
    }
}
