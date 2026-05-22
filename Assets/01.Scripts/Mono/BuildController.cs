using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BuildController : MonoBehaviour
{
    [SerializeField] private GameObject turretGhostPrefab;
    [SerializeField] private GameObject mortarGhostPrefab;
    [SerializeField] private GameObject wallGhostPrefab;
    [SerializeField] private Material ghostValidMat;
    [SerializeField] private Material ghostInvalidMat;

    private Camera _cam;
    private InputManager _inputManager;
    private EntityManager _em;

    private EntityQuery _gridQuery, _occupancyQuery, _resourceQuery;
    private EntityQuery _queueQuery, _registryQuery;

    private BuildMode _mode = BuildMode.Idle;
    private GameObject _ghostInstance;
    private MeshRenderer _ghostRenderer;

    private enum BuildMode { Idle, Turret, Mortar, Wall }

    void Start()
    {
        _inputManager = InputManager.Instance;
        _cam = Camera.main;
        _em = World.DefaultGameObjectInjectionWorld.EntityManager;

        _gridQuery = _em.CreateEntityQuery(typeof(GridConfigSingleton));
        _occupancyQuery = _em.CreateEntityQuery(typeof(GridOccupancySingleton));
        _resourceQuery = _em.CreateEntityQuery(typeof(ResourceSingleton));
        _queueQuery = _em.CreateEntityQuery(typeof(SpawnBuildQueueSingleton));
        _registryQuery = _em.CreateEntityQuery(typeof(BuildableRegistrySingleton));
    }

    void Update()
    {
        HandleModeInput();
        if (_mode == BuildMode.Idle) return;
        if (_registryQuery.IsEmpty) return;

        if (TryGetMouseCell(out int2 cell))
        {
            UpdateGhost(cell);
            if (Input.GetMouseButtonDown(0)) EnqueueBuild(cell);
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
        if (_inputManager.TryConsumeMortarBuildModeInput()) Toggle(BuildMode.Mortar);
    }

    private void Toggle(BuildMode target)
    {
        if (_mode == target) { Exit(); return; }
        Exit();
        _mode = target;
        var ghost = target switch
        {
            BuildMode.Turret => turretGhostPrefab,
            BuildMode.Mortar => mortarGhostPrefab,
            BuildMode.Wall => wallGhostPrefab,
            _ => null,
        };
        SpawnGhost(ghost);
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

    private BuildableKind CurrentKind() => _mode switch
    {
        BuildMode.Turret => BuildableKind.Turret_Gunner,
        BuildMode.Mortar => BuildableKind.Turret_Mortar,
        BuildMode.Wall => BuildableKind.Wall,
        _ => BuildableKind.None,
    };

    private int GetCurrentCost()
    {
        var reg = _registryQuery.GetSingleton<BuildableRegistrySingleton>();
        return reg.TryGet(CurrentKind(), out var info) ? info.Cost : 0;
    }

    private void UpdateGhost(int2 cursorCell)
    {
        var grid = _gridQuery.GetSingleton<GridConfigSingleton>();
        var occupancy = _occupancyQuery.GetSingleton<GridOccupancySingleton>();
        var res = _resourceQuery.GetSingleton<ResourceSingleton>();
        var reg = _registryQuery.GetSingleton<BuildableRegistrySingleton>();

        if (!reg.TryGet(CurrentKind(), out var info)) return;

        int2 anchor = new int2(
        cursorCell.x - info.Size.x / 2,
        cursorCell.y - info.Size.y / 2);

        Vector3 ghostPos = GridUtility.FootprintCenterWorld(
            anchor, info.Size, grid.Origin, grid.CellSize);
        _ghostInstance.transform.position = ghostPos;
        if (!_ghostInstance.activeSelf) _ghostInstance.SetActive(true);

        bool canPlace =
            GridUtility.AreAllCellsFree(anchor, info.Size, grid.GridSize, occupancy.Map)
            && res.Gold >= info.Cost;

        if (_ghostRenderer != null)
            _ghostRenderer.sharedMaterial = canPlace ? ghostValidMat : ghostInvalidMat;
    }

    private void EnqueueBuild(int2 cursorCell)
    {
        var reg = _registryQuery.GetSingleton<BuildableRegistrySingleton>();
        if (!reg.TryGet(CurrentKind(), out var info)) return;

        int2 anchor = new int2(
            cursorCell.x - info.Size.x / 2,
            cursorCell.y - info.Size.y / 2);

        var queue = _queueQuery.GetSingleton<SpawnBuildQueueSingleton>().Queue;
        queue.Enqueue(new BuildCommand { Cell = anchor, Kind = CurrentKind() });
    }
}