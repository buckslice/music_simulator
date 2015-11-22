using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Chunk {
    public int coordX;
    public int coordY;
    public Mesh mesh;
    public GameObject obj;
}

public class TerrainGenerator : MonoBehaviour {

    public List<Chunk> chunks = new List<Chunk>();
    public Material mat;
    private Transform player;

    public const int CHUNK_RADIUS = 2;
    public const int SIZE = 32;
    public const int HSIZE = SIZE / 2;
    public const float TRI_SIZE = 2f;
    private int playerX = 0;
    private int playerY = 0;
    private float seed;
    Texture2D colorWave;
    Texture2D heightWave;
    public int waveGradientLength = 100;
    public int heightGradientLength = 100;

    public int waveColorRedBand = 1;
    public int waveColorGreenBand = 1;
    public int waveColorBlueBand = 1;
    public int waveHeightBand = 0;
    public int abberationBand = 2;

    public float maxDistortAmplitude;
    public int distortionBand = 0;

    public float[] bandMaxes = new float[2];

    public static TerrainGenerator thi;

    private UnityStandardAssets.ImageEffects.VignetteAndChromaticAberration chromaticAbberation;

    public void resetMaxes() {
        for (int i = 0; i < 2; i++) {
            bandMaxes[i] = 0;
        }
    }
    // Use this for initialization
    void Start() {
        thi = this;
        player = GameObject.Find("PlayerTest").transform;
        seed = Random.Range(1000f, 100000f);
        colorWave = new Texture2D(waveGradientLength, 1);
        heightWave = new Texture2D(heightGradientLength, 1);
        chromaticAbberation = Camera.main.GetComponent<UnityStandardAssets.ImageEffects.VignetteAndChromaticAberration>();
    }

    void FixedUpdate() {
        for(int i = 0; i < 3; i++) {
            bandMaxes[i] = Mathf.Max(0.999f * bandMaxes[i], FFT.thi.band[i]);
        }


        for(int i = waveGradientLength - 1; i >= 1; i--) {
            colorWave.SetPixel(i, 0, colorWave.GetPixel(i - 1, 0));
        }
        float r = FFT.thi.band[waveColorRedBand];
        float g = FFT.thi.band[waveColorGreenBand];
        float b = FFT.thi.band[waveColorBlueBand];
        Color waveColor = new Color(r / bandMaxes[waveColorRedBand], g / bandMaxes[waveColorGreenBand], b / bandMaxes[waveColorBlueBand]);
        colorWave.SetPixel(0, 0, waveColor);
        Camera.main.backgroundColor = new HSBColor((Time.realtimeSinceStartup / 100) % 1, 1, 0.125f).ToColor();
        //colorWave.SetPixel(0, 0, new Color(r, g, b));
        colorWave.Apply(); //apply changes
        mat.SetTexture(Shader.PropertyToID("_ColorGradient"), colorWave);

        for (int i = heightGradientLength - 1; i >= 1; i--) {
            heightWave.SetPixel(i, 0, heightWave.GetPixel(i - 1, 0));
        }
        heightWave.SetPixel(0, 0, new Color(FFT.thi.band[waveHeightBand], FFT.thi.band[waveHeightBand], FFT.thi.band[waveHeightBand]));
        heightWave.Apply(); //apply changes
        mat.SetTexture(Shader.PropertyToID("_HeightGradient"), heightWave);

        mat.SetFloat(Shader.PropertyToID("_NoiseAmp"), maxDistortAmplitude * FFT.thi.band[distortionBand]);
    }

    // Update is called once per frame
    void Update() {
        Vector3 p = player.position;
        mat.SetVector(Shader.PropertyToID("_PlayerPos"), new Vector4(p.x, p.y, p.z, 1.0f));
        float hx = HSIZE * TRI_SIZE;
        if (player.position.x > 0) {
            hx *= -1f;
        }
        float hy = HSIZE * TRI_SIZE;
        if (player.position.z > 0) {
            hy *= -1f;
        }

        int px = (int)(player.position.x - hx) / (int)(SIZE * TRI_SIZE);
        int py = (int)(player.position.z - hy) / (int)(SIZE * TRI_SIZE);
        //int px = (int)(player.position.x - HSIZE * TRI_SIZE) / (int)(SIZE * TRI_SIZE);
        //int py = (int)(player.position.z - HSIZE * TRI_SIZE) / (int)(SIZE * TRI_SIZE);
        playerX = px;
        playerY = py;
        //Debug.Log(px + " " + py);

        chromaticAbberation.chromaticAberration = -50f * FFT.thi.band[abberationBand];

        checkMeshes();
    }


