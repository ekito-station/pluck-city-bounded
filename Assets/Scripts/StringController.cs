using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringController : MonoBehaviour
{
    public AudioClip[] soundGroup;
    public int soundIndex;
    public GameObject endBall;
    private AudioSource audioSource;
    public float untilNext = 2.0f;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "MainCamera")
        {
            PlaySound();
        }
    }

    public void PlaySound()
    {
        Debug.Log("PlaySound");
        audioSource.PlayOneShot(soundGroup[soundIndex]);
        // audioSource.clip = soundGroup[soundIndex];
        // audioSource.Play();

        // 隣の弦を鳴らす
        // PlayNextSound();
        Invoke("PlayNextSound", untilNext);
    }

    public void PlayNextSound()
    {
        Debug.Log("PlayNextSound");
        FixedBallController endballController = endBall.GetComponent<FixedBallController>();
        List<GameObject> stringList = endballController.strings;
        Debug.Log("stringList");
        Debug.Log(stringList);
        Debug.Log(stringList.Count);
        if (stringList?.Count > 0)
        {
            foreach (GameObject str in stringList)
            {
                Debug.Log(str);
                StringController nextStringController = str.GetComponent<StringController>();
                nextStringController.PlaySound();
            }
        }
    }
}
