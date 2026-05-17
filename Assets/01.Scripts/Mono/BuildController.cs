using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BuildController : MonoBehaviour
{
    [SerializeField] private GameObject turretPrefab;
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private Material ghostValidMat;
    [SerializeField] private Material ghostInvalidMat;

    private Camera _cam;
    private InputManager _inputManager;


    private enum Mode { Idle, Building }
    private Mode _mode = Mode.Idle;

    private GameObject _ghostInstance;
    private MeshRenderer _ghostRenderer;
    private Entity _turretPrefabEntity;
    private EntityManager _entityManager;

    private void Start()
    {
        _inputManager = InputManager.Instance;
        _cam = Camera.main;

        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    private void Update()
    {
        if (_inputManager.TryConsumeBuildModeChangeInput()) ToggleMode();
        if (_mode != Mode.Building) return;

        if (TryGetMouseCell(out int2 cell, out Vector3 worldPos))
        {
            UpdateGhost(cell, worldPos);
            if (Input.GetMouseButtonDown(0)) TryPlace(cell);
        }
        else
        {
            if (_ghostInstance != null) _ghostInstance.SetActive(false);
        }
    }

    private void ToggleMode()
    {
        if (_mode == Mode.Idle) EnterBuild();
        else ExitBuild();
    }

    private void EnterBuild()
    {
        _mode = Mode.Building;
        if (_ghostInstance == null)
        {
            _ghostInstance = Instantiate(ghostPrefab);
            _ghostRenderer = _ghostInstance.GetComponentInChildren<MeshRenderer>();
        }
        _ghostInstance.SetActive(true);
    }

    private void ExitBuild()
    {
        _mode = Mode.Idle;
        if (_ghostInstance != null) _ghostInstance.SetActive(false);
    }

    private bool TryGetMouseCell(out int2 cell, out Vector3 worldPos)
    {
        var ray = _cam.ScreenPointToRay(Input.mousePosition);
        if (Mathf.Approximately(ray.direction.y, 0f))
        {
            cell = default; worldPos = default; return false;
        }
        float t = -ray.origin.y / ray.direction.y;
        if (t < 0f) { cell = default; worldPos = default; return false; }

        worldPos = ray.origin + ray.direction * t;

        var grid = _entityManager.CreateEntityQuery(typeof(GridConfigSingleton))
                      .GetSingleton<GridConfigSingleton>();
        cell = GridUtility.WorldToCell(worldPos, grid.Origin, grid.CellSize);

        return GridUtility.IsInBounds(cell, grid.GridSize);
    }

    private void UpdateGhost(int2 cell, Vector3 _)
    {
        var grid = _entityManager.CreateEntityQuery(typeof(GridConfigSingleton))
                      .GetSingleton<GridConfigSingleton>();
        var occupancy = _entityManager.CreateEntityQuery(typeof(GridOccupancySingleton))
                           .GetSingleton<GridOccupancySingleton>();

        Vector3 snap = GridUtility.CellToWorld(cell, grid.Origin, grid.CellSize);
        _ghostInstance.transform.position = snap;

        bool canPlace = !occupancy.Map.ContainsKey(cell);
        if (_ghostRenderer != null)
            _ghostRenderer.sharedMaterial = canPlace ? ghostValidMat : ghostInvalidMat;
    }

    private void TryPlace(int2 cell)
    {
        var occupancy = _entityManager.CreateEntityQuery(typeof(GridOccupancySingleton))
                           .GetSingleton<GridOccupancySingleton>();
        if (occupancy.Map.ContainsKey(cell)) return;

        var queueSingleton = _entityManager.CreateEntityQuery(typeof(SpawnTurretQueueSingleton))
                                .GetSingleton<SpawnTurretQueueSingleton>();

        queueSingleton.Queue.Enqueue(new SpawnTurretCommand { Cell = cell });
    }
}