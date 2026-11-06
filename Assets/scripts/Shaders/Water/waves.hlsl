

#ifdef __INTELLISENSE__
struct Light{
    float3 direction;

    float4 color;
};
Light GetMainLight(){
    Light l;
    return l;
}
float3 SampleSH(float3 normal){ return normal;}
float3 TransformWorldToHClip(float3 worldPos){ return worldPos ;}

float3 TransformObjectToWorld( float3 vertex ){
    return vertex;
}
float4 UnityObjectToClipPos(float4 vertex)
{
    return vertex;
}
float4x4 unity_CameraToWorld;
float3 _WorldSpaceCameraPos;
float4x4 UNITY_MATRIX_V ;
float _CTime = 1;
float _TimeScales[];
float amplitudes[2];
float2 directions[2];
float frequencies[2];

#endif

// 
//  ######   #######  ########  ######## 
// ##    ## ##     ## ##     ## ##       
// ##       ##     ## ##     ## ##       
// ##       ##     ## ##     ## ######   
// ##       ##     ## ##     ## ##       
// ##    ## ##     ## ##     ## ##       
//  ######   #######  ########  ######## 
// 

const float TAU  = 2* radians(180);

float4 _BaseColor = float4(1,1,1,1);
float4 _FogColor = float4(1,1,1,1);

float _FogStartDist = 10;
float _FogGradientLength = 10;
struct v2f {
    float4 pos : SV_POSITION;
    float3 worldPos : TEXCOORD0;
};

struct Attributes
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
};


float3 offset(float3 vertex){
    float3 ofs = float3(0,0,0);
    for(int i = 0; i < 2; i++){
        float2 dir = normalize(directions[i].xy);
        float phase = dot(vertex.xz, dir)
            * frequencies[i]
            + _CTime / _TimeScales[i];

    //    ofs += float3(cosOff * dir.x,sinOff, cosOff * dir.y) * amplitudes[i];
    float cosPhase = cos(phase);
    ofs += float3( cosPhase * dir.x,sin(phase)* amplitudes[i], cosPhase * dir.y);
    }
    return ofs;
}



v2f vert (Attributes v)
{
    v2f o;
    float3 world = TransformObjectToWorld( v.vertex.xyz);
    float3 of = offset(world);
    float3 worldDisplacement = world + of;

    o.pos = TransformWorldToHClip(worldDisplacement);
    o.worldPos = worldDisplacement;
    return o; 
}


float mag (float3 v){
    return sqrt(v.x*v.x + v.y*v.y +v.z*v.z);
}


float angle(float3 a, float3 b){
    return acos(dot(a, b)/(mag(a),mag(b)));
}

float inverseLerp(float value, float a,float b){
    return (value-a) / (b-a);
}

float remap(float value, float a, float b, float ta, float tb){
    return lerp(ta, tb, inverseLerp(value, a, b));
}
float hashTriangle(uint x)
{
    x ^= x >> 16;
    x *= 0x7feb352du;
    x ^= x >> 15;
    x *= 0x846ca68bu;
    x ^= x >> 16;
    return (x & 0x00ffffffu) / 16777215.0;
}

float4 frag (v2f i, uint primitiveID: SV_PrimitiveID) : SV_Target {
    float3 normal = normalize(cross(ddy(i.worldPos), ddx(i.worldPos)));

    half3 n = normalize(normal);
    Light mainLight = GetMainLight();
    
    float NdotL =  saturate(dot(n, mainLight.direction));
    NdotL = pow(NdotL, 3);
    float3 ambient = SampleSH(n);

    float3 lighting =   NdotL * mainLight.color.xyz ;

    
    float variation = lerp(15, 20, hashTriangle(primitiveID));
    float3 forward = mul((float3x3)unity_CameraToWorld, float3(0,0,1));
    float fogEnd = _FogStartDist + _FogGradientLength;
    float dist = clamp(mag(i.worldPos - _WorldSpaceCameraPos), _FogStartDist, fogEnd);

    float fogValue = inverseLerp(dist, _FogStartDist, fogEnd);
    
    float3 lightedColor = _BaseColor.rgb * lighting * variation;

    return float4( lerp(lightedColor, _FogColor.xyz, fogValue) , _BaseColor.a);
}

