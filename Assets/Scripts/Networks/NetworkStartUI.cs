using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkStartUI : MonoBehaviour
{
   /*private void OnGUI()
   {
      float w = 200, h = 200;
      float x = 10f, y = 10f;

      if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
      {
         if (GUI.Button(new Rect(x,y,w,h),"Host")) NetworkManager.Singleton.StartHost();
         if (GUI.Button(new Rect(x,y+h+10,w,h),"Client")) NetworkManager.Singleton.StartClient();
         if (GUI.Button(new Rect(x,y+2*(h+10),w,h),"Server") ) NetworkManager.Singleton.StartServer();
      }
   }*/
   
   public void Host()
   {
      NetworkManager.Singleton.StartHost();
      transform.gameObject.SetActive(false);
   }

   public void Client()
   {
      NetworkManager.Singleton.StartClient();
      transform.gameObject.SetActive(false);
   }
}
