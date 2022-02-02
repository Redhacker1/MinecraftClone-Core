using Engine.Objects;

namespace Engine
{
    public abstract class Game
    {
    
        public virtual void Gamestart()
        {
        }
        
        public virtual void GameEnded()
        {
        }
        
        double _physicsDelta;
        internal void Update(double delta)
        {
            
            _physicsDelta += delta;

            bool physicsProcess = _physicsDelta >= 0.0166666;

            for (int  index = 0;  index < GameObject.Objects.Count; index++)
            {
                GameObject gameObject = GameObject.Objects[index];
                if(gameObject?.Started == false)
                {
                    gameObject._Ready();
                    gameObject.Started = true;
                }

                if (gameObject != null)
                {
                    gameObject._Process(delta);
                    if (physicsProcess)
                    {
                        gameObject._PhysicsProcess(_physicsDelta);
                    }
                }
            }

            if (physicsProcess)
            {
                _physicsDelta = 0;
            }
        }
    }
}