using System;
using Engine.Debugging;
using Engine.Input;
using Engine.Objects;
using Engine.Renderable;
using ImGuiNET;

namespace Engine
{
    struct GameMetricsData
    {
        public float tickTime;
        public float renderTime;
    }
    
    
    class GameMetrics : ImGUIPanel
    {

        GameMetricsData _metricsData;

        float _highestTick = 0, _lowestTick = 0;
        float _highestFrameTime = 0, _lowestFrameTime = 0;
        public override void CreateUI()
        {

            _highestTick = Math.Max(_highestTick, _metricsData.tickTime);
            _lowestTick = Math.Min(_highestTick, _metricsData.tickTime);
            
            _highestFrameTime= Math.Max(_highestFrameTime, _metricsData.renderTime);
            _lowestFrameTime = Math.Min(_highestFrameTime, _metricsData.renderTime);
            
            
            ImGui.Text($"Highest Tick time: {_highestTick}");
            ImGui.Text($"Tick time: {_metricsData.tickTime}");
            ImGui.Text($"Lowest Tick time: {_lowestTick}");
            ImGui.Text("\n");
            
            ImGui.Text($"Highest Render time: {_highestTick}");
            ImGui.Text($"Render time: {_metricsData.renderTime}");
            ImGui.Text($"Lowest Render time: {_lowestFrameTime}");
            
        }

        internal void UpdateMetrics( GameMetricsData metricsData)
        {
            _metricsData = metricsData;
        }
    }
    
    
    
    /// <summary>
    /// Game entrypoint, define all startup logic here. TODO: we could probably abstract most of this away in the simple cases, 
    /// </summary>
    public abstract class GameEntry : BaseMountable
    {

        GameMetrics _metrics = new GameMetrics();

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


        float tickTime = 0;
        
        internal void Update(double delta)
        {
            InputHandler.PollInputs();

            tickTime = (float)delta;
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

        protected internal virtual void OnRender(float deltaT)
        {
            _metrics.UpdateMetrics(new GameMetricsData()
            {
                tickTime = tickTime,
                renderTime = deltaT
                
            });
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