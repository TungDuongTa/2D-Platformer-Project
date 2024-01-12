using UnityEngine;


public class Parallax : MonoBehaviour
{
    public Transform Cam;
    Vector3 CamStartPosition;
    public float distance;

    public GameObject[] BackGround;
    public Material[] mat;
    public float[] backSpeed;
    public float farthestBack;

    [Range(0.01f, 0.05f)]
    public float parallaxSpeed;
    // Start is called before the first frame update
    void Start()
    {
        Cam = Camera.main.transform;
        CamStartPosition = Cam.position;
        int backCount = transform.childCount;
        mat = new Material[backCount];
        backSpeed = new float[backCount];
        BackGround = new GameObject[backCount];

        for (int i = 0; i < backCount; i++)
        {
            BackGround[i] = transform.GetChild(i).gameObject;
            mat[i] = BackGround[i].GetComponent<Renderer>().material;
        }
        BackSpeedCalculate(backCount);
    }

    void BackSpeedCalculate(int backCount)
    {
        for (int i = 0; i < backCount; i++) // find the farhthest background
        {
            if ((BackGround[i].transform.position.z - Cam.position.z) > farthestBack)
            {

                farthestBack = BackGround[i].transform.position.z - Cam.position.z;
            }
        }
        for (int i = 0; i < backCount; i++)
        {
            // set the speed of backgrounds
            backSpeed[i] = 1 - (BackGround[i].transform.position.z - Cam.position.z + 0.1f) / farthestBack;
        }

    }
    private void LateUpdate()
    {
        distance = Cam.position.x - CamStartPosition.x;
        transform.position = new Vector3(Cam.position.x-10, 0, 0);
        for (int i = 0; i < BackGround.Length; i++)
        {
            float speed = backSpeed[i] * parallaxSpeed;
            mat[i].SetTextureOffset("_MainTex", new Vector2(distance, 0) * speed);
        }
    }
}
