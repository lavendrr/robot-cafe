using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class PickerPanel<T> : MonoBehaviour
{
    [SerializeField] private Transform contentRoot;
    [SerializeField] private PickerButton buttonPrefab;

    public Action<T> OnItemSelected;

    protected virtual void OnAwake() { }

    void Awake()
    {
        OnAwake();
        Close();
    }

    protected abstract IEnumerable<T> GetAllItems();
    protected abstract string GetLabel(T item);

    public bool HasEligibleItems(IEnumerable<T> exclude = null)
    {
        var excluded = ToHashSet(exclude);
        foreach (var item in GetAllItems())
            if (!excluded.Contains(item))
                return true;
        return false;
    }

    public void Open(IEnumerable<T> exclude = null)
    {
        BuildList(exclude);
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    void BuildList(IEnumerable<T> exclude)
    {
        var excluded = ToHashSet(exclude);

        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        foreach (var item in GetAllItems())
        {
            if (excluded.Contains(item)) continue;

            var captured = item;
            var btn = Instantiate(buttonPrefab, contentRoot);
            btn.Initialize(GetLabel(captured), () =>
            {
                OnItemSelected?.Invoke(captured);
                Close();
            });
        }
    }

    HashSet<T> ToHashSet(IEnumerable<T> source) =>
        source != null ? new HashSet<T>(source) : new HashSet<T>();
}
