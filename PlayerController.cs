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

    //�J����
    public GameObject mainCamera;  //�O�l�̎��_�̃J�����i���C���J�����j
    public GameObject subCamera;   //��l�̎��_�̃J����
    public Vector3 cameraForward;  //�J�������猩������
    public Vector3 moveForward;    //���ʕ����ւ̈ړ���
    public static bool CameraM;     //�J���������C���J�����i�O�l�̎��_�j�ł��邩�̔���

    //�ړ��E�ʒu���
    public float speed;              //���݂̈ړ����x
    public float walkspeed = 3.0f;   //�������̈ړ����x
    public float runSpeed = 5.0f;    //���鎞�̈ړ����x
    public float moveX;              // X�����̈ړ�����
    public float moveZ;              // Z�����̈ړ�����
    public static bool isMove;       //�ړ����\���̔���
    Vector3 playerPos;               //�v���C���[�̈ʒu���

    //�W�����v
    public float jumpForce = 5.0f;  //�W�����v��
    bool isJump, isJumpWait;        //�W�����v���s���邩�̔���
    float JumpWaitTimer;

    //�X�R�A
    public Text scoreTxt;
    public int score;

    //�^�C�}�[
    public float timer;
    public Text TimeTxt;
    public Text clearTimeTxt;

    //�C�x���g����
    public Text clearTxt;
    public Text MissionTxt;
    public GameObject Event;
    public GameObject Stairs;
    private GameObject[] ChildStairs;

    //�|�[�Y���
    public static bool isPause;     //�|�[�Y��ʂł��邩�̔���
    public GameObject Pause;
    public static bool isClear;

    //���ʉ�
    public AudioClip walkSound;  // ���s���̉�
    public AudioClip runSound;   // ���s���̉�
    public AudioClip getSound;   // �A�C�e���l����
    public AudioClip fallSound;  // ������
    public AudioClip eventSound; // �C�x���g�̉�
    public AudioClip bgmClip;    // BGM

    private AudioSource footstepSource;  // �����p AudioSource
    private AudioSource bgmSource;       // BGM�p AudioSource
    private AudioSource sfxSource;       // ���ʉ��p AudioSource



    //�K�i
    public int cntStair;    //�\������Ă���K�i�̌��𐔂���
    public bool stairFlg;   //�K�i��\���o����󋵂ł��邩�̔���

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        skinMeshRen = GetComponentsInChildren<SkinnedMeshRenderer>();

        speed = walkspeed;      //�ŏ��͈ړ��X�s�[�h���uwalkSpeed�v��
        score = 3;
        timer = 0;

        // �����p AudioSource
        footstepSource = gameObject.AddComponent<AudioSource>();
        footstepSource.loop = true;  // �����̓��[�v�Đ�
        footstepSource.volume = 0.03f; // ���ʒ���

        // BGM�p AudioSource
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.clip = bgmClip;
        bgmSource.loop = true;   // BGM�̓��[�v�Đ�
        bgmSource.volume = 0.007f; // ���ʒ���
        bgmSource.Play();        // �Q�[���J�n����BGM�Đ�

        // ���ʉ��p AudioSourse
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.volume = 0.03f; // ���ʒ���


        scoreTxt.text = "�c�莕�ԁ~" + score;
        MissionTxt.text = "<MISSION> ���Ԃ��W�߂悤";
        TimeTxt.text = "00:00";

        clearTxt.enabled = false;       //�uCLEAR�v�̃e�L�X�g���\����
        clearTimeTxt.enabled = false;   //�N���A���Ԃ̃e�L�X�g���\����
        Event.SetActive(false);
        subCamera.SetActive(false);     //�T�u�J�����i��l�̎��_�j���\����
        CameraM = true;
        isMove = true;
        isPause = false;
        Pause.SetActive(false);
        isClear = false;

        ChildStairs = new GameObject[Stairs.transform.childCount];

        //�K�i�̎q�I�u�W�F�N�g���擾���z��Ɋi�[
        for (int i = 0; i < Stairs.transform.childCount; i++)
        {
            ChildStairs[i] = Stairs.transform.GetChild(i).gameObject;
        }

        cntStair = 0;
        stairFlg = false;

        // Rigidbody�̉�]���Œ�
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
            //WASD�Ɩ��L�[�ňړ����������
            moveX = Input.GetAxis("Horizontal");
            moveZ = Input.GetAxis("Vertical");

            //�^�C�}�[�̊Ǘ�
            timer += Time.deltaTime;
            var span = new TimeSpan(0, 0, (int)timer);
            TimeTxt.text = span.ToString(@"mm\:ss");

            //�_�b�V��
            if (Input.GetKey(KeyCode.LeftShift) && !isJump)
            {
                speed = runSpeed;           //�ړ����x���urunSpeed�v��
                anim.SetBool("run", true);  //�A�j���[�V�����uRun�v���Đ�
            }
            else
            {
                speed = walkspeed;           //�ړ����x���uwalkSpeed�v��
                anim.SetBool("run", false);  //�A�j���[�V�����uRun�v������
            }

            //�W�����v
            Jump();

            //�J�����̐؂�ւ�
            CameraSwitch();

            //���Ԃ��W�ߏI��������̏���
            if (score <= 0)
            {
                MissionTxt.text = "<MISSION> ��֌��������I";
                Event.SetActive(true);
                scoreTxt.enabled = false;
            }

        }

        //�|�[�Y���
        PauseMenue();

        //�K�i������\��
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
        isJump = false;     //�v���C���[���n�ʂɐڂ��Ă��邩�̔���
    }

    private void OnTriggerEnter(Collider other)
    {
        //�A�C�e���ɐG�ꂽ��
        string name = other.gameObject.name;
        if (name.Contains("Item"))
        {
            Destroy(other.gameObject);      //�G�ꂽ�A�C�e������ʂ���폜����
            score -= 1;
            scoreTxt.text = "�c�莕�ԁ~" + score;
            sfxSource.PlayOneShot(getSound);    //���ʉ����Đ�
        }

        //�C�x���g�|�C���g�ɓ��������Ƃ�
        string eventName = other.gameObject.name;
        if (name.Contains("EventPoint"))
        {
            stairFlg = true;
            sfxSource.PlayOneShot(eventSound);    //���ʉ����Đ�

        }

        //�N���A�|�C���g�ɓ��������Ƃ�
        string ClearName = other.gameObject.name;
        if (name.Contains("ClearPoint"))
        {
            GameClear();
            sfxSource.PlayOneShot(eventSound);    //���ʉ����Đ�
            speed = 0;
            rb.velocity = Vector3.zero;  // �����I�ɓ����Ȃ�����
            anim.SetBool("walk", false);
            anim.SetBool("run", false);
            footstepSource.Stop();   //���s�����I��

        }

        //���������Ƃ�
        string DeadName = other.gameObject.name;
        if (name.Contains("DeadLine"))
        {
            transform.position = new Vector3(0,5,-160);
            sfxSource.PlayOneShot(fallSound);    //���ʉ����Đ�
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

        //�N���A��ʂ�\��
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
        yield return new WaitForSeconds(0.5f);      //0.5�b�҂�
        cntStair += 1;
     }


    //�J�����̐؂�ւ�
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

    //�W�����v
    private void Jump()
    {
        if (Input.GetKeyUp("space"))
        {
            if (!isJump && !isJumpWait)  //isJump��isJumpWait�̗�����false�̂Ƃ�
            {
                anim.Play("Jump", 0, 0);    //�A�j���[�V�����uJump�v���Đ�
                isJumpWait = true;
                JumpWaitTimer = 0.2f;
            }
        }
        //�����o���Ă���W�����v
        if (isJumpWait)
        {
            JumpWaitTimer -= Time.deltaTime;
            if (JumpWaitTimer < 0)
            {
                GetComponent<Rigidbody>().velocity = transform.up * jumpForce;    //��̕����ɗ͂�������
                isJumpWait = false;
                isJump = true;
            }
        }
    }


    //�v���C���[�̈ړ�
    private void MovePlayer()
    {
        if (CameraM == true)
        {
            // ���C���J�����̕�������AX-Z���ʂ̒P�ʃx�N�g�����擾
            cameraForward = Vector3.Scale(mainCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
            // �����L�[�̓��͒l�ƃJ�����̌�������A�ړ�����������
            moveForward = cameraForward * moveZ + mainCamera.transform.right * moveX;
        }
        else
        {
            // �T�u�J�����̕�������AX-Z���ʂ̒P�ʃx�N�g�����擾
            cameraForward = Vector3.Scale(subCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
            // �����L�[�̓��͒l�ƃJ�����̌�������A�ړ�����������
            moveForward = cameraForward * moveZ + subCamera.transform.right * moveX;
        }

        // �ړ������ɃX�s�[�h���|����B�W�����v�◎��������ꍇ�́A�ʓrY�������̑��x�x�N�g���𑫂��B
        rb.velocity = moveForward * speed + new Vector3(0, rb.velocity.y, 0);

        // �W�����v���͈ړ����x�����ɕۂ�
        if (isJump)
        {
            // �W�����v���͉������̑��x��speed�œ��ꂵ�AY���̑��x�i�W�����v�j�͂��̂܂ܕێ�
            rb.velocity = new Vector3(moveForward.x * speed, rb.velocity.y, moveForward.z * speed);
        }
        else
        {
            // �n�ʂɂ���ꍇ�A�ʏ�̈ړ�
            rb.velocity = new Vector3(moveForward.x * speed, rb.velocity.y, moveForward.z * speed);
        }

        // �L�����N�^�[�̌�����i�s������
        if (moveForward != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveForward);
        }

        //�����A�j���[�V����
        anim.SetBool("walk", moveForward.magnitude > 0f && speed <= walkspeed);

        // ���s���E���s���̐���
        HandleFootstepSound();
    }

    //�|�[�Y���
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

    //�K�i����i���\��
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

    //�v���C���[�̃��f����\������
    private void SetCharacterVisibility(bool isVisible)
    {
        foreach (var skinnedMeshRenderer in skinMeshRen)
        {
            skinnedMeshRenderer.enabled = isVisible;
        }
    }

    //�����̍Đ�
    private void HandleFootstepSound()
    {
        bool isWalking = moveForward.magnitude > 0f && speed == walkspeed;
        bool isRunning = moveForward.magnitude > 0f && speed == runSpeed;

        //���s��
        if (isWalking)
        {
            if (footstepSource.clip != walkSound || !footstepSource.isPlaying)
            {
                footstepSource.clip = walkSound;
                footstepSource.loop = true;
                footstepSource.Play();
            }
        }
        //���s��
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
