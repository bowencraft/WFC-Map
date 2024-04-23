using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperPosition
{
    List<int> _possibleValues = new List<int>();

    public List<int> PossibleValues => _possibleValues;

    public GameObject gridObject;

    bool _observed = false;
    
    int currentCount = 0;

    public SuperPosition(int maxValue)
    {
        for (int i = 0; i < maxValue; i++)
        {
            _possibleValues.Add(i);
        }
    }
    
    public SuperPosition(SuperPosition other)
    {
        _possibleValues = new List<int>(other._possibleValues);
        _observed = other._observed;
    }

    public int GetObservedValue()
    {
        return _possibleValues[0];
    }
    
    public int GetCurrentValue()
    {
        return currentCount;
    }

    public void SetCurrentValue(int value)
    {
        currentCount = value;
    }
    
    public int SelectOption()
    {
        if (_observed || _possibleValues.Count == 0)
        {
            Debug.LogWarning("Cell already observed or no options left");
            return -1; // Indicates an error or that the cell is already observed
        }

        // Select a random index from the possible values
        int randomIndex = Random.Range(0, _possibleValues.Count);
        int observedValue = _possibleValues[randomIndex];

        // Clear the list and add back only the observed value
        // _possibleValues.Clear();
        // _possibleValues.Add(observedValue);

        // // Mark as observed
        // _observed = true;

        return observedValue;
    }
    
    public void SetObserved(bool observed)
    {
        _observed = observed;
    }


    public bool IsObserved()
    {
        return _observed;
    }

    public void RemovePossibleValue(int value)
    {
        _possibleValues.Remove(value);
    }

    public int NumOptions{
        get
        {
            return _possibleValues.Count;
        }
    }

}
