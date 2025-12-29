using UnityEngine;
using System.Reflection;

/// <summary>
/// 상자방에서 사용하는 Chest 클래스
/// Chest를 상속받아서 ChestRoom에서 상자를 열었을 때 다른 상자의 상호작용을 막는 기능 추가
/// </summary>
public class ChestForEventRoom : Chest
{
    private ChestRoom chestRoom;
    private int chestIndex;
    private bool canInteract = true; // 상호작용 가능 여부
    
    /// <summary>
    /// ChestRoom에서 초기화할 때 호출
    /// </summary>
    public void InitializeForEventRoom(ChestRoom room, int index)
    {
        chestRoom = room;
        chestIndex = index;
    }
    
    /// <summary>
    /// 상호작용 가능 여부를 설정합니다.
    /// </summary>
    public void SetCanInteract(bool value)
    {
        canInteract = value;
    }

    /// <summary>
    /// OnInteract 오버라이드 - ChestRoom 로직 추가 후 부모의 OnInteract 호출
    /// Chest의 OnInteract 호출 전에 ChestRoom의 OnChestInteracted 호출 (상속으로 구현, Chest에 virtual 추가했습니다.)
    /// </summary>
    public override void OnInteract()
    {
        Debug.Log($"[ChestForEventRoom] OnInteract 호출됨 - chestIndex: {chestIndex}, canInteract: {canInteract}");
        
        // 상호작용 불가능하면 반환
        if (!canInteract)
        {
            Debug.Log($"[ChestForEventRoom] 상호작용 불가능 - canInteract가 false입니다.");
            return;
        }
        
        // ChestRoom이 있으면 다른 상자 비활성화
        if (chestRoom != null)
        {
            Debug.Log($"[ChestForEventRoom] ChestRoom.OnChestInteracted 호출 - chestIndex: {chestIndex}");
            chestRoom.OnChestInteracted(chestIndex);
        }
        else
        {
            Debug.LogWarning($"[ChestForEventRoom] chestRoom이 null입니다.");
        }
        
        // 기존 Chest의 OnInteract 호출
        Debug.Log($"[ChestForEventRoom] base.OnInteract() 호출");
        base.OnInteract();
    }

    /// <summary>
    /// OnTriggerEnter2D 오버라이드 - ChestRoom 상태를 확인하여 상호작용 가능 여부 체크
    /// canInteract가 false인 상자(다른 상자)는 상호작용 불가능하게 하고,
    /// 이미 열린 상자(OpenedLeft)는 여전히 상호작용 가능하도록 허용
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // canInteract가 false면 상호작용 불가능 (다른 상자)
        if (!canInteract)
        {
            return;
        }

        // 기존 Chest의 OnTriggerEnter2D 로직 (chestState 체크 포함)
        if (collision.CompareTag("Player"))
        {
            // 리플렉션을 사용하여 chestState 확인
            FieldInfo stateField = typeof(Chest).GetField("chestState", BindingFlags.NonPublic | BindingFlags.Instance);
            if (stateField != null)
            {
                object stateValue = stateField.GetValue(this);
                if (stateValue != null && stateValue.ToString() == "OpenedTaken")
                {
                    // 이미 무기를 가져간 상자는 상호작용 불가
                    return;
                }
            }

            // canInteract가 true이고 OpenedTaken이 아니면 상호작용 가능
            // (Closed 상태: 첫 상호작용, OpenedLeft 상태: 무기를 다시 꺼낼 수 있음)
            GameManager.Instance.InteractObj = this;
        }
    }
    
    /// <summary>
    /// OnTriggerExit2D 오버라이드 - 기존 Chest 로직과 동일
    /// </summary>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (GameManager.Instance.InteractObj == this)
            {
                GameManager.Instance.InteractObj = null;
            }
        }
    }
}

