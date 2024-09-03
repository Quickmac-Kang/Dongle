using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("----------- [Core ]")]
    public int score; 
    public int maxLevel;
    public bool isOver;

    [Header("-----------[ Object Pooling] ")]
    public GameObject donglePrefebs;
    public Transform dongleGroup;
    public List<Dongle> donglePool;
    public GameObject effectPrefebs;
    public Transform effectGroup;
    public List<ParticleSystem> effectPool;
    [Range(1, 30)]
    public int poolSize;
    public int poolCursor;
    public Dongle lastDongle;

    [Header("---------- [ Audio]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;

    public enum Sfx { LeveUp, Next,  Attach, Button, Over };
    int sfxCusor;

    [Header("-------[ UI ]")]
    public GameObject startGroup;
    public GameObject endGroup;
    public Text scoreText;
    public Text maxScoreText;
    public Text subScoreText;

    [Header("-------[ Etc ]")]
    public GameObject line;
    public GameObject bottom;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();

        for(int i = 0; i < poolSize; i++)
        {
            MakeDongle();
        }
        if(!PlayerPrefs.HasKey("MaxScore"))
        {
            PlayerPrefs.SetInt("MaxScore", 0);
        }
        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();
    }

    // Start is called before the first frame update
    public void GameStart()
    {
        line.SetActive(true);
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);

        bgmPlayer.Play();
        SfxPlayer(Sfx.Button);

        Invoke("NextDongle", 1.5f);
      
    }

    Dongle MakeDongle()
    {
        GameObject instantEffectObj = Instantiate(effectPrefebs, effectGroup);
        instantEffectObj.name = "Effect " + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        GameObject instantDongleObj = Instantiate(donglePrefebs, dongleGroup);
        instantDongleObj.name = "Dongle " + donglePool.Count;
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        instantDongle.manager = this;
        instantDongle.effect = instantEffect;

        donglePool.Add(instantDongle);

        return instantDongle;
    }

    Dongle GetDongle()
    {
        for(int i  = 0; i < donglePool.Count; i++)
        {
            poolCursor = (poolCursor + 1) % donglePool.Count;
            if(!donglePool[poolCursor].gameObject.activeSelf )
            {
                return donglePool[poolCursor];
            }
        }

        return MakeDongle();
    }

    void NextDongle()
    {
        if(isOver)
        {
            return;
        }

        lastDongle = GetDongle();
        lastDongle.level = Random.Range(0, maxLevel);
        lastDongle.gameObject.SetActive(true);

        SfxPlayer(Sfx.Next);
        StartCoroutine(WaitNext());
    }

    IEnumerator WaitNext()
    {
        while(lastDongle != null)
        {
            yield return null;
        }

        yield return new WaitForSeconds(2.5f);

        NextDongle();
    }
    
    public void TouchDown()
    {
        if (lastDongle == null)
            return;
        lastDongle.Drag();
    }

    public void TouchUp()
    {
        if (lastDongle == null)
            return;
        lastDongle.Drop();
        lastDongle = null;

    }    

    public void GameOver()
    {
        if(isOver)
        {
            return;
        }
        isOver = true;

        StartCoroutine("GameOverRoutine");
    }

    IEnumerator GameOverRoutine()
    {
        // 장면안에 활성화된 모든 동글을 가져온다.
        Dongle[] dongle = FindObjectsOfType<Dongle>();

        for (int i = 0; i < dongle.Length; i++)
        {
            dongle[i].rigid.simulated = false;
            
        }
        // 목록에 있는 동글을 하나씩 지운다. 
        for (int i = 0; i < dongle.Length; i++)
        {
            dongle[i].Hide(Vector3.up * 100);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);

        // 최고 점수 저장
        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);
        //게임오버 표시
        subScoreText.text = "점수 : " + scoreText.text;
        endGroup.SetActive(true);

        bgmPlayer.Stop();
        SfxPlayer(Sfx.Over);
    }

    public void Reset()
    {
        SfxPlayer(Sfx.Button);
        StartCoroutine("ResetRoutine");
    }

    IEnumerator ResetRoutine()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Main");
    }


    public void SfxPlayer(Sfx type)
    {
        switch(type)
        {
            case Sfx.LeveUp:
                sfxPlayer[sfxCusor].clip = sfxClip[Random.Range(0, 3)];
                break;
            case Sfx.Next:
                sfxPlayer[sfxCusor].clip = sfxClip[3];
                break;
            case Sfx.Attach:
                sfxPlayer[sfxCusor].clip = sfxClip[4];
                break;
            case Sfx.Button:
                sfxPlayer[sfxCusor].clip = sfxClip[5];
                break;
            case Sfx.Over:
                sfxPlayer[sfxCusor].clip = sfxClip[6];
                break;
        }
        sfxPlayer[sfxCusor].Play();
        sfxCusor = (sfxCusor + 1) % sfxPlayer.Length;

    }

    private void Update()
    {
        if(Input.GetButtonDown("Cancel"))
        {
            Application.Quit();
        }
    }

    // update 이후 실행
    private void LateUpdate()
    {
        scoreText.text = score.ToString();
    }
}
