//The texture to distort
Texture tex;

//The corners of the quad which we are interpolating across
float2 corners[4];

sampler TextureSampler = sampler_state
{
    Texture = (tex);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	//Just pass our input through as output, no changes
    VertexShaderOutput output;
    output.Position = input.Position;
	output.TexCoord = input.TexCoord;

    return output;
}

float2 interpolateQuad(float s, float t)
{
	float2 p0_p1 = lerp(corners[0], corners[1], s);
	float2 p3_p2 = lerp(corners[3], corners[2], s);
	return lerp(p0_p1, p3_p2, t);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	//Just interpolate the input texture coordinates across the quad,
	//and then sample the texture at those interpolated coordinates
    return tex2D(TextureSampler, interpolateQuad(input.TexCoord.x, input.TexCoord.y));
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
