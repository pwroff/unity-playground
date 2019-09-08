using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CharactersProperties
{
    public static Vector3 CursorAt { get; private set; }
    public static void SetCursorAt(Vector3 cp)
    {
        CursorAt = cp;
    }
    public static Vector3 NextVelocity { get; private set; }
    public static void SetNextVelocity(Vector3 cp)
    {
        NextVelocity = cp;
    }
}
