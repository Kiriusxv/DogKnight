using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public class SceneController : Singleton<SceneController>, IEndGameObserver
{
    GameObject player;
    NavMeshAgent playerAgent;
    bool fadeFinshed;

    [Header("Prefabs")]
    public GameObject playerPrefab;
    public SceneFader sceneFaderPrefab;

    #region 生命周期函数
    protected override void Awake()
    {
        base.Awake();
        fadeFinshed = true;

        // 切换场景时不要销毁控制器
        DontDestroyOnLoad(this);
    }

    void OnEnable()
    {
        GameManager.Instance.AddObserver(this);
    }

    #endregion

    #region 传送门转换
    public void TransitionToDestination(TransitionPoint transitionPoint)
    {
        switch (transitionPoint.transitionType)
        {
            case TransitionPoint.TransitionType.SameScene:
                {
                    StartCoroutine(Transition(SceneManager.GetActiveScene().name, transitionPoint.destinationTag));
                    break;
                }
            case TransitionPoint.TransitionType.DifferenctScene:
                {
                    StartCoroutine(Transition(transitionPoint.sceneName, transitionPoint.destinationTag));
                    break;
                }
        }
    }

    IEnumerator Transition(string sceneName, TransitionDestination.DestinationTag destinationTag)
    {
        // 保存数据
        SaveManager.Instance.SavePlayerData();
        // 判断是否是当前场景
        if (SceneManager.GetActiveScene().name != sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName);
            var entranc = GetDestination(destinationTag);
            yield return Instantiate(playerPrefab, entranc.transform.position, entranc.transform.rotation);
            // 加载数据
            // SaveManager.Instance.LoadPlayerData();
            yield break;
        }
        else
        {
            player = GameManager.Instance.playerStats.gameObject;
            playerAgent = player.GetComponent<NavMeshAgent>();
            playerAgent.enabled = false;
            var entranc = GetDestination(destinationTag);
            if (entranc)
            {
                player.transform.SetPositionAndRotation(entranc.transform.position, entranc.transform.rotation);
            }
            playerAgent.enabled = true;
            yield return null;
        }
    }

    private TransitionDestination GetDestination(TransitionDestination.DestinationTag destinationTag)
    {
        var entrances = FindObjectsOfType<TransitionDestination>();
        foreach (var entrance in entrances)
        {
            if (entrance.destinationTag == destinationTag)
            {
                return entrance;
            }
        }
        return null;
    }
    #endregion

    #region 主菜单加载关卡
    public void TransitionToFirstLevel()
    {
        StartCoroutine(LoadLevel("Game"));
    }

    public void TransitionToLoadGame()
    {
        StartCoroutine(LoadLevel(SaveManager.Instance.SceneName));
    }

    public void TransitionToMainMenu()
    {
        StartCoroutine(LoadMain());
    }

    IEnumerator LoadLevel(string scene)
    {
        SceneFader fade = Instantiate(sceneFaderPrefab);
        if (scene != "")
        {
            yield return fade.FadeIn(fade.fadeInDuration);
            yield return SceneManager.LoadSceneAsync(scene);
            yield return player = Instantiate(playerPrefab, GameManager.Instance.GetEntrance().position, GameManager.Instance.GetEntrance().rotation);
            // 保存数据
            SaveManager.Instance.SavePlayerData();
            yield return fade.FadeOut(fade.fadeOutDuration);
        }
    }

    IEnumerator LoadMain()
    {
        SceneFader fade = Instantiate(sceneFaderPrefab);
        yield return fade.FadeIn(fade.fadeInDuration);
        yield return SceneManager.LoadSceneAsync("Main");
        yield return fade.FadeOut(fade.fadeOutDuration);
        yield break;
    }
    #endregion


    #region 观察者接口
    public void EndNotify()
    {
        if (fadeFinshed)
        {
            fadeFinshed = false;
            StartCoroutine(LoadMain());
        }
    }

    #endregion
}