namespace GLHDN.ReactiveBuffers.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Text;
    using Xunit;

    public class ObservableHelpersTests
    {
        private ObservableCollection<In> collection;
        private IObservable<IObservable<Out>> target;

        public ObservableHelpersTests()
        {
            this.collection = new ObservableCollection<In>();
            this.target = ObservableHelpers.FromObservableCollection(collection, (In a) => new Out(a.value));
        }

        public static IEnumerable<object[]> TestCases => new List<object[]>()
        {
            MakeTestCase( "addition",
                a =>
                {
                    a.Add(new In(1));
                    a.Add(new In(2));
                },
                new[] { "new:1", "val:1:1", "new:2", "val:2:2" }),
            MakeTestCase( "removal",
                a =>
                {
                    a.Add(new In(1));
                    a.Add(new In(2));
                    a.RemoveAt(0);
                },
                new[] { "new:1", "val:1:1", "new:2", "val:2:2", "del:1" }),
            MakeTestCase( "removal from end",
                a =>
                {
                    a.Add(new In(1));
                    a.Add(new In(2));
                    a.RemoveAt(1);
                },
                new[] { "new:1", "val:1:1", "new:2", "val:2:2", "del:2" }),
            MakeTestCase( "replacement",
                a =>
                {
                    a.Add(new In(1));
                    a.Add(new In(2));
                    a[0] = new In(3);
                },
                new[] { "new:1", "val:1:1", "new:2", "val:2:2", "del:1", "new:3", "val:3:3" }),
            MakeTestCase( "clear",
                a =>
                {
                    a.Add(new In(1));
                    a.Add(new In(2));
                    a.Clear();
                    a.Add(new In(3));
                },
                new[] { "new:1", "val:1:1", "new:2", "val:2:2", "del:1", "del:2", "new:3", "val:3:3" })
        };

        [Theory]
        [MemberData(nameof(TestCases))]
        public void Test(string description, Action<ObservableCollection<In>> action, ICollection<string> expectedObservations)
        {
            var observed = new StringBuilder();
            var itemCount = 0;
            var subscription = this.target.Subscribe(
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
                action(this.collection);
                Assert.Equal(string.Join(Environment.NewLine, expectedObservations) + Environment.NewLine, observed.ToString());
            }
            finally
            {
                subscription.Dispose();
            }
        }

        private static object[] MakeTestCase(
            string description,
            Action<ObservableCollection<In>> action,
            ICollection<string> expectedObservations)
        {
            return new object[]
            {
                description,
                action,
                expectedObservations
            };
        }

        public class In : INotifyPropertyChanged
        {
            private PropertyChangedEventHandler _pc;
            public int value;

            public In(int value)
            {
                this.value = value;
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
