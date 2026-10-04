using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameSceneManager : NetworkBehaviour
{


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ChangeSceneRpc(string sceneName)
    {

        if (!IsServer)
        {
            return;
        }

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == sceneName)
        {
            return;
        }

        SceneTransition.EnsureExists();
        NotifyTransitionRpc();
        StartCoroutine(LoadSceneAfterFade(sceneName));
    }

    private IEnumerator LoadSceneAfterFade(string sceneName)
    {
        yield return new WaitForSeconds(SceneTransition.Instance.FadeDuration);

        if (NetworkManager == null || !IsServer)
        {
            yield break;
        }

        NetworkManager.Singleton.SceneManager.LoadScene(
            sceneName,
            LoadSceneMode.Single
        );
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void NotifyTransitionRpc()
    {
        SceneTransition.EnsureExists();
        SceneTransition.Instance.FadeOut();
    }

}
