using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkStartUI : MonoBehaviour
{
   #region Serializedfields
   [SerializeField] private string _sceneName;
   #endregion Serializedfields
   #region Methodes
   
   
   public void Host()
   {
      
      //NetworkManager.Singleton.SetServer
      NetworkManager.Singleton.StartHost();
      transform.gameObject.SetActive(false);
      SceneManager.LoadScene(_sceneName);
   }

   public void Client()
   {
      NetworkManager.Singleton.StartClient();
      transform.gameObject.SetActive(false);
      SceneManager.LoadScene(_sceneName);
   }
   
   #endregion Methodes
}
