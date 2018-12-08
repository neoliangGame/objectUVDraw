//在摄像机后处理上进行绘画处理
//并且保存到本地
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class neoDraw : MonoBehaviour {

    public Material drawUtil = null;

    public GameObject draw = null;

    Texture2D drawTexture = null;
    Texture2D currentTexture = null;
    Dictionary<Vector2, Vector2> alreadyWritePivot;

    RenderTexture activeTex;
    
	void Start () {
        drawTexture = new Texture2D(1024, 1024, TextureFormat.ARGB32, false);
        for(int i = 0;i < drawTexture.width; i++)
        {
            for(int j = 0;j < drawTexture.height; j++)
            {
                drawTexture.SetPixel(i,j,new Color(1,1,1,0));
            }
        }
        drawTexture.wrapMode = TextureWrapMode.Clamp;
        drawTexture.Apply();
        currentTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
        currentTexture.wrapMode = TextureWrapMode.Clamp;

        
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (draw.GetComponent<screenToObjects>().needToCompute)
        {
            RenderTexture temp1 = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
            //获取grab texture
            Graphics.Blit(temp1, temp1, drawUtil, 0);

            //获取screenPoint，转换到对应texture尺寸下的位置
            Vector2 screenPoint = draw.GetComponent<screenToObjects>().screenPos;

            int gx = (int)screenPoint.x;
            int gy = (int)screenPoint.y;
            //从该点向外扩散遍历，把grab texture上标志的uv坐标标记
            bool lineClear = false;
            activeTex = RenderTexture.active;
            RenderTexture.active = temp1;
            currentTexture.ReadPixels(new Rect(0, 0, temp1.width, temp1.height), 0, 0);
            currentTexture.Apply();
            RenderTexture.active = activeTex;
            Color drawColor = draw.GetComponent<Renderer>().material.GetColor("_PointColor");
            //testStr = "";
            alreadyWritePivot = new Dictionary<Vector2, Vector2>();
            for (int x = gx; x < currentTexture.width; x++)
            {
                lineClear = true;
                for (int y = gy; y < currentTexture.height; y++)
                {
                    if(DrawColor(x, y,ref drawColor))
                    {
                        lineClear = false;
                    }
                }
                for (int y = gy - 1; y >= 0; y--)
                {
                    if (DrawColor(x, y,ref drawColor))
                    {
                        lineClear = false;
                    }
                }
                if (lineClear)
                    break;
            }

            for (int x = gx - 1; x >= 0; x--)
            {
                lineClear = true;
                for (int y = gy; y < currentTexture.height; y++)
                {
                    if (DrawColor(x, y,ref drawColor))
                    {
                        lineClear = false;
                    }
                }
                for (int y = gy - 1; y >= 0; y--)
                {
                    if (DrawColor(x, y,ref drawColor))
                    {
                        lineClear = false;
                    }
                }
                if (lineClear)
                    break;
            }
            //Debug.Log(testStr);
            drawTexture.Apply();
            draw.GetComponent<Renderer>().material.SetTexture("_MainTex", drawTexture);

            RenderTexture.ReleaseTemporary(temp1);
            alreadyWritePivot.Clear();
        }
        
        Graphics.Blit(source, destination);
    }

    bool DrawColor(int x,int y, ref Color color)
    {
        int dx, dy;
        Color currentPixel = currentTexture.GetPixel(x, y);
        if (currentPixel.a < 0.5)
            return false;
        dx = (int)(currentPixel.r * (float)drawTexture.width);
        dy = (int)(currentPixel.g * (float)drawTexture.height);
        //在转换的过程中，发现很多不同的(x,y)转换后的(dx,dy)是一样的，所以做了以下判重处理机制
        Vector2 dVec = new Vector2(dx, dy);
        if (alreadyWritePivot.ContainsKey(dVec))
        {
            Vector2 vec = alreadyWritePivot[dVec];
            dx += x - (int)vec.x;
            dy += y - (int)vec.y;
        }
        else
        {
            alreadyWritePivot[dVec] = new Vector2(x,y);
        }
        int[,] around = new int[5,2]{
                     { 0,-1 },
            {-1, 0 },{ 0, 0 },{ 1, 0 },
                     { 0, 1 },
        };
        for(int i = 0;i < 5; i++)
        {
            x = dx + around[i, 0];
            x = (x < 0) ? 0 : x;
            x = (x >= drawTexture.width) ? drawTexture.width - 1 : x;
            y = dy + around[i, 1];
            y = (y < 0) ? 0 : y;
            y = (y >= drawTexture.height) ? drawTexture.height - 1 : y;
            drawTexture.SetPixel(x + around[i,0], y + around[i, 1], color);
        }
        return true;
        
    }


    public void SaveDrawPicture()
    {
        string path = Application.dataPath + "/draw/drawTexture.png";
        byte[] bytes = drawTexture.EncodeToPNG();
        FileStream file = File.Open(path, FileMode.Create);
        BinaryWriter bw = new BinaryWriter(file);
        bw.Write(bytes);
        file.Flush();
        file.Close();

    }

}
