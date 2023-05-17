using System;

namespace Engine.Attributes;

[AttributeUsage(AttributeTargets.Struct)]
public class ExceptionEventAttribute : Attribute
{
    
}



/// <summary>
/// Declares to the engine the game, from the assembly to actually start up. Will likely be responsible for cosmetics of game
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class GameDefinition : Attribute
{

    string GameName;
    bool Testing;
    public GameDefinition(string Name, bool test)
    {
        GameName = Name;
        Testing = test;
    }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class ConCommand : Attribute
{
    public ConCommand(string commandName, string Desc, string Usage)
    {
        
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class ConVar : Attribute
{
    public ConVar(string Alias)
    {
        
    }
}

