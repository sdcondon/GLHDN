#version 330 core

// Interpolated values from the vertex shaders
in vec4 matColor;
in float vborderWidth;
in vec2 velementPos;
flat in vec2 velementSize;
flat in float elementLayer;

// Ouput data
out vec4 color;

// uniforms
uniform sampler2DArray text;

void main() {
	//color = matColor;
	
	if (elementLayer == -1) {
		if (velementPos.x < vborderWidth
			|| velementPos.y < vborderWidth
			|| abs(velementPos.x - velementSize.x) < vborderWidth
			|| abs(velementPos.y - velementSize.y) < vborderWidth) {
			color = vec4(1, 1, 1, 1);
		}
		else {
			color = matColor;
		}
	}
	else {
		vec4 sampled = vec4(1.0, 1.0, 1.0, texelFetch(text, ivec3(velementPos.x, velementSize.y - velementPos.y, elementLayer), 0).a);
		//vec4 sampled = vec4(1.0, 1.0, 1.0, texture(text, vec3(velementPos.x, velementSize.y - velementPos.y, elementLayer)).a);
		color = matColor * sampled;
	}
}