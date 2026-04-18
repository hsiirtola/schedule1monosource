namespace ScheduleOne.Combat;

public struct ExplosionData(float damageRadius, float maxDamage, float maxPushForce, bool checkLoS, EExplosionType explosionType = EExplosionType.Default)
{
	public float DamageRadius = damageRadius;

	public float MaxDamage = maxDamage;

	public float PushForceRadius = damageRadius * 2f;

	public float MaxPushForce = maxPushForce;

	public bool CheckLoS = checkLoS;

	public EExplosionType ExplosionType = explosionType;

	public static readonly ExplosionData DefaultSmall = new ExplosionData(6f, 200f, 500f, checkLoS: true);

	public static readonly ExplosionData LightningStrike = new ExplosionData(5f, 200f, 500f, checkLoS: true, EExplosionType.Lightning);
}
