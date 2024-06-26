namespace ProjectGra
{
    public enum EntityState
    {
        SpawnEffect,
        Init,
        Follow,
        Dead,

        // Normal Melee
        MeleeAttack,

        // Normal Sprint
        SprintAttack,

        // Normal Ranged
        RangedAttack,
        Flee,

        // Egg
        EggHatching,

        // Summoner
        Summon,
        ChasingToSelfDestruct,
        Explode,

        // Elite
        DoingSkill,
        StageOne,
        StageTwo,
    }
}