namespace GLHDN.ReactiveBuffers.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Text;
    using Xunit;

    public class ObservableExtensionsTests
    {
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

                    makeTestCase("removal",
                        a => { a.Add(new In(1)); a.Add(new In(2)); a.RemoveAt(0); },
                        new[] { "new:1", "val:1:1", "new:2", "val:2:2", "del:1" }),

                    makeTestCase("update",
                        a => { var i = new In(1); a.Add(i); i.Value = 2; },
                        new[] { "new:1", "val:1:1", "val:1:2" }),

                    makeTestCase("removal",
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
        public void ObservableCollectionToObservableTest(string description, Action<ObservableCollection<In>> action, ICollection<string> expectedObservations)
        {
            var collection = new ObservableCollection<In>();
            var target = collection.ToObservable((In a) => new Out(a.Value));
            var observed = new StringBuilder();
            var itemCount = 0;
            var subscription = target.Subscribe(
                obs =>
                {
                    var thisItem = ++itemCount;
                    observed.AppendLine($"new:{thisItem}");
                    obs.Subscribe(
                        item => observed.AppendLine($"val:{thisItem}:{item.value}"),
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
            private PropertyChangedEventHandler _pc;
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
                    _pc?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged
            {
                add => _pc += value;
                remove => _pc -= value;
            }
        }

        public struct Out
        {
            public int value;

            public Out(int value)
            {
                this.value = value;
            }
        }
    }
}
