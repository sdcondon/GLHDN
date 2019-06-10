using FluentAssertions;
using System;
using System.Collections.Generic;
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
                object[][] makeTestCases(Action<ObservableComposite<int>> action, ICollection<string> expectedObservations) => new object[][]
                {
                    new object[] { action, expectedObservations, false },
                    new object[] { action, expectedObservations, true },
                };

                return new List<object[][]>()
                {
                    makeTestCases( // addition, update & removal
                        a => { var (i, iv) = a.Add(1); iv.OnNext(2); i.Remove(); },
                        new[] { "0+", "0=0", "1+", "1=1", "1=2", "1-" }),

                    makeTestCases( // nested addition, update and removal
                        a => { var (i, _) = a.Add(1); var (j, jv) = i.Add(2); jv.OnNext(3); j.Remove(); },
                        new[] { "0+", "0=0", "1+", "1=1", "2+", "2=2", "2=3", "2-" }),

                    makeTestCases( // parent removal
                        a => { var (i, _) = a.Add(1); var j = i.Add(2); i.Remove(); },
                        new[] { "0+", "0=0", "1+", "1=1", "2+", "2=2", "1-", "2-" }),

                    makeTestCases( // grandparent removal
                        a => { var (i, iv) = a.Add(1); var (j, jv) = i.Add(2); var (k, kv) = j.Add(3); iv.OnNext(4); jv.OnNext(5); kv.OnNext(6); i.Remove(); },
                        new[] { "0+", "0=0", "1+", "1=1", "2+", "2=2", "3+", "3=3", "1=4", "2=5", "3=6", "1-", "2-", "3-" }),

                    makeTestCases( // sibling independence
                        a => { var (s1, _) = a.Add(1); var (s2, _) = a.Add(2); var s11 = s1.Add(11); var (s21, s21v) = s2.Add(21); s1.Remove(); s21v.OnNext(22); },
                        new[] { "0+", "0=0", "1+", "1=1", "2+", "2=2", "3+", "3=11", "4+", "4=21", "1-", "3-", "4=22" }),
                }
                .SelectMany(a => a);
            }
        }

        // todo: test for subscription after child addition

        [Theory]
        [MemberData(nameof(FlattenTestCases))]
        public void FlattenTests(Action<ObservableComposite<int>> action, ICollection<string> expectedObservations, bool dispose)
        {
            // Arrange
            var subjectMonitor = new Dictionary<string, object>();
            var subjectId = 0;
            var root = new ObservableComposite<int>(new BehaviorSubject<int>(0), subjectMonitor, ref subjectId);
            var observed = new StringBuilder();
            var itemCount = 0;
            var subscription = root.Flatten().Subscribe(
                obs =>
                {
                    var thisItem = itemCount++;
                    observed.Append($"{thisItem}+; ");
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
                case Subject<ObservableComposite<int>> sc:
                    return !sc.HasObservers;
                default:
                    throw new Exception($"Unexpected type of monitored object");
            }
        }
    }

    public static class Extensions
    {
        public static (ObservableComposite<int>, BehaviorSubject<int>) Add(this ObservableComposite<int> comp, int initialValue)
        {
            var subject = new BehaviorSubject<int>(initialValue);
            var child = new ObservableComposite<int>(subject); // todo: re-add subject monitoring
            comp.Add(child);
            return (child, subject);
        }
    }
}
