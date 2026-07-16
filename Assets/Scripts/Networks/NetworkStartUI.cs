using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkStartUI : MonoBehaviour
{
   #region Methodes
   
   
   public void Host()
   {
      
      //NetworkManager.Singleton.SetServer
      NetworkManager.Singleton.StartHost();
      transform.gameObject.SetActive(false);
   }

   public void Client()
   {
      NetworkManager.Singleton.StartClient();
      transform.gameObject.SetActive(false);
   }
   
   #endregion Methodes
}
