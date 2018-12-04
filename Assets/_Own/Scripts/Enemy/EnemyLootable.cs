using UnityEngine;
using UnityEngine.Assertions;

public class EnemyLootable : MonoBehaviour
{
    [SerializeField] GameObject lootIndicatorPrefab;
    [SerializeField] int minAmmo = 1;
    [SerializeField] int maxAmmo = 3;
    
    public bool isLooted { get; private set; }
    public EnemyLootIndicator lootIndicator { get; private set; }
    
    private void Start() => Assert.IsNotNull(lootIndicatorPrefab);

    public void ShowIndicator()
    {
        Assert.IsNull(lootIndicator);
        
        lootIndicator = CanvasObjectBuilder.CreateAndAddObjectToCanvas(lootIndicatorPrefab)?.GetComponent<EnemyLootIndicator>();
        lootIndicator.SetTrackedRenderer(GetComponentInChildren<Renderer>());
    }
    
    public void Loot()
    {
        int ammo      = Random.Range(minAmmo, maxAmmo);
        isLooted      = true;
        if(lootIndicator != null)
            lootIndicator.DoPopUp(ammo);
        lootIndicator = null;

        new OnAmmoLooted(ammo).PostEvent();
    }
}
