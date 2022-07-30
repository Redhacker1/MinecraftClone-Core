using System.Numerics;

namespace Engine.Debugging;


public enum LogLevel : uint
{
    None = 1,
    Input = 2,
    Debug = 4,
    Info = 8,
    Warning = 16,
    Error = 32,
    Critical = 64
}

public struct LogPacket
{
    LogLevel priority;
    string LogMessage;
    Vector4 colorData;
}