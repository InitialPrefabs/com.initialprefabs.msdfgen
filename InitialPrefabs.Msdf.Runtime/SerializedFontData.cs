using UnityEngine;
using UnityEngine.Serialization;

namespace InitialPrefabs.Msdf.Runtime {
#if MSDF_BINARY
    [PreferBinarySerialization]
#endif
    [CreateAssetMenu(menuName = "InitialPrefabs/MSDF/SerializedFontData")]
    public class SerializedFontData : ScriptableObject {
        [FormerlySerializedAs("FontData")]
        public RuntimeFaceData FaceData;
        public RuntimeGlyphData[] Glyphs;

        public void CopyFrom(SerializedFontData other) {
            FaceData = other.FaceData;
            Glyphs = new RuntimeGlyphData[other.Glyphs.Length];

            for (var i = 0; i < other.Glyphs.Length; i++) {
                Glyphs[i] = other.Glyphs[i];
            }
        }
    }
}
