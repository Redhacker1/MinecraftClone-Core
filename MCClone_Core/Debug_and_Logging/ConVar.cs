using System;
using System.Collections.Generic;

namespace MCClone_Core.Debug_and_Logging
{
    public class Convar
    {
        public string Description; 
        public string HelpMessage;
        readonly bool _canEdit;
        readonly List<Action<string>> _changeNotifications;
        string _variable;
        
        Convar(string name, string desc, string help, string thing, bool readOnly)
        {
            SetVariable(thing);
            _changeNotifications = new List<Action<string>>();
            Description = desc;
            HelpMessage = help;
            _canEdit = readOnly;
            
            ConsoleLibrary.BindConvar(name ,this);
        }
        
        public string GetVariable()
        {
            return _variable;
        }
        
        public void SetVariable(string var)
        {
            if (_canEdit)
            {
                _variable = var;
                        
                // Callbacks to run when variable changed.
                foreach (Action<string> action in _changeNotifications)
                {
                    action(_variable);
                }   
            }
        }

        public void AddListener(Action<string> listener)
        {
            _changeNotifications.Add(listener);
        }
    }
}