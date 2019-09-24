namespace GLHDN.ReactiveBuffers
{
    using FluentAssertions;
    using GLHDN.Core;
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using Xunit;

    public class ReactiveBufferTests
    {
        public static IEnumerable<object[]> TestCases
        {
            get
            {
                static object[] MakeTestCase(Action<TestSource> action, ICollection<(int, int)> expectedVertices) =>
                    new object[] { action, expectedVertices, Enumerable.Range(0, expectedVertices.Count).ToArray() };

#pragma warning disable SA1107
                return new List<object[]>()
                {
                    MakeTestCase( // addition, const size
                        a => { a.Add(1, 2); a.Add(2, 2); },
                        new[] { (1, 1), (1, 2), (2, 1), (2, 2) }),

                    MakeTestCase( // removal from middle, const size
                        a => { a.Add(1, 2); a.Add(2, 2); a.RemoveAt(0); },
                        new[] { (2, 1), (2, 2) }),

                    MakeTestCase( // removal from end, const size
                        a => { a.Add(1, 2); a.Add(2, 2); a.RemoveAt(1); },
                        new[] { (1, 1), (1, 2) }),

                    MakeTestCase( // replacement, const size
                        a => { a.Add(1, 2); a.Add(2, 2); a[0] = (3, 2); },
                        new[] { (3, 1), (3, 2), (2, 1), (2, 2) }),

                    MakeTestCase( // clear
                        a => { a.Add(1, 2); a.Add(2, 2); a.Clear(); a.Add(3, 2); },
                        new[] { (3, 1), (3, 2) }),

                    MakeTestCase( // addition, varying sizes
                        a => { a.Add(1, 4); a.Add(2, 2); },
                        new[] { (1, 1), (1, 2), (1, 3), (1, 4), (2, 1), (2, 2) }),

                    MakeTestCase( // removal, varying sizes
                        a => { a.Add(1, 2); a.Add(2, 4); a.RemoveAt(0); },
                        new[] { (2, 3), (2, 4), (2, 1), (2, 2) }),

                    MakeTestCase( // replacement, varying sizes - bigger
                        a => { a.Add(1, 2); a.Add(2, 2); a[0] = (3, 4); },
                        new[] { (3, 1), (3, 2), (2, 1), (2, 2), (3, 3), (3, 4) }),

                    MakeTestCase( // replacement, varying sizes - smaller
                        a => { a.Add(1, 4); a.Add(2, 2); a[0] = (3, 2); },
                        new[] { (3, 1), (3, 2), (2, 1), (2, 2) }),

                    MakeTestCase( // replacement at end, varying sizes - smaller
                        a => { a.Add(1, 2); a.Add(2, 4); a[1] = (3, 2); },
                        new[] { (1, 1), (1, 2), (3, 1), (3, 2) }),
                };
#pragma warning restore SA1107
            }
        }

        [Theory]
        [MemberData(nameof(TestCases))]
        public void Test(
            Action<TestSource> action,
            ICollection<(int, int)> expectedVertices,
            ICollection<int> expectedIndices)
        {
            // Arrange
            var sourceObservable = new TestSource();
            var targetVao = new MemoryVertexArrayObject(
                new (BufferUsage, Type, int, Array)[] { (BufferUsage.DynamicDraw, typeof((int, int)), 100, null) },
                (100, null));

            using (var sut = new ReactiveBuffer<(int, int)>(targetVao, sourceObservable, new[] { 0, 1 }))
            {
                // Act
                action(sourceObservable);
            }

            // Assert
            targetVao.AttributeBuffers[0].Content.Take(expectedVertices.Count).Should().BeEquivalentTo(expectedVertices, opts => opts.WithStrictOrdering());
            targetVao.IndexBuffer.Content.Take(expectedIndices.Count).Should().BeEquivalentTo(expectedIndices, opts => opts.WithStrictOrdering());
        }

        public class TestSource : IObservable<IObservable<IList<(int, int)>>>
        {
            private readonly Subject<Subject<IList<(int, int)>>> outerSubject = new Subject<Subject<IList<(int, int)>>>();
            private readonly List<Subject<IList<(int, int)>>> innerSubjects = new List<Subject<IList<(int, int)>>>();

            public (int, int) this[int index]
            {
                set
                {
                    innerSubjects[index].OnNext(
                        Enumerable.Range(1, value.Item2).Select(i => (value.Item1, i)).ToArray());
                }
            }

            public void Add(int id, int count)
            {
                var innerSubject = new Subject<IList<(int, int)>>();
                outerSubject.OnNext(innerSubject);
                innerSubjects.Add(innerSubject);
                this[innerSubjects.Count - 1] = (id, count);
            }

            public void RemoveAt(int index)
            {
                innerSubjects[index].OnCompleted();
                innerSubjects.RemoveAt(index);
            }

            public void Clear()
            {
                innerSubjects.ForEach(s => s.OnCompleted());
                innerSubjects.Clear();
            }

            public IDisposable Subscribe(IObserver<IObservable<IList<(int, int)>>> observer)
            {
                return outerSubject.Subscribe(observer);
            }
        }
    }
}
