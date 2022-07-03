using System;
using System.Collections.Generic;

namespace Endciv
{
    [Serializable]
    public sealed class TaskSaveData : ISaveable
    {
        public StateMachineSaveData stateMachineSaveData;
        public int currentState;
        public object currentAction;
        public Dictionary<string, object> globalMembers;
        public Type taskType;
        public string unitUID;

        public ISaveable CollectData()
        {
            return this;
        }
        
        public AITask ToTask(Type type)
        {
            if (!type.IsSubclassOf(typeof(AITask)))
                return null;
            var task = Activator.CreateInstance(type) as AITask;
            task.ApplySaveData(this);
            return task;
        }
    }
}