﻿using System;

namespace Engine.ExceptionHandler;

public delegate void UnhandledExceptionThrown(Exception ThrownException);  // delegate
public delegate void ExceptionThrown(Exception ThrownException);  // delegate
struct ExceptionEvent
{
    
}


static class GlobalExceptionHandler
{
    
        
    static event Action<Exception> UnhandledExceptionThrown; 
    
    static event Action<Exception, bool> ExceptionThrown;
    
    static event Action<Exception> HandledExceptionThrown;


    static void InitHandler()
    {
        
    }
}