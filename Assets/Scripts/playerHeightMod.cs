using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class playerHeightMod : MonoBehaviour {

    Transform player;
    float threshold = 2;
    public float jumpRange = 100;
    float score = -1;
    public Text scoreText;
	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player").transform;
	}
	
	// Update is called once per frame
	void Update () {
        Vector2 playerPos = new Vector2(player.position.x, player.position.z);
        Vector2 thisPos = new Vector2(transform.position.x, transform.position.z);
        float distance = (playerPos - thisPos).magnitude;
        if(distance < threshold) {
            this.transform.position = Random.insideUnitSphere * jumpRange;
            thisPos = new Vector2(transform.position.x, transform.position.z);
            distance = (playerPos - thisPos).magnitude;
            scoreText.text = (++score).ToString();
            int temp = TerrainGenerator.thi.waveColorRedBand;
            TerrainGenerator.thi.waveColorRedBand = TerrainGenerator.thi.waveColorGreenBand;
            TerrainGenerator.thi.waveColorGreenBand = TerrainGenerator.thi.waveColorBlueBand;
            TerrainGenerator.thi.waveColorBlueBand = temp;
        }
        this.transform.position = new Vector3(thisPos.x, player.position.y + distance / 3, thisPos.y);
        distance /= 5;
        this.transform.localScale = new Vector3(distance, distance, distance);
	}
}
