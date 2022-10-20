using System;
using Engine.Debugging;
using Engine.Input;
using Engine.Objects;

namespace Engine
{
    /// <summary>
    /// Game entrypoint, define all startup logic here. TODO: we could probably abstract most of this away in the simple cases, 
    /// </summary>
    public abstract class GameEntry : BaseMountable
    {

        ILevelContract Level;
        
        /// <summary>
        /// The object (usually a level) that you pin to the engine, events run off of here. 
        /// </summary>
        public ILevelContract PinnedObject
        {
            get => Level;
            set
            {
                Level?.OnLevelUnload();
                Level = value;
                Level?.OnLevelLoad();
            }
        }

        /// <summary>
        /// Called as the game is being created, by this point all engine functions are safe to call
        /// </summary>
        protected internal virtual void GameStart()
        {
            PinnedObject?.OnLevelLoad();
        }
        
        internal void Update(double delta)
        {
            InputHandler.PollInputs();
            PinnedObject?.OnTick((float)delta);

        }
        
        /// <summary>
        /// Calls after the engine has been shut down, any finalization logic can and should be done here! rendering functions are not safe to call here!
        /// </summary>
        protected internal virtual void GameEnded()
        {
            PinnedObject?.OnLevelUnload();
            
            Environment.ExitCode = 0;
        }

        /// <summary>
        /// Logging callbacks should run here, still, it is best to run the engine logging function to write text, as other callbacks can and will be executed
        /// </summary>
        /// <param name="packet">This is the packet of data the logger receives, it contains information on how to handle the message</param>
        public virtual void LogCallback(LogPacket packet)
        {
            
        }

        /// <summary>
        /// Runs on (almost) ANY thrown exception, handled or not, the only exceptions it does not get are corrupted state exceptions. which cannot be caught
        /// </summary>
        public virtual void OnException(Exception ex, bool caught)
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