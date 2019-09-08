Shader "Unlit/GeomCloud"
{
Properties{
	/// X = width; Y = height; Z = randomization factor width; W = randomiztion factor height
	  _Radius("Shape Size", Vector) = (1.0, 1.0, 0.0, 0.0)
	  _Tint("Color", Color) = (1.0, 1.0, 1.0, 1.0)
	_BendRotationRandom("Bend Rotation Random", Range(-1, 1)) = 0.2
	_TessellationUniform("Tessellation Uniform", Range(1, 64)) = 1
	_InnerShapes("Inner shape count", Range(1, 10)) = 1
	_Speed("Speed", float) = 1
	_MinDisplacement("_Min Displacement", float) = -1
	_MaxDisplacement("_Max Displacement", float) = 0.5
}
SubShader{
LOD 200
Tags { "RenderType" = "Opaque" }
//if you want transparency
//Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
//Blend SrcAlpha OneMinusSrcAlpha
Pass {
	CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#pragma geometry geom
	#pragma hull hull
	#pragma domain domain
	#pragma target 4.0                  // Use shader model 3.0 target, to get nicer looking lighting
	#include "UnityCG.cginc"
	#include "CustomTessellation.cginc"
	
	
	struct geomOut {
		float4 pos : POSITION;
		float2 uv: TEXCOORD0;
	};

	float rand(float3 p) {
		return frac(sin(dot(p.xyz, float3(12.9898, 78.233, 45.5432))) * 43758.5453);
	}
	float2x2 rotate2d(float a) {
	   float s = sin(a);
	   float c = cos(a);
	   return float2x2(c,-s,s,c);
	}
	float3x3 AngleAxis3x3(float angle, float3 axis)
	{
		float c, s;
		sincos(angle, s, c);

		float t = 1 - c;
		float x = axis.x;
		float y = axis.y;
		float z = axis.z;

		return float3x3(
			t * x * x + c, t * x * y - s * z, t * x * z + s * y,
			t * x * y + s * z, t * y * y + c, t * y * z - s * x,
			t * x * z - s * y, t * y * z + s * x, t * z * z + c
			);
	}

	float _BendRotationRandom, _Speed, _MinDisplacement, _MaxDisplacement, _InnerShapes;
	float4 _Tint, _Radius;
	
	float3 GerstnerWave(
		float4 wave, float3 p, inout float3 tangent, inout float3 binormal
	) {
		float steepness = wave.z;
		float wavelength = wave.w;
		float k = 2 * UNITY_PI / wavelength;
		float c = _Speed * sqrt(9.8 / k);
		float2 d = normalize(wave.xy);
		float f = k * (dot(d, p.xz) - c * _Time.y);
		float a = steepness / k;

		tangent += float3(
			-d.x * d.x * (steepness * sin(f)),
			d.x * (steepness * cos(f)),
			-d.x * d.y * (steepness * sin(f))
			);
		binormal += float3(
			-d.x * d.y * (steepness * sin(f)),
			d.y * (steepness * cos(f)),
			-d.y * d.y * (steepness * sin(f))
			);
		return float3(
			d.x * (a * cos(f)),
			a * sin(f),
			d.y * (a * cos(f))
			);
	}

	//Geometry shaders: Creates an equilateral triangle with the original vertex in the orthocenter
	[maxvertexcount(50)]
	void geom(point vertexOutput IN[1], inout LineStream<geomOut> OutputStream)
	{
		float4 vertexINIT = IN[0].vertex;
		float val = sin(_Time.y * _Speed * (10 * ( (vertexINIT.x + 0.00001) * (vertexINIT.y + 0.00001)) % 7));
		float valc = cos(_Time.y * _Speed * (10 * (vertexINIT.y + 0.00001) % 7));
		val = (val + sin(_Time.y * _Speed)) / 2;
		valc = (valc + cos(_Time.y * _Speed)) / 2;
		vertexINIT = lerp(_MaxDisplacement * IN[0].vertex, _MinDisplacement * IN[0].vertex, sqrt(val * 0.5 + 0.5));
	   float3 vNormal = IN[0].normal;
	   float3 vTangent = IN[0].tangent.xyz;
	   float3 vBinormal = cross(vNormal, vTangent) * IN[0].tangent.w;
	  // vertexINIT += float4(GerstnerWave(_WaveA, IN[0].vertex.xyz, vTangent, vBinormal), 0);
	   //vertexINIT += float4(GerstnerWave(_WaveB, IN[0].vertex.xyz, vTangent, vBinormal), 0);
	   float3x3 tangentToLocal = float3x3(
		   vTangent.x, vBinormal.x, vNormal.x,
		   vTangent.y, vBinormal.y, vNormal.y,
		   vTangent.z, vBinormal.z, vNormal.z
		   );
	   
	   float height = (rand(IN[0].vertex.zyx) * 2 - 1) * _Radius.z + _Radius.x;
	   float width = (rand(IN[0].vertex.zyx) * 2 - 1) * _Radius.w + _Radius.y;
	   float4 gridOffset = float4(0, 0, 0, 0);
	   
	   
	   float2 uvs[4];
	   uvs[0] = float2(0, 0);
	   uvs[1] = float2(1, 0);
	   uvs[2] = float2(1, 1);
	   uvs[3] = float2(0, 1);
	   for (int j = 0; j < (int)_InnerShapes; j++)
	   {
		   float3 p[4];
		   float minScale = (_InnerShapes - j) / _InnerShapes;
		   p[0] = float3(-width * minScale, -height * minScale, 0);
		   p[1] = float3(width * minScale, -height * minScale, 0);
		   p[2] = float3(width * minScale, height * minScale, 0);
		   p[3] = float3(-width * minScale, height * minScale, 0);
		   float3x3 facingRotationMatrix = AngleAxis3x3(rand(IN[0].vertex.zyx * minScale) * _BendRotationRandom * UNITY_PI * valc, float3(0, 0, 1));
		   float3x3 transformationMatrix = mul(tangentToLocal, facingRotationMatrix);
		   geomOut OUT;
		   for (int i = 0; i < 4; i++) {
			   OUT.pos = UnityObjectToClipPos(vertexINIT + mul(transformationMatrix, p[i]));
			   OUT.uv = uvs[i];
			   OutputStream.Append(OUT);
		   }
		   OUT.pos = UnityObjectToClipPos(vertexINIT + mul(transformationMatrix, p[0]));
		   OUT.uv = uvs[0];
		   OutputStream.Append(OUT);
		   OutputStream.RestartStrip();
	   }
	}
	float4 frag(geomOut i) : COLOR
	{
		return lerp(_Tint*0.1, _Tint, i.uv.y);;
		// could do some additional lighting calculation here based on normal
	}
ENDCG
}
}
FallBack "Diffuse"
}