    void checkMeshes() {
        // check for chunks within player radius
        for (int x = -CHUNK_RADIUS; x <= CHUNK_RADIUS; x++) {
            for (int y = -CHUNK_RADIUS; y <= CHUNK_RADIUS; y++) {
                bool found = false;
                for (int i = 0; i < chunks.Count; i++) {
                    if (chunks[i].coordX == playerX + x && chunks[i].coordY == playerY + y) {
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    generateMesh(x + playerX, y + playerY);
                }
            }
        }

        // check for chunks that are no longer nearby player
        for (int i = chunks.Count - 1; i >= 0; i--) {
            bool shouldDelete = true;
            for (int x = -CHUNK_RADIUS; x <= CHUNK_RADIUS; x++) {
                for (int y = -CHUNK_RADIUS; y <= CHUNK_RADIUS; y++) {
                    // if the chunk is valid then continue
                    if (chunks[i].coordX == playerX + x && chunks[i].coordY == playerY + y) {
                        shouldDelete = false;
                        break;
                    }
                }
            }

            if (shouldDelete) {
                // delete chunk mesh and gameobject
                Chunk c = chunks[i];
                chunks.RemoveAt(i);
                Destroy(c.obj.GetComponent<MeshFilter>().mesh);
                Destroy(c.obj);
            }
        }
    }

    void generateMesh(int chunkX, int chunkY) {
        List<Vector3> verts = new List<Vector3>();
        List<Color32> cols = new List<Color32>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector2> uvs2 = new List<Vector2>();
        List<int> tris = new List<int>();

        int t = 0;
        for (int x = 0; x < SIZE; x++) {
            for (int y = 0; y < SIZE; y++) {

                float x1 = x + chunkX * SIZE - HSIZE;
                float y1 = y + chunkY * SIZE - HSIZE;
                float x2 = (x + 1) + chunkX * SIZE - HSIZE;
                float y2 = (y + 1) + chunkY * SIZE - HSIZE;
                x1 *= TRI_SIZE;
                y1 *= TRI_SIZE;
                x2 *= TRI_SIZE;
                y2 *= TRI_SIZE;

                float scale = 10f;
                float mod = 1.0f / 20.0f;
                Vector3 v1 = new Vector3(x1, scale * Mathf.PerlinNoise(x1 * mod + seed, y1 * mod + seed) - scale / 2.0f, y1);
                Vector3 v2 = new Vector3(x1, scale * Mathf.PerlinNoise(x1 * mod + seed, y2 * mod + seed) - scale / 2.0f, y2);
                Vector3 v3 = new Vector3(x2, scale * Mathf.PerlinNoise(x2 * mod + seed, y2 * mod + seed) - scale / 2.0f, y2);
                Vector3 v4 = new Vector3(x2, scale * Mathf.PerlinNoise(x2 * mod + seed, y1 * mod + seed) - scale / 2.0f, y1);

                verts.Add(v1);
                verts.Add(v2);
                verts.Add(v3);
                verts.Add(v3);
                verts.Add(v4);
                verts.Add(v1);

                // main texture uvs
                uvs.Add(new Vector2(0.0f, 0.0f));
                uvs.Add(new Vector2(0.0f, 1.0f));
                uvs.Add(new Vector2(1.0f, 1.0f));
                uvs.Add(new Vector2(1.0f, 1.0f));
                uvs.Add(new Vector2(1.0f, 0.0f));
                uvs.Add(new Vector2(0.0f, 0.0f));

                // noise uvs
                uvs2.Add(new Vector2((float)x / SIZE, (float)y / SIZE));
                uvs2.Add(new Vector2((float)x / SIZE, (float)(y + 1) / SIZE));
                uvs2.Add(new Vector2((float)(x + 1) / SIZE, (float)(y + 1) / SIZE));
                uvs2.Add(new Vector2((float)(x + 1) / SIZE, (float)(y + 1) / SIZE));
                uvs2.Add(new Vector2((float)(x + 1) / SIZE, (float)y / SIZE));
                uvs2.Add(new Vector2((float)x / SIZE, (float)y / SIZE));

                Color col = (chunkX + chunkY) % 2 == 0 ? Color.red : Color.blue;
                col = new HSBColor(Random.value, 1.0f, 1.0f).ToColor();

                for (int i = 0; i < 6; i++) {
                    tris.Add(t++);
                    cols.Add(col);
                }
            }
        }

        GameObject go = new GameObject("Chunk: " + chunkX + " " + chunkY);
#if UNITY_EDITOR
        go.transform.SetParent(transform, true);
#endif
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        MeshCollider mc = go.AddComponent<MeshCollider>();
        mr.material = mat;
        MeshFilter mf = go.AddComponent<MeshFilter>();

        Mesh m = new Mesh();
        m.vertices = verts.ToArray();
        m.colors32 = cols.ToArray();
        m.uv = uvs.ToArray();
        m.uv2 = uvs2.ToArray();
        m.triangles = tris.ToArray();
        m.RecalculateBounds();
        Bounds newb = new Bounds(m.bounds.center, m.bounds.size + Vector3.up * 100.0f);
        m.bounds = newb;
        m.RecalculateNormals();
        mf.mesh = m;
        mc.sharedMesh = m;
        Chunk c = new Chunk();
        c.obj = go;
        c.mesh = m;
        c.coordX = chunkX;
        c.coordY = chunkY;
        chunks.Add(c);
        // add to mesh list
    }
}
