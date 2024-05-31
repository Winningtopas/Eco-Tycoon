// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Grid"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		[HDR]_GridColour("Grid Colour", Color) = (.255,.0,.0,1)
		[HDR]_BaseColour("Base Colour", Color) = (.255,.0,.0,1)
		[HDR]_HighlightColour("Highlight Colour", Color) = (.255,.0,.0,1)
		[HDR]_Test1("Test 1 Colour", Color) = (.255,.0,.0,1)

		_GridAreaSize("Grid Area Size", Range(1, 1000)) = 1000
		_GridAreaVisibleArea("Grid Area Visible Area", Range(0, 1000)) = 1000
		_GridThickness("Grid Thickness", Range(0.01, 1.0)) = 0.1
		_GridTileSize("Grid Tile SIze", Range(1, 10)) = 10
		_GridPositionOrigin ("Grid Position Origin", Vector) = (0, 0, 0)

		_Intensity("Emission Intensity", Range(-5,5)) = 0
	}
		SubShader
		{
        Tags { "Queue"="Transparent" }
			//Tags { "RenderType" = "Opaque" }
			LOD 100

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				// make fog work
				#pragma multi_compile_fog

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					float4 vertex : SV_POSITION;
					float3 worldPos : TEXCOORD1;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float4 _GridColour;
				float4 _Test1;
				float4 _BaseColour;
				float4 _HighlightColour;

				int _HighlightIsActive = 0; // 0 inactive, 1 active
				int _HighlightPositionsCount = 100;
				float _HighlightPositionsX[100];
				float _HighlightPositionsY[100];

				float _GridThickness;
				int _GridAreaSize;
				float _GridAreaVisibleArea;
				int _GridTileSize;
				float3 _GridPositionOrigin;

				float _GridLineThickness;
				float _Intensity;

				v2f vert(appdata v) // takes input information of object, this exists to create a frag (that's why it's called v2f, vertToFrag)
				{
					v2f o;
					o.worldPos = mul(unity_ObjectToWorld, v.vertex) - _GridPositionOrigin;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					//fixed4 gridColour = _BaseColour;
					fixed4 gridColour = tex2D(_MainTex, i.uv) * _BaseColour;

					float3 absoluteWorldPosition = i.worldPos + _GridPositionOrigin;
					float2 highlightPosition;

					float gridAreaSize = _GridAreaSize;
					float gridThickness = _GridThickness;
					int gridTileSize = _GridTileSize;
	
					float center = gridAreaSize / 2;
					float distanceToCenterX = distance(i.worldPos.x, center);
					float distanceToCenterY = distance(i.worldPos.y, center);
					float difference = _GridAreaSize - _GridAreaVisibleArea;
	
					// Used so the grid is closed on all sides
					int extraGridLineValue = 1;
	
					//Error prevention
					if (_GridAreaVisibleArea > gridAreaSize)
						_GridAreaVisibleArea = gridAreaSize;
	
					//Color the highlight area					
					if (_HighlightIsActive == 1)
					{
						for (int j = 0; j < _HighlightPositionsCount; j++)
						{
							highlightPosition = float2(_HighlightPositionsX[j], _HighlightPositionsY[j]);

							if (absoluteWorldPosition.x >= highlightPosition.x && absoluteWorldPosition.x <= highlightPosition.x + 1)
							{
								if (absoluteWorldPosition.y >= highlightPosition.y && absoluteWorldPosition.y <= highlightPosition.y + 1)
								{
									gridColour.rgb = lerp(gridColour.xyz, _HighlightColour.rgb, _HighlightColour.a * _HighlightColour.a); // lerp with the previous color and take into account the layer color opacity
								}
							}
						}
					}

					//Grid lines
					for (int j = -gridAreaSize / 2; j < gridAreaSize / 2 + extraGridLineValue; j += gridTileSize)
					{
						//Cut off extra lines from grid area
						if (i.worldPos.x < _GridAreaVisibleArea / 2 && i.worldPos.x > -(_GridAreaVisibleArea / 2))
							{
							if (i.worldPos.y < _GridAreaVisibleArea / 2 && i.worldPos.y > -(_GridAreaVisibleArea / 2))
								{				
									if (i.worldPos.x >= j - gridThickness / 2 && i.worldPos.x <= j + gridThickness / 2)
										gridColour.rgb = lerp(gridColour.xyz, _GridColour.rgb, _GridColour.a * _GridColour.a);
									if (i.worldPos.y >= j - gridThickness / 2 && i.worldPos.y <= j + gridThickness / 2)
										gridColour.rgb = lerp(gridColour.xyz, _GridColour.rgb, _GridColour.a * _GridColour.a);
							}            
						}
					}
				return float4(gridColour);
				}
					ENDCG
			}
		}
}