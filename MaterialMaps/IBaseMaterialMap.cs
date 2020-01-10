using UnityEngine;

namespace Gru.MaterialMaps
{
	public interface IBaseMaterialMap
	{
		Material Material { get; }

		Texture NormalTexture { get; set; }
		int NormalTexCoord { get; set; }
		float NormalTexScale { get; set; }

		Texture OcclusionTexture { get; set; }
		int OcclusionTexCoord { get; set; }
		float OcclusionTexStrength { get; set; }

		Texture EmissiveTexture { get; set; }
		int EmissiveTexCoord { get; set; }
		Color EmissiveFactor { get; set; }

		GLTF.Schema.AlphaMode AlphaMode { get; set; }
		float AlphaCutoff { get; set; }
	}
}