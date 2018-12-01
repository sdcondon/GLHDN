namespace GLHDN.ReactiveBuffers.UnitTests
{
    using FluentAssertions;
    using GLHDN.Core;
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using Xunit;

    public class ReactiveBufferTests
    {
        private ObservableCollection<Element> collection;
        private ReactiveBuffer<Vertex> target;
        private MockVertexArrayObject vao;

        public ReactiveBufferTests()
        {
            this.collection = new ObservableCollection<Element>();
            this.target = new ReactiveBuffer<Vertex>(
                vertexSource: ObservableHelpers.FromObservableCollection(
                    collection,
                    (Element a) => Enumerable.Range(1, a.vertexCount).Select(b => new Vertex(a.id, b)).ToArray()
                ),
                primitiveType: PrimitiveType.Points,
                atomCapacity: 5,
                indices: new[] { 0, 1 },
                makeVertexArrayObject: (p, a, i) => this.vao = new MockVertexArrayObject(p, a, i));
        }

        public static IEnumerable<object[]> TestCases => new List<object[]>()
        {
            MakeTestCase( "addition, const size",
                a =>
                {
                    a.Add(new Element(1, 2));
                    a.Add(new Element(2, 2));
                },
                new[] { new Vertex(1, 1), new Vertex(1, 2), new Vertex(2, 1), new Vertex(2, 2) }),
            MakeTestCase( "removal from middle, const size",
                a =>
                {
                    a.Add(new Element(1, 2));
                    a.Add(new Element(2, 2));
                    a.RemoveAt(0);
                },
                new[] { new Vertex(2, 1), new Vertex(2, 2) }),
            MakeTestCase( "removal from end, const size",
                a =>
                {
                    a.Add(new Element(1, 2));
                    a.Add(new Element(2, 2));
                    a.RemoveAt(1);
                },
                new[] { new Vertex(1, 1), new Vertex(1, 2) }),
            MakeTestCase( "replacement, const size",
                a =>
                {
                    a.Add(new Element(1, 2));
                    a.Add(new Element(2, 2));
                    a[0] = new Element(3, 2);
                },
                new[] { new Vertex(3, 1), new Vertex(3, 2), new Vertex(2, 1), new Vertex(2, 2) }),
            MakeTestCase( "clear",
                a =>
                {
                    a.Add(new Element(1, 2));
                    a.Add(new Element(2, 2));
                    a.Clear();
                    a.Add(new Element(3, 2));
                },
                new[] { new Vertex(3, 1), new Vertex(3, 2) }),
            MakeTestCase( "addition, varying sizes",
                a =>
                {
                    a.Add(new Element(1, 4));
                    a.Add(new Element(2, 2));
                },
                new[] { new Vertex(1, 1), new Vertex(1, 2), new Vertex(1, 3), new Vertex(1, 4), new Vertex(2, 1), new Vertex(2, 2) }),
            MakeTestCase( "removal, varying sizes",
                a =>
                {
                    a.Add(new Element(1, 2));
                    a.Add(new Element(2, 4));
                    a.RemoveAt(0);
                },
                new[] { new Vertex(2, 3), new Vertex(2, 4), new Vertex(2, 1), new Vertex(2, 2) }),
            MakeTestCase( "replacement, varying sizes - bigger",
                a =>
                {
                    a.Add(new Element(1, 2));
                    a.Add(new Element(2, 2));
                    a[0] = new Element(3, 4);
                },
                new[] { new Vertex(3, 1), new Vertex(3, 2), new Vertex(2, 1), new Vertex(2, 2), new Vertex(3, 3), new Vertex(3, 4) }),
            MakeTestCase( "replacement, varying sizes - smaller",
                a =>
                {
                    a.Add(new Element(1, 4));
                    a.Add(new Element(2, 2));
                    a[0] = new Element(3, 2);
                },
                new[] { new Vertex(3, 1), new Vertex(3, 2), new Vertex(2, 2), new Vertex(2, 1) }),
            MakeTestCase( "replacement at end, varying sizes - smaller",
                a =>
                {
                    a.Add(new Element(1, 2));
                    a.Add(new Element(2, 4));
                    a[1] = new Element(3, 2);
                },
                new[] { new Vertex(1, 1), new Vertex(1, 2), new Vertex(3, 1), new Vertex(3, 2) })
        };

        [Theory]
        [MemberData(nameof(TestCases))]
        public void Test(string description, Action<ObservableCollection<Element>> action, ICollection<Vertex> expectedVertices, ICollection<int> expectedIndices)
        {
            action(this.collection);
            this.vao.AttributeBuffers[0].Contents.Take(expectedVertices.Count).Should().BeEquivalentTo(expectedVertices);
            this.vao.IndexBuffer.Contents.Take(expectedIndices.Count).Should().BeEquivalentTo(expectedIndices);
        }

        private static object[] MakeTestCase(
            string description,
            Action<ObservableCollection<Element>> action,
            ICollection<Vertex> expectedVertices)
        {
            return new object[]
            {
                description,
                action,
                expectedVertices,
                Enumerable.Range(0, expectedVertices.Count).ToArray()
            };
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
