namespace GLHDN.Core
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Numerics;

    /// <summary>
    /// Very, VERY simple OBJ loader (nabbed from an OpenGL tutorial)
    /// Here is a short list of features a real function would provide : 
    /// - Binary files. Reading a model should be just a few memcpy's away, not parsing a file at runtime. In short : OBJ is not very great.
    /// - Animations and bones (includes bones weights)
    /// - Multiple UVs
    /// - All attributes should be optional, not "forced"
    /// - More stable. Change a line in the OBJ file and it crashes.
    /// - More secure. Change another line and you can inject code.
    /// - Loading from memory, stream, etc
    /// </summary>
    public static class ObjLoader
    {
        /// <summary>
        /// Loads data from an OBJ file.
        /// </summary>
        /// <param name="path">The path of the OBJ file.</param>
        /// <param name="vertexPositions">The list to be populated with the position of each vertex.</param>
        /// <param name="vertexUvs">The list to be populated with the texture co-ordinate of each vertex.</param>
        /// <param name="vertexNormals">The list to be populated with the normal of each vertex.</param>
        /// <returns>True if the OBJ file was successfully loaded, otherwise false.</returns>
        public static bool LoadObj(
            string path,
            IList<Vector3> vertexPositions,
            IList<Vector2> vertexUvs,
            IList<Vector3> vertexNormals)
        {
            Trace.WriteLine($"Loading OBJ file {path}");

            var vertexIndices = new List<int>();
            var uvIndices = new List<int>();
            var normalIndices = new List<int>();
            var temp_vertices = new List<Vector3>();
            var temp_uvs = new List<Vector2>();
            var temp_normals = new List<Vector3>();

            using (var file = File.Open(path, FileMode.Open, FileAccess.Read))
            using (var fileReader = new StreamReader(file))
            {
                while (!fileReader.EndOfStream)
                {
                    var line = fileReader.ReadLine();

                    if (line.StartsWith("v "))
                    {
                        var coords = line.Split(' ').Skip(1).Select(f => float.Parse(f)).ToArray();
                        temp_vertices.Add(new Vector3(coords[0], coords[1], coords[2]));
                    }
                    else if (line.StartsWith("vt "))
                    {
                        var coords = line.Split(' ').Skip(1).Select(f => float.Parse(f)).ToArray();
                        // Invert V coordinate since we will only use DDS texture, which are inverted. Remove if you want to use TGA or BMP loaders.
                        temp_uvs.Add(new Vector2(coords[0], -coords[1]));
                    }
                    else if (line.StartsWith("vn "))
                    {
                        var coords = line.Split(' ').Skip(1).Select(f => float.Parse(f)).ToArray();
                        temp_normals.Add(new Vector3(coords[0], coords[1], coords[2]));
                    }
                    else if (line.StartsWith("f "))
                    {
                        var vertexIndex = new int[3];
                        var uvIndex = new int[3];
                        var normalIndex = new int[3];

                        var values = line.Split(' ').Skip(1).Select(s => s.Split('/').Select(d => int.Parse(d)).ToArray()).ToArray();

                        vertexIndices.Add(values[0][0]);
                        vertexIndices.Add(values[1][0]);
                        vertexIndices.Add(values[2][0]);
                        uvIndices.Add(values[0][1]);
                        uvIndices.Add(values[1][1]);
                        uvIndices.Add(values[2][1]);
                        normalIndices.Add(values[0][2]);
                        normalIndices.Add(values[1][2]);
                        normalIndices.Add(values[2][2]);
                    }
                    // Else probably a comment, just ignore
                }

                // For each vertex of each triangle
                for (int i = 0; i < vertexIndices.Count; i++)
                {
                    // Get the indices of its attributes
                    var vertexIndex = vertexIndices[i];
                    var uvIndex = uvIndices[i];
                    var normalIndex = normalIndices[i];

                    // Get the attributes thanks to the index
                    var vertex = temp_vertices[vertexIndex - 1];
                    var uv = temp_uvs[uvIndex - 1];
                    var normal = temp_normals[normalIndex - 1];

                    // Put the attributes in buffers
                    vertexPositions.Add(vertex);
                    vertexUvs.Add(uv);
                    vertexNormals.Add(normal);
                }
            }

	        return true;
        }
    }
}
