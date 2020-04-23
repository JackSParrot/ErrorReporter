using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ErrorReporter : MonoBehaviour
{
    [System.Serializable]
    public struct LogStruct
    {
        public string Version;
        public string UserId;
        public string Type;
        public string Message;
        public string StackTrace;
        public string Date;
    }

    [System.Serializable]
    public struct LogList
    {
        public List<LogStruct> Logs;
    }

    [SerializeField] GameObject _panelRoot = null;
    [SerializeField] TMPro.TextMeshProUGUI _userIdText = null;
    [SerializeField] TMPro.TextMeshProUGUI _versionText = null;
    [SerializeField] TMPro.TextMeshProUGUI _messageText = null;

    string _userId;
    string _version;
    LogList _logs = new LogList();

    public void TestException()
    {
        throw new Exception("Kaboom");
    }

    public void TestError()
    {
        Debug.LogError("Kaberror");
    }

    void Start()
    {
        _logs.Logs = new List<LogStruct>();

        _version = Application.version;

        if(PlayerPrefs.HasKey("UserID"))
        {
            _userId = PlayerPrefs.GetString("UserID");
        }
        else
        {
            _userId += (char)UnityEngine.Random.Range('a', 'z');
            _userId += (char)UnityEngine.Random.Range('a', 'z');
            _userId += ((ulong)(DateTime.UtcNow - new DateTime(1970,1,1)).TotalSeconds % 10000000ul).ToString();
            PlayerPrefs.SetString("UserID", _userId);
        }

        Application.logMessageReceived += OnLog;

        if(_panelRoot != null)
        {
            _panelRoot.SetActive(false);
        }
    }

    private void OnLog(string condition, string stackTrace, LogType type)
    {
        if(type == LogType.Exception || type == LogType.Error)
        {
            var log = new LogStruct
            {
                Version = _version,
                UserId = _userId,
                Type = type.ToString(),
                Message = condition,
                StackTrace = stackTrace.Substring(0, Mathf.Min(stackTrace.Length, 200)),
                Date = DateTime.UtcNow.ToString("dd/MM/yyyy H:mm:ss")
            };

            _logs.Logs.Add(log);

            if(_panelRoot != null)
            {
                _panelRoot.SetActive(true);
                _userIdText.text = _userId;
                _versionText.text = _version;
                _messageText.text = condition;
            }
            else
            {
                SendError();
            }
        }
    }

    public void SendError()
    {
        StartCoroutine(ErrorSender());
    }

    IEnumerator ErrorSender()
    {
        var message = JsonUtility.ToJson(_logs);

        const string url = "aqui tu url de servidor";
        var downloadHandler = new DownloadHandlerBuffer();
        var uploader = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(message));
        var request = new UnityWebRequest(url);
        request.method = UnityWebRequest.kHttpVerbPOST;
        request.useHttpContinue = false;
        request.redirectLimit = 50;
        request.timeout = 60;
        request.downloadHandler = downloadHandler;
        request.uploadHandler = uploader;
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log("Error, puede que no tenga red");
        }
        else
        {
            Debug.Log("Enviado con exito: " + request.downloadHandler.text);
            _logs.Logs.Clear();
        }
    }
}
