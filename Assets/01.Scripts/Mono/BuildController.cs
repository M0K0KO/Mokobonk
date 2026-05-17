using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BuildController : MonoBehaviour
{
    [SerializeField] private GameObject turretGhostPrefab;
    [SerializeField] private GameObject wallGhostPrefab;

    [SerializeField] private Material ghostValidMat;
    [SerializeField] private Material ghostInvalidMat;

    private Camera _cam;
    private InputManager _inputManager;


    private enum BuildMode { Idle, Turret, Wall }
    private BuildMode _mode = BuildMode.Idle;

    private GameObject _ghostInstance;
    private MeshRenderer _ghostRenderer;
    private EntityManager _entityManager;

    private EntityQuery _gridQuery, _occupancyQuery, _resourceQuery;
    private EntityQuery _turretQueueQuery, _wallQueueQuery;
    private EntityQuery _turretCfgQuery, _wallCfgQuery;

    private void Start()
    {
        _inputManager = InputManager.Instance;
        _cam = Camera.main;

        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        _gridQuery = _entityManager.CreateEntityQuery(typeof(GridConfigSingleton));
        _occupancyQuery = _entityManager.CreateEntityQuery(typeof(GridOccupancySingleton));
        _resourceQuery = _entityManager.CreateEntityQuery(typeof(ResourceSingleton));
        _turretQueueQuery = _entityManager.CreateEntityQuery(typeof(SpawnTurretQueueSingleton));
        _wallQueueQuery = _entityManager.CreateEntityQuery(typeof(SpawnWallQueueSingleton));
        _turretCfgQuery = _entityManager.CreateEntityQuery(typeof(TurretSpawnConfigSingleton));
        _wallCfgQuery = _entityManager.CreateEntityQuery(typeof(WallSpawnConfigSingleton));
    }

    private void Update()
    {
        HandleModeInput();
        if (_mode == BuildMode.Idle) return;

        if (TryGetMouseCell(out int2 cell))
        {
            UpdateGhost(cell);
            if (Input.GetMouseButtonDown(0)) TryPlace(cell);
        }
        else if (_ghostInstance != null)
        {
            _ghostInstance.SetActive(false);
        }
    }

    private void HandleModeInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) { Exit(); return; }
        if (_inputManager.TryConsumeTurretBuildModeInput()) Toggle(BuildMode.Turret);
        if (_inputManager.TryConsumeWallBuildModeInput()) Toggle(BuildMode.Wall);
    }

    private void Toggle(BuildMode target)
    {
        if (_mode == target) { Exit(); return; }
        Exit();
        _mode = target;
        SpawnGhost(target == BuildMode.Turret ? turretGhostPrefab : wallGhostPrefab);
    }

    private void Exit()
    {
        _mode = BuildMode.Idle;
        if (_ghostInstance != null)
        {
            Destroy(_ghostInstance);
            _ghostInstance = null;
            _ghostRenderer = null;
        }
    }

    private void SpawnGhost(GameObject prefab)
    {
        _ghostInstance = Instantiate(prefab);
        _ghostRenderer = _ghostInstance.GetComponentInChildren<MeshRenderer>();
    }

    private bool TryGetMouseCell(out int2 cell)
    {
        cell = default;
        var ray = _cam.ScreenPointToRay(Input.mousePosition);
        if (Mathf.Approximately(ray.direction.y, 0f)) return false;

        float t = -ray.origin.y / ray.direction.y;
        if (t < 0f) return false;

        Vector3 worldPos = ray.origin + ray.direction * t;
        var grid = _gridQuery.GetSingleton<GridConfigSingleton>();
        cell = GridUtility.WorldToCell(worldPos, grid.Origin, grid.CellSize);
        return GridUtility.IsInBounds(cell, grid.GridSize);
    }

    private void UpdateGhost(int2 cell)
    {
        var grid = _gridQuery.GetSingleton<GridConfigSingleton>();
        var occupancy = _occupancyQuery.GetSingleton<GridOccupancySingleton>();
        var res = _resourceQuery.GetSingleton<ResourceSingleton>();

        Vector3 snap = GridUtility.CellToWorld(cell, grid.Origin, grid.CellSize);
        _ghostInstance.transform.position = snap;
        if (!_ghostInstance.activeSelf) _ghostInstance.SetActive(true);

        int cost = GetCurrentCost();
        bool canPlace = !occupancy.Map.ContainsKey(cell) && res.Gold >= cost;

        if (_ghostRenderer != null)
            _ghostRenderer.sharedMaterial = canPlace ? ghostValidMat : ghostInvalidMat;
    }

    private void TryPlace(int2 cell)
    {
        var occupancy = _occupancyQuery.GetSingleton<GridOccupancySingleton>();
        if (occupancy.Map.ContainsKey(cell)) return;

        int cost = GetCurrentCost();
        var resRW = _resourceQuery.GetSingletonRW<ResourceSingleton>();
        if (resRW.ValueRO.Gold < cost) return;
        resRW.ValueRW.Gold -= cost;

        if (_mode == BuildMode.Turret)
        {
            var queue = _turretQueueQuery.GetSingleton<SpawnTurretQueueSingleton>().Queue;
            queue.Enqueue(new SpawnTurretCommand { Cell = cell });
        }
        else if (_mode == BuildMode.Wall)
        {
            var queue = _wallQueueQuery.GetSingleton<SpawnWallQueueSingleton>().Queue;
            queue.Enqueue(new SpawnWallCommand { Cell = cell });
        }
    }

    private int GetCurrentCost()
    {
        return _mode switch
        {
            BuildMode.Turret => _turretCfgQuery.GetSingleton<TurretSpawnConfigSingleton>().Cost,
            BuildMode.Wall => _wallCfgQuery.GetSingleton<WallSpawnConfigSingleton>().Cost,
            _ => 0
        };
    }
}