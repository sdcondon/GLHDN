namespace GLHDN.ReactiveBuffers.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
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
                object[] makeTestCase(string description, Action<ObservableCollection<In>> action, ICollection<string> expectedObservations) =>
                    new object[] { description, action, expectedObservations };

                return new List<object[]>()
                {
                    makeTestCase("addition",
                        a => { a.Add(new In(1)); a.Add(new In(2)); },
                        new[] { "new:1", "val:1:1", "new:2", "val:2:2" }),

                    makeTestCase("update",
                        a => { var i = new In(1); a.Add(i); i.Value = 2; },
                        new[] { "new:1", "val:1:1", "val:1:2" }),

                    makeTestCase("removal at start",
                        a => { a.Add(new In(1)); a.Add(new In(2)); a.RemoveAt(0); },
                        new[] { "new:1", "val:1:1", "new:2", "val:2:2", "del:1" }),

                    makeTestCase("removal at end",
                        a => { a.Add(new In(1)); a.Add(new In(2)); a.RemoveAt(1); },
                        new[] { "new:1", "val:1:1", "new:2", "val:2:2", "del:2" }),

                    makeTestCase("replacement",
                        a => { a.Add(new In(1)); a.Add(new In(2)); a[0] = new In(3); },
                        new[] { "new:1", "val:1:1", "new:2", "val:2:2", "del:1", "new:3", "val:3:3" }),

                    makeTestCase("clear",
                        a => { a.Add(new In(1)); a.Add(new In(2)); a.Clear(); a.Add(new In(3)); },
                        new[] { "new:1", "val:1:1", "new:2", "val:2:2", "del:1", "del:2", "new:3", "val:3:3" })
                };
            }
        }

        [Theory]
        [MemberData(nameof(ObservableCollectionToObservableTestCases))]
        public void ObservableCollectionToObservableTests(string description, Action<ObservableCollection<In>> action, ICollection<string> expectedObservations)
        {
            var collection = new ObservableCollection<In>();
            var target = collection.ToObservable((In a) => a.Value);
            var observed = new StringBuilder();
            var itemCount = 0;
            var subscription = target.Subscribe(
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
                action(collection);
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
                object[] makeTestCase(string description, Action<Composite> action, ICollection<string> expectedObservations) =>
                    new object[] { description, action, expectedObservations };

                return new List<object[]>()
                {
                    makeTestCase(
                        "addition",
                        a => { a.Add(1); a.Add(2); },
                        new[] { "new:1", "val:1:1", "new:2", "val:2:2" }),

                    makeTestCase(
                        "update",
                        a => { var i = a.Add(1); i.Value = 2; },
                        new[] { "new:1", "val:1:1", "val:1:2" }),

                    makeTestCase(
                        "removal",
                        a => { var i = a.Add(1); a.Add(2); i.Remove(); },
                        new[] { "new:1", "val:1:1", "new:2", "val:2:2", "del:1" }),

                    makeTestCase(
                        "nested addition & update",
                        a => { var i = a.Add(1); var j = i.Add(2); j.Value = 3; },
                        new[] { "new:1", "val:1:1", "new:2", "val:2:2", "val:2:3" }),

                    makeTestCase(
                        "nested removal",
                        a => { var i = a.Add(1); var j = i.Add(2); j.Remove(); },
                        new[] { "new:1", "val:1:1", "new:2", "val:2:2", "del:2" }),

                    makeTestCase(
                        "parent removal",
                        a => { var i = a.Add(1); var j = i.Add(2); i.Remove(); },
                        new[] { "new:1", "val:1:1", "new:2", "val:2:2", "del:1", "del:2" }),

                    makeTestCase(
                        "grandparent removal",
                        a => { var i = a.Add(1); var j = i.Add(2); var k = j.Add(3); i.Remove(); },
                        new[] { "new:1", "val:1:1", "new:2", "val:2:2", "new:3", "val:3:3", "del:1", "del:2", "del:3" })
                };
            }
        }

        [Theory]
        [MemberData(nameof(FlattenCompositeTestCases))]
        public void FlattenCompositeTests(string description, Action<Composite> action, ICollection<string> expectedObservations)
        {
            var root = new Composite(0);
            var observed = new StringBuilder();
            var itemCount = 0;
            var subscription = root.subject.FlattenComposite(c => c.Children, c => c.Value).Subscribe(
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
                action(root);  
            }
            finally
            {
                subscription.Dispose();
            }

            Assert.Equal(string.Join(Environment.NewLine, expectedObservations) + Environment.NewLine, observed.ToString());
        }

        public class Composite
        {
            public readonly BehaviorSubject<Composite> subject;
            private int value;

            public Composite(int value)
            {
                this.subject = new BehaviorSubject<Composite>(this);
                this.Value = value;
            }

            public Subject<BehaviorSubject<Composite>> Children { get; } = new Subject<BehaviorSubject<Composite>>();

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
                var child = new Composite(initialValue);
                Children.OnNext(child.subject);
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
