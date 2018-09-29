namespace GLHDN.Core.UnitTests.VaoWrappers
{
    using FluentAssertions;
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using Xunit;

    public class BoundBufferTests
    {
        private ObservableCollection<Element> collection;
        private BoundBuffer<Element, Vertex> target;
        private MockVertexArrayObject vao;

        public BoundBufferTests()
        {
            this.collection = new ObservableCollection<Element>();
            this.target = new BoundBuffer<Element, Vertex>(
                collection: this.collection,
                primitiveType: PrimitiveType.Points,
                objectCapacity: 5,
                attributeGetter: a => Enumerable.Range(1, a.vertexCount).Select(b => new Vertex(a.id, b)).ToArray(),
                indices: new[] { 0, 1 },
                makeVertexArrayObject: (p, a, i) => this.vao = new MockVertexArrayObject(p, a, i));
        }

        public static IEnumerable<object[]> TestCases => new List<object[]>()
        { 
            MakeTestCase( // addition, const size
                a =>
                {
                    a.Add(new Element(1, 2));
                    a.Add(new Element(2, 2));
                },
                new[] { new Vertex(1, 1), new Vertex(1, 2), new Vertex(2, 1), new Vertex(2, 2) }),
            MakeTestCase( // removal, const size
                a =>
                {
                    a.Add(new Element(1, 2));
                    a.Add(new Element(2, 2));
                    a.Remove(new Element(1, 2));
                },
                new[] { new Vertex(2, 1), new Vertex(2, 2) }),
            MakeTestCase( // addition, varying sizes
                a =>
                {
                    a.Add(new Element(1, 4));
                    a.Add(new Element(2, 2));
                    a.Remove(new Element(1, 4));
                },
                new[] { new Vertex(2, 1), new Vertex(2, 2) }),
            MakeTestCase( // removal, varying sizes
                a =>
                {
                    a.Add(new Element(1, 2));
                    a.Add(new Element(2, 4));
                    a.Remove(new Element(1, 2));
                },
                new[] { new Vertex(2, 3), new Vertex(2, 4), new Vertex(2, 1), new Vertex(2, 2) }),
            MakeTestCase( // clear
                a =>
                {
                    a.Add(new Element(1, 2));
                    a.Add(new Element(2, 2));
                    a.Clear();
                    a.Add(new Element(3, 2));
                },
                new[] { new Vertex(3, 1), new Vertex(3, 2) }),
        };

        [Theory]
        [MemberData(nameof(TestCases))]
        public void Test(Action<ObservableCollection<Element>> action, ICollection<Vertex> expectedVertices, ICollection<int> expectedIndices)
        {
            action(this.collection);
            this.vao.AttributeBuffers[0].Contents.Take(expectedVertices.Count).Should().BeEquivalentTo(expectedVertices);
            this.vao.IndexBuffer.Contents.Take(expectedIndices.Count).Should().BeEquivalentTo(expectedIndices);
        }

        private static object[] MakeTestCase(
            Action<ObservableCollection<Element>> action,
            ICollection<Vertex> expectedVertices)
        {
            return new object[]
            {
                action,
                expectedVertices,
                Enumerable.Range(0, expectedVertices.Count)
            };
        }

        public struct Element : INotifyPropertyChanged
        {
            public readonly int id;
            public readonly int vertexCount;

            public Element(int id, int vertexCount)
            {
                this.id = id;
                this.vertexCount = vertexCount;
                this.PropertyChanged = null;
            }

            public event PropertyChangedEventHandler PropertyChanged;
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
