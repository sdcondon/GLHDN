namespace GLHDN.Core.UnitTests
{
    using FluentAssertions;
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using Xunit;

    public class CollectionBoundBufferTests
    {
        public static IEnumerable<object[]> TestCases
        {
            get
            {
                object[] makeTestCase(string description, Action<ObservableCollection<Element>> action, ICollection<Vertex> expectedVertices) =>
                    new object[] { description, action, expectedVertices, Enumerable.Range(0, expectedVertices.Count).ToArray() };

                return new[]
                {
                    makeTestCase( "addition, const size",
                        a =>
                        {
                            a.Add(new Element(1, 2));
                            a.Add(new Element(2, 2));
                        },
                        new[] { new Vertex(1, 1), new Vertex(1, 2), new Vertex(2, 1), new Vertex(2, 2) }),
                    makeTestCase( "removal from middle, const size",
                        a =>
                        {
                            a.Add(new Element(1, 2));
                            a.Add(new Element(2, 2));
                            a.RemoveAt(0);
                        },
                        new[] { new Vertex(2, 1), new Vertex(2, 2) }),
                    makeTestCase( "removal from end, const size",
                        a =>
                        {
                            a.Add(new Element(1, 2));
                            a.Add(new Element(2, 2));
                            a.RemoveAt(1);
                        },
                        new[] { new Vertex(1, 1), new Vertex(1, 2) }),
                    makeTestCase( "replacement, const size",
                        a =>
                        {
                            a.Add(new Element(1, 2));
                            a.Add(new Element(2, 2));
                            a[0] = new Element(3, 2);
                        },
                        new[] { new Vertex(3, 1), new Vertex(3, 2), new Vertex(2, 1), new Vertex(2, 2) }),
                    makeTestCase( "clear",
                        a =>
                        {
                            a.Add(new Element(1, 2));
                            a.Add(new Element(2, 2));
                            a.Clear();
                            a.Add(new Element(3, 2));
                        },
                        new[] { new Vertex(3, 1), new Vertex(3, 2) }),
                    makeTestCase( "addition, varying sizes",
                        a =>
                        {
                            a.Add(new Element(1, 4));
                            a.Add(new Element(2, 2));
                        },
                        new[] { new Vertex(1, 1), new Vertex(1, 2), new Vertex(1, 3), new Vertex(1, 4), new Vertex(2, 1), new Vertex(2, 2) }),
                    makeTestCase( "removal, varying sizes",
                        a =>
                        {
                            a.Add(new Element(1, 2));
                            a.Add(new Element(2, 4));
                            a.RemoveAt(0);
                        },
                        new[] { new Vertex(2, 3), new Vertex(2, 4), new Vertex(2, 1), new Vertex(2, 2) }),
                    makeTestCase( "replacement, varying sizes - bigger",
                        a =>
                        {
                            a.Add(new Element(1, 2));
                            a.Add(new Element(2, 2));
                            a[0] = new Element(3, 4);
                        },
                        new[] { new Vertex(3, 1), new Vertex(3, 2), new Vertex(2, 1), new Vertex(2, 2), new Vertex(3, 3), new Vertex(3, 4) }),
                    makeTestCase( "replacement, varying sizes - smaller",
                        a =>
                        {
                            a.Add(new Element(1, 4));
                            a.Add(new Element(2, 2));
                            a[0] = new Element(3, 2);
                        },
                        new[] { new Vertex(3, 1), new Vertex(3, 2), new Vertex(2, 2), new Vertex(2, 1) }),
                    makeTestCase( "replacement at end, varying sizes - smaller",
                        a =>
                        {
                            a.Add(new Element(1, 2));
                            a.Add(new Element(2, 4));
                            a[1] = new Element(3, 2);
                        },
                        new[] { new Vertex(1, 1), new Vertex(1, 2), new Vertex(3, 1), new Vertex(3, 2) })
                };
            }
        }

        [Theory]
        [MemberData(nameof(TestCases))]
        public void Test(string description, Action<ObservableCollection<Element>> action, ICollection<Vertex> expectedVertices, ICollection<int> expectedIndices)
        {
            var collection = new ObservableCollection<Element>();
            MockVertexArrayObject vao = null;
            var target = new CollectionBoundBuffer<Element, Vertex>(
                collection: collection,
                primitiveType: PrimitiveType.Points,
                atomCapacity: 5,
                vertexGetter: a => Enumerable.Range(1, a.vertexCount).Select(b => new Vertex(a.id, b)).ToArray(),
                indices: new[] { 0, 1 },
                makeVertexArrayObject: (p, a, i) => vao = new MockVertexArrayObject(p, a, i));

            action(collection);

            vao.AttributeBuffers[0].Contents.Take(expectedVertices.Count).Should().BeEquivalentTo(expectedVertices);
            vao.IndexBuffer.Contents.Take(expectedIndices.Count).Should().BeEquivalentTo(expectedIndices);
        }

        public class Element : INotifyPropertyChanged
        {
            public PropertyChangedEventHandler _pc;

            public readonly int id;
            public readonly int vertexCount;

            public Element(int id, int vertexCount)
            {
                this.id = id;
                this.vertexCount = vertexCount;
                //this.PropertyChanged = null;
            }

            public event PropertyChangedEventHandler PropertyChanged
            {
                add => _pc += value;
                remove => _pc -= value;
            }
        }

        public struct Vertex
        {
            public readonly int elementId;
            public readonly int vertexId;

            public Vertex(int elementId, int vertexId)
            {
                this.elementId = elementId;
                this.vertexId = vertexId;
            }
        }
    }
}
