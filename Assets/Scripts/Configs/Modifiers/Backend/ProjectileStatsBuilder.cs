using UnityEngine;

public static class ProjectileStatsBuilder
{
    public static ProjectileStats FromConfig(ProjectileConfigSO config)
    {
        return new ProjectileStats
        {
            damage = config.damage,
            speed = config.speed,
            lifetime = config.lifetime,
            destroyOnHit = config.destroyOnHit,
            pierceCount = config.pierceCount
        };
    }
}