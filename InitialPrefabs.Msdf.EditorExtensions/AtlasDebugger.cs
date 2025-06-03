using System.Collections.Generic;
using InitialPrefabs.Msdf.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class AtlasDebugger : EditorWindow {
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Tools/InitialPrefabs/Atlas Debugger")]
    public static void ShowExample() {
        AtlasDebugger wnd = GetWindow<AtlasDebugger>();
        wnd.titleContent = new GUIContent("AtlasDebugger");
    }

    private readonly List<SerializedFontData> serializedFontDatas = new List<SerializedFontData>();
    private readonly List<string> choices = new List<string>();
    private readonly List<Image> glyphImages = new List<Image>();

    private SerializedFontData selectedFontData;

    public void CreateGUI() {
        // Each editor window contains a root VisualElement object
        var root = rootVisualElement;
        root.Add(m_VisualTreeAsset.CloneTree());

        var dropdown = root.Q<DropdownField>("atlas");
        dropdown.RegisterCallback<MouseUpEvent>(callback => {
            serializedFontDatas.Clear();
            choices.Clear();
            var guids = AssetDatabase.FindAssets("t: SerializedFontData");
            foreach (var guid in guids) {
                var asset = AssetDatabase.LoadAssetAtPath<SerializedFontData>(AssetDatabase.GUIDToAssetPath(guid));
                serializedFontDatas.Add(asset);
                choices.Add(asset.name);
            }

            dropdown.choices = choices;
        });

        dropdown.RegisterValueChangedCallback(changeEvt => {
            if (changeEvt.newValue != changeEvt.previousValue) {
                var idx = choices.IndexOf(changeEvt.newValue);
                selectedFontData = serializedFontDatas[idx];
                GeneratePreview(root);
            }
        });
    }

    private void GeneratePreview(VisualElement root) {
        if (selectedFontData == null) {
            return;
        }

        var name = selectedFontData.name;
        // Find the associated image

        var guids = AssetDatabase.FindAssets($"t: Texture {name}");
        if (guids.Length == 0) {
            return;
        }
        var sourceImage = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(guids[0]));
        glyphImages.Clear();
        var preview = root.Q<VisualElement>("preview");
        foreach (var glyph in selectedFontData.Glyphs) {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.paddingBottom = 10;

            var img = new Image();
            img.image = sourceImage;
            var width = glyph.Uvs.z - glyph.Uvs.x;
            var height = glyph.Uvs.w - glyph.Uvs.y;
            img.uv = new Rect(glyph.Uvs.x, glyph.Uvs.y, width, height);

            container.Add(new Label($"{glyph.Char}"));
            container.Add(img);
            preview.Add(container);
        }
    }
}
