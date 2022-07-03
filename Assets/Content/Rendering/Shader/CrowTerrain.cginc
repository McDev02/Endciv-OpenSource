// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

struct Input
{
	float2 uv_Splat0 : TEXCOORD0;
	float2 uv_Splat1 : TEXCOORD1;
	float3 wPos : TEXCOORD2;
	UNITY_FOG_COORDS(5)
};

sampler2D  _SplatHeight;
sampler2D _Splat0,_Splat1,_Splat2,_Splat3,_Splat4;
half _HeightTile;
half4 _Edges;

	sampler2D _Normal0, _Normal1, _Normal2, _Normal3, _Normal4;

void SplatmapVert(inout appdata_full v, out Input data)
{
	UNITY_INITIALIZE_OUTPUT(Input, data);
	float4 pos = UnityObjectToClipPos (v.vertex);
	data.wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
	UNITY_TRANSFER_FOG(data, pos);
		v.tangent.xyz = cross(v.normal, float3(0,0,1));
		v.tangent.w = -1;
}

void SplatmapMix(Input IN, inout half4 splat_control, out fixed4 mixedDiffuse, out fixed3 mixedNormal)
{
	half4 splat_height = tex2D(_SplatHeight, IN.uv_Splat1*_HeightTile);
	//splat_control = saturate(pow((splat_control + (splat_height-0.5)), 4));
	splat_control.r = saturate(pow((splat_control.r*1.5 + (splat_height.r-0.5)), _Edges.r));
	splat_control.g = saturate(pow((splat_control.g*1.5 + (splat_height.g-0.5)), _Edges.g));
	splat_control.b = saturate(pow((splat_control.b*1.5 + (splat_height.b-0.5)), _Edges.b));
	splat_control.a = saturate(pow((splat_control.a*1.5 + (splat_height.a-0.5)), _Edges.a));
		
		
	mixedDiffuse = tex2D(_Splat0, IN.uv_Splat0);
	mixedDiffuse = lerp(mixedDiffuse, tex2D(_Splat1, IN.uv_Splat1),  splat_control.r);
	mixedDiffuse = lerp(mixedDiffuse, tex2D(_Splat2, IN.uv_Splat1),  splat_control.g);
	mixedDiffuse = lerp(mixedDiffuse, tex2D(_Splat3, IN.uv_Splat1),  splat_control.b);
	mixedDiffuse = lerp(mixedDiffuse, tex2D(_Splat4, IN.uv_Splat1),  splat_control.a);
	
		fixed4 nrm = tex2D(_Normal0, IN.uv_Splat0);
		nrm = lerp(nrm, tex2D(_Normal1, IN.uv_Splat1),  splat_control.r);
		nrm = lerp(nrm, tex2D(_Normal2, IN.uv_Splat1),  splat_control.g);
		nrm = lerp(nrm, tex2D(_Normal3, IN.uv_Splat1),  splat_control.b);
		nrm = lerp(nrm, tex2D(_Normal4, IN.uv_Splat1),  splat_control.a);
		mixedNormal = UnpackNormal(nrm);

}