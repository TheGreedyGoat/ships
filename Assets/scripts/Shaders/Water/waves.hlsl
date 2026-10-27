

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
float4x4 UNITY_MATRIX_V ;
float _CTime = 1;
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
            + _CTime;

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

float3 multiLerp(float3 a, float3 b, float t, int iterations){
    float3 value = b;
    for(int i = 0; i < iterations; i++){
        value  = lerp(a, value, t);
    }

    return value;
}
    
// float pow

float4 frag (v2f i) : SV_Target {
    float3 normal = -normalize(cross(ddx(i.worldPos), ddy(i.worldPos)));
    Light mainLight = GetMainLight();
    
    float NdotL =  saturate(dot(normal, mainLight.direction));
    NdotL = pow(NdotL , 1);
    float3 ambient = SampleSH(normal);
    float3 lighting =  mainLight.color.xyz;// * NdotL;
    
    // float3 cameraForwardWS = normalize(-UNITY_MATRIX_V[2].xyz);

    float3 c = multiLerp(_BaseColor, lighting, NdotL, 10);
    return float4(c, _BaseColor.a);
}

