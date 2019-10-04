#version 330

// Interpolated values from the vertex shaders
in vec4 matColor;
in vec3 Position_worldspace;
in vec3 Normal_worldspace;
in vec3 Normal_cameraspace;
in vec3 EyeDirection_cameraspace;
in vec3 PointLightDirection_cameraspace;

// Ouput data
out vec4 color;

// Values that stay constant for the whole mesh.
//uniform mat4 MV;
uniform vec3 AmbientLightColor;
uniform vec3 DirectedLightDirection;
uniform vec3 DirectedLightColor;
uniform vec3 PointLightPosition_worldspace;
uniform vec3 PointLightColor;
uniform float PointLightPower;

void main(){

	// Material properties
	vec4 MaterialDiffuseColor = matColor;
	vec4 MaterialSpecularColor = matColor * vec4(0.1, 0.1, 0.1, 1);

	// Distance to the point light
	float distance = length(PointLightPosition_worldspace - Position_worldspace);

	float directedLightCosTheta;
	float cosTheta;
	float cosAlpha;
	if (dot(Normal_worldspace, Normal_worldspace) != 0)
	{
		directedLightCosTheta = clamp(dot(normalize(Normal_worldspace), normalize(-DirectedLightDirection)), 0, 1);

		// Normal of the computed fragment, in camera space
		vec3 n = normalize(Normal_cameraspace);
		// Direction of the light (from the fragment to the light)
		vec3 l = normalize(PointLightDirection_cameraspace);
		// Cosine of the angle between the normal and the light direction, 
		// clamped above 0
		//  - light is at the vertical of the triangle -> 1
		//  - light is perpendicular to the triangle -> 0
		//  - light is behind the triangle -> 0
		cosTheta = clamp(dot(n, l), 0, 1);

		// Eye vector (towards the camera)
		vec3 E = normalize(EyeDirection_cameraspace);
		// Direction in which the triangle reflects the light
		vec3 R = reflect(-l, n);
		// Cosine of the angle between the Eye vector and the Reflect vector,
		// clamped to 0
		//  - Looking into the reflection -> 1
		//  - Looking elsewhere -> < 1
		cosAlpha = clamp(dot(E, R), 0, 1);
	}
	else
	{
		// a bit hacky: probably a line fragment, which have zero normals
		directedLightCosTheta = 1;
		cosTheta = 1;
		cosAlpha = 1;
	}
	
	color = 
		// Ambient : simulates indirect lighting
		MaterialDiffuseColor * vec4(AmbientLightColor, 1)
		// Directed
		+ MaterialDiffuseColor * vec4(DirectedLightColor, 1) * directedLightCosTheta
		// Diffuse : "color" of the object
		+ MaterialDiffuseColor * vec4(PointLightColor, 1) * PointLightPower * cosTheta / (distance * distance)
		// Specular : reflective highlight, like a mirror
		+ MaterialSpecularColor * vec4(PointLightColor, 1) * PointLightPower * pow(cosAlpha, 5) / (distance * distance);
}