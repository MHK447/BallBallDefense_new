using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class ShooterPathGroup : MonoBehaviour
{
    [SerializeField]
    private GameObject futurePathRoot;

    [SerializeField]
    private Transform[] futurePathItems;


    private Vector2 direction;


    private List<Vector2> futurePath = new();

    [SerializeField]
    private Transform FireTr;

    [SerializeField]
    private PlayerUnitWeapon CurrentWeapon;

    public void Clear()
    {
        direction = Vector2.zero;
        futurePath.Clear();
    }


    public void Init()
    {
        // 경로 표시 초기에는 비활성화 (버튼/터치를 누를 때만 보임)
        if (futurePathRoot != null)
        {
            futurePathRoot.SetActive(false);
        }
        
        // 디버그: 경로 아이템 개수 확인
        if (futurePathItems == null || futurePathItems.Length == 0)
        {
            Debug.LogWarning("ShooterPathGroup: futurePathItems가 설정되지 않았습니다!");
        }
    }


    private Vector2 GetShootVelocity()
    {
        if (CurrentWeapon == null) return Vector2.zero;
        return direction.normalized * 15;
    }


    private void UpdateFuturePath()
    {
        if (CurrentWeapon == null) return;
        if (futurePathItems == null || futurePathItems.Length == 0) return;

        var fireTr = FireTr;
        
        // ProjectileSpeed 가져오기 (기본값 20)
        float speed = 15;
        if (speed <= 0.1f) speed = 20f;

        // 무기의 현재 방향(위쪽)에 속도를 곱해서 초기 velocity 계산
        // fireTr.up는 Transform의 Z축 rotation을 자동으로 반영함
        Vector2 vel = fireTr.up * speed;
        
        // 디버그: 무기 rotation과 velocity 방향 확인 (필요시 주석 해제)
        // Debug.Log($"무기 rotation: {fireTr.rotation.eulerAngles.z}, velocity: {vel}");

        CurrentWeapon.GetFuturePath(futurePath, vel);

 
        // 경로 점들을 시각화
        for (int i = 0; i < futurePathItems.Length; i++)
        {
            if (futurePathItems[i] == null) continue;
            
            if (i >= futurePath.Count - 1)
            {
                ProjectUtility.SetActiveCheck(futurePathItems[i].gameObject, false);
                continue;
            }
            
            // 경로 점 위치 설정
            futurePathItems[i].transform.position = futurePath[i];
            
            // 다음 점을 향하도록 회전
            Vector2 dir = futurePath[i + 1] - futurePath[i];
            if (dir.magnitude > 0.001f)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                futurePathItems[i].transform.rotation = Quaternion.Euler(0, 0, angle + 90f);
            }
            
            ProjectUtility.SetActiveCheck(futurePathItems[i].gameObject, true);
        }
    }

    void Update()
    {
        if (CurrentWeapon == null) return;

        // 마우스 버튼이나 터치를 누르고 있는지 확인
        bool isPressed = Input.GetMouseButton(0) || (Input.touchCount > 0);

        // futurePathRoot 활성화 상태 업데이트
        if (futurePathRoot != null && futurePathRoot.activeSelf != isPressed)
        {
            futurePathRoot.SetActive(isPressed);
        }

        // 누르고 있을 때만 경로 업데이트
        if (isPressed)
        {
            UpdateFuturePath();
        }
    }


    


}

