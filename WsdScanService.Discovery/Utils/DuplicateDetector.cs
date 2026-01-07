//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace WsdScanService.Discovery.Utils;

internal class DuplicateDetector<T>
    where T : class
{
    private readonly LinkedList<T> _fifoList;
    private readonly Dictionary<T, LinkedListNode<T>> _items;
    private readonly int _capacity;
    private readonly object _thisLock;

    public DuplicateDetector(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(capacity, 0,
            "The capacity parameter must be a positive value.");

        _capacity = capacity;
        _items = new Dictionary<T, LinkedListNode<T>>();
        _fifoList = new LinkedList<T>();
        _thisLock = new object();
    }

    public bool AddIfNotDuplicate(T value)
    {
        ArgumentNullException.ThrowIfNull(value, "The value must be non null.");
        var success = false;

        lock (_thisLock)
        {
            if (!_items.ContainsKey(value))
            {
                Add(value);
                success = true;
            }
        }

        return success;
    }

    private void Add(T value)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(
            _items.Count, _fifoList.Count,
            "The items and fifoList must be synchronized."
        );

        if (_items.Count == _capacity)
        {
            var node = _fifoList.Last;

            if (node != null)
            {
                _items.Remove(node.Value);
                _fifoList.Remove(node);
            }
        }

        _items.Add(value, _fifoList.AddFirst(value));
    }

    public bool Remove(T value)
    {
        ArgumentNullException.ThrowIfNull(value, "The value must be non null.");

        var success = false;
        lock (_thisLock)
        {
            if (_items.Remove(value, out var node))
            {
                _fifoList.Remove(node);
                success = true;
            }
        }

        return success;
    }

    public void Clear()
    {
        lock (_thisLock)
        {
            _fifoList.Clear();
            _items.Clear();
        }
    }
}