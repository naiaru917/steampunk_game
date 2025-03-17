using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class FPS_Controller : MonoBehaviour
{
    public GameObject player;
    Vector3 currentPos;//���݂̃J�����ʒu
    Vector3 pastPos;//�ߋ��̃J�����ʒu

    Vector3 diff;//�ړ�����


    // Start is called before the first frame update
    void Start()
    {
        //�ŏ��̃v���C���[�̈ʒu�̎擾
        pastPos = player.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(PlayerController.isMove == true)
        {
            if (PlayerController.CameraM == true)
            {
                //------�J�����̈ړ�------

                //�v���C���[�̌��ݒn�̎擾
                currentPos = player.transform.position;

                diff = currentPos - pastPos;

                transform.position = Vector3.Lerp(transform.position, transform.position + diff, 1.0f);//�J�������v���C���[�̈ړ�������������������

                pastPos = currentPos;
            }
            else
            {
                transform.position = player.transform.position + new Vector3(0, 1, 0);
            }



            //------�J�����̉�]------

            // �}�E�X�̈ړ��ʂ��擾
            float mx = Input.GetAxis("Mouse X") * 4f;
            float my = Input.GetAxis("Mouse Y") * 2f;

            // X�����Ɉ��ʈړ����Ă���Ή���]
            if (Mathf.Abs(mx) > 0.01f)
            {
                // ��]���̓��[���h���W��Y��
                transform.RotateAround(player.transform.position, Vector3.up, mx);
            }

            // Y�����Ɉ��ʈړ����Ă���Ώc��]
            if (Mathf.Abs(my) > 0.01f)
            {
                // ��]���̓J�������g��X��
                transform.RotateAround(player.transform.position, transform.right, -my);
            }
        }
        
    }
    
}
