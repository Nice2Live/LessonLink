using UnityEngine;
using System;
using System.Security.Cryptography;
using System.Text;

public static class SecureStorage
{
    private static string key = "test";

    public static void Save(string id, string value)
    {
        string encrypted = Encrypt(value, key);
        PlayerPrefs.SetString(id, encrypted);
        PlayerPrefs.Save();
    }
    
    public static void ReSave(string id, string value)
    {
        PlayerPrefs.DeleteKey(id);
        Save(id, value);
    }
    
    public static string Load(string id)
    {   
        if (!PlayerPrefs.HasKey(id))
            return null;

        string encrypted = PlayerPrefs.GetString(id);
        return Decrypt(encrypted, key);
    }
    
    public static string Encrypt(string text, string key = "test")
    {
        byte[] data = Encoding.UTF8.GetBytes(text);
        using (Aes aes = Aes.Create())
        {
            aes.Key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(key));
            aes.IV = new byte[16];
            var encryptor = aes.CreateEncryptor();
            byte[] result = encryptor.TransformFinalBlock(data, 0, data.Length);
            return Convert.ToBase64String(result);
        }
    }

    private static string Decrypt(string text, string key)
    {
        byte[] data = Convert.FromBase64String(text);
        using (Aes aes = Aes.Create())
        {
            aes.Key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(key));
            aes.IV = new byte[16];
            var decryptor = aes.CreateDecryptor();
            byte[] result = decryptor.TransformFinalBlock(data, 0, data.Length);
            return Encoding.UTF8.GetString(result);
        }
    }
}