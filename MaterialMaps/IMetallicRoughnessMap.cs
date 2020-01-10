using UnityEngine;

namespace Gru.MaterialMaps
{
	public interface IMetallicRoughnessMap : IBaseMaterialMap
	{
		Texture BaseColorTexture { get; set; }
		int BaseColorTexCoord { get; set; }
		Color BaseColorFactor { get; set; }

		Texture MetallicRoughnessTexture { get; set; }
		int MetallicRoughnessTexCoord { get; set; }
		float MetallicFactor { get; set; }
		float RoughnessFactor { get; set; }
	}
}