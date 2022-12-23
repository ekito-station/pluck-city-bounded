using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class BallController : MonoBehaviourPunCallbacks
{
    private GameObject aRCamera;
    private GameObject preFixedBall;
    private Vector3 prePos;
    private Vector3 curPos;
    public float stringRadius = 0.5f;
    public float standardLength = 3.0f;
    public float[] pitches = { 1.78f, 1.60f, 1.50f, 1.33f, 1.20f, 1.00f, 0.80f, 0.75f, 0.67f, 0.60f, 0.0f };
    private int collisionCount;
    public int collisionLimit = 8;

    // Start is called before the first frame update
    void Start()
    {
        aRCamera = Camera.main.gameObject;
        // ボールが発射された時のユーザーの位置を取得
        prePos = aRCamera.transform.position - aRCamera.transform.forward * 1.0f;
        // ボールが発射された地点にもFixedBallを生成
        preFixedBall = PhotonNetwork.Instantiate("FixedBall", prePos, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("OnCollisionEnter");
        GameObject otherObj = other.gameObject;

        if (otherObj.tag == "Building")         // 建物に衝突した場合
        {
            Debug.Log("Collided with building.");
            // 衝突地点でFixedBallを生成
            curPos = this.transform.position;
            GameObject fixedBall = PhotonNetwork.Instantiate("FixedBall", curPos, Quaternion.identity);
            // 弦を配置
            PlaceString(prePos, curPos, preFixedBall, fixedBall);
            // preFixedBallを現在のFixedBallに更新
            preFixedBall = fixedBall;
        }
        else if (otherObj.tag == "FixedBall")   // FixedBallに衝突した場合
        {
            PhotonView otherPhotonView = otherObj.GetComponent<PhotonView>();
            if (!otherPhotonView.IsMine)
            {
                Debug.Log("Collided with fixedBall.");
                curPos = otherObj.transform.position;
                // 弦を配置
                PlaceString(prePos, curPos, preFixedBall, otherObj);
                // preFixedBallを衝突したFixedBallに更新
                preFixedBall = otherObj;
            }
        }

        // prePosを衝突地点に更新
        prePos = curPos;

        // 衝突回数が上限に達したらボールを消す
        collisionCount++;
        if (collisionCount > collisionLimit)
        {
            Destroy(this.gameObject);
        }
    }

    private void PlaceString(Vector3 _prePos, Vector3 _curPos, GameObject _preFixedBall, GameObject _fixedBall)
    {
        Debug.Log("PlaceString.");
        // 弦を配置
        Vector3 strVec = _curPos - _prePos;             // 弦の方向を取得
        float dist = strVec.magnitude;                  // 弦の長さを取得
        Vector3 strY = new Vector3(0f, dist, 0f);
        Vector3 halfStrVec = strVec * 0.5f;
        Vector3 centerCoord = _prePos + halfStrVec;     // 中点の座標        

        GameObject str = PhotonNetwork.Instantiate("MyString", centerCoord, Quaternion.identity);
        str.transform.localScale = new Vector3(stringRadius, dist / 2, stringRadius);   // ひとまずY軸方向に伸ばす
        str.transform.rotation = Quaternion.FromToRotation(strY, strVec);               // 弦を回転

        Debug.Log("Placed string.");

        // distの値に応じて、弦から鳴る音の高さを決定
        StringController stringController = str.GetComponent<StringController>();
        float ratio = dist / standardLength;
        for (int i = 0; i < pitches.Length; i++)
        {
            if (ratio > pitches[i])
            {
                stringController.soundIndex = i;
                break;
            }
        }

        // 弦にFixedBallを登録
        stringController.endBall = _fixedBall;

        // PreFixedBallに弦を登録
        FixedBallController preFixedBallController = _preFixedBall.GetComponent<FixedBallController>();
        preFixedBallController.strings.Add(str);
    }
}
