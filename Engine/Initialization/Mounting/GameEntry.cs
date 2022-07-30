using System;
using System.Data;
using Engine.Debugging;

namespace Engine
{
    /// <summary>
    /// Game entrypoint, define all startup logic here. TODO: we could probably abstract most of this away in the simple cases, 
    /// </summary>
    public abstract class GameEntry
    {
        /// <summary>
        /// Called as the game is being created, by this point all engine functions are safe to call
        /// </summary>
        public virtual void Gamestart()
        {
        }
        
        /// <summary>
        /// Calls after the engine has been shut down, any finalization logic can and should be done here! engine creation and removal functions might not be safe to call here!
        /// </summary>
        public virtual void GameEnded()
        {
        }

        /// <summary>
        /// Logging callbacks should run here, still, it is best to run the engine logging function to write text, as other callbacks can and will be executed
        /// </summary>
        /// <param name="packet">This is the packet of data the logger receives, it contains information on how to handle the packet</param>
        public virtual void LogCallback(LogPacket packet)
        {
            
        }

        /// <summary>
        /// Runs on (almost) ANY thrown exception, handled or not, the only exceptions it does not get are corrupted state exceptions. which cannot be caught
        /// </summary>
        public virtual void OnException(Exception ex)
        {
            ConsoleLibrary.DebugPrint($"{ex.GetType()} thrown at: {ex.StackTrace} with, message:\n {ex.StackTrace}");
        }

        /// <summary>
        /// Runs on unhandled exceptions, the only exceptions it does not get are corrupted state exceptions, which cannot be caught
        /// </summary>
        public virtual void OnUnhandledException(Exception ex)
        {
            
        }
    }
}