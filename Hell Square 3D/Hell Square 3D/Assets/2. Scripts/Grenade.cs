using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public GameObject meshObject;
    public GameObject effectObject;
    public Rigidbody rigid;
    public AudioSource explosionAudio;

    // Start is called before the first frame update
    void Awake()
    {
        StartCoroutine(Explosion());
    }

    IEnumerator Explosion()
    {
        yield return new WaitForSeconds(2f);
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        meshObject.SetActive(false);
        effectObject.SetActive(true);

        RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, 10, Vector3.up, 0, LayerMask.GetMask("Enemy"));

        foreach(RaycastHit hitObject in rayHits)
        {
            hitObject.transform.GetComponent<Enemy>().HitByGrenade(transform.position);
        }
        explosionAudio.Play();
        Destroy(gameObject, 5);
    }
}
