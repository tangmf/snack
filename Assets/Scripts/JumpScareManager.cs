using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class JumpScareManager : MonoBehaviour
{
    public static JumpScareManager Instance;
    Coroutine currentJumpScare;

    [Header("UI")]
    public Image jumpScareImage;

    [Header("Audio")]
    public float jumpScareVolume = 1.0f;

    [System.Serializable]
    public class JumpScareData
    {
        public MonsterType monsterType;
        public Sprite image;
        public AudioClip sound;
        public float duration = 0.5f;
    }

    public JumpScareData[] jumpScares;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (jumpScareImage != null)
            jumpScareImage.gameObject.SetActive(false);
    }

    public void PlayJumpScare(MonsterType type)
    {
        Debug.Log("PlayJumpScare called with " + type);

        JumpScareData data = GetData(type);
        if (data == null) return;

        if (currentJumpScare != null)
            StopCoroutine(currentJumpScare);

        currentJumpScare = StartCoroutine(DoJumpScare(data));
    }

    JumpScareData GetData(MonsterType type)
    {
        foreach (var js in jumpScares)
            if (js.monsterType == type)
                return js;

        return null;
    }

    IEnumerator DoJumpScare(JumpScareData data)
    {
        jumpScareImage.sprite = data.image;
        jumpScareImage.color = Color.white;
        jumpScareImage.gameObject.SetActive(true);
        jumpScareImage.transform.SetAsLastSibling();

        if (AudioManager.instance != null && data.sound != null)
        {
            AudioManager.instance.PlayAudioClip(
                data.sound,
                Camera.main.transform,
                jumpScareVolume
            );
        }

        yield return new WaitForSecondsRealtime(data.duration);

        jumpScareImage.gameObject.SetActive(false);
    }
}
