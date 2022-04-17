using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Cache;
using GameOffsets;

namespace ExileCore.PoEMemory.Components
{
    public class DeployedObject : RemoteMemoryObject
    {
        private readonly FrameCache<ActorDeployedObject> cacheValue;
        private Entity _entity;
        public DeployedObject()
        {
            cacheValue = new FrameCache<ActorDeployedObject>(() => M.Read<ActorDeployedObject>(Address));
        }

        private ActorDeployedObject Struct => cacheValue.Value;
        public ushort ObjectId => Struct.ObjectId;
        public ushort SkillKey => Struct.SkillId;

        public Entity Entity => _entity = _entity ?? EntityListWrapper.GetEntityById(ObjectId);
        //public ActorSkill Skill => ObjectManager.Instance.GameController.Player.GetComponent<Actor>().ActorSkills.Find(x => x.Id == ObjectKey);
    }
}