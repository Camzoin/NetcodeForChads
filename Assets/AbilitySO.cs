using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "ScriptableObjects/Ability", order = 1)]
public class AbilitySO : ScriptableObject
{
    public int abilityID = 0;

    public float projectileSpeed = 1;
    public float damage = 3;
    public float castDelay = 0;
    public float chargeTime = 0;
    public float cooldown = 1;
    public float manaCost = 0;
    public float stamCost = 0;
    public float timeLookingAfterCast = 0.05f;
    public float abiltiyScale = 1;
    public float knockBackTime = 0.2f;
    public float knockBackVelocity = 50;

    public bool followPlayer = false;
    public bool breaksOnContact = true;

    public bool canBeChannelled = true;

    public float impactAnimationSpeed = 0.4f;

    public string abilityDescrption = "";

    public GameObject spellPrefab;
    public Texture2D abilityIcon;
    public Material abilityIconMat;
}