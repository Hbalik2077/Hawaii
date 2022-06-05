using UnityEngine;
using System.Collections;

public class RangeAttack : MonoBehaviour
{
    public Transform FirePoint;
    [Tooltip("fire projectile after this delay, useful to sync with the animation of firing action")]
    public float fireDelay;
    public float fireRate;
    public float bulletSpeed = 10;

    [Header("+++BULLET+++")]
    public Projectile Projectile;
     int normalDamage = 30;

    float nextFire = 0;
    Vector2 direction;
    public AudioClip soundAttack;

    public bool Fire(Vector3 _direction)
    {
        if ((GameManager.Instance.isTestLevel ||  GlobalValue.Bullets > 0) && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            if(!GameManager.Instance.isTestLevel)       //only subtract the bullet if not in test mode
                GlobalValue.Bullets--;
            direction = _direction;
            StartCoroutine(DelayAttack(fireDelay, false));
            return true;
        }
        else
            return false;
    }

    IEnumerator DelayAttack(float time, bool powerBullet)
    {
        yield return new WaitForSeconds(time);

        Vector2 firePoint = FirePoint.position;
      
        var projectile = Instantiate(Projectile.gameObject, firePoint, Projectile.gameObject.transform.rotation).GetComponent<Projectile>();
        projectile.Initialize(gameObject, direction, Vector2.zero, powerBullet, false, normalDamage, bulletSpeed);
        projectile.gameObject.SetActive(true);

        SoundManager.PlaySfx(soundAttack);
    }
}
