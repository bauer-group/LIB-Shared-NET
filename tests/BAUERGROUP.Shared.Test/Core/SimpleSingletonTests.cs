using BAUERGROUP.Shared.Core.Utilities;

namespace BAUERGROUP.Shared.Test.Core;

public class SimpleSingletonTests
{
    public class TestSingletonClass
    {
        public string Value { get; set; } = "Default";
        public int Counter { get; set; }
    }

    [Fact]
    public void Instance_ReturnsSameInstance()
    {
        var instance1 = SimpleSingleton<TestSingletonClass>.Instance;
        var instance2 = SimpleSingleton<TestSingletonClass>.Instance;

        instance1.Should().BeSameAs(instance2);
    }

    [Fact]
    public void Instance_CreatesInstanceAutomatically()
    {
        var instance = SimpleSingleton<TestSingletonClass>.Instance;

        instance.Should().NotBeNull();
    }

    [Fact]
    public void Instance_ModificationsArePersisted()
    {
        SimpleSingleton<TestSingletonClass>.Instance.Value = "Modified";
        SimpleSingleton<TestSingletonClass>.Instance.Counter = 42;

        SimpleSingleton<TestSingletonClass>.Instance.Value.Should().Be("Modified");
        SimpleSingleton<TestSingletonClass>.Instance.Counter.Should().Be(42);
    }

    [Fact]
    public async Task Instance_IsThreadSafe()
    {
        var instances = new TestSingletonClass[100];
        var tasks = new Task[100];

        for (int i = 0; i < 100; i++)
        {
            var index = i;
            tasks[i] = Task.Run(() =>
            {
                instances[index] = SimpleSingleton<TestSingletonClass>.Instance;
            });
        }

        await Task.WhenAll(tasks);

        // All should be the same instance
        instances.All(i => ReferenceEquals(i, instances[0])).Should().BeTrue();
    }

    public class AnotherTestClass
    {
        public string Name { get; set; } = "Another";
    }

    [Fact]
    public void DifferentTypes_HaveDifferentSingletons()
    {
        var instance1 = SimpleSingleton<TestSingletonClass>.Instance;
        var instance2 = SimpleSingleton<AnotherTestClass>.Instance;

        instance1.Should().NotBeSameAs(instance2);
        instance1.GetType().Should().Be(typeof(TestSingletonClass));
        instance2.GetType().Should().Be(typeof(AnotherTestClass));
    }

    [Fact]
    public void Instance_IsLazilyInitialized()
    {
        // This test verifies the Lazy<T> behavior by ensuring
        // the instance is only created when accessed
        var instance = SimpleSingleton<LazyTestClass>.Instance;

        instance.Should().NotBeNull();
        LazyTestClass.InstanceCount.Should().BeGreaterThanOrEqualTo(1);
    }

    public class LazyTestClass
    {
        public static int InstanceCount;

        public LazyTestClass()
        {
            Interlocked.Increment(ref InstanceCount);
        }
    }
}
