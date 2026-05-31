using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendMessage : ThingBase
{
    public GameObject target;

    public void Start()
    {
        OnTriggerEnter2DCallBack += (thing, collider2d) =>
        {
            target.SendMessage("OnTriggerEnter2D", collider2d);
        };

        OnCollisionEnter2DCallBack += (thing, collision2d) =>
        {
            target.SendMessage("OnCollisionEnter2D", collision2d);
        };
    }
}
