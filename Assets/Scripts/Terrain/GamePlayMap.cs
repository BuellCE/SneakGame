using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//the 2d UI of the map layout
public class GamePlayMap : MonoBehaviour{

    public float mapScale = 2f;
    public Color mapColor;
    public Color mapEndPointColor;
    public Color mapPlayerColor;

    private Vector2 previousPlayerLocation;
    private Color[,] playerPreviousLocationColors;
    private Image mapImage;
    private Texture2D texture;
    private float wallScale;

    void Start() {
        mapImage = GetComponent<Image>();
        TerrainGenerator tg = GameObject.FindGameObjectWithTag("GameController").GetComponent<TerrainGenerator>();
        wallScale = tg.GetWallSize();
        texture = new Texture2D(0, 0);
    }

    public void GenerateMap(int[,] blockMap, Vector2 endLocation) {

        playerPreviousLocationColors = new Color[5, 5];

        if (texture == null) {
            texture = new Texture2D(blockMap.GetLength(0), blockMap.GetLength(1));
        }
        texture.Resize(blockMap.GetLength(0), blockMap.GetLength(1));

        for (int x = 0; x < blockMap.GetLength(0); x++) {
            for (int y = 0; y < blockMap.GetLength(1); y++) {
                Color color = Color.clear;
                if (blockMap[x,y] == 0) {
                    color = mapColor; 
                }
                texture.SetPixel(x, y, color);
            }
        }

        for (int x = -2; x <= 2; x++) {
            for (int y = -2; y <= 2; y++) {
                texture.SetPixel((int) endLocation.x + x, (int) endLocation.y + y, mapEndPointColor);
            }
        }

        ApplyTexture();

        RectTransform rectTrans = GetComponent<RectTransform>();
        rectTrans.sizeDelta = new Vector2(texture.width * mapScale, texture.height * mapScale);

    }

    public void PaintPlayer(Vector3 position) {

        Vector2 pos = new Vector2(position.x, position.z);
        if (pos != previousPlayerLocation) {

            if (playerPreviousLocationColors[0, 0] != null) {
                for (int x = -2; x <= 2; x++) {
                    for (int y = -2; y <= 2; y++) {
                        int toSetX = (int)(previousPlayerLocation.x / wallScale) + x;
                        int toSetY = (int)(previousPlayerLocation.y / wallScale) + y;
                        texture.SetPixel(toSetX, toSetY, playerPreviousLocationColors[x + 2, y + 2]);
                    }
                }
            }

            for (int x = -2; x <= 2; x++) {
                for (int y = -2; y <= 2; y++) {
                    int toSetX = (int)(pos.x / wallScale) + x;
                    int toSetY = (int)(pos.y / wallScale) + y;

                    playerPreviousLocationColors[x + 2, y + 2] = texture.GetPixel(toSetX, toSetY);

                    texture.SetPixel(toSetX, toSetY, mapPlayerColor);
                }
            }

            ApplyTexture();
            previousPlayerLocation = pos;
        }
    }

    private void ApplyTexture() {
        mapImage.material.mainTexture = texture;
        texture.Apply();
        //mapImage.material.SetTexture("_MainTex", texture);
    }

}

