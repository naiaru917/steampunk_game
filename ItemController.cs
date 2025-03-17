using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    public GameObject particle;
    public GameObject childObject;
    public Vector3 ItemPos;

    // Start is called before the first frame update
    void Start()
    {
        ItemPos = transform.position;

        //パーティクルをアイテムの子オブジェクトとして生成
        childObject = Instantiate(particle, this.transform);
    }

    // Update is called once per frame
    void Update()
    {
        //アイテムの回転
        transform.Rotate(0.5f, 0.2f, 0.1f);
        
        //パーティクルの移動と回転を抑える処理
        childObject.transform.position = new Vector3(ItemPos.x, ItemPos.y + 3.5f, ItemPos.z);
        childObject.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
    }
}
