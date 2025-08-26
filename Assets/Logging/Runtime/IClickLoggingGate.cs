using UnityEngine;

/// <summary>
/// Gate that lets a component veto logging for the current click.
/// If any gate on the same GameObject returns false, the ButtonLoggerConnector will skip logging.
/// </summary>
public interface IClickLoggingGate
{
    bool CanLogClick();
}