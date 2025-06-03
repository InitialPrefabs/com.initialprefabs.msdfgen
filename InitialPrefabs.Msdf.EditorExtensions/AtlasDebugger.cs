using System.Collections.Generic;
using System.Linq;
using InitialPrefabs.Msdf.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace InitialPrefabs.Msdf.EditorExtensions {

    // TODO: Figure out pagination
    public class AtlasDebugger : EditorWindow {

        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        [MenuItem("Tools/InitialPrefabs/Atlas Debugger")]
        public static void Initialize() {
            AtlasDebugger wnd = GetWindow<AtlasDebugger>();
            wnd.titleContent = new GUIContent("AtlasDebugger");
        }

        private readonly List<SerializedFontData> serializedFontDatas = new List<SerializedFontData>();
        private readonly List<string> choices = new List<string>();
        private readonly List<VisualElement> glyphContainers = new List<VisualElement>();

        private SerializedFontData selectedFontData;

        private VisualTreeAsset previewTree;

        private void OnEnable() {
            var guids = AssetDatabase.FindAssets("t: VisualTreeAsset DebugPreview");
            previewTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private void TryInstantiateTree() {
            if (m_VisualTreeAsset == null) {
                var guids = AssetDatabase.FindAssets($"t: VisualTreeAsset {nameof(AtlasDebugger)}");
                m_VisualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            rootVisualElement.Add(m_VisualTreeAsset.CloneTree());
        }

        public void CreateGUI() {
            TryInstantiateTree();
            var root = rootVisualElement;

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

            var btn = root.Q<Button>("refresh");
            btn.RegisterCallback<MouseUpEvent>(callback => {
                GeneratePreview(root);
            });
        }

        private void InstantiateAndFillData(RuntimeGlyphData glyphData, VisualElement containerToAppend, Texture texture) {
            var preview = previewTree.CloneTree();
            FillData(glyphData, preview, texture);
            containerToAppend.Add(preview);
        }

        private void FillData(RuntimeGlyphData glyph, TemplateContainer preview, Texture texture) {
            preview.Q<IntegerField>("unicode").value = glyph.Unicode;
            preview.Q<TextField>("char").value = $"{glyph.Char}";
            preview.Q<FloatField>("advance").value = glyph.Advance;
            preview.Q<Vector2Field>("metrics").value = glyph.Metrics;
            preview.Q<Vector2Field>("bearings").value = glyph.Bearings;
            preview.Q<Vector4Field>("uvs").value = glyph.Uvs;

            var img = preview.Q<Image>("glyph-preview");
            img.image = texture;
            var width = glyph.Uvs.z - glyph.Uvs.x;
            var height = glyph.Uvs.w - glyph.Uvs.y;
            img.uv = new Rect(glyph.Uvs.x, glyph.Uvs.y, width, height);
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
            var preview = root.Q<VisualElement>("preview");
            var children = preview.Children().ToArray();
            for (int i = children.Length - 1; i >= 0; i--) {
                children[i].RemoveFromHierarchy();
            }
            glyphContainers.Clear();
            foreach (var glyph in selectedFontData.Glyphs) {
                InstantiateAndFillData(glyph, preview, sourceImage);
            }
        }
    }
}