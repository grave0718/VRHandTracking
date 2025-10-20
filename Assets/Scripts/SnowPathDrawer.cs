using UnityEngine;

  

public class SnowPathDrawer : MonoBehaviour

{

    public ComputeShader snowComputeShader;

    public RenderTexture snowRT;

  

    private string snowImageProperty = "snowImage";

    private string colorValueProperty = "colorValueToAdd";

    private string resolutionProperty = "resolution";

    private string positionXProperty = "positionX";

    private string positionYProperty = "positionY";

    private string spotSizeProperty = "spotSize";

  

    private string drawSpotKernel = "DrawSpot";

  

    private Vector2Int position = new Vector2Int(256, 256);

    public float spotSize = 5f;

    public float maxSnowHeight = 0.3f; // 눈의 최대 높이

    public float minVelocity = 0.01f; // 이 속도 이상으로 움직여야 지움

  

    private SnowController snowController;

    private GameObject[] snowControllerObjs;

    private Vector3 lastPosition;

  

    private void Awake()

    {

        snowControllerObjs = GameObject.FindGameObjectsWithTag("SnowGround");

        lastPosition = transform.position;

    }

  

    private void FixedUpdate()

    {

        // 손 움직임 계산

        float velocity = Vector3.Distance(transform.position, lastPosition) / Time.fixedDeltaTime;

        for(int i = 0; i < snowControllerObjs.Length; i++)

        {

            if (Vector3.Distance(snowControllerObjs[i].transform.position, transform.position) > spotSize * 5f) continue;

  

            float groundY = snowControllerObjs[i].transform.position.y;

            float handY = transform.position.y;

            // 바닥 ~ 눈 최대 높이 사이에 있고, 움직이고 있으면 지우기

            if (handY >= groundY && handY <= groundY + maxSnowHeight && velocity > minVelocity)

            {

                snowController = snowControllerObjs[i].GetComponent<SnowController>();

                snowRT = snowController.snowRT;

                GetPosition();

                DrawSpot();

            }

        }

        lastPosition = transform.position;

    }

  

    void GetPosition()

    {

        float scaleX = snowController.transform.localScale.x;

        float scaleY = snowController.transform.localScale.z;

  

        float snowPosX = snowController.transform.position.x;

        float snowPosY = snowController.transform.position.z;

  

        int posX = snowRT.width / 2 - (int)(((transform.position.x - snowPosX) * snowRT.width / 2) / scaleX);

        int posY = snowRT.height / 2 - (int)(((transform.position.z - snowPosY) * snowRT.height / 2) / scaleY);

        position = new Vector2Int(posX, posY);

    }

  

    void DrawSpot()

    {

        if (snowRT == null) return;

        if (snowComputeShader == null) return;

  

        int kernel_handle = snowComputeShader.FindKernel(drawSpotKernel);

        snowComputeShader.SetTexture(kernel_handle, snowImageProperty, snowRT);

        snowComputeShader.SetFloat(colorValueProperty, 0);

        snowComputeShader.SetFloat(resolutionProperty, snowRT.width);

        snowComputeShader.SetFloat(positionXProperty, position.x);

        snowComputeShader.SetFloat(positionYProperty, position.y);

        snowComputeShader.SetFloat(spotSizeProperty, spotSize);

        snowComputeShader.Dispatch(kernel_handle, snowRT.width / 8, snowRT.height / 8, 1);

    }

}