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
        public static IEnumerable<object[]> TestCases
        {
            get
            {
                object[] makeTestCase(string description, Action<ObservableCollection<In>> action, ICollection<Vertex> expectedVertices) =>
                    new object[] { description, action, expectedVertices, Enumerable.Range(0, expectedVertices.Count).ToArray() };

                return new List<object[]>()
                {
                    makeTestCase(
                        "addition, const size",
                        a => { a.Add(new In(1, 2)); a.Add(new In(2, 2)); },
                        new[] { new Vertex(1, 1), new Vertex(1, 2), new Vertex(2, 1), new Vertex(2, 2) }),

                    makeTestCase(
                        "removal from middle, const size",
                        a => { a.Add(new In(1, 2)); a.Add(new In(2, 2)); a.RemoveAt(0); },
                        new[] { new Vertex(2, 1), new Vertex(2, 2) }),

                    makeTestCase(
                        "removal from end, const size",
                        a => { a.Add(new In(1, 2)); a.Add(new In(2, 2)); a.RemoveAt(1); },
                        new[] { new Vertex(1, 1), new Vertex(1, 2) }),

                    makeTestCase(
                        "replacement, const size",
                        a => { a.Add(new In(1, 2)); a.Add(new In(2, 2)); a[0] = new In(3, 2); },
                        new[] { new Vertex(3, 1), new Vertex(3, 2), new Vertex(2, 1), new Vertex(2, 2) }),

                    makeTestCase(
                        "clear",
                        a => { a.Add(new In(1, 2));  a.Add(new In(2, 2)); a.Clear(); a.Add(new In(3, 2)); },
                        new[] { new Vertex(3, 1), new Vertex(3, 2) }),

                    makeTestCase(
                        "addition, varying sizes",
                        a => { a.Add(new In(1, 4)); a.Add(new In(2, 2)); },
                        new[] { new Vertex(1, 1), new Vertex(1, 2), new Vertex(1, 3), new Vertex(1, 4), new Vertex(2, 1), new Vertex(2, 2) }),

                    makeTestCase(
                        "removal, varying sizes",
                        a => { a.Add(new In(1, 2)); a.Add(new In(2, 4)); a.RemoveAt(0); },
                        new[] { new Vertex(2, 3), new Vertex(2, 4), new Vertex(2, 1), new Vertex(2, 2) }),

                    makeTestCase(
                        "replacement, varying sizes - bigger",
                        a => { a.Add(new In(1, 2)); a.Add(new In(2, 2)); a[0] = new In(3, 4); },
                        new[] { new Vertex(3, 1), new Vertex(3, 2), new Vertex(2, 1), new Vertex(2, 2), new Vertex(3, 3), new Vertex(3, 4) }),

                    makeTestCase(
                        "replacement, varying sizes - smaller",
                        a => { a.Add(new In(1, 4)); a.Add(new In(2, 2)); a[0] = new In(3, 2); },
                        new[] { new Vertex(3, 1), new Vertex(3, 2), new Vertex(2, 2), new Vertex(2, 1) }),

                    makeTestCase(
                        "replacement at end, varying sizes - smaller",
                        a => { a.Add(new In(1, 2)); a.Add(new In(2, 4)); a[1] = new In(3, 2); },
                        new[] { new Vertex(1, 1), new Vertex(1, 2), new Vertex(3, 1), new Vertex(3, 2) })
                };
            }
        }

        [Theory]
        [MemberData(nameof(TestCases))]
        public void Test(string description, Action<ObservableCollection<In>> action, ICollection<Vertex> expectedVertices, ICollection<int> expectedIndices)
        {
            var collection = new ObservableCollection<In>(); // todo: use a subject instead
            MockVertexArrayObject vao = null;
            var target = new ReactiveBuffer<Vertex>(
                collection.ToObservable((In a) => Enumerable.Range(1, a.vertexCount).Select(b => new Vertex(a.id, b)).ToArray()),
                PrimitiveType.Points,
                5,
                new[] { 0, 1 },
                (p, a, i) => vao = new MockVertexArrayObject(p, a, i));

            action(collection);

            vao.AttributeBuffers[0].Contents.Take(expectedVertices.Count).Should().BeEquivalentTo(expectedVertices);
            vao.IndexBuffer.Contents.Take(expectedIndices.Count).Should().BeEquivalentTo(expectedIndices);
        }

        public class In : INotifyPropertyChanged
        {
            public readonly int id;
            public readonly int vertexCount;

            public In(int id, int vertexCount)
            {
                this.id = id;
                this.vertexCount = vertexCount;
                //this.PropertyChanged = null;
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
