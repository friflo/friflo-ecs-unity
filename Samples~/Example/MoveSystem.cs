using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;

namespace Friflo.Example
{
    public class MoveSystem : QuerySystem<Position>
    {
        public System.Numerics.Vector3 move;
        
        protected override void OnUpdate()
        {
            var delta = Tick.deltaTime;
            foreach (var (positions, entities) in Query.Chunks) {
                for (int n = 0; n < entities.Length; n++) {
                    positions[n].value += delta * move;
                }
            }
        }
    }
}