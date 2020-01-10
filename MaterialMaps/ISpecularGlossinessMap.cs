using UnityEngine;

namespace Gru.MaterialMaps
{
	public interface ISpecularGlossinessMap : IBaseMaterialMap
	{
		Texture DiffuseTexture { get; set; }
		int DiffuseTexCoord { get; set; }
		Color DiffuseFactor { get; set; }

		Texture SpecularGlossinessTexture { get; set; }
		int SpecularGlossinessTexCoord { get; set; }
		Vector3 SpecularFactor { get; set; }
		float GlossinessFactor { get; set; }
	}
}