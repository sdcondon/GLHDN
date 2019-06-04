namespace GLHDN.ReactiveBuffers.UnitTests
{
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Text;
    using Xunit;

    public partial class IObservableExtensionsTests
    {
        #region ToObservable(this INotifyCollectionChanged) tests
    
        public static IEnumerable<object[]> ObservableCollectionToObservableTestCases
        {
            get
            {
                object[] makeTestCase(Action<ObservableCollection<In>> action, ICollection<string> expectedObservations) =>
                    new object[] { action, expectedObservations };

                return new List<object[]>()
                {
                    makeTestCase( // addition
                        a => { a.Add(new In(1)); a.Add(new In(2)); },
                        new[] { "new:1", "val:1:1", "new:2", "val:2:2" }),

                    makeTestCase( // update
                        a => { var i = new In(1); a.Add(i); i.Value = 2; },
                        new[] { "new:1", "val:1:1", "val:1:2" }),

                    makeTestCase( // removal at start
                        a => { a.Add(new In(1)); a.Add(new In(2)); a.RemoveAt(0); },
                        new[] { "new:1", "val:1:1", "new:2", "val:2:2", "del:1" }),

                    makeTestCase( // removal at end
                        a => { a.Add(new In(1)); a.Add(new In(2)); a.RemoveAt(1); },
                        new[] { "new:1", "val:1:1", "new:2", "val:2:2", "del:2" }),

                    makeTestCase( // replacement
                        a => { a.Add(new In(1)); a.Add(new In(2)); a[0] = new In(3); },
                        new[] { "new:1", "val:1:1", "new:2", "val:2:2", "del:1", "new:3", "val:3:3" }),

                    makeTestCase( // clear
                        a => { a.Add(new In(1)); a.Add(new In(2)); a.Clear(); a.Add(new In(3)); },
                        new[] { "new:1", "val:1:1", "new:2", "val:2:2", "del:1", "del:2", "new:3", "val:3:3" })
                };
            }
        }

        [Theory]
        [MemberData(nameof(ObservableCollectionToObservableTestCases))]
        public void ObservableCollectionToObservableTests(Action<ObservableCollection<In>> action, ICollection<string> expectedObservations)
        {
            // Arrange
            var collection = new ObservableCollection<In>();
            var observed = new StringBuilder();
            var itemCount = 0;
            var subscription = collection.ToObservable((In a) => a.Value).Subscribe(
                obs =>
                {
                    var thisItem = ++itemCount;
                    observed.AppendLine($"new:{thisItem}");
                    obs.Subscribe(
                        i => observed.AppendLine($"val:{thisItem}:{i}"),
                        e => observed.AppendLine($"err:{thisItem}"),
                        () => observed.AppendLine($"del:{thisItem}"));
                },
                e => observed.AppendLine("Error"),
                () => observed.AppendLine("Complete"));
            try
            {
                // Act
                action(collection);

                // Assert
                Assert.Equal(string.Join(Environment.NewLine, expectedObservations) + Environment.NewLine, observed.ToString());
            }
            finally
            {
                subscription.Dispose();
            }
        }

        public class In : INotifyPropertyChanged
        {
            private int value;

            public In(int value)
            {
                this.value = value;
            }

            public int Value
            {
                get => value;
                set
                {
                    this.value = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        #endregion

        #region FlattenComposite tests

        public static IEnumerable<object[]> FlattenCompositeTestCases
        {
            get
            {
                object[][] makeTestCases(Action<Composite> action, ICollection<string> expectedObservations) => new object[][]
                {
                    new object[] { action, expectedObservations, false },
                    new object[] { action, expectedObservations, true },
                };

                return new List<object[][]>()
                {
                    makeTestCases( // addition, update & removal
                        a => { var i = a.Add(1); i.Value = 2; i.Remove(); },
                        new[] { "0+", "0=0", "1+", "1=1", "1=2", "1-" }),

                    makeTestCases( // nested addition, update and removal
                        a => { var i = a.Add(1); var j = i.Add(2); j.Value = 3; j.Remove(); },
                        new[] { "0+", "0=0", "1+", "1=1", "2+", "2=2", "2=3", "2-" }),

                    makeTestCases( // parent removal
                        a => { var i = a.Add(1); var j = i.Add(2); i.Remove(); },
                        new[] { "0+", "0=0", "1+", "1=1", "2+", "2=2", "1-", "2-" }),

                    makeTestCases( // grandparent removal
                        a => { var i = a.Add(1); var j = i.Add(2); var k = j.Add(3); i.Value = 4; j.Value = 5; k.Value = 6; i.Remove(); },
                        new[] { "0+", "0=0", "1+", "1=1", "2+", "2=2", "3+", "3=3", "1=4", "2=5", "3=6", "1-", "2-", "3-" }),

                    makeTestCases( // sibling independence
                        a => { var s1 = a.Add(1); var s2 = a.Add(2); var s11 = s1.Add(11); var s21 = s2.Add(21); s1.Remove(); s21.Value = 22; },
                        new[] { "0+", "0=0", "1+", "1=1", "2+", "2=2", "3+", "3=11", "4+", "4=21", "1-", "3-", "4=22" }),
                }
                .SelectMany(a => a);
            }
        }

        [Theory]
        [MemberData(nameof(FlattenCompositeTestCases))]
        public void FlattenCompositeTests(Action<Composite> action, ICollection<string> expectedObservations, bool dispose)
        {
            // Arrange
            var allSubjects = new Dictionary<string, object>();
            var root = new Composite(0, allSubjects);
            var observed = new StringBuilder();
            var itemCount = 0;
            var subscription = root.Subject
                .FlattenComposite(c => c.Children, c => c.Value)
                .Subscribe(
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
            allSubjects.Keys.Should().OnlyContain(
                k => SubjectHasNoObservers(allSubjects[k]),
                "No observers should be left at the end of the test");
        }

        private bool SubjectHasNoObservers(object subject)
        {
            switch (subject)
            {
                case BehaviorSubject<Composite> bsc:
                    return !bsc.HasObservers;
                case Subject<BehaviorSubject<Composite>> sbsc:
                    return !sbsc.HasObservers;
                default:
                    throw new Exception("Unexpected type of monitored object");
            }
        }

        public class Composite
        {
            private readonly Dictionary<string, object> allSubjects;
            private readonly BehaviorSubject<Composite> subject;
            private readonly Subject<BehaviorSubject<Composite>> children;
            private int value;

            public Composite(int value, Dictionary<string, object> allSubjects)
            {
                this.allSubjects = allSubjects;
                allSubjects.Add($"item {value} self subject", this.subject = new BehaviorSubject<Composite>(this));
                allSubjects.Add($"item {value} children subject", this.children = new Subject<BehaviorSubject<Composite>>());
                this.Value = value;
            }

            public IObservable<Composite> Subject => subject;

            public IObservable<IObservable<Composite>> Children => children;

            public int Value
            {
                get => value;
                set
                {
                    this.value = value;
                    this.subject.OnNext(this);
                }
            }

            public Composite Add(int initialValue)
            {
                var child = new Composite(initialValue, allSubjects);
                children.OnNext(child.subject);
                return child;
            }

            public void Remove()
            {
                this.subject.OnCompleted();
            }
        }

        #endregion
    }
}
