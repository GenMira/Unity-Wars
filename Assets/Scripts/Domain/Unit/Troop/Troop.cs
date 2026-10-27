using R3;

public class Troop : ITroop
{
    public string Id{get;private set;}
    public TroopType Type{get;private set;}
    public PlayerSide Owner{get;private set;}

    public int MaxHealth{get;private set;}
    public int MaxMovementRange{get;private set;}
    public TroopType[] MountableType{get;private set;}
    public int MountableAmount{get;private set;}
    
    protected readonly ReactiveProperty<int> _currentHealth;
    public ReadOnlyReactiveProperty<int>  CurrentHealth => _currentHealth;
    protected readonly ReactiveProperty<int> _currentMovementRange;
    public ReadOnlyReactiveProperty<int>  CurrentMovementRange =>_currentMovementRange;
    public bool IsAlreadyMoved{get;protected set;}


    protected Troop(
        TroopType type,
        PlayerSide owner,
        int maxHealth,
        int movementRange,
        TroopType[] mountableType = null,
        int mountableAmount = 0)
    {
        Type = type;
        Id = NameSolver.TroopName(this.Type);
        Owner = owner;
        MaxHealth = maxHealth;
        _currentHealth = new ReactiveProperty<int>(maxHealth);
        MaxMovementRange = movementRange;
        _currentMovementRange = new ReactiveProperty<int>(MaxMovementRange);
        MountableType = mountableType ?? System.Array.Empty<TroopType>();
        MountableAmount = mountableAmount;
    }

    public void Reset()
    {
        _currentMovementRange.Value = MaxMovementRange;
        IsAlreadyMoved = false;   
    }

    public virtual int Attack(int type)
    {
        throw new System.NotImplementedException();
    }   

    public virtual int GetAttackRange(int type)
    {
        throw new System.NotImplementedException();
    }

    public void recieveDamage(int damage)
    {
        if (_currentHealth.Value < damage)
        {
            _currentHealth.Value = 0;
        }
        else
        {
            _currentHealth.Value -= damage;
        }
    }

}