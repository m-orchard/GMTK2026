using System.Collections;

// Implement this on any system that needs to pause the game between the
// camera returning to build framing and the belt/timer actually starting
// (e.g. tutorials, an upcoming piece-selection screen). Register/unregister
// with GameManager.RegisterPreBuildGate / UnregisterPreBuildGate, typically
// from OnEnable/OnDisable.
public interface IPreBuildGate
{
    // Lower runs first. Gates run strictly in order, one at a time.
    int Order { get; }

    IEnumerator WaitUntilReady(int level);
}
