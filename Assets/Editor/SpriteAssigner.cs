using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// [COAT 도구] PetSpriteAnimator의 스프라이트 배열을 자동 할당한다.
/// 메뉴: Tools > COAT > Auto Assign Sprites
/// </summary>
public static class SpriteAssigner
{
    private const string SpritesRoot = "Assets/Sprites/Pet";

    private static readonly (string dir, string field)[] Mapping =
    {
        ("idle",  "idleSprites"),
        ("walk",  "walkSprites"),
        ("run",   "runSprites"),
        ("sleep", "sleepSprites"),
        ("sit",   "sitSprites"),
        ("react", "reactSprites"),
    };

    [MenuItem("Tools/COAT/Auto Assign Sprites")]
    public static void AutoAssign()
    {
        // 씬에서 PetSpriteAnimator 찾기
        var anim = Object.FindFirstObjectByType<PetSpriteAnimator>();
        if (anim == null)
        {
            EditorUtility.DisplayDialog("COAT", "PetSpriteAnimator 컴포넌트를 찾을 수 없습니다.\nDesktopPet GameObject에 추가 후 다시 실행하세요.", "확인");
            return;
        }

        var so = new SerializedObject(anim);
        int total = 0;

        foreach (var (dir, field) in Mapping)
        {
            string path = $"{SpritesRoot}/{dir}";
            var sprites = LoadSortedSprites(path);

            if (sprites.Count == 0)
            {
                Debug.LogWarning($"[COAT] 스프라이트 없음: {path}");
                continue;
            }

            var prop = so.FindProperty(field);
            prop.arraySize = sprites.Count;
            for (int i = 0; i < sprites.Count; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];

            Debug.Log($"[COAT] {field}: {sprites.Count}개 할당 완료");
            total += sprites.Count;
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(anim);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("COAT", $"스프라이트 자동 할당 완료\n총 {total}개", "확인");
    }

    private static List<Sprite> LoadSortedSprites(string folderPath)
    {
        var sprites = new List<Sprite>();
        var guids   = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });

        foreach (var guid in guids)
        {
            string p   = AssetDatabase.GUIDToAssetPath(guid);
            var    all = AssetDatabase.LoadAllAssetsAtPath(p);
            foreach (var obj in all)
                if (obj is Sprite s) sprites.Add(s);
        }

        // 인덱스 번호 순 정렬 (pet_idle_0, pet_idle_1 ...)
        sprites.Sort((a, b) =>
        {
            int ai = ExtractIndex(a.name);
            int bi = ExtractIndex(b.name);
            return ai.CompareTo(bi);
        });

        return sprites;
    }

    private static int ExtractIndex(string name)
    {
        int i = name.LastIndexOf('_');
        return i >= 0 && int.TryParse(name.Substring(i + 1), out int idx) ? idx : 0;
    }
}
