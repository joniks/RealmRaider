using System.Collections.Generic;
using UnityEngine;

namespace RealmRaiders.Controllers
{
    public static class GameplayInput
    {
        static readonly HashSet<int> uiPointers = new();
        static readonly HashSet<int> directControllers = new();
        public static Vector2 Movement { get; private set; }
        public static bool DirectControlActive => directControllers.Count > 0;
        public static bool TerminalState { get; private set; }
        public static void SetTerminalState(bool terminal) { TerminalState = terminal; if (terminal) ClearMovement(); }
        public static void SetDirectControl(int controllerId, bool active) { if (active) directControllers.Add(controllerId); else directControllers.Remove(controllerId); if (!DirectControlActive) ClearMovement(); }
        public static void SetMovement(Vector2 value) => Movement = Vector2.ClampMagnitude(value, 1);
        public static void ClearMovement() => Movement = Vector2.zero;
        public static int InteractionRevision { get; private set; }
        public static void ResetTransientInput() { ClearMovement(); ClearPointers(); InteractionRevision++; }
        public static void ClaimUiPointer(int id) => uiPointers.Add(id);
        public static void ReleaseUiPointer(int id) => uiPointers.Remove(id);
        public static bool IsUiOwned(int id) => uiPointers.Contains(id);
        public static bool HasUiOwnership => uiPointers.Count > 0;
        public static void ClearPointers() { uiPointers.Clear(); ClearMovement(); }
        public static void ResetForTests() { uiPointers.Clear(); directControllers.Clear(); ClearMovement(); TerminalState = false; }
    }
}
