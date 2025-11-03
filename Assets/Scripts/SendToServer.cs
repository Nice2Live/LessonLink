using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class SendToServer : MonoBehaviour
{
    [SerializeField] 
    private string serverUrl = "https://darnell-stockier-overfondly.ngrok-free.dev/"; // убедись, что URL корректен
    void Start()
    {
        string aboba = @"[ { ""subject_name"" : ""Алгебра и начала математического анализа"", ""subject_id"" : 33623649, ""periods"" : [ { ""name"" : ""1 полугодие"", ""marks"" : [ { ""id"" : 2858017437, ""comment"" : """", ""values"" : [ { ""five"" : 5.0, ""hundred"" : 100.0, ""ten"" : 10.0, ""original"" : ""5"", ""nmax"" : 5.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""05.09.2025"", ""is_point"" : false, ""control_form_id"" : 29409571, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Комбинированная работа"" }, { ""id"" : 2856772106, ""comment"" : """", ""values"" : [ { ""five"" : 5.0, ""hundred"" : 100.0, ""ten"" : 10.0, ""original"" : ""5"", ""nmax"" : 5.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""22.09.2025"", ""is_point"" : false, ""control_form_id"" : 14095812, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Опрос"" } ], ""start"" : ""01.09"", ""end"" : ""30.12"", ""start_iso"" : ""2025-09-01"", ""end_iso"" : ""2025-12-30"", ""avg_five"" : ""5.00"", ""avg_hundred"" : ""100.00"", ""avg_ten"" : ""10.00"" } ], ""avg_five"" : ""5.00"", ""avg_hundred"" : ""100.00"", ""avg_ten"" : ""10.00""} ... ]";
        Push(aboba);
    }
    public async Task Push(string message)
    {
        try
        {
            string encryptedId = SecureStorage.Encrypt(
                SecureStorage.Load("last_name") + " " + SecureStorage.Load("first_name")
            );

            string encryptedMsg = SecureStorage.Encrypt(message);

            string json = $"{{\"id\":\"{encryptedId}\", \"msg\":\"{encryptedMsg}\"}}";
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest www = new UnityWebRequest(serverUrl, "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                var operation = www.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield(); // позволяет не блокировать Unity main thread

#if UNITY_2020_1_OR_NEWER
                if (www.result != UnityWebRequest.Result.Success)
#else
                if (www.isNetworkError || www.isHttpError)
#endif
                {
                    //Debug.LogError($"Error sending to server: {www.error}");
                }
                else
                {
                    //Debug.Log($"Server response: {www.downloadHandler.text}");
                }
            }
        }
        catch (Exception ex)
        {
           // Debug.LogError($"Exception: {ex.Message}\n{ex.StackTrace}");
        }
    }
}