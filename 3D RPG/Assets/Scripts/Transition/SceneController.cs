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

    #region �������ں���
    protected override void Awake()
    {
        base.Awake();
        fadeFinshed = true;

        // �л�����ʱ��Ҫ���ٿ�����
        DontDestroyOnLoad(this);
    }

    void OnEnable()
    {
        GameManager.Instance.AddObserver(this);
    }

    #endregion

    #region ������ת��
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
        // ��������
        SaveManager.Instance.SavePlayerData();
        // �ж��Ƿ��ǵ�ǰ����
        if (SceneManager.GetActiveScene().name != sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName);
            var entranc = GetDestination(destinationTag);
            yield return Instantiate(playerPrefab, entranc.transform.position, entranc.transform.rotation);
            // ��������
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

    #region ���˵����عؿ�
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
            // ��������
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


    #region �۲��߽ӿ�
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