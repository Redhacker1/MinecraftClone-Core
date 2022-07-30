using System;
using System.Reflection;

namespace Engine.ExceptionHandler;
using System.Runtime.ExceptionServices;


public delegate void UnhandledExceptionThrown(Exception ThrownException);  // delegate
public delegate void ExceptionThrown(Exception ThrownException);  // delegate
struct ExceptionEvent
{
    
}


static class GlobalExceptionHandler
{
    
        
    static event Action<Exception> UnhandledExceptionThrown; 


    static void InitHandler()
    {
        
    }
}