using System.Collections.Generic;
using System.Linq;
using LORAP.Playthru;

namespace LORAP.Gameplay.Systems.Goals
{
    internal class ClearGoal : BaseGoal
    {
        public int ClearsRequired { get; set; }
        public List<int> Stages { get; set; } = new List<int>();

        public override bool ConditionCheck()
        {
            return PlaythruManager.ReceptionsCompleted.Where(Stages.Contains).Count() >= ClearsRequired;
        }
    }
}
