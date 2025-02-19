namespace LORAP.Gameplay.Systems.Goals
{
    internal abstract class BaseGoal
    {
        public string Name { get; set; }
        internal bool Active { get; set; } = false;
        public bool Completed => ConditionCheck();

        public virtual bool ConditionCheck()
        {
            return false;
        }
    }
}
