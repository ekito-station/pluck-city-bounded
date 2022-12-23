using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Google.XR.ARCoreExtensions;

using Photon.Pun;
using Photon.Realtime;

public class HomeManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private ARSessionOrigin sessionOrigin;
    [SerializeField] private AREarthManager earthManager;
    [SerializeField] private ARAnchorManager anchorManager;
    public GameObject plateauPrefab;
    public GameObject ballPrefab;
    public GameObject origin;
    public TextMeshProUGUI infoText;
    public double headingThreshold = 25;
    public double horizontalThreshold = 20;
    public float ballSpeed = 300;
    private GameObject plateauObj;
    private GameObject aRCamera;

    // Start is called before the first frame update
    void Start()
    {
        infoText.text = "Let's play!";
        aRCamera = Camera.main.gameObject;

        // Photonのマスターサーバーへ接続
        PhotonNetwork.ConnectUsingSettings();
    }

    // Update is called once per frame
    void Update()
    {
        // トラッキングが出来ていない場合は何もしない
        if (earthManager.EarthTrackingState != TrackingState.Tracking)
        {
            infoText.text = "Not tracked.";
            return;
        }
        // トラッキング結果を取得
        GeospatialPose pose = earthManager.CameraGeospatialPose;
        // トラッキング精度がthresholdより悪い（値が大きい）場合
        if (pose.HeadingAccuracy > headingThreshold || pose.HorizontalAccuracy > horizontalThreshold)
        {
            infoText.text = "Please look around you.";
            // Debug.Log("Low accuracy.");
        }
        else
        {
            infoText.text = "";
            // Debug.Log("High accuracy.");
            // 初めてトラッキング精度が良くなったタイミングでオブジェクト生成
            if (plateauObj == null)
            {
                Debug.Log("Running PlacePlateau.");
                PlacePlateau();
            }
        }
    }

    // Photonのマスターサーバーへ接続できた時呼ばれるコールバック
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions(), TypedLobby.Default);
    }

    public void PlacePlateau()
    {
        if (!anchorManager)
        {
            Debug.Log("anchorManager is null.");
            return;
        }
        // 東京都庁第一本庁舎をアンカーとする
        var anchor = anchorManager.AddAnchor(
            35.689614031723785,     // アンカーの緯度
            139.69351102588638,     // アンカーの経度
            36.9982 - 1.5,          // アンカーのジオイド高 - スマートフォンの高さ(地面から1.5mと想定) 
            Quaternion.identity
        );

        // // 安田講堂をアンカーとする
        // var anchor = anchorManager.AddAnchor(
        //     35.71343739484052,     // アンカーの緯度
        //     139.76221580993843,     // アンカーの経度
        //     36.9157 - 1.5,          // アンカーのジオイド高 - スマートフォンの高さ(地面から1.5mと想定) 
        //     Quaternion.identity
        // );

        // AR Session Originをアンカーの位置に移動
        sessionOrigin.MakeContentAppearAt(origin.transform, anchor.transform.position, anchor.transform.rotation);

        // Plateauを配置
        plateauObj = Instantiate(plateauPrefab, anchor.transform);
        // infoText.text = "Loaded 3D building models.";
        Debug.Log("Loaded 3D building models.");
    }

    public void OnBallButtonClicked()
    {
        Debug.Log("BallButton clicked.");
        GameObject ballObj = Instantiate(ballPrefab, aRCamera.transform.position, Quaternion.identity);
        Rigidbody ballRigidBody = ballObj.GetComponent<Rigidbody>();
        ballRigidBody.AddForce(aRCamera.transform.forward * ballSpeed);
    }
}
