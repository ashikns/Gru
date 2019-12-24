using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gru.MaterialMaps
{
	public class BaseMaterialMap
	{
		public Material Material { get; }

		public virtual Texture NormalTexture
		{
			get => Material.HasProperty("_BumpMap") ? Material.GetTexture("_BumpMap") : null;
			set
			{
				if (Material.HasProperty("_BumpMap"))
				{
					Material.SetTexture("_BumpMap", value);
					Material.EnableKeyword("_NORMALMAP");
				}
				else
				{
					Debug.LogWarning("Tried to set a normal map texture to a material that does not support it.");
				}
			}
		}
		public virtual int NormalTexCoord
		{
			get { return 0; }
			set { return; }
		}
		public virtual float NormalTexScale
		{
			get => Material.HasProperty("_BumpScale") ? Material.GetFloat("_BumpScale") : 1;
			set
			{
				if (Material.HasProperty("_BumpScale"))
				{
					Material.SetFloat("_BumpScale", value);
				}
				else
				{
					Debug.LogWarning("Tried to set a normal map scale to a material that does not support it.");
				}
			}
		}

		public virtual Texture OcclusionTexture
		{
			get => Material.HasProperty("_OcclusionMap") ? Material.GetTexture("_OcclusionMap") : null;
			set
			{
				if (Material.HasProperty("_OcclusionMap"))
				{
					Material.SetTexture("_OcclusionMap", value);
				}
				else
				{
					Debug.LogWarning("Tried to set an occlusion map to a material that does not support it.");
				}
			}
		}
		public virtual int OcclusionTexCoord
		{
			get { return 0; }
			set { return; }
		}
		public virtual float OcclusionTexStrength
		{
			get => Material.HasProperty("_OcclusionStrength") ? Material.GetFloat("_OcclusionStrength") : 1;
			set
			{
				if (Material.HasProperty("_OcclusionStrength"))
				{
					Material.SetFloat("_OcclusionStrength", value);
				}
				else
				{
					Debug.LogWarning("Tried to set occlusion strength to a material that does not support it.");
				}
			}
		}

		public virtual Texture EmissiveTexture
		{
			get => Material.HasProperty("_EmissionMap") ? Material.GetTexture("_EmissionMap") : null;
			set
			{
				if (Material.HasProperty("_EmissionMap"))
				{
					Material.SetTexture("_EmissionMap", value);
					Material.EnableKeyword("_EMISSION");
				}
				else
				{
					Debug.LogWarning("Tried to set an emission map to a material that does not support it.");
				}
			}
		}
		public virtual int EmissiveTexCoord
		{
			get { return 0; }
			set { return; }
		}
		public virtual Color EmissiveFactor
		{
			get => Material.HasProperty("_EmissionColor") ? Material.GetColor("_EmissionColor") : Color.white;
			set
			{
				if (Material.HasProperty("_EmissionColor"))
				{
					Material.SetColor("_EmissionColor", value);
				}
				else
				{
					Debug.LogWarning("Tried to set an emission factor to a material that does not support it.");
				}
			}
		}

		public virtual GLTF.Schema.AlphaMode AlphaMode
		{
			get
			{
				switch (Material.renderQueue)
				{
					case (int)RenderQueue.AlphaTest:
						return GLTF.Schema.AlphaMode.MASK;
					case (int)RenderQueue.Transparent:
						return GLTF.Schema.AlphaMode.BLEND;
					case (int)RenderQueue.Geometry:
					default:
						return GLTF.Schema.AlphaMode.OPAQUE;
				}
			}
			set
			{
				switch (value)
				{
					case GLTF.Schema.AlphaMode.MASK:
						Material.SetOverrideTag("RenderType", "TransparentCutout");
						Material.SetInt("_SrcBlend", (int)BlendMode.One);
						Material.SetInt("_DstBlend", (int)BlendMode.Zero);
						Material.SetInt("_ZWrite", 1);
						Material.EnableKeyword("_ALPHATEST_ON");
						Material.DisableKeyword("_ALPHABLEND_ON");
						Material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
						Material.renderQueue = (int)RenderQueue.AlphaTest;
						break;
					case GLTF.Schema.AlphaMode.BLEND:
						Material.SetOverrideTag("RenderType", "Transparent");
						Material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
						Material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
						Material.SetInt("_ZWrite", 0);
						Material.DisableKeyword("_ALPHATEST_ON");
						Material.EnableKeyword("_ALPHABLEND_ON");
						Material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
						Material.renderQueue = (int)RenderQueue.Transparent;
						break;
					case GLTF.Schema.AlphaMode.OPAQUE:
					default:
						Material.SetOverrideTag("RenderType", "Opaque");
						Material.SetInt("_SrcBlend", (int)BlendMode.One);
						Material.SetInt("_DstBlend", (int)BlendMode.Zero);
						Material.SetInt("_ZWrite", 1);
						Material.DisableKeyword("_ALPHATEST_ON");
						Material.DisableKeyword("_ALPHABLEND_ON");
						Material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
						Material.renderQueue = (int)RenderQueue.Geometry;
						break;
				}
			}
		}
		public virtual float AlphaCutoff
		{
			get => Material.HasProperty("_Cutoff") ? Material.GetFloat("_Cutoff") : 0.5f;
			set
			{
				if (Material.HasProperty("_Cutoff"))
				{
					Material.SetFloat("_Cutoff", value);
				}
			}
		}
		public virtual bool DoubleSided
		{
			get => Material.GetInt("_Cull") == (int)CullMode.Off;
			set
			{
				if (value)
				{
					Material.SetInt("_Cull", (int)CullMode.Off);
				}
				else
				{
					Material.SetInt("_Cull", (int)CullMode.Back);
				}
			}
		}
		public virtual bool VertexColorsEnabled
		{
			get => Material.IsKeywordEnabled("VERTEX_COLOR_ON");
			set
			{
				if (value)
				{
					Material.EnableKeyword("VERTEX_COLOR_ON");
				}
				else
				{
					Material.DisableKeyword("VERTEX_COLOR_ON");
				}
			}
		}

		protected BaseMaterialMap(string shaderName, int maxLOD)
		{
			var shader = Shader.Find(shaderName);
			if (shader == null)
			{
				throw new Exception($"{shaderName} not found. Did you forget to add it to the build?");
			}

			shader.maximumLOD = maxLOD;
			Material = new Material(shader);
		}
	}
}
