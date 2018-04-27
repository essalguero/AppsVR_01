using UnityEngine;
using UnityEngine.Rendering;

public class VRCamera : MonoBehaviour {

    public string filename;
    public string folder = "sequence";

    public bool renderStereo = true;

    public bool poleMerge = false;

    // Inter-Pupil-Distance (Distancia entre pupilas)
    public float IPD = 0.064f;

    //width should be 2 * height
    public int width = 1000;
    public int height = 500;

    public int secW = 1;
    public int secH = 1;

    public bool HDR = false;


    public int frames = 1;

    public int framerate = 30;

    public Rect crop;



    RenderTexture equirect;

    Texture2D frame;

    int frameWidth;
    int frameHeight;

    RenderTextureFormat renderFormat;
    TextureFormat textureFormat;
    int currentFrame;

	// Use this for initialization
	void Start () {
        currentFrame = 0;

        frameWidth = (int)crop.width;
        frameHeight = (int)crop.height;

        Time.captureFramerate = framerate;

        if (HDR)
        {
            renderFormat = RenderTextureFormat.ARGBFloat;
            textureFormat = TextureFormat.RGBAFloat;
        }
        else
        {
            renderFormat = RenderTextureFormat.ARGB32;
            textureFormat = TextureFormat.RGBA32;
        }

        equirect = new RenderTexture(frameWidth, 2 * frameHeight, 24, renderFormat);
        equirect.dimension = TextureDimension.Tex2D;
        equirect.antiAliasing = 8;

        frame = new Texture2D(frameWidth, 2 * frameHeight, textureFormat, false);

        if (!System.IO.Directory.Exists(Application.dataPath + "/" + folder + "/"))
        {
            System.IO.Directory.CreateDirectory(Application.dataPath + "/" + folder + "/");
        }


    }
	
	// LateUpdate is called once per frame
	void LateUpdate () {
		if (currentFrame >= frames)
        {
            UnityEditor.EditorApplication.isPlaying = false;
            return;
        }

        Camera cam = GetComponent<Camera>();

        if (cam == null)
        {
            cam = GetComponentInParent<Camera>();

            if (cam == null)
            {
                Debug.Log("VR Camera node has no camara or parent camera");
                UnityEditor.EditorApplication.isPlaying = false;
            }
        }

        // set up Renderizar
        RenderTexture.active = equirect;

        cam.SetTargetBuffers(equirect.colorBuffer, equirect.depthBuffer);
        cam.fieldOfView = (180.0f * secH) / height;

        Rect rec;

        Vector3 originalPos = cam.transform.localPosition;

        // Render
        for (int i = 0; i < frameWidth / secW; ++i)
        {
            for (int j = 0; j < frameHeight / secH; ++j)
            {
                rec = new Rect(i * secW, j * secH, secW, secH);

                int ii;
                int jj;
                float xOffset;
                float yOffset;

                ii = i + (int)(crop.x / secW);
                if (j < (frameHeight / secH))
                {
                    jj = j  + (int)(crop.y / secH);
                }
                else
                {
                    jj = (j - (frameHeight / secH)) + (int)(crop.y / secH);
                }
                //jj = (j % (frameHeight / secH)) + (int)(crop.y / secH);

                float xArgle = 90 - ((2 * jj + 1) * 180.0f / (2.0f * height / secH));
                float yAngle = -180 + ((2 * ii + 1) * 360.0f / (2.0f * width / secW));

                cam.transform.localRotation = Quaternion.Euler(xArgle, yAngle, 0);
                cam.pixelRect = rec;
                cam.allowMSAA = true;
                cam.Render();
            }
        }

        // Render finished
        cam.transform.localPosition = originalPos; // para evitar que en el siguiente frame la camara este movida
        frame.ReadPixels(new Rect(0, 0, frameWidth, 2 * frameHeight), 0, 0);
        RenderTexture.active = null;

        if (HDR)
        {
            byte[] bytes = frame.EncodeToEXR();
            System.IO.File.WriteAllBytes(Application.dataPath + "/" + folder + "/" +
                                         filename + currentFrame.ToString().PadLeft(4, '0') + ".exr", bytes);
        }
        else
        {
            byte[] bytes = frame.EncodeToPNG();
            System.IO.File.WriteAllBytes(Application.dataPath + "/" + folder + "/" +
                                         filename + currentFrame.ToString().PadLeft(4, '0') + ".png", bytes);
        }

        cam.targetTexture = null;

        ++currentFrame;

        Debug.Log("File saved to: " + Application.dataPath + "/" + folder + "/" +
                  filename + currentFrame.ToString().PadLeft(4, '0') + ".png");
	}
}
