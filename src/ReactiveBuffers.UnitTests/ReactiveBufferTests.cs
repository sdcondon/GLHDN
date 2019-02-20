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
                object[] makeTestCase(Action<ObservableCollection<In>> action, ICollection<(int, int)> expectedVertices) =>
                    new object[] { action, expectedVertices, Enumerable.Range(0, expectedVertices.Count).ToArray() };

                return new List<object[]>()
                {
                    makeTestCase( // addition, const size
                        a => { a.Add(1, 2); a.Add(2, 2); },
                        new[] { (1, 1), (1, 2), (2, 1), (2, 2) }),

                    makeTestCase( // removal from middle, const size
                        a => { a.Add(1, 2); a.Add(2, 2); a.RemoveAt(0); },
                        new[] { (2, 1), (2, 2) }),

                    makeTestCase( // removal from end, const size
                        a => { a.Add(1, 2); a.Add(2, 2); a.RemoveAt(1); },
                        new[] { (1, 1), (1, 2) }),

                    makeTestCase( // replacement, const size
                        a => { a.Add(1, 2); a.Add(2, 2); a[0] = (3, 2); },
                        new[] { (3, 1), (3, 2), (2, 1), (2, 2) }),

                    makeTestCase( // clear
                        a => { a.Add(1, 2); a.Add(2, 2); a.Clear(); a.Add(3, 2); },
                        new[] { (3, 1), (3, 2) }),

                    makeTestCase( // addition, varying sizes
                        a => { a.Add(1, 4); a.Add(2, 2); },
                        new[] { (1, 1), (1, 2), (1, 3), (1, 4), (2, 1), (2, 2) }),

                    makeTestCase( // removal, varying sizes
                        a => { a.Add(1, 2); a.Add(2, 4); a.RemoveAt(0); },
                        new[] { (2, 3), (2, 4), (2, 1), (2, 2) }),

                    makeTestCase( // replacement, varying sizes - bigger
                        a => { a.Add(1, 2); a.Add(2, 2); a[0] = (3, 4); },
                        new[] { (3, 1), (3, 2), (2, 1), (2, 2), (3, 3), (3, 4) }),

                    makeTestCase( // replacement, varying sizes - smaller
                        a => { a.Add(1, 4); a.Add(2, 2); a[0] = (3, 2); },
                        new[] { (3, 1), (3, 2), (2, 2), (2, 1) }),

                    makeTestCase( // replacement at end, varying sizes - smaller
                        a => { a.Add(1, 2); a.Add(2, 4); a[1] = (3, 2); },
                        new[] { (1, 1), (1, 2), (3, 1), (3, 2) })
                };
            }
        }

        [Theory]
        [MemberData(nameof(TestCases))]
        public void Test(
            Action<ObservableCollection<In>> action,
            ICollection<(int, int)> expectedVertices,
            ICollection<int> expectedIndices)
        {
            // Arrange
            var source = new ObservableCollection<In>(); // todo: use a subject instead
            var target = (MemoryVertexArrayObject)null;
            var subject = new ReactiveBuffer<(int elementId, int vertexId)>(
                source.ToObservable((In a) => Enumerable.Range(1, a.vertexCount).Select(b => (a.id, b)).ToArray()),
                PrimitiveType.Points,
                5,
                new[] { 0, 1 },
                (p, a, i) => target = new MemoryVertexArrayObject(p, a, i));

            // Act 
            action(source);

            // Assert
            target.AttributeBuffers[0].Contents.Take(expectedVertices.Count).Should().BeEquivalentTo(expectedVertices);
            target.IndexBuffer.Contents.Take(expectedIndices.Count).Should().BeEquivalentTo(expectedIndices);
        }

        public class In : INotifyPropertyChanged
        {
            public readonly int id;
            public readonly int vertexCount;

            public In(int id, int vertexCount)
            {
                this.id = id;
                this.vertexCount = vertexCount;
            }

            public static implicit operator In((int id, int vertexCount) tuple)
            {
                return new In(tuple.id, tuple.vertexCount);
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }
    }

    internal static class ReactiveBufferTestExtensions
    {
        public static void Add(this ObservableCollection<ReactiveBufferTests.In> collection, int id, int vertexCount)
        {
            collection.Add(new ReactiveBufferTests.In(id, vertexCount));
        }
    }
}
