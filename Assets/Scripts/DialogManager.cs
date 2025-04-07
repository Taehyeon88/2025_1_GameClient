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

    [SerializeField] private Image portraitImage;                //캐릭터 초상화 이미지UI 요소 추가
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Button NextButton;

    [Header("Dialog Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private bool useTypewriterEffect = true;

    [Header("DialogChoices")]
    [SerializeField] private GameObject choicePanel;
    [SerializeField] private GameObject choiceButtonPrefab;

    private bool isTyping = false;
    private Coroutine typingCoroutine;  //코루틴 선언

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
            NextButton.onClick.AddListener(NextDialog);        //버튼 리스너 등록
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

        //대화 텍스트 설정 부분 수정
        if (useTypewriterEffect)
        {
            StartTypingEffect(currentDialog.text);
        }
        else
        {
            dialogText.text = currentDialog.text;                //대화 텍스트 설정
        }

            dialogText.text = currentDialog.text;                     //대화 텍스트 설정

        //초상화 설정 (새로 추가된 부분)
        if (currentDialog.portrait != null)
        {
            portraitImage.sprite = currentDialog.portrait;
            portraitImage.gameObject.SetActive(true);
        }
        else if (!string.IsNullOrEmpty(currentDialog.portraitPath))
        {
            //Resources 폴더에서 이미지 로드
            Sprite portrait = Resources.Load<Sprite>(currentDialog.portraitPath);   //Assets/Resources/Characters/Narrator.png
            if (portrait != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"Portrait not found at Path : {currentDialog.portraitPath}");
                portraitImage.gameObject.SetActive(false);
            }
        }
        else
        {
            portraitImage.gameObject.SetActive(false);         //초상화가 없으면 이미지 비활성화
        }

        //선택지 표시
        ClearChoices();
        if (currentDialog.choices != null && currentDialog.choices.Count > 0)
        {
            ShowChoices();
            NextButton.gameObject.SetActive(false);
        }
        else
        {
            NextButton.gameObject.SetActive(true);
        }

    }

    public void NextDialog()          //다음 대화로 진행
    {

        if (isTyping)      //타이핑 중이면 타이핑 완료처리()
        {
            StopTypingEffect();
            dialogText.text = currentDialog.text;
            isTyping = false;
            return;
        }

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

    //텍스트 타이핑 효과 코루틴
    private IEnumerator TypeText(string text)
    {
        dialogText.text = "";
        foreach (char c in text)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    //타이핑 효과 중지
    private void StopTypingEffect()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    private void StartTypingEffect(string text)
    {
        isTyping = true;
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine) ;
        }
        typingCoroutine = StartCoroutine(TypeText(text));
    }

    public void CloseDialog()         //대화종료
    {
        dialogPanel.SetActive(false);
        currentDialog = null;
        StopTypingEffect();           //타이핑 효과 중지 추가
    }

    //선택지 초기화
    private void ClearChoices()
    {
        foreach (Transform child in choicePanel.transform)
        {
            Destroy(child.gameObject);
        }
        choicePanel.SetActive(false);
    }

    //선택지 선택 처리
    public void SelectChoice(DialogChoiceSO choice)
    {
        if (choice != null && choice.nextId > 0)
        {
            DialogSO nextDialog = dialogDatabase.GetDialogById(choice.nextId);
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

    //선택지 표시
    private void ShowChoices()
    {
        choicePanel.SetActive(true);

        foreach (var choice in currentDialog.choices)
        {
            GameObject choiceGO = Instantiate(choiceButtonPrefab, choicePanel.transform);
            TextMeshProUGUI buttonText = choiceGO.GetComponentInChildren<TextMeshProUGUI>();
            Button button = choiceGO.GetComponent<Button>();

            if (buttonText != null)
            {
                buttonText.text = choice.text;
            }
            if (button != null)
            {
                DialogChoiceSO choiceSO = choice;          //람다식에서 사용하기 위해서 지역변수에 할당
                button.onClick.AddListener(() => SelectChoice(choiceSO));
            }
        }
    }
}
