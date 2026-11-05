public enum DamageType
{
    PhysDamage = 0,
    EnergyDamage = 1
}

public interface IDamageable
{
    // ƒ_ƒ[ƒW‚ğó‚¯æ‚éŠÖ”
    void TakeDamage(int Damage, DamageType damageType);
}

