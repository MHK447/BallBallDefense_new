using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class TileUnlockChecker : MonoBehaviour
{
    private TileWeaponGroup TileWeaponGroup;

    private TileComponent TargetTileComponent;

    private TileAddComponent DraggingTileAddComponent;

    private bool IsDragging = false;

    private bool CanUnlock = false;

    private List<TileComponent> TemporarilyActivatedTiles = new List<TileComponent>();

    private List<TileComponent> CurrentAdjacentTiles = new List<TileComponent>();

    private TileMergeChecker TileMergeChecker;

    // 다른 스크립트가 드래깅 중인지 확인할 수 있는 public 프로퍼티
    public bool IsCurrentlyDragging => IsDragging;

    public void Init()
    {
        TileWeaponGroup = GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup;
        TileMergeChecker = GetComponent<TileMergeChecker>();

        DraggingTileAddComponent = null;
        TargetTileComponent = null;
        IsDragging = false;
        CanUnlock = false;
        TemporarilyActivatedTiles.Clear();
        CurrentAdjacentTiles.Clear();
    }

    private void Update()
    {
        // TileMergeChecker가 드래깅 중이면 이 스크립트는 작동하지 않음
        if (TileMergeChecker != null && TileMergeChecker.IsCurrentlyDragging)
        {
            return;
        }

        // 1. 클릭/터치 시작
        if (Input.GetMouseButtonDown(0))
        {
            DraggingTileAddComponent = null;
            CheckInitialTileAdd();
        }

        // 2. 클릭/터치 유지 중 - DraggingTileAddComponent가 있을 때만 TileComponent 감지
        if (Input.GetMouseButton(0) && DraggingTileAddComponent != null)
        {
            CheckTileComponentAtPosition();
        }

        // 3. 클릭/터치 종료
        if (Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }

        MoveDragToTouchPos();
    }

    private void CheckInitialTileAdd()
    {
        if (!GameRoot.Instance.UserData.InGamePlayerData.IsWaveRestProperty.Value) return;

        // UI EventSystem으로 감지 (UI Only)
        if (EventSystem.current != null)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                // TileAddComponent 찾기
                var tileAdd = result.gameObject.GetComponent<TileAddComponent>();
                if (tileAdd == null) tileAdd = result.gameObject.GetComponentInParent<TileAddComponent>();

                if (tileAdd != null)
                {
                    DraggingTileAddComponent = tileAdd;
                    IsDragging = true;

                    // 드래그 시작 시 맨 위로 올리기
                    DraggingTileAddComponent.transform.SetAsLastSibling();

                    SoundPlayer.Instance.PlaySound("sfx_get_equip_weapon");

                    // 잠긴 타일들을 임시로 활성화
                    ShowLockedTiles();

                    TileWeaponGroup.SortQueueTileWeapon();
                    break;
                }
            }
        }

        if (DraggingTileAddComponent == null)
        {
            Debug.Log("TileAddComponent를 찾을 수 없습니다");
        }
    }

    private void CheckTileComponentAtPosition()
    {
        if (!GameRoot.Instance.UserData.InGamePlayerData.IsWaveRestProperty.Value) return;

        // UI EventSystem으로 감지 (UI Only)
        if (EventSystem.current != null)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            // 터치 위치에 weapon_offset을 뺀 값을 기준점으로 사용
            Vector2 checkPosition = Input.mousePosition;
            if (DraggingTileAddComponent != null)
            {
                var td = Tables.Instance.GetTable<EquipInfo>().GetData(DraggingTileAddComponent.EquipIdx);
                if (td != null)
                {
                    if (td.weapon_offset != null && td.weapon_offset.Count >= 2)
                    {
                        Vector3 offsetPos = new Vector3(td.weapon_offset[0], td.weapon_offset[1], 0f);
                        checkPosition -= (Vector2)offsetPos;
                    }
                    // noneequip_ypos를 터치 위치에 적용 (TileAddComponent는 항상 장착되지 않은 상태)
                    checkPosition.y += td.noneequip_ypos + 20;
                }
            }
            pointerData.position = checkPosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            bool foundTarget = false;

            foreach (var result in results)
            {
                // TileComponent 찾기 (잠겨있는 타일과 열린 타일 모두)
                var tileComponent = result.gameObject.GetComponent<TileComponent>();
                if (tileComponent == null) tileComponent = result.gameObject.GetComponentInParent<TileComponent>();

                if (tileComponent != null)
                {
                    ResetTileColor();
                    TargetTileComponent = tileComponent;

                    // 모든 타일에 대해 인접 타일 체크 (열려있는 타일도 포함)
                    bool canUnlockTiles = CheckAdjacentTiles(tileComponent);

                    // 검사한 인접 타일들 가져오기
                    CurrentAdjacentTiles = GetAdjacentTiles(tileComponent);

                    if (canUnlockTiles)
                    {
                        // 초록색으로 변경 (인접 타일 중 하나라도 잠금 해제 가능)
                        // 현재 타일이 열려있으면 색상 표시 안함
                        if (!tileComponent.IsUnLock)
                        {
                            tileComponent.TileColorChange(Config.Instance.GetImageColor("TileGreen_Color"));
                        }

                        // 인접 타일들도 초록색으로 변경 (잠긴 타일만)
                        foreach (var adjacentTile in CurrentAdjacentTiles)
                        {
                            if (!adjacentTile.IsUnLock)
                            {
                                adjacentTile.TileColorChange(Config.Instance.GetImageColor("TileGreen_Color"));
                            }
                        }

                        CanUnlock = true;
                        Debug.Log($"TileComponent 감지 (해제 가능): {tileComponent.name}");
                    }
                    else
                    {
                        // 빨강색으로 변경 (잠금 해제 불가)
                        // 현재 타일이 열려있으면 색상 표시 안함
                        if (!tileComponent.IsUnLock)
                        {
                            tileComponent.TileColorChange(Config.Instance.GetImageColor("TileRed_Color"));
                        }

                        // 인접 타일들도 빨강색으로 변경 (잠긴 타일만)
                        foreach (var adjacentTile in CurrentAdjacentTiles)
                        {
                            if (!adjacentTile.IsUnLock)
                            {
                                adjacentTile.TileColorChange(Config.Instance.GetImageColor("TileRed_Color"));
                            }
                        }

                        CanUnlock = false;
                        Debug.Log($"TileComponent 감지 (해제 불가): {tileComponent.name}");
                    }

                    foundTarget = true;
                    break;
                }
            }

            if (!foundTarget)
            {
                ResetTileColor();
            }
        }
    }

    private bool CheckAdjacentTiles(TileComponent baseTileComponent)
    {
        if (DraggingTileAddComponent == null) return false;

        int equipIdx = DraggingTileAddComponent.EquipIdx;
        int listIndex = equipIdx - 1001;

        // TileUnLockCheckList 인덱스 범위 체크
        if (listIndex < 0 || listIndex >= GameRoot.Instance.TileSystem.TileUnLockCheckList.Count)
        {
            Debug.LogWarning($"Invalid TileUnLockCheckList index: {listIndex} (EquipIdx: {equipIdx})");
            return false;
        }

        List<Vector2> checkPositions = GameRoot.Instance.TileSystem.TileUnLockCheckList[listIndex];

        bool hasLockedTile = false; // 잠긴 타일이 하나라도 있는지 체크

        // 인접 타일 중 하나라도 잠겨있으면 장착 가능
        foreach (Vector2 offset in checkPositions)
        {
            Vector2 targetPosition = baseTileComponent.TileOrderVec + offset;
            TileComponent adjacentTile = TileWeaponGroup.GetTileComponent(targetPosition, false);

            // 인접 타일이 없으면 실패
            if (adjacentTile == null)
            {
                Debug.Log($"인접 타일 없음: {targetPosition}");
                return false;
            }

            // 잠긴 타일이 하나라도 있으면 체크
            if (!adjacentTile.IsUnLock)
            {
                hasLockedTile = true;
            }
        }

        return hasLockedTile; // 잠긴 타일이 하나라도 있으면 true
    }

    private List<TileComponent> GetAdjacentTiles(TileComponent baseTileComponent)
    {
        List<TileComponent> adjacentTiles = new List<TileComponent>();

        if (DraggingTileAddComponent == null) return adjacentTiles;

        int equipIdx = DraggingTileAddComponent.EquipIdx;
        int listIndex = equipIdx - 1001;

        if (listIndex < 0 || listIndex >= GameRoot.Instance.TileSystem.TileUnLockCheckList.Count)
        {
            return adjacentTiles;
        }

        List<Vector2> checkPositions = GameRoot.Instance.TileSystem.TileUnLockCheckList[listIndex];

        // 인접 타일들 수집 (잠긴 타일과 열린 타일 모두 포함)
        foreach (Vector2 offset in checkPositions)
        {
            Vector2 targetPosition = baseTileComponent.TileOrderVec + offset;
            TileComponent adjacentTile = TileWeaponGroup.GetTileComponent(targetPosition, false);

            if (adjacentTile != null && adjacentTile != baseTileComponent)
            {
                adjacentTiles.Add(adjacentTile);
                Debug.Log($"인접 타일 추가: {adjacentTile.name} (IsUnLock: {adjacentTile.IsUnLock})");
            }
        }

        return adjacentTiles;
    }

    private void MoveDragToTouchPos()
    {
        if (DraggingTileAddComponent == null || !IsDragging) return;

        // 타겟의 부모 RectTransform 기준 (로컬 좌표 변환용)
        RectTransform parentRect = DraggingTileAddComponent.transform.parent as RectTransform;
        if (parentRect == null) return;

        // 캔버스의 렌더 모드 확인 (Overlay vs Camera)
        Canvas canvas = DraggingTileAddComponent.GetComponentInParent<Canvas>();
        Camera uiCamera = null;

        // Screen Space - Camera 혹은 World Space일 경우 카메라 필요
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
        }

        // 화면 좌표(마우스/터치)를 월드 좌표로 변환하여 UI 이동
        Vector3 globalMousePos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(parentRect, Input.mousePosition, uiCamera, out globalMousePos))
        {
            Vector3 offsetPos = Vector3.zero;
            var td = Tables.Instance.GetTable<EquipInfo>().GetData(DraggingTileAddComponent.EquipIdx);
            if (td != null)
            {
                if (td.weapon_offset != null && td.weapon_offset.Count >= 2)
                {
                    offsetPos = new Vector3(td.weapon_offset[0], td.weapon_offset[1], 0f);
                }
                // noneequip_ypos를 Y축에 적용 (TileAddComponent는 항상 장착되지 않은 상태, 부모의 로컬 스케일 고려)
                float yOffset = td.noneequip_ypos * (parentRect != null ? parentRect.lossyScale.y : 1f);
                offsetPos += new Vector3(0, -yOffset, 0);
            }
            DraggingTileAddComponent.transform.position = globalMousePos - offsetPos;
        }
    }

    public void EndDrag()
    {
        if (DraggingTileAddComponent == null) return;

        // 타일 잠금 해제
        if (CanUnlock && TargetTileComponent != null)
        {
            if (DraggingTileAddComponent.IsAd)
            {
                GameRoot.Instance.PluginSystem.ADProp.ShowRewardAD(TpMaxProp.AdRewardType.WeaponMergeOrEquip, null);
            }

            // 인접 타일들도 모두 잠금 해제
            UnlockAdjacentTiles(TargetTileComponent);

            // TileAddComponent 사용 후 비활성화
            ProjectUtility.SetActiveCheck(DraggingTileAddComponent.gameObject, false);

            Debug.Log($"타일 잠금 해제: {TargetTileComponent.name}");
        }
        else
        {
            // 원래 위치로 돌아가는 애니메이션 추가 가능
            Debug.Log("잠금 해제 실패 - 대상 타일 없음 또는 조건 미충족");
        }

        TileWeaponGroup.SortQueueTileWeapon();

        Clear();
    }

    private void UnlockAdjacentTiles(TileComponent baseTileComponent)
    {
        if (DraggingTileAddComponent == null) return;

        int equipIdx = DraggingTileAddComponent.EquipIdx;
        int listIndex = equipIdx - 1001;

        if (listIndex < 0 || listIndex >= GameRoot.Instance.TileSystem.TileUnLockCheckList.Count)
        {
            return;
        }

        List<Vector2> checkPositions = GameRoot.Instance.TileSystem.TileUnLockCheckList[listIndex];

        // 모든 인접 타일 잠금 해제
        foreach (Vector2 offset in checkPositions)
        {
            Vector2 targetPosition = baseTileComponent.TileOrderVec + offset;
            TileComponent adjacentTile = TileWeaponGroup.TileComponentList.FirstOrDefault(x => x.TileOrderVec == targetPosition);

            if (adjacentTile != null && !adjacentTile.IsUnLock)
            {
                UnlockTile(adjacentTile);
                Debug.Log($"인접 타일 잠금 해제: {adjacentTile.name} at {targetPosition}");
            }
        }
    }

    private void UnlockTile(TileComponent tileComponent)
    {
        tileComponent.IsUnLock = true;
        ProjectUtility.SetActiveCheck(tileComponent.gameObject, true);
        // 잠금 해제된 타일은 TileUnLockObj 비활성화
        ProjectUtility.SetActiveCheck(tileComponent.TileUnLockObj, false);

        tileComponent.TileColorChange(Config.Instance.GetImageColor("AddTileEquip_Color"));
    }

    public void Clear()
    {
        ResetTileColor();

        // 임시로 활성화했던 타일들을 다시 비활성화
        HideLockedTiles();

        DraggingTileAddComponent = null;
        TargetTileComponent = null;
        IsDragging = false;
        CanUnlock = false;
    }

    private void ResetTileColor()
    {
        if (TargetTileComponent != null)
        {
            // 타겟 타일을 원래 색상으로 리셋
            if (TargetTileComponent.IsUnLock)
            {
                // 이미 열려있는 타일은 TileBase_Color로
                TargetTileComponent.TileColorChange(Config.Instance.GetImageColor("TileBase_Color"));
            }
            else
            {
                // 잠긴 타일은 AddTileEquip_Color로
                TargetTileComponent.TileColorChange(Config.Instance.GetImageColor("AddTileEquip_Color"));
            }
        }

        // 인접 타일들도 원래 색상으로 리셋
        foreach (var adjacentTile in CurrentAdjacentTiles)
        {
            if (adjacentTile.IsUnLock)
            {
                // 이미 열려있는 타일은 TileBase_Color로
                adjacentTile.TileColorChange(Config.Instance.GetImageColor("TileBase_Color"));
            }
            else
            {
                // 잠긴 타일은 AddTileEquip_Color로
                adjacentTile.TileColorChange(Config.Instance.GetImageColor("AddTileEquip_Color"));
            }
        }

        CurrentAdjacentTiles.Clear();
        TargetTileComponent = null;
        CanUnlock = false;
    }

    private void ShowLockedTiles()
    {
        TemporarilyActivatedTiles.Clear();

        foreach (var tileComponent in TileWeaponGroup.TileComponentList)
        {
            if (!tileComponent.IsUnLock)
            {
                // 잠긴 타일을 활성화하고 리스트에 추가
                ProjectUtility.SetActiveCheck(tileComponent.gameObject, true);
                ProjectUtility.SetActiveCheck(tileComponent.TileUnLockObj, false);
                tileComponent.TileColorChange(Config.Instance.GetImageColor("AddTileEquip_Color"));
                TemporarilyActivatedTiles.Add(tileComponent);
            }
            else
            {
                // 잠금 해제된 타일은 모두 TileUnLockObj 비활성화
                ProjectUtility.SetActiveCheck(tileComponent.TileUnLockObj, false);
            }
        }
    }

    private void HideLockedTiles()
    {
        foreach (var tileComponent in TemporarilyActivatedTiles)
        {
            if (!tileComponent.IsUnLock)
            {
                // 타일을 다시 비활성화
                ProjectUtility.SetActiveCheck(tileComponent.gameObject, false);
                ProjectUtility.SetActiveCheck(tileComponent.TileUnLockObj, false);
            }
        }

        TemporarilyActivatedTiles.Clear();
    }
}
