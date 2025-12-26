using UnityEngine;

public class EquipSlotPresenter
{
    private Player _model;          // 모델 (플레이어)
    private EquipSlotView _view;    // 뷰 (무기 장착 슬롯)

    public EquipSlotPresenter(Player model, EquipSlotView view)
    {
        _model = model;
        _view = view;


        // 모델(플레이어)을 담당한 담당자와 상의 후 Action 연결하기
        // 1. 1, 2번 키보드 키를 통해 프레임 색 변경
        // 2. 무기 획득 시 서브 슬롯의 아이콘 변경
        // 3. 무기로 몬스터를 공격 시 내구도 바 수치 변경
    }
}
