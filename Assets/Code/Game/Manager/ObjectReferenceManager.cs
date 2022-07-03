using System.Collections.Generic;
using System;

namespace Endciv
{
    public static class ObjectReferenceManager
    {
        private static HashSet<Guid> AssignedUIDs { get; set; }        

        public static void Initialize()
        {
            if (AssignedUIDs == null)
                AssignedUIDs = new HashSet<Guid>();
            else
                AssignedUIDs.Clear();
        }

        public static void SetRandomEntityGuid(this BaseEntity entity)
        {
            //Remove old reference if existing
            if (entity.UID != Guid.Empty && AssignedUIDs.Contains(entity.UID))
                AssignedUIDs.Remove(entity.UID);
            //Handle collisions
            Guid newGuid = Guid.NewGuid();
            while(AssignedUIDs.Contains(newGuid))
                newGuid = Guid.NewGuid();
            //Register new used UID
            AssignedUIDs.Add(newGuid);
            //add uid to entity
            entity.UID = newGuid;
        }

        public static void SetEntityGuid(this BaseEntity entity, Guid newGuid)
        {
            if (entity.UID == newGuid)
                return;
            //Check for invanlid UID assignment
            if (AssignedUIDs.Contains(newGuid))
                throw new InvalidOperationException("UID " + newGuid + " already in use!");
            //Remove old reference if existing
            if (entity.UID != Guid.Empty && AssignedUIDs.Contains(entity.UID))
                AssignedUIDs.Remove(entity.UID);
            //Register new used UID
            AssignedUIDs.Add(newGuid);
            //add uid to entity
            entity.UID = newGuid;
        }        
    }
}