using UnityEngine;
using System;

public sealed class Wallet : MonoBehaviour
{
    public static Wallet Instance { get; private set; }

    [Header("Init (used only once on the first instance)")]
    [SerializeField] private int startReputation = 0;

    public int Reputation { get; private set; }
    public event Action<int> OnChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[Wallet] Duplicate detected in scene '{gameObject.scene.name}'. "
                           + $"Keeping id={Instance.GetInstanceID()}, destroying id={GetInstanceID()}.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // IMPORTANT: Do not reset if already carrying a value (e.g., coming back from Combat).
        // Only apply the initial value the very first time.
        if (Reputation == 0 && startReputation != 0)
            Reputation = startReputation;

        Debug.Log($"[Wallet] Awake id={GetInstanceID()} rep={Reputation}");
    }

    public void Set(int value)
    {
        Reputation = Mathf.Max(0, value);
        Debug.Log($"[Wallet] Set => {Reputation} (id {GetInstanceID()})");
        OnChanged?.Invoke(Reputation);
    }

    public void Add(int delta)
    {
        if (delta == 0) return;
        Reputation = Mathf.Max(0, Reputation + delta);
        Debug.Log($"[Wallet] Add {delta} => {Reputation} (id {GetInstanceID()})");
        OnChanged?.Invoke(Reputation);
    }
}