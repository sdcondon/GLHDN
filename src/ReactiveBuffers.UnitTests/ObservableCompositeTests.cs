using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Xunit;

namespace GLHDN.ReactiveBuffers.UnitTests
{
    public class ObservableCompositeTests
    {
        public static IEnumerable<object[]> FlattenTestCases
        {
            get
            {
                object[][] makeTestCases(Action<ObservableComposite<Node, int>> action, ICollection<string> expectedObservations) => new object[][]
                {
                    new object[] { action, expectedObservations, false },
                    new object[] { action, expectedObservations, true },
                };

                return new List<object[][]>()
                {
                    makeTestCases( // addition, update & removal
                        a => { var i = a.Add(1); i.Node.Value = 2; i.Remove(); },
                        new[] { "0+", "0=0", "1+", "1=1", "1=2", "1-" }),

                    makeTestCases( // nested addition, update and removal
                        a => { var i = a.Add(1); var j = i.Add(2); j.Node.Value = 3; j.Remove(); },
                        new[] { "0+", "0=0", "1+", "1=1", "2+", "2=2", "2=3", "2-" }),

                    makeTestCases( // parent removal
                        a => { var i = a.Add(1); var j = i.Add(2); i.Remove(); },
                        new[] { "0+", "0=0", "1+", "1=1", "2+", "2=2", "1-", "2-" }),

                    makeTestCases( // grandparent removal
                        a => { var i = a.Add(1); var j = i.Add(2); var k = j.Add(3); i.Node.Value = 4; j.Node.Value = 5; k.Node.Value = 6; i.Remove(); },
                        new[] { "0+", "0=0", "1+", "1=1", "2+", "2=2", "3+", "3=3", "1=4", "2=5", "3=6", "1-", "2-", "3-" }),

                    makeTestCases( // sibling independence
                        a => { var s1 = a.Add(1); var s2 = a.Add(2); var s11 = s1.Add(11); var s21 = s2.Add(21); s1.Remove(); s21.Node.Value = 22; },
                        new[] { "0+", "0=0", "1+", "1=1", "2+", "2=2", "3+", "3=11", "4+", "4=21", "1-", "3-", "4=22" }),
                }
                .SelectMany(a => a);
            }
        }

        [Theory]
        [MemberData(nameof(FlattenTestCases))]
        public void FlattenTests(Action<ObservableComposite<Node, int>> action, ICollection<string> expectedObservations, bool dispose)
        {
            // Arrange
            var subjectMonitor = new Dictionary<string, object>();
            var root = new ObservableComposite<Node, int>(null, new Node(0), n => n.Values, subjectMonitor);
            var observed = new StringBuilder();
            var itemCount = 0;
            var subscription = root.Flatten().Subscribe(
                obs =>
                {
                    var thisItem = itemCount++;
                    observed.Append($"{thisItem}+; ");
                    obs.Subscribe(_ => { }, () => Debugger.Break());
                    obs.Subscribe(
                        i => observed.Append($"{thisItem}={i}; "),
                        e => observed.Append($"{thisItem}:err; "),
                        () => observed.Append($"{thisItem}-; "));
                },
                e => observed.Append("Error; "),
                () => observed.Append("Complete; "));

            try
            {
                // Act
                action(root);

                // Assert - observations
                observed.ToString().Should().BeEquivalentTo(string.Join("; ", expectedObservations) + "; ");
            }
            finally
            {
                if (dispose)
                {
                    subscription.Dispose();
                }
                else
                {
                    root.Remove();
                }
            }

            // Assert - tidy-up
            subjectMonitor.Keys.Should().OnlyContain(
                k => SubjectHasNoObservers(subjectMonitor[k]),
                "No observers should be left at the end of the test");
        }

        private bool SubjectHasNoObservers(object subject)
        {
            switch (subject)
            {
                case BehaviorSubject<int> bsi:
                    return !bsi.HasObservers;
                case Subject<ObservableComposite<Node, int>> sc:
                    return !sc.HasObservers;
                default:
                    //return true;
                    throw new Exception($"Unexpected type of monitored object");
            }
        }

        public class Node
        {
            public Node(int value) => Values = new BehaviorSubject<int>(value);

            public BehaviorSubject<int> Values { get; }

            public int Value
            {
                get => Values.Value;
                set => Values.OnNext(value);
            }

            public static implicit operator Node(int value) => new Node(value);
        }
    }
}
