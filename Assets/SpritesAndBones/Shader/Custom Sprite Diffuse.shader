Shader "Custom Sprite Diffuse"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Main Colour", Color) = (1,1,1,1)
		_mainTex("Base (RGB)", 2D) = "white" { }
		_Normal("Normal", vector) = (0,0,-1, 0) // Normals set by script
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_RimColor("Rim Color", Color) = (0.26,0.19,0.16,0.0)
		_RimPower("Rim Power", Range(0.5,8.0)) = 3.0
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

		Material
		{
			Diffuse[_Color]
		}

		Cull Off
		Lighting On
		ZWrite Off
		Fog{ Mode Off }
		Blend One OneMinusSrcAlpha

		CGPROGRAM
		#pragma surface surf Lambert vertex:vert nofog keepalpha
		#pragma multi_compile _ PIXELSNAP_ON
		#pragma shader_feature ETC1_EXTERNAL_ALPHA

		sampler2D _MainTex;
		fixed4 _Color;
		float3 _Normal;
		sampler2D _AlphaTex;
		float4 _RimColor;
		float _RimPower;

		struct Input
		{
			float2 uv_MainTex;
			fixed4 color;
			float3 viewDir;
		};

		void vert(inout appdata_full v, out Input o)
		{
			#if defined(PIXELSNAP_ON)
			v.vertex = UnityPixelSnap(v.vertex);
			#endif
			v.normal = _Normal;

			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.color = v.color * _Color;
		}

		fixed4 SampleSpriteTexture(float2 uv)
		{
			fixed4 color = tex2D(_MainTex, uv);

			#if ETC1_EXTERNAL_ALPHA
			color.a = tex2D(_AlphaTex, uv).r;
			#endif

			return color;
		}

		void surf(Input IN, inout SurfaceOutput o)
		{
			fixed4 c = SampleSpriteTexture(IN.uv_MainTex);
			o.Albedo = c.rgb * c.a;
			o.Alpha = c.a;
			half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
			o.Emission = _RimColor.rgb * pow(rim, _RimPower);
		}
	ENDCG
	}
	Fallback "Transparent/VertexLit"
}