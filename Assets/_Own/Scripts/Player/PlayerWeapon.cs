﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerWeapon : MyBehaviour, IEventReceiver<OnAmmoLooted>
{
    [SerializeField] int baseClipSize = 5;
    [SerializeField] int maxMagazines = 3;
    [SerializeField] int startingNumberOfMagazines = 2;
    [SerializeField] float reloadAnimationLength = 2f;
    [SerializeField] string reloadWeaponKey = "Reload";

    public class Magazine
    {
        public int ammoCount { get; set; }
        public int clipSize { get; }

        public Magazine(int clipSize)
        {
            this.clipSize = clipSize;
            ammoCount = clipSize;
        }
    }
    
    /********* PUBLIC *********/
    public delegate void OnAmmoChangedHandler(int newValue);
    public event OnAmmoChangedHandler OnAmmoChanged;
    
    /********* PRIVATE *********/
    private bool isReloading;

    private Magazine currentMagazine;    
    private List<Magazine> magazines = new List<Magazine>();  
    private PlayerShootingController shootingController;
    
    private void Start()
    {
        currentMagazine = new Magazine(baseClipSize);

        for (int i = 0; i < startingNumberOfMagazines; i++)
            if (magazines.Count <= maxMagazines)
                magazines.Add(new Magazine(baseClipSize));
    }

    private void Update()
    {
        if (!isReloading && Input.GetButtonDown(reloadWeaponKey))
            Reload();
    }

    private void Reload()
    {
        // If there are no magazines left don't do anything
        if (magazines.Count == 0)
            return;

        // If the current magazine is full don't do anything either
        if (currentMagazine.ammoCount == currentMagazine.clipSize)
            return;

        // Otherwise calculate how many ammo is left in the current magazine and get it from the other one
        // this way no ammo is wasted
        int ammo = 0;
        int ammoLeft = currentMagazine.ammoCount;
        int ammoNeeded = currentMagazine.clipSize - ammoLeft;

        List<Magazine> magazinesToRemove = new List<Magazine>();

        for (int i = 0; i < magazines.Count; i++)
        {
            if (currentMagazine.ammoCount + ammo >= currentMagazine.clipSize)
                break;

            Magazine magazine = magazines[i];
            int newAmmoCount = magazine.ammoCount - ammoNeeded;

            if (newAmmoCount >= 0)
            {
                ammo                += ammoNeeded;
                magazine.ammoCount = newAmmoCount;

                if (newAmmoCount == 0)
                    magazinesToRemove.Add(magazine);
            }
            else
            {
                ammo       += magazine.ammoCount;
                ammoNeeded -= magazine.ammoCount;
                
                magazinesToRemove.Add(magazine);
            }
        }

        foreach (Magazine magazine in magazinesToRemove)
            magazines.Remove(magazine);

        // TODO: Play reloading animation

        isReloading = true;
        this.Delay(reloadAnimationLength, () =>
        {
            isReloading                = false;
            currentMagazine.ammoCount += ammo;
            OnAmmoChanged?.Invoke(currentMagazine.ammoCount);
        });
    }

    public void DeductAmmo()
    {       
        currentMagazine.ammoCount--;

        if (currentMagazine.ammoCount < 0)
        {
            currentMagazine.ammoCount = 0;
            Reload();
        }

        OnAmmoChanged?.Invoke(currentMagazine.ammoCount);
    }

    public void IncreaseAmmo(int increment)
    {
        int newAmount = currentMagazine.ammoCount + increment;

        if (newAmount > currentMagazine.clipSize)
        {
            currentMagazine.ammoCount = currentMagazine.clipSize;
            int ammoLeftover          = newAmount - currentMagazine.clipSize;

            for (int i = 0; i < magazines.Count; i++)
            {
                if (ammoLeftover <= 0)
                    break;

                Magazine magazine = magazines[i];
                if(magazine == null)
                    continue;

                // magazine is already full so continue
                if(magazine.ammoCount == magazine.clipSize)
                    continue;

                int newMagazineAmount = magazine.ammoCount + ammoLeftover;

                if (newMagazineAmount > magazine.clipSize)
                {
                    magazine.ammoCount = magazine.clipSize;
                    ammoLeftover = newMagazineAmount - magazine.clipSize;
                }
                else magazine.ammoCount = newMagazineAmount;
            }

            // Put the remaining ammo into a new clip
            if (ammoLeftover > 0 && magazines.Count < maxMagazines)
            {
                Magazine magazine = new Magazine(baseClipSize);
                magazine.ammoCount = Mathf.Clamp(ammoLeftover, 0, magazine.clipSize);
                magazines.Add(magazine);
            }
        }
        else currentMagazine.ammoCount = newAmount;
        
        OnAmmoChanged?.Invoke(currentMagazine.ammoCount);
    }
    
    public bool CanShootOrZoomIn()
    {
        return currentMagazine != null && currentMagazine.ammoCount > 0 &&
            !isReloading;
    }

    public void On(OnAmmoLooted loot) => IncreaseAmmo(loot.ammoAmount);
}