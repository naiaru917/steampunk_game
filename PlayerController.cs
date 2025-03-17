using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private Animator anim;
    private Rigidbody rb;
    private SkinnedMeshRenderer[] skinMeshRen;

    //カメラ
    public GameObject mainCamera;  //三人称視点のカメラ（メインカメラ）
    public GameObject subCamera;   //一人称視点のカメラ
    public Vector3 cameraForward;  //カメラから見た正面
    public Vector3 moveForward;    //正面方向への移動量
    public static bool CameraM;     //カメラがメインカメラ（三人称視点）であるかの判定

    //移動・位置情報
    public float speed;              //現在の移動速度
    public float walkspeed = 3.0f;   //歩く時の移動速度
    public float runSpeed = 5.0f;    //走る時の移動速度
    public float moveX;              // X方向の移動距離
    public float moveZ;              // Z方向の移動距離
    public static bool isMove;       //移動が可能かの判定
    Vector3 playerPos;               //プレイヤーの位置情報

    //ジャンプ
    public float jumpForce = 5.0f;  //ジャンプ力
    bool isJump, isJumpWait;        //ジャンプを行えるかの判定
    float JumpWaitTimer;

    //スコア
    public Text scoreTxt;
    public int score;

    //タイマー
    public float timer;
    public Text TimeTxt;
    public Text clearTimeTxt;

    //イベント処理
    public Text clearTxt;
    public Text MissionTxt;
    public GameObject Event;
    public GameObject Stairs;
    private GameObject[] ChildStairs;

    //ポーズ画面
    public static bool isPause;     //ポーズ画面であるかの判定
    public GameObject Pause;
    public static bool isClear;

    //効果音
    public AudioClip walkSound;  // 歩行時の音
    public AudioClip runSound;   // 走行時の音
    public AudioClip getSound;   // アイテム獲得音
    public AudioClip fallSound;  // 落下音
    public AudioClip eventSound; // イベントの音
    public AudioClip bgmClip;    // BGM

    private AudioSource footstepSource;  // 足音用 AudioSource
    private AudioSource bgmSource;       // BGM用 AudioSource
    private AudioSource sfxSource;       // 効果音用 AudioSource



    //階段
    public int cntStair;    //表示されている階段の個数を数える
    public bool stairFlg;   //階段を表示出来る状況であるかの判定

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        skinMeshRen = GetComponentsInChildren<SkinnedMeshRenderer>();

        speed = walkspeed;      //最初は移動スピードを「walkSpeed」に
        score = 3;
        timer = 0;

        // 足音用 AudioSource
        footstepSource = gameObject.AddComponent<AudioSource>();
        footstepSource.loop = true;  // 足音はループ再生
        footstepSource.volume = 0.03f; // 音量調整

        // BGM用 AudioSource
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.clip = bgmClip;
        bgmSource.loop = true;   // BGMはループ再生
        bgmSource.volume = 0.007f; // 音量調整
        bgmSource.Play();        // ゲーム開始時にBGM再生

        // 効果音用 AudioSourse
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.volume = 0.03f; // 音量調整


        scoreTxt.text = "残り歯車×" + score;
        MissionTxt.text = "<MISSION> 歯車を集めよう";
        TimeTxt.text = "00:00";

        clearTxt.enabled = false;       //「CLEAR」のテキストを非表示に
        clearTimeTxt.enabled = false;   //クリア時間のテキストを非表示に
        Event.SetActive(false);
        subCamera.SetActive(false);     //サブカメラ（一人称視点）を非表示に
        CameraM = true;
        isMove = true;
        isPause = false;
        Pause.SetActive(false);
        isClear = false;

        ChildStairs = new GameObject[Stairs.transform.childCount];

        //階段の子オブジェクトを取得し配列に格納
        for (int i = 0; i < Stairs.transform.childCount; i++)
        {
            ChildStairs[i] = Stairs.transform.GetChild(i).gameObject;
        }

        cntStair = 0;
        stairFlg = false;

        // Rigidbodyの回転を固定
        if (rb == null)
        {
            Debug.LogWarning("Rigidbody not found on " + gameObject.name);
        }
        else
        {
            rb.freezeRotation = true;
        }

        SetCharacterVisibility(true);
    }

   

    // Update is called once per frame
    void Update()
    {
        if(isMove == true)
        {
            //WASDと矢印キーで移動距離を入力
            moveX = Input.GetAxis("Horizontal");
            moveZ = Input.GetAxis("Vertical");

            //タイマーの管理
            timer += Time.deltaTime;
            var span = new TimeSpan(0, 0, (int)timer);
            TimeTxt.text = span.ToString(@"mm\:ss");

            //ダッシュ
            if (Input.GetKey(KeyCode.LeftShift) && !isJump)
            {
                speed = runSpeed;           //移動速度を「runSpeed」に
                anim.SetBool("run", true);  //アニメーション「Run」を再生
            }
            else
            {
                speed = walkspeed;           //移動速度を「walkSpeed」に
                anim.SetBool("run", false);  //アニメーション「Run」を解除
            }

            //ジャンプ
            Jump();

            //カメラの切り替え
            CameraSwitch();

            //歯車を集め終わった時の処理
            if (score <= 0)
            {
                MissionTxt.text = "<MISSION> 城へ向かおう！";
                Event.SetActive(true);
                scoreTxt.enabled = false;
            }

        }

        //ポーズ画面
        PauseMenue();

        //階段を一つずつ表示
        StairsEvent();
    }

    private void FixedUpdate()
    {
        if (isClear == false)
        {
            if (isMove == true)
            {
                MovePlayer();
            }
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        isJump = false;     //プレイヤーが地面に接しているかの判定
    }

    private void OnTriggerEnter(Collider other)
    {
        //アイテムに触れた時
        string name = other.gameObject.name;
        if (name.Contains("Item"))
        {
            Destroy(other.gameObject);      //触れたアイテムを画面から削除する
            score -= 1;
            scoreTxt.text = "残り歯車×" + score;
            sfxSource.PlayOneShot(getSound);    //効果音を再生
        }

        //イベントポイントに到着したとき
        string eventName = other.gameObject.name;
        if (name.Contains("EventPoint"))
        {
            stairFlg = true;
            sfxSource.PlayOneShot(eventSound);    //効果音を再生

        }

        //クリアポイントに到着したとき
        string ClearName = other.gameObject.name;
        if (name.Contains("ClearPoint"))
        {
            GameClear();
            sfxSource.PlayOneShot(eventSound);    //効果音を再生
            speed = 0;
            rb.velocity = Vector3.zero;  // 物理的に動かなくする
            anim.SetBool("walk", false);
            anim.SetBool("run", false);
            footstepSource.Stop();   //歩行音を終了

        }

        //落下したとき
        string DeadName = other.gameObject.name;
        if (name.Contains("DeadLine"))
        {
            transform.position = new Vector3(0,5,-160);
            sfxSource.PlayOneShot(fallSound);    //効果音を再生
        }

    }

    public void Reset()
    {
        SceneManager.LoadScene(0);
    }

    public void Resume()
    {
        isMove = true;
        isPause = false;
        Pause.SetActive(false);
        bgmSource.UnPause();
    }

    public void End()
    {
        Application.Quit();
    }

    public void GameClear()
    {
        isMove = false;
        isPause = true;
        isClear = true;

        //クリア画面を表示
        Pause.SetActive(true);
        clearTxt.enabled = true;
        clearTimeTxt.text = TimeTxt.text;
        clearTimeTxt.enabled = true;
        TimeTxt.enabled = false;
        scoreTxt.enabled = false;
        MissionTxt.enabled = false;
    }

    IEnumerator wait()
    {
        yield return new WaitForSeconds(0.5f);      //0.5秒待つ
        cntStair += 1;
     }


    //カメラの切り替え
    private void CameraSwitch()
    {
        if (CameraM == true)
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                mainCamera.SetActive(false);
                subCamera.SetActive(true);
                CameraM = false;
                SetCharacterVisibility(false);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                mainCamera.SetActive(true);
                subCamera.SetActive(false);
                CameraM = true;
                SetCharacterVisibility(true);
            }
        }
    }

    //ジャンプ
    private void Jump()
    {
        if (Input.GetKeyUp("space"))
        {
            if (!isJump && !isJumpWait)  //isJumpとisJumpWaitの両方がfalseのとき
            {
                anim.Play("Jump", 0, 0);    //アニメーション「Jump」を再生
                isJumpWait = true;
                JumpWaitTimer = 0.2f;
            }
        }
        //少し経ってからジャンプ
        if (isJumpWait)
        {
            JumpWaitTimer -= Time.deltaTime;
            if (JumpWaitTimer < 0)
            {
                GetComponent<Rigidbody>().velocity = transform.up * jumpForce;    //上の方向に力を加える
                isJumpWait = false;
                isJump = true;
            }
        }
    }


    //プレイヤーの移動
    private void MovePlayer()
    {
        if (CameraM == true)
        {
            // メインカメラの方向から、X-Z平面の単位ベクトルを取得
            cameraForward = Vector3.Scale(mainCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
            // 方向キーの入力値とカメラの向きから、移動方向を決定
            moveForward = cameraForward * moveZ + mainCamera.transform.right * moveX;
        }
        else
        {
            // サブカメラの方向から、X-Z平面の単位ベクトルを取得
            cameraForward = Vector3.Scale(subCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
            // 方向キーの入力値とカメラの向きから、移動方向を決定
            moveForward = cameraForward * moveZ + subCamera.transform.right * moveX;
        }

        // 移動方向にスピードを掛ける。ジャンプや落下がある場合は、別途Y軸方向の速度ベクトルを足す。
        rb.velocity = moveForward * speed + new Vector3(0, rb.velocity.y, 0);

        // ジャンプ中は移動速度を一定に保つ
        if (isJump)
        {
            // ジャンプ中は横方向の速度をspeedで統一し、Y軸の速度（ジャンプ）はそのまま保持
            rb.velocity = new Vector3(moveForward.x * speed, rb.velocity.y, moveForward.z * speed);
        }
        else
        {
            // 地面にいる場合、通常の移動
            rb.velocity = new Vector3(moveForward.x * speed, rb.velocity.y, moveForward.z * speed);
        }

        // キャラクターの向きを進行方向に
        if (moveForward != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveForward);
        }

        //歩くアニメーション
        anim.SetBool("walk", moveForward.magnitude > 0f && speed <= walkspeed);

        // 歩行音・走行音の制御
        HandleFootstepSound();
    }

    //ポーズ画面
    private void PauseMenue()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isPause == false)
            {
                isMove = false;
                isPause = true;
                Pause.SetActive(true);
                bgmSource.Pause();
            }
            else
            {
                Resume();
            }
        }
    }

    //階段を一段ずつ表示
    private void StairsEvent()
    {
        if (cntStair < 9)
        {
            if (stairFlg == true)
            {
                if (ChildStairs[cntStair].activeSelf == false)
                {
                    ChildStairs[cntStair].SetActive(true);
                    StartCoroutine(wait());
                }
            }
        }
    }

    //プレイヤーのモデルを表示する
    private void SetCharacterVisibility(bool isVisible)
    {
        foreach (var skinnedMeshRenderer in skinMeshRen)
        {
            skinnedMeshRenderer.enabled = isVisible;
        }
    }

    //足音の再生
    private void HandleFootstepSound()
    {
        bool isWalking = moveForward.magnitude > 0f && speed == walkspeed;
        bool isRunning = moveForward.magnitude > 0f && speed == runSpeed;

        //歩行音
        if (isWalking)
        {
            if (footstepSource.clip != walkSound || !footstepSource.isPlaying)
            {
                footstepSource.clip = walkSound;
                footstepSource.loop = true;
                footstepSource.Play();
            }
        }
        //走行音
        else if (isRunning)
        {
            if (footstepSource.clip != runSound || !footstepSource.isPlaying)
            {
                footstepSource.clip = runSound;
                footstepSource.loop = true;
                footstepSource.Play();
            }
        }
        else
        {
            if (footstepSource.isPlaying)
            {
                footstepSource.Stop();
            }
        }
    }

}
