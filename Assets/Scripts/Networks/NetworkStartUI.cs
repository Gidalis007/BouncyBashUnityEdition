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
      if (AudioManager.Instance != null)
      {
          AudioManager.Instance.PlayUI("ui_click");
      }

      NetworkManager.Singleton.StartHost();
      transform.gameObject.SetActive(false);
   }

   public void Client()
   {
      if (AudioManager.Instance != null)
      {
          AudioManager.Instance.PlayUI("ui_click");
      }

      NetworkManager.Singleton.StartClient();
      transform.gameObject.SetActive(false);
   }

   #endregion Methodes
}
