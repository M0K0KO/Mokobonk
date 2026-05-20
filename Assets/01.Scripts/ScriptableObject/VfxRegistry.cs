using UnityEngine;

[CreateAssetMenu(fileName = "VfxRegistry", menuName = "Game/Vfx Registry")]
public class VfxRegistry : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public VfxId Id;
        public VfxType Type;
        public GameObject Prefab;
        public int PrewarmCount;
        public float Lifetime;
    }

    public Entry[] Entries;

    // for fast lookup
    private Entry[] _table;

    
    // we can lookup the vfx by VfxId!
    public void BuildLookup()
    {
        int max = 0;
        foreach(var e in Entries)
        {
            if ((int)e.Id > max) max = (int)e.Id;
        }

        _table = new Entry[max + 1];
        foreach (var e in Entries)
            _table[(int)e.Id] = e;
    }

    public Entry Get(VfxId id) => _table[(int)id];
}