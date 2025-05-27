using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class WeaponTests
{
    private GameObject weaponObj;
    private Weapon weapon;

    [SetUp]
    public void SetUp()
    {
        weaponObj = new GameObject();
        weapon = weaponObj.AddComponent<Weapon>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(weaponObj);
    }

    [Test]
    public void AmmoPerShot_AlwaysAtLeastOne()
    {
        weapon.AmmoPerShot = 0;
        Assert.GreaterOrEqual(weapon.AmmoPerShot, 1);
    }

    [Test]
    public void MaxAttackDistance_NegativeValue_ClampedToZero()
    {
        weapon.MaxAttackDistance = -10f;
        Assert.AreEqual(0f, weapon.MaxAttackDistance);
    }

    [Test]
    public void Name_SetAndGet()
    {
        weapon.Name = "TestWeapon";
        Assert.AreEqual("TestWeapon", weapon.Name);
    }

    [Test]
    public void Damage_SetAndGet()
    {
        weapon.Damage = 123.4f;
        Assert.AreEqual(123.4f, weapon.Damage);
    }

    [Test]
    public void AttackSpeed_SetAndGet()
    {
        weapon.AttackSpeed = 2.5f;
        Assert.AreEqual(2.5f, weapon.AttackSpeed);
    }

    // ...可根據需求擴充更多測試...
}
