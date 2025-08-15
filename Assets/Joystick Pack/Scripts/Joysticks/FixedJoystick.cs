using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // ← PointerEventData を使うため追加

public class FixedJoystick : Joystick
{
    public int CurrentFingerId { get; private set; } = -1; // 今触っている指のID（なければ -1）

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        CurrentFingerId = eventData.pointerId;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        CurrentFingerId = -1;
    }
}
