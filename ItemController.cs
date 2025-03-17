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

        //�p�[�e�B�N�����A�C�e���̎q�I�u�W�F�N�g�Ƃ��Đ���
        childObject = Instantiate(particle, this.transform);
    }

    // Update is called once per frame
    void Update()
    {
        //�A�C�e���̉�]
        transform.Rotate(0.5f, 0.2f, 0.1f);
        
        //�p�[�e�B�N���̈ړ��Ɖ�]��}���鏈��
        childObject.transform.position = new Vector3(ItemPos.x, ItemPos.y + 3.5f, ItemPos.z);
        childObject.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
    }
}
