public class OnAmmoLooted : BroadcastEvent<OnAmmoLooted>
{
    public readonly int ammoAmount;

    public OnAmmoLooted(int ammoAmount)
    {
        this.ammoAmount = ammoAmount;
    }
}
