using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// [COAT 도구] 임포트 설정 → 슬라이싱 → 클립 생성 → Controller 연결을 한 번에 처리한다.
/// 메뉴: Tools > COAT > Build All (Fix + Animate)
/// </summary>
public static class SpriteAnimationBuilder
{
    private const string SpritesRoot    = "Assets/Sprites/Pet";
    private const string AnimsRoot      = "Assets/Animations";
    private const string ControllerPath = "Assets/Animations/PetAnimator.controller";
    private const int    SampleRate     = 8;
    private const int    FrameSize      = 128;

    private static readonly (string dir, string png, string clip, bool loop, int frames)[] Defs =
    {
        ("idle",  "pet_idle.png",  "Pet_Idle",  true,  4),
        ("walk",  "pet_walk.png",  "Pet_Walk",  true,  6),
        ("run",   "pet_run.png",   "Pet_Run",   true,  6),
        ("sleep", "pet_sleep.png", "Pet_Sleep", true,  4),
        ("sit",   "pet_sit.png",   "Pet_Sit",   true,  2),
        ("react", "pet_react.png", "Pet_React", false, 4),
    };

    // ── Step 1: 임포트 + 슬라이싱 ────────────────────────────────────────────

    [MenuItem("Tools/COAT/Step1: Fix Import + Slice")]
    public static void Step1_FixAndSlice()
    {
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var (dir, png, _, _, frames) in Defs)
            {
                string path = $"{SpritesRoot}/{dir}/{png}";
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) { Debug.LogError($"[COAT] 파일 없음: {path}"); continue; }

                // Multiple 모드 + Point Filter + PPU 100
                importer.textureType         = TextureImporterType.Sprite;
                importer.spriteImportMode    = SpriteImportMode.Multiple;
                importer.filterMode          = FilterMode.Point;
                importer.spritePixelsPerUnit = 100f;
                importer.textureCompression  = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled       = false;
                importer.isReadable          = true;

                // 슬라이싱 메타데이터 직접 설정
                var metas = new List<SpriteMetaData>();
                for (int i = 0; i < frames; i++)
                {
                    metas.Add(new SpriteMetaData
                    {
                        name      = $"{Path.GetFileNameWithoutExtension(png)}_{i}",
                        rect      = new Rect(i * FrameSize, 0, FrameSize, FrameSize),
                        pivot     = new Vector2(0.5f, 0.5f),
                        alignment = 9
                    });
                }
                importer.spritesheet = metas.ToArray();

                EditorUtility.SetDirty(importer);
                Debug.Log($"[COAT] 설정 완료: {png} ({frames}프레임)");
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        // 일괄 강제 임포트
        foreach (var (dir, png, _, _, _) in Defs)
        {
            string path = $"{SpritesRoot}/{dir}/{png}";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        AssetDatabase.Refresh();
        Debug.Log("[COAT] Step1 완료 — Step2 실행하세요");
        EditorUtility.DisplayDialog("COAT Step1", "임포트 + 슬라이싱 완료\n\nTools > COAT > Step2 실행하세요", "확인");
    }

    // ── Step 2: 클립 생성 + Controller 연결 ──────────────────────────────────

    [MenuItem("Tools/COAT/Step2: Build Animations")]
    public static void Step2_BuildAnimations()
    {
        if (!Directory.Exists(AnimsRoot))
            AssetDatabase.CreateFolder("Assets", "Animations");

        int ok = 0;
        foreach (var (dir, png, clipName, loop, expectedFrames) in Defs)
        {
            string path    = $"{SpritesRoot}/{dir}/{png}";
            var    sprites = LoadSubSprites(path);

            if (sprites.Count == 0)
            {
                Debug.LogError($"[COAT] 서브 스프라이트 없음: {path}\nStep1 먼저 실행하세요!");
                continue;
            }
            if (sprites.Count != expectedFrames)
                Debug.LogWarning($"[COAT] {clipName}: 예상 {expectedFrames}프레임, 실제 {sprites.Count}프레임");

            Debug.Log($"[COAT] {clipName}: {sprites.Count}프레임 로드 성공");
            var clip = MakeClip(clipName, sprites, loop);
            SaveClip(clip, clipName);
            ok++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        ConnectController();

        string result = $"클립 생성: {ok}/{Defs.Length}\nController 연결 완료";
        Debug.Log($"[COAT] Step2 완료: {result}");
        EditorUtility.DisplayDialog("COAT Step2", result + "\n\nRebuild Animator Transitions도 실행하세요", "확인");
    }

    // ── 서브 스프라이트 로드 ──────────────────────────────────────────────────

    private static List<Sprite> LoadSubSprites(string pngPath)
    {
        var list = new List<Sprite>();
        var all  = AssetDatabase.LoadAllAssetsAtPath(pngPath);
        foreach (var obj in all)
            if (obj is Sprite s) list.Add(s);

        // 이름의 숫자 부분으로 정렬 (pet_idle_0, pet_idle_1 ...)
        list.Sort((a, b) =>
        {
            int ai = ExtractIndex(a.name);
            int bi = ExtractIndex(b.name);
            return ai.CompareTo(bi);
        });
        return list;
    }

    private static int ExtractIndex(string name)
    {
        int lastUnderscore = name.LastIndexOf('_');
        if (lastUnderscore >= 0 && int.TryParse(name.Substring(lastUnderscore + 1), out int idx))
            return idx;
        return 0;
    }

    // ── 클립 생성 ─────────────────────────────────────────────────────────────

    private static AnimationClip MakeClip(string name, List<Sprite> sprites, bool loop)
    {
        var clip    = new AnimationClip { name = name, frameRate = SampleRate };
        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        float dt    = 1f / SampleRate;

        var keys = new ObjectReferenceKeyframe[sprites.Count];
        for (int i = 0; i < sprites.Count; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i * dt, value = sprites[i] };

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        var s = AnimationUtility.GetAnimationClipSettings(clip);
        s.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, s);

        return clip;
    }

    private static void SaveClip(AnimationClip clip, string clipName)
    {
        string path     = $"{AnimsRoot}/{clipName}.anim";
        var    existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (existing != null) EditorUtility.CopySerialized(clip, existing);
        else                  AssetDatabase.CreateAsset(clip, path);
    }

    // ── Controller 연결 ───────────────────────────────────────────────────────

    private static void ConnectController()
    {
        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (ctrl == null) { Debug.LogWarning("[COAT] PetAnimator.controller 없음 — 수동 연결 필요"); return; }

        foreach (var (_, _, clipName, _, _) in Defs)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimsRoot}/{clipName}.anim");
            if (clip == null) continue;

            string shortName = clipName.StartsWith("Pet_") ? clipName.Substring(4) : clipName;
            foreach (var layer in ctrl.layers)
            {
                foreach (var cs in layer.stateMachine.states)
                {
                    if (cs.state.name == clipName || cs.state.name == shortName)
                    {
                        cs.state.motion = clip;
                        EditorUtility.SetDirty(ctrl);
                        Debug.Log($"[COAT] 연결: '{cs.state.name}' ← {clipName}.anim");
                        break;
                    }
                }
            }
        }
        AssetDatabase.SaveAssets();
    }
}
