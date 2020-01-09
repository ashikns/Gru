using UnityEngine;

namespace Gru.MaterialMaps
{
	public class SpecularGlossinessMap : BaseMaterialMap, ISpecularGlossinessMap
	{
		// LOD levels in shader - 300, 150
		public SpecularGlossinessMap(int maxLOD = 300) : base("GLTF/PbrSpecularGlossiness", maxLOD)
		{
		}

		public Texture DiffuseTexture
		{
			get { return Material.GetTexture("_MainTex"); }
			set { Material.SetTexture("_MainTex", value); }
		}
		public int DiffuseTexCoord
		{
			get { return 0; }
			set { return; }
		}

		public Color DiffuseFactor
		{
			get { return Material.GetColor("_Color"); }
			set { Material.SetColor("_Color", value); }
		}

		public Texture SpecularGlossinessTexture
		{
			get { return Material.GetTexture("_SpecGlossMap"); }
			set
			{
				Material.SetTexture("_SpecGlossMap", value);
				Material.SetFloat("_SmoothnessTextureChannel", 0);
				Material.EnableKeyword("_SPECGLOSSMAP");
			}
		}
		public int SpecularGlossinessTexCoord
		{
			get { return 0; }
			set { return; }
		}

		public Vector3 SpecularFactor
		{
			get { return Material.GetVector("_SpecColor"); }
			set { Material.SetVector("_SpecColor", value); }
		}
		public float GlossinessFactor
		{
			get { return Material.GetFloat("_Glossiness"); }
			set
			{
				Material.SetFloat("_Glossiness", value);
			}
		}
	}
}