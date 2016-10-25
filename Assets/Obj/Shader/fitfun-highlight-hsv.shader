//高亮+hsv改色
Shader "Custom/fitfun-highlight-hsv" 
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}		
		_Color("Color", Color ) = ( 0,0,0,0 )
		_Factor("Factor", Range( 0.1, 1.0 ) ) = 0.9
		_Scale("Scale", float ) = 0.9
		_H ("H", Range(0,359)) = 0
        _S ("S", Range(0,3.0)) = 1.0
        _V ("V", Range(0,3.0)) = 1.0
    }
    SubShader
    {
		Tags
		{
			"RenderType" = "Opaque"
		}
		PASS
		{
			Lighting Off
			ZWrite   On
			Fog { Mode Off }
			Blend Off
			
			LOD 150
	
			CGPROGRAM
			
			#pragma vertex		vertFunc
			#pragma fragment	fragFunc
		
			#include "UnityCG.cginc"

			sampler2D	_MainTex;
			fixed4		_Color;
			float		_Factor;
			float		_Scale;
			half _H;
			half _S;
			half _V;

			struct	VsInData
			{
				float4	vertex   : POSITION;
				float4	normal   : NORMAL;
				float2	texcoord : TEXCOORD0;
			};

			struct	VsOutData
			{
				fixed4	pos		 : POSITION;
				fixed3	texcoord : TEXCOORD0;
			};

			 fixed3 RGBConvertToHSV(fixed3 rgb)
			{
				fixed R = rgb.x,G = rgb.y,B = rgb.z;
				fixed3 hsv;
				fixed max1=max(R,max(G,B));
				fixed min1=min(R,min(G,B));
				if (R == max1) 
				{
					hsv.x = (G-B)/(max1-min1);
				}
				if (G == max1) 
				{
					hsv.x = 2 + (B-R)/(max1-min1);
					}
				if (B == max1) 
				{
					hsv.x = 4 + (R-G)/(max1-min1);
					}
				hsv.x = hsv.x * 60.0;   
				if (hsv.x < 0) 
					hsv.x = hsv.x + 360;
				hsv.z=max1;
				hsv.y=(max1-min1)/max1;
				return hsv;
			}
			 fixed3 HSVConvertToRGB(fixed3 hsv)
			{
				fixed R,G,B;
				//float3 rgb;
				if( hsv.y == 0 )
				{
					R=G=B=hsv.z;
				}
				else
				{
					hsv.x = hsv.x/60.0; 
					int i = (int)hsv.x;
					fixed f = hsv.x - (float)i;
					fixed a = hsv.z * ( 1 - hsv.y );
					fixed b = hsv.z * ( 1 - hsv.y * f );
					fixed c = hsv.z * ( 1 - hsv.y * (1 - f ) );
					if(i==0){
						 R = hsv.z; 
						 G = c; 
						 B = a;
					}
					else if(i==1){
						R = b; 
						G = hsv.z;
						B = a; 
					}
					else if(i==2){
						R = a; 
						G = hsv.z; 
						B = c; 
					}
					else if(i==3){
						R = a; 
						G = b; 
						B = hsv.z; 
					}
					else if(i==4){
						R = c; 
						G = a; 
						B = hsv.z; 
					}else{
						R = hsv.z; 
						G = a; 
						B = b; 
					}
					
					
				}
				return fixed3(R,G,B);
			}       

			VsOutData	vertFunc( VsInData vsDataIn )
			{
				VsOutData	outVal;

				fixed3 viewDir  = ObjSpaceViewDir( vsDataIn.vertex );

				outVal.pos = mul( UNITY_MATRIX_MVP, vsDataIn.vertex );
				outVal.texcoord.xy = vsDataIn.texcoord;
				outVal.texcoord.z  = max( 0.0, dot( normalize( viewDir ), normalize( vsDataIn.normal ) ) + _Factor - 1.0  );

				return	outVal;
			}

			half4		fragFunc( VsOutData val ) : COLOR			
			{
				half4 result = tex2D( _MainTex, val.texcoord.xy );
				//result.rgb *= ( 1.0 + 5*val.texcoord.z * result.a * _Color.rgb * _Scale );
				result.rgb *= ( 1.0 + 5*val.texcoord.z * result.a * _Scale );
				result.rgb+=_Color.rgb;
				result.a = 1.0;

				fixed3 colorHSV;    
				colorHSV.xyz = RGBConvertToHSV(result.xyz);   //转换为HSV
				colorHSV.x += _H; //调整偏移Hue值
				colorHSV.x = colorHSV.x%360;    //超过360的值从0开始

				colorHSV.y *= _S;  //调整饱和度
				colorHSV.z *= _V;                           

				result.xyz = HSVConvertToRGB(colorHSV.xyz);   //将调整后的HSV，转换为RGB颜色

				return result;
			}

			ENDCG

		}		
    }

	Fallback "DMCore/Root/Solid"
}

