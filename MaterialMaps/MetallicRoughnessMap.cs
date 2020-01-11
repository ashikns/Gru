using UnityEngine;

namespace Gru.MaterialMaps
{
	public class MetallicRoughnessMap : BaseMaterialMap, IMetallicRoughnessMap
	{
		// LOD levels in shader - 300, 150
		public MetallicRoughnessMap(int maxLOD = 300) : base("GLTF/PbrMetallicRoughness", maxLOD)
		{
		}

		public Texture BaseColorTexture
		{
			get { return Material.GetTexture("_MainTex"); }
			set { Material.SetTexture("_MainTex", value); }
		}
		public int BaseColorTexCoord
		{
			get { return 0; }
			set { return; }
		}

		public Color BaseColorFactor
		{
			get { return Material.GetColor("_Color"); }
			set { Material.SetColor("_Color", value); }
		}

		public Texture MetallicRoughnessTexture
		{
			get { return Material.GetTexture("_MetallicGlossMap"); }
			set
			{
				Material.SetTexture("_MetallicGlossMap", value);
				Material.EnableKeyword("_METALLICGLOSSMAP");
			}
		}
		public int MetallicRoughnessTexCoord
		{
			get { return 0; }
			set { return; }
		}

		public float MetallicFactor
		{
			get { return Material.GetFloat("_Metallic"); }
			set { Material.SetFloat("_Metallic", value); }
		}
		public float RoughnessFactor
		{
			get { return 1 - Material.GetFloat("_Glossiness"); }
			set { Material.SetFloat("_Glossiness", 1 - value); }
		}
	}
}