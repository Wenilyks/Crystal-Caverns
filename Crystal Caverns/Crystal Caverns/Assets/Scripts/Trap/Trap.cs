using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public class Trap : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == Hero2.Instance.gameObject)
        {
            Hero2.Instance.GetDamage();
        }        
    }

}
