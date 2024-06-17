using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Spawns Grid and Marker elements.
/// </summary>
public class GridSpawner : MonoBehaviour
{
    [Header("Set in Inspector")]
    [SerializeField]
    public GameObject prefab; 
    [SerializeField]
    public Transform cellsContainer; 
    [SerializeField]
    public Transform marksContainer;
    [SerializeField]
    public float offset = 1.0f;
    [SerializeField]
    private Marker markerPrefab;
    [SerializeField]
    private List<Light> winningSequenceLights = new List<Light>();
    [SerializeField, Tooltip("Seconds in between positioning the lights over winning sequence")]
    private float lightInterval = 0.5f;
    [SerializeField]
    private AudioClip lightTurnedOnSfx;
    
    // Variables
    private List<Marker> _markersPool = new List<Marker>();
    private Queue<CellData> cellDataQueue = new Queue<CellData>();
    private List<Cell_Interact> _cellInteracts = new List<Cell_Interact>();
    private Coroutine spawnCoroutine;
    private Coroutine lightsPositioningCoroutine;
    private WaitForSeconds lightsWait;

    #region Monobehaviour

    private void Start()
    {
        Initialize();
    }

    private void OnDestroy()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }
    #endregion Monobehaviour

    private void Initialize()
    {
        lightsWait = new WaitForSeconds(lightInterval);
        ToggleLights(false);
        SpawnGrid();
        GlobalActions.OnGameStateChanged += OnGameStateChanged;
        GlobalActions.CellDataSelected += CellDataSelected;
        GlobalActions.WinningSequence += OnWinningSequence;
    }
    private void UnInitialize()
    {
        GlobalActions.OnGameStateChanged -= OnGameStateChanged;
        GlobalActions.CellDataSelected -= CellDataSelected;
        GlobalActions.WinningSequence -= OnWinningSequence;
    }

    private void OnGameStateChanged(GameState fromState, GameState toState)
    {
        if (fromState == GameState.MainMenu && toState == GameState.GameLoop)
        {
            CallSpawnActions();
        }

        if (fromState == GameState.Results && toState == GameState.MainMenu)
        {
            CallDeSpawnActions();
        }
    }

    private void CellDataSelected(CellData cellData)
    {
        if (!GameManager.S.TryGetCurrentPlayerTurnData(out PlayerData playerData))
        {
            return;
        }
        
        if (!TryGetNextAvailableMarker(out Marker availableMarker))
        {
            availableMarker = Instantiate(markerPrefab, marksContainer);
            _markersPool.Add(availableMarker);
        }
        
        availableMarker.Initialize(playerData, cellData);
    }

    private void OnWinningSequence(List<Vector2> coords)
    {
        if (lightsPositioningCoroutine != null)
        {
            StopCoroutine(lightsPositioningCoroutine);
        }

        lightsPositioningCoroutine = StartCoroutine(PositionAndTurnOnLights(coords));
    }

    private bool TryGetNextAvailableMarker(out Marker availableMarker)
    {
        availableMarker = null;
        
        foreach (Marker marker in _markersPool)
        {
            if (!marker.IsMarked())
            {
                availableMarker = marker;
                break;
            }
        }

        return availableMarker != null;
    }
    
    [ContextMenu("SpawnGrid")]
    private void SpawnGrid()
    {
        if (_cellInteracts.Count == 0)
        {
            GenerateGridData();
            spawnCoroutine = StartCoroutine(SpawnGridPrefabs());
        }
    }

    private void CallSpawnActions()
    {
        foreach (Cell_Interact cell_interact in _cellInteracts)
        {
            cell_interact.SpawnActions();
        }
    }
    
    private void CallDeSpawnActions()
    {
        foreach (Cell_Interact cell_interact in _cellInteracts)
        {
            cell_interact.DeSpawnActions();
        }

        foreach (Marker marker in _markersPool)
        {
            marker.MakeDormant();
        }

        ToggleLights(false);
    }

    private void ToggleLights(bool value)
    {
        foreach (Light light in winningSequenceLights)
        {
            light.enabled = value;
        }
    }
    
    [ContextMenu("Test SpawnGrid")]
    public void GenerateGridData()
    {
        if (prefab == null || cellsContainer == null)
        {
            Debug.LogError("Prefab or parent is not assigned.");
            return;
        }

        int gridDimension = GameLogic.GRID_DIMENSION;
        int arraySize = GameLogic.GRID_DIMENSION * GameLogic.GRID_DIMENSION;
        //int[] grid = new int[arraySize];

        Vector3 centerOffset = GetCenterOffset();

        for (int i = 0; i < gridDimension; i++)
        {
            for (int j = 0; j < gridDimension; j++)
            {
                //int index = i * gridDimension + j;
                Vector3 position = new Vector3(i * offset, 0, j * offset) - centerOffset;
                
                int row = i;
                int column = j;
                
                CellData cellData = new CellData
                (
                    localPosition: position,
                    coords: new Vector2(row,column),
                    isMarked: false
                );
                
                cellDataQueue.Enqueue(cellData);
            }
        }
    }

    private Vector3 GetCenterOffset()
    {
         Vector3 centeroffset = new Vector3((GameLogic.GRID_DIMENSION - 1) * offset / 2, 0, (GameLogic.GRID_DIMENSION - 1) * offset / 2);
         return centeroffset;
    }

    private IEnumerator SpawnGridPrefabs()
    {
        while (cellDataQueue.Count > 0)
        {
            CellData nextCellData = cellDataQueue.Dequeue();
            
            // we instantiate one per frame, in order to avoid halts on slower machines, and for general smoother flow.
            GameObject instance = Instantiate(prefab, nextCellData.LocalPosition, Quaternion.identity, cellsContainer);
            instance.transform.localScale *= offset;
            
            if (!instance.TryGetComponent(out Cell_Interact cellInteract))
            {
                cellInteract =  instance.AddComponent<Cell_Interact>();
            }
            
            cellInteract.Initialize(nextCellData);
            _cellInteracts.Add(cellInteract);
            yield return null;
        }
        
        // When finished spawning grid call the Loading Phase finished
        GlobalActions.TryGoToGameState?.Invoke(GameState.MainMenu);
        spawnCoroutine = null;
    }

    private IEnumerator PositionAndTurnOnLights(List<Vector2> coords)
    {
        int lightsCount = winningSequenceLights.Count;
        if (coords.Count != lightsCount)
        {
            Debug.LogError($"Expected same Count for  lights: {lightsCount} and Coords: {coords.Count}");
        }

        Vector3 centerOffset = GetCenterOffset();
        
        for (int i = 0; i < lightsCount; i++)
        {
            Transform lightTransform = winningSequenceLights[i].transform;
            
            int x = (int)coords[i].x;
            int y = (int)coords[i].y;
            
            Vector3 position = new Vector3(x * offset, 0, y * offset) - centerOffset;
            // keep the light's Altitude
            position.y = lightTransform.position.y;
            
            winningSequenceLights[i].enabled = true;
            lightTransform.localPosition = position;
            GameManager.S.AudioManager.PlayClipAt(lightTurnedOnSfx, position);
            yield return lightsWait;
        }

        lightsPositioningCoroutine = null;
    }
}
