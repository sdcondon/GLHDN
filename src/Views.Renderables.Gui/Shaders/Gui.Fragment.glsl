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
		ivec3 texCoord = ivec3(velementPos.x, velementSize.y - velementPos.y, elementLayer);
		vec4 texel = texelFetch(text, texCoord, 0);

		//vec3 texCoord = vec3(velementPos.x / velementSize.x, (velementSize.y - velementPos.y) / velementSize.y, int(elementLayer));
		//vec4 texel = texture(text, texCoord);

		color = matColor * vec4(1.0, 1.0, 1.0, texel.a);
		color.a = max(color.a, 0.3);
	}
}