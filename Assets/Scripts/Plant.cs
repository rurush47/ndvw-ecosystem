using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public void Die()
    {
        if(!GameManager.Instance.deathEnabled) return;
        
        transform.DOScale(Vector3.zero, 1).onComplete += () =>
        {
            Destroy(gameObject);
        };
    }
}
