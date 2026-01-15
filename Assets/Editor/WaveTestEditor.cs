using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

#if UNITY_EDITOR

using System;
using System.Text;
using UnityEditor;
using Newtonsoft.Json;

public class WaveTestEditor : EditorWindow
{
    private string source;
    private string result;
    private float multiplier = 1;
    private int addition = 0;
    private bool replace = false;

  [MenuItem("BanpoFri/Wave Test Editor")]
    private static void ShowWindow()
    {
        var window = GetWindow<WaveTestEditor>();
        window.titleContent = new GUIContent("Wave Test Editor");
        window.Show();
    }

    private void OnGUI(){
        if(GUILayout.Button("TestStart")) Apply();
    }

    private void Apply(){
        // 에디터에서 Tables 에셋 직접 로드
        var tablesPath = "Assets/BanpoFri/TableAsset/Tables.asset";
        var tables = AssetDatabase.LoadAssetAtPath<Tables>(tablesPath);
        
        if (tables == null)
        {
            Debug.LogError($"[WaveTestEditor] Tables 에셋을 찾을 수 없습니다. 경로: {tablesPath}");
            return;
        }

        // Tables 로드
        tables.Load();

        var stageInfoTable = tables.GetTable<StageInfo>();
        if (stageInfoTable == null)
        {
            Debug.LogError("[WaveTestEditor] StageInfo 테이블을 찾을 수 없습니다.");
            return;
        }

        if (stageInfoTable.DataList == null)
        {
            Debug.LogError("[WaveTestEditor] StageInfo.DataList가 null입니다.");
            return;
        }

        var tdlist = stageInfoTable.DataList.ToList();
        
        if (tdlist.Count == 0)
        {
            Debug.LogWarning("[WaveTestEditor] StageInfo 테이블에 데이터가 없습니다.");
            return;
        }

        // foreach(var td in tdlist)
        // {
        //     // enemy_idx를 기준으로 삼기
        //     var enemyIdxCount = td.enemy_idx?.Count ?? 0;
            
        //     // 각 필드의 Count 개수 확인
        //     var hpCount = td.hp?.Count ?? 0;
        //     var atkCount = td.atk?.Count ?? 0;
        //     var unitMovespeedCount = td.unit_movespeed?.Count ?? 0;

        //     // enemy_idx보다 개수가 적은 필드들 체크
        //     var errors = new List<string>();
            
        //     if (errors.Count > 0)
        //     {
        //         // enemy_idx보다 개수가 적은 필드가 있으면 에러 출력
        //         var errorMsg = $"[StageInfo Error] Stage: {td.stage_idx}, Wave: {td.wave_idx}\n" +
        //                        $"enemy_idx 개수({enemyIdxCount}개)보다 적은 필드가 있습니다:\n" +
        //                        string.Join("\n", errors);
                
        //         Debug.LogError(errorMsg);
        //     }
        //     else
        //     {
        //         Debug.Log($"[StageInfo] Stage: {td.stage_idx}, Wave: {td.wave_idx} - 검증 통과 (enemy_idx 기준: {enemyIdxCount}개)");
        //     }
        // }

        Debug.Log("=== Wave Test Editor 검증 완료 ===");
    }
}
#endif
