using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TerrainGenerator : MonoBehaviour {

    public GameObject wall;
    public GameObject endpointObject;
    public GameObject patrollers;
    public GameObject pursuers;
    public GameObject player;

    [Range(50, 200)]
    public int width = 100;
    [Range(50, 200)]
    public int length = 100;
    [Range(3, 30)]
    public int borderWidth = 20;
    [Range(7, 50)]
    public int startingBoxSize = 8;
    [Range(0.05f, 0.4f)]
    public float noise = 0.1f;
    [Range(0.3f, 0.6f)]
    public float density = 0.5f;
    public int patrollerCount;
    public int pursuerCount;
    public int[] offsetTerrain;

    private float wallScale;
    private float[,] heightMap;
    private int[,] blockMap;
    private TerrainMap perlinNoise = new TerrainMap();
    private Position endPoint;
    private int[,] movingArea;
    private List<Vector2> movingAreaTiles = new List<Vector2>();
    private GameObject spawnedInPlayer;
    private List<GameObject> spawnedInEnemies;
    private List<GameObject> spawnedInWalls;
    private GameObject finish;

    void Start() {
        spawnedInWalls = new List<GameObject>();
        spawnedInEnemies = new List<GameObject>();
        wallScale = wall.transform.localScale.x;
    }

    public void GenerateNewLevelVariables(int difficulty) {

        //Random.InitState((int) (Time.realtimeSinceStartup * 10000f));

        noise = Random.Range(0.09f, 0.15f);
        density = Random.Range(0.45f, 0.51f);
        patrollerCount = (int) difficulty + 1;
        pursuerCount = Mathf.Max((difficulty / 2) - 1, 0);
        offsetTerrain[0] = Random.Range(-100000, 100000);

        float factor = difficulty;
        if (difficulty > 7) {
            factor = 7f;
        }

        length = (int) (55 + (45 * (factor / 7f)));
        width = (int) (80 + (70 * (factor / 7f)));
        blockMap = new int[width, length];
    }

    public void GenerateLevel() {
        DestroyTerrain();
        GenerateMaps();
        CreateTheTerrain();
        GetEndPoint();
        CalculateMoveableLocations();
        SpawnEndPoint();
    }

    public void DestroyTerrain() {
        foreach (GameObject wall in spawnedInWalls) {
            Destroy(wall);
        }
        spawnedInWalls.Clear();
    }

    public void DestroySpawnedEnemies() {
        foreach (GameObject enemy in spawnedInEnemies) {
            Destroy(enemy);
        }
        spawnedInEnemies.Clear();
    }

    public void DestroySpawnedPlayer() {
        if (spawnedInPlayer != null) {
            Destroy(spawnedInPlayer);
        }
    }

    public void SpawnInEnemies() {
        Random.InitState(offsetTerrain[0] + offsetTerrain[1]);
        SpawnInEnemyAtRandomLocation(patrollers, patrollerCount, 25);
        SpawnInEnemyAtRandomLocation(pursuers, pursuerCount, 50);
    }

    public void AllowEnemyMovement() {
        foreach (GameObject enemy in spawnedInEnemies) {
            enemy.GetComponent<IEnemy>().StartMovement();
        }
    }

    public void DisableEnemyMovement() {
        foreach (GameObject enemy in spawnedInEnemies) {
            enemy.GetComponent<IEnemy>().StopMovement();
        }
    }

    public void SpawnInPlayer() {
        float location = borderWidth + startingBoxSize;
        spawnedInPlayer = SpawnInGameObjectAtLocation(player, new Vector3(location, 1, location));
        spawnedInPlayer.GetComponent<PlayerController>().Initialize();
    }

    private GameObject SpawnInGameObjectAtLocation(GameObject go, Vector3 location) {
        return Instantiate(
            go,
            location,
            Quaternion.identity
        );
    }

    private void SpawnInEnemyAtRandomLocation(GameObject enemyType, int amount, float minFromSpawn) {
        int i = amount;
        while (i > 0) {
            int random = Random.Range(0, movingAreaTiles.Count - 1);
            Vector2 location = movingAreaTiles[random];

            if (Vector2.Distance(location, new Vector2(borderWidth, borderWidth)) > minFromSpawn) {
                i--;
                GameObject enemy = SpawnInGameObjectAtLocation(enemyType, new Vector3(location.x * wallScale, 1, location.y * wallScale));
                IEnemy en = enemy.GetComponent<IEnemy>();
                en.Initialize();
                spawnedInEnemies.Add(enemy);
            }
        }
    }


    public List<Vector2> GetMovingAreaTiles() {
        return movingAreaTiles;
    }

    private void CalculateMoveableLocations() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < length; y++) {
                if (movingArea[x, y] == 1) {
                    movingAreaTiles.Add(new Vector2(x, y));
                }
            }
        }
    }


    private void GenerateMaps() {

        heightMap = perlinNoise.FillMap(width, length, noise, offsetTerrain);

        //generate the map
        for (int w = 0; w < width; w++) {
            for (int h = 0; h < length; h++) {

                float height = heightMap[w, h];

                //normal density of terrain
                if (w >= borderWidth && w <= width - borderWidth && h >= borderWidth && h <= length - borderWidth) {
                    if (height >= density) {
                        blockMap[w, h] = 1;
                        continue;
                    }

                //make a border (more dense placement of walls)
                } else {

                    //The closer to the edge of the map, the more dense the block placements will be
                    int distanceFromEdge = 0;
                    if (w < borderWidth) {
                        distanceFromEdge += borderWidth - w;
                    } else if (w > width - borderWidth) {
                        distanceFromEdge += borderWidth - (width - w);
                    }
                    if (h < borderWidth) {
                        distanceFromEdge += borderWidth - h;
                    } else if (h > length - borderWidth) {
                        distanceFromEdge += borderWidth - (length - h);
                    }

                    float jump = (1 - density) / borderWidth;
                    float comp = density - (jump * (distanceFromEdge + 1));

                    if (height >= comp) {
                        blockMap[w, h] = 1;
                        continue;
                    }
                }
                blockMap[w, h] = 0;
            }
        }

        AddStartingArea(startingBoxSize);
        RemoveSmallSections(0, 48);
        RemoveSmallSections(1, 15);
        EnsureConnectivity(4);

    }

    private void SpawnEndPoint() {
        finish = Instantiate(
            endpointObject,
            new Vector3(endPoint.x* wallScale, 1, endPoint.y * wallScale),
            Quaternion.identity
        );
        spawnedInWalls.Add(finish);
    }

    private void AddStartingArea(int startingBoxSize) {
        for (int w = borderWidth; w < startingBoxSize + borderWidth; w++) {
            for (int h = borderWidth; h < startingBoxSize + borderWidth; h++) {
                blockMap[w, h] = 0;
            }
        }
    }

    private void GetEndPoint() {

        int[,] movingArea = new int[width, length];

        Position tileInSection = new Position(borderWidth + 1, borderWidth + 1);

        Position toReturn = tileInSection;

        Queue<Position> tilesToCheck = new Queue<Position>();
        bool[,] checkedTiles = new bool[width, length];

        tilesToCheck.Enqueue(tileInSection);
        checkedTiles[tileInSection.x, tileInSection.y] = true;

        int blockType = blockMap[tileInSection.x, tileInSection.y];

        while (tilesToCheck.Count > 0) {

            Position tile = tilesToCheck.Dequeue();

            movingArea[tile.x, tile.y] = 1;

            toReturn = tile;

            for (int x = -1; x <= 1; x += 2) {
                Position tileToCheck = new Position(tile.x + x, tile.y);
                if (tileToCheck.x > 0 && tileToCheck.x < width) {
                    if (!checkedTiles[tileToCheck.x, tileToCheck.y]) {
                        if (blockMap[tileToCheck.x, tileToCheck.y] == blockType) {
                            checkedTiles[tileToCheck.x, tileToCheck.y] = true;
                            tilesToCheck.Enqueue(tileToCheck);
                        }
                    }
                }
            }
            for (int y = -1; y <= 1; y += 2) {
                Position tileToCheck = new Position(tile.x, tile.y + y);
                if (tileToCheck.y > 0 && tileToCheck.y < length) {
                    if (!checkedTiles[tileToCheck.x, tileToCheck.y]) {
                        if (blockMap[tileToCheck.x, tileToCheck.y] == blockType) {
                            checkedTiles[tileToCheck.x, tileToCheck.y] = true;
                            tilesToCheck.Enqueue(tileToCheck);
                        }
                    }
                }
            }
        }
        this.movingArea = movingArea;
        endPoint = toReturn;
    }

    private void CreateTheTerrain() {

        for (int w = 0; w < width; w++) {
            for (int h = 0; h < length; h++) {
                if (blockMap[w, h] == 1) {
                    float height = heightMap[w, h];
                    CreateWallAtLocation(new Vector3(w * wallScale, (height * 3) , h * wallScale));
                }
            }
        }
    }

    private void EnsureConnectivity(int createdTunnelWidth) {
        List<List<Position>> sections = FindGroupedSections();

        for (int i = 0; i < sections.Count; i++) {

            List<Position> section1 = sections[i];
            if (blockMap[section1[0].x, section1[0].y] == 0) {

                Position closest1 = new Position(0, 0);
                Position closest2 = new Position(0, 0);
                int distance = int.MaxValue;

                for (int q = 0; q < sections.Count; q++) {
                    if (q == i) {
                        continue;
                    }
                    List<Position> section2 = sections[q];
                    if (blockMap[section2[0].x, section2[0].y] == 0) {

                        foreach (Position p1 in section1) {
                            foreach (Position p2 in section2) {
                                int dis = p1.Distance(p2);
                                if (dis < distance) {
                                    distance = dis;
                                    closest1.SetValues(p1.x, p1.y);
                                    closest2.SetValues(p2.x, p2.y);
                                }
                            }
                        }
                    }
                }

                while (closest1.x != closest2.x || closest1.y != closest2.y) {

                    if (closest1.x < closest2.x) closest1.x++;
                    else if (closest1.x > closest2.x) closest1.x--;
                    if (closest1.y < closest2.y) closest1.y++;
                    else if (closest1.y > closest2.y) closest1.y--;

                    for (int x = -createdTunnelWidth / 2; x <= createdTunnelWidth / 2; x++) {
                        for (int y = -createdTunnelWidth / 2; y <= createdTunnelWidth / 2; y++) {
                            blockMap[closest1.x + x, closest1.y + y] = 0;
                        }
                    }
                }

                if (distance < 10000) {
                    sections = FindGroupedSections();
                    i = 0;
                }

            }
        }

    }


    private void RemoveSmallSections(int blockType, int minimumSize) {
        foreach (List<Position> section in FindGroupedSections()) {
            if (section.Count < minimumSize) {
                if (blockMap[section[0].x, section[0].y] == blockType) {
                    foreach (Position p in section) {
                        blockMap[p.x, p.y] = Mathf.Abs(blockMap[p.x, p.y] - 1);
                    }
                }
            }
        }
    }


    private List<List<Position>> FindGroupedSections() {

        List < List <Position>> mapSections = new List<List<Position>>();

        bool[,] checkedTiles = new bool[width, length];

        for (int w = 0; w < width; w++) {
            for (int h = 0; h < length; h++) {
                if (!checkedTiles[w, h]) {
                    List<Position> area = GetGroupOfTilesInSection(new Position(w, h));
                    mapSections.Add(area);

                    foreach (Position p in area) {
                        checkedTiles[p.x, p.y] = true;
                    }

                }
            }
        }

        return mapSections;
    }


    private List<Position> GetGroupOfTilesInSection(Position tileInSection) {
        List<Position> section = new List<Position>();
        Queue<Position> tilesToCheck = new Queue<Position>();
        bool[,] checkedTiles = new bool[width, length];

        tilesToCheck.Enqueue(tileInSection);
        checkedTiles[tileInSection.x, tileInSection.y] = true;

        int blockType = blockMap[tileInSection.x, tileInSection.y];

        while(tilesToCheck.Count > 0) {

            Position tile = tilesToCheck.Dequeue();

            section.Add(tile);

            for (int x = -1; x <= 1; x += 2) {
                Position tileToCheck = new Position(tile.x + x, tile.y);
                if (tileToCheck.x > 0 && tileToCheck.x < width) {
                    if (!checkedTiles[tileToCheck.x, tileToCheck.y]) {
                        if (blockMap[tileToCheck.x, tileToCheck.y] == blockType) {
                            checkedTiles[tileToCheck.x, tileToCheck.y] = true;
                            tilesToCheck.Enqueue(tileToCheck);
                        }
                    }
                }
            }
            for (int y = -1; y <= 1; y += 2) {
                Position tileToCheck = new Position(tile.x, tile.y + y);
                if (tileToCheck.y > 0 && tileToCheck.y < length) {
                    if (!checkedTiles[tileToCheck.x, tileToCheck.y]) {
                        if (blockMap[tileToCheck.x, tileToCheck.y] == blockType) {
                            checkedTiles[tileToCheck.x, tileToCheck.y] = true;
                            tilesToCheck.Enqueue(tileToCheck);
                        }
                    }
                }
            }

        }

        return section;
    }

    private void CreateWallAtLocation(Vector3 loc) {
        GameObject newWall = Instantiate(
            wall,
            loc,
            Quaternion.identity
        );
        spawnedInWalls.Add(newWall);
    }

    public Vector3 GetEndLocation() {
        return finish.transform.position;
    }

    public Vector2 GetEndPosition() {
        return new Vector2(endPoint.x, endPoint.y);
    }

    public int[,] GetMovingArea() {
        return movingArea;
    }

    public int[,] GetBlockMap() {
        return blockMap;
    }

    public float GetWallSize() {
        return wallScale;
    }

    public float GetEndOfMapX() {
        return width * wallScale;
    }

    public float GetEndOfMapY() {
        return length * wallScale;
    }

    private struct Position {
        public int x, y;
        public Position(int x, int y) {
            this.x = x;
            this.y = y;
        }
        public int Distance(Position other) {
            return Mathf.Abs(x - other.x) + Mathf.Abs(y - other.y);
        }
        public void SetValues(int newX, int newY) {
            x = newX;
            y = newY;
        }
    }

}
