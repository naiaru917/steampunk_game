using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class FPS_Controller : MonoBehaviour
{
    public GameObject player;
    Vector3 currentPos;//現在のカメラ位置
    Vector3 pastPos;//過去のカメラ位置

    Vector3 diff;//移動距離


    // Start is called before the first frame update
    void Start()
    {
        //最初のプレイヤーの位置の取得
        pastPos = player.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(PlayerController.isMove == true)
        {
            if (PlayerController.CameraM == true)
            {
                //------カメラの移動------

                //プレイヤーの現在地の取得
                currentPos = player.transform.position;

                diff = currentPos - pastPos;

                transform.position = Vector3.Lerp(transform.position, transform.position + diff, 1.0f);//カメラをプレイヤーの移動差分だけうごかすよ

                pastPos = currentPos;
            }
            else
            {
                transform.position = player.transform.position + new Vector3(0, 1, 0);
            }



            //------カメラの回転------

            // マウスの移動量を取得
            float mx = Input.GetAxis("Mouse X") * 4f;
            float my = Input.GetAxis("Mouse Y") * 2f;

            // X方向に一定量移動していれば横回転
            if (Mathf.Abs(mx) > 0.01f)
            {
                // 回転軸はワールド座標のY軸
                transform.RotateAround(player.transform.position, Vector3.up, mx);
            }

            // Y方向に一定量移動していれば縦回転
            if (Mathf.Abs(my) > 0.01f)
            {
                // 回転軸はカメラ自身のX軸
                transform.RotateAround(player.transform.position, transform.right, -my);
            }
        }
        
    }
    
}
