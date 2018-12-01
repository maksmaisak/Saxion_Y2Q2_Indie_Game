using UnityEngine;

public static class CursorHelper
{
    public static void SetLock(bool newIsLocked)
    {
        if (newIsLocked) Lock();
        else Unlock();
    }

    public static void Unlock()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public static void Lock()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}