using System.Security.Cryptography;
using System.Text;

namespace PMS.API.Application.Common.Security;

public class EncryptionService
{
  public static string Encrypt(string textToEncrypt, string encryptionKey = "")
  {
    var toEncryptArray = Encoding.UTF8.GetBytes(textToEncrypt);

    var hashmd5 = MD5.Create();
    var keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));
    hashmd5.Clear();

    var aes = Aes.Create();
    aes.Key = keyArray;

    var cTransform = aes.CreateEncryptor();
    //transform the specified region of bytes array to resultArray
    var resultArray = cTransform.TransformFinalBlock
            (toEncryptArray, 0, toEncryptArray.Length);
    //Release resources held by TripleDes Encryptor
    aes.Clear();

    //Return the encrypted data into unreadable string format
    return Convert.ToBase64String(resultArray, 0, resultArray.Length);
  }

  public static string Decrypt(string textToDecrypt, string encryptionKey = "")
  {
    var toEncryptArray = Convert.FromBase64String(textToDecrypt);

    //if hashing was used get the hash code with regards to your key
    var hashmd5 = MD5.Create();
    var keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));
    hashmd5.Clear();

    var aes = Aes.Create();
    aes.Key = keyArray;

    var cTransform = aes.CreateDecryptor();
    var resultArray = cTransform.TransformFinalBlock
            (toEncryptArray, 0, toEncryptArray.Length);
    //Release resources held by TripleDes Encryptor
    aes.Clear();
    //return the Clear decrypted TEXT
    return Encoding.UTF8.GetString(resultArray);
  }
}
