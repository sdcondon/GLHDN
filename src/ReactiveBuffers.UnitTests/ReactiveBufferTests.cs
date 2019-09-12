﻿namespace GLHDN.ReactiveBuffers.UnitTests
{
    using FluentAssertions;
    using GLHDN.Core;
    using OpenGL;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
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
                object[] makeTestCase(Action<TestSource> action, ICollection<(int, int)> expectedVertices) =>
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
                        new[] { (3, 1), (3, 2), (2, 1), (2, 2) }),

                    makeTestCase( // replacement at end, varying sizes - smaller
                        a => { a.Add(1, 2); a.Add(2, 4); a[1] = (3, 2); },
                        new[] { (1, 1), (1, 2), (3, 1), (3, 2) })
                };
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
            var source = new TestSource();
            var target = new MemoryVertexArrayObject(
                new(BufferUsage, Type, int, Array)[] { (BufferUsage.DynamicDraw, typeof((int, int)), 100, null) },
                (100, null));
            var subject = new ReactiveBuffer<(int, int)>(target, source, new[] { 0, 1 });

            // Act 
            action(source);

            // Assert
            target.AttributeBuffers[0].Content.Take(expectedVertices.Count).Should().BeEquivalentTo(expectedVertices, opts => opts.WithStrictOrdering());
            target.IndexBuffer.Content.Take(expectedIndices.Count).Should().BeEquivalentTo(expectedIndices, opts => opts.WithStrictOrdering());
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
