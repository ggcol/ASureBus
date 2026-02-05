using ASureBus.Core.Caching;
using Moq;

namespace ASureBus.Tests.ASureBus.Core.Caching;

[TestFixture]
public class AsbCacheTests
{
    private IAsbCache _cache;
    
    [SetUp]
    public void SetUp()
    {
        _cache = new AsbCache();
    }
    
    [Test]
    public void Set_GivenAKeyAndAnObject_StoresInCache()
    {
        //Arrange
        var key = Guid.NewGuid();
        var obj = It.IsAny<object>();
        _cache.Set(key, obj);
        
        //Act
        var exists = _cache.TryGetValue(key, out _);

        //Assert
        Assert.That(exists, Is.True);
    }
    
    [Test]
    public void TryGetValue_GivenAKey_RetrieveRelativeCachedObject()
    {
        //Arrange
        var key = Guid.NewGuid();
        var obj = It.IsAny<object>();
        _cache.Set(key, obj);
        
        //Act
        _ = _cache.TryGetValue(key, out var retrieved);

        //Assert
        Assert.That(retrieved, Is.EqualTo(obj));
    }

    [TestCase(5, 1, true)]
    [TestCase(2, 20, false)]
    public void TryGetValue_GivenExpiration_ReturnsAccordingToExpiration(
        int expiresAfterMilliseconds, int waitingMilliseconds, bool expected)
    {
        //Arrange
        var key = Guid.NewGuid();
        var obj = It.IsAny<object>();
        var expiresAfter = TimeSpan.FromMilliseconds(expiresAfterMilliseconds);
        _cache.Set(key, obj, expiresAfter);
        
        //Act
        Thread.Sleep(waitingMilliseconds);
        var result = _cache.TryGetValue(key, out _);
        
        Assert.That(result, Is.EqualTo(expected));
    }
    
    [Test]
    public void Remove_GivenAKey_RemovesItemFromCache()
    {
        // Arrange
        var key = Guid.NewGuid();
        var obj = It.IsAny<object>();
        _cache.Set(key, obj);

        // Act
        _cache.Remove(key);
        var exists = _cache.TryGetValue(key, out _);

        // Assert
        Assert.That(exists, Is.False);
    }
    
    [Test]
    public void Set_GivenAKeyAndNullObject_StoresNullInCache()
    {
        // Arrange
        var key = Guid.NewGuid();
        object? obj = null;
        _cache.Set(key, obj);

        // Act
        var exists = _cache.TryGetValue(key, out var retrieved);

        // Assert
        Assert.That(exists, Is.True);
        Assert.That(retrieved, Is.Null);
    }
    
    [Test]
    public void Set_GivenExpiration_TriggersExpirationEvent()
    {
        // Arrange
        var key = Guid.NewGuid();
        var obj = It.IsAny<object>();
        var expiresAfter = TimeSpan.FromMilliseconds(2);
        _cache.Set(key, obj, expiresAfter);

        // Act
        Thread.Sleep(20);
        var exists = _cache.TryGetValue(key, out _);

        // Assert
        Assert.That(exists, Is.False);
    }
    
    [Test]
    public void SetAndGet_DifferentTypes_StoresAndRetrievesCorrectly()
    {
        // Arrange
        var key1 = Guid.NewGuid();
        var key2 = Guid.NewGuid();
        var obj1 = "StringObject";
        var obj2 = 12345;
        _cache.Set(key1, obj1);
        _cache.Set(key2, obj2);

        // Act
        _cache.TryGetValue(key1, out string? retrieved1);
        _cache.TryGetValue(key2, out int? retrieved2);

        // Assert
        Assert.That(retrieved1, Is.EqualTo(obj1));
        Assert.That(retrieved2, Is.EqualTo(obj2));
    }
    
    [Test]
    public void Upsert_GivenAKeyAndAnObject_UpdatesCache()
    {
        // Arrange
        var key = Guid.NewGuid();
        var initialObj = "InitialObject";
        var updatedObj = "UpdatedObject";
        _cache.Set(key, initialObj);

        // Act
        _cache.Upsert(key, updatedObj);
        var exists = _cache.TryGetValue(key, out var retrieved);

        // Assert
        Assert.That(exists, Is.True);
        Assert.That(retrieved, Is.EqualTo(updatedObj));
    }

    [Test]
    public void Upsert_GivenAKeyAndAnObjectWithExpiration_UpdatesCacheWithExpiration()
    {
        // Arrange
        var key = Guid.NewGuid();
        var initialObj = "InitialObject";
        var updatedObj = "UpdatedObject";
        var expiresAfter = TimeSpan.FromMilliseconds(2);
        _cache.Set(key, initialObj, expiresAfter);

        // Act
        _cache.Upsert(key, updatedObj, expiresAfter);
        Thread.Sleep(20);
        var exists = _cache.TryGetValue(key, out _);

        // Assert
        Assert.That(exists, Is.False);
    }
}