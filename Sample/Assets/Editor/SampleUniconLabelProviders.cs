using System;
using Unicon;
using UnityEngine;

// Example IUniconLabelWrappable implementations for the Unicon sample project.
// Select one in Edit > Preferences > Unicon > Badge Text Source.
public class FixedLabelProvider : IUniconLabelWrappable
{
    public string GetLabel()
    {
        return "SMP";
    }
}

public class UnityVersionLabelProvider : IUniconLabelWrappable
{
    public string GetLabel()
    {
        // e.g. "6000" from "6000.2.9f1"
        return Application.unityVersion.Split('.')[0];
    }
}

public class DateTimeLabelProvider : IUniconLabelWrappable
{
    public string GetLabel()
    {
        return DateTime.Today.ToString();
    }
}