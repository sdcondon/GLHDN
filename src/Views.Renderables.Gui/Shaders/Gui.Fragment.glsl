#version 330 core

// Interpolated values from the vertex shaders
in vec4 vColor;
flat in float iTexZ;
in vec2 vTexXY;

// Ouput data
out vec4 color;

// uniforms
uniform sampler2DArray text;

void main() {
	if (iTexZ > -1) {
		vec3 texCoord = vec3(vTexXY.x, vTexXY.y, int(iTexZ));
		vec4 texel = texture(text, texCoord);

		color = vColor * vec4(1.0, 1.0, 1.0, texel.a);
		//color.a = max(color.a, 0.3);
	}
	else {
		color = vColor;
	}
}