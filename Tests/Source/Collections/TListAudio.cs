using System.Diagnostics;
using Lineri.ESS.Core;
using Lineri.ESS.Core.Interfaces;
using Xunit.Abstractions;
using Moq;
using System.Reflection;
using Tests.Source._Utils;


namespace Tests.Source.Collections;

public class TListAudio
{
    readonly ITestOutputHelper _testOutputHelper;

    public TListAudio(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private void Fill(ListAudio<Audio> l, int count)
    {
        for (int i = 0; i < count; i++)
        {
            l.Add(new Audio());
        }
    }

    [Fact]
    public void CheckIdentityToDictionary()
    {
        var l = new ListAudio<Audio>(64);
        var d = new Dictionary<int, Audio>(64);
        var v = new Audio[128];
        
        for (int i = 0; i < 128; i++)
        {
            v[i] = new Audio();
        }
        
        for (int i = 0; i < 128; i++)
        {
            l.Add(v[i]);
        }
        for (int i = 0; i < 128; i++)
        {
            d.Add(i, v[i]);
        }
        
        for (int i = 0; i < 64; i++)
        {
            l.Remove(i);
            d.Remove(i);
        }
        
        l.Add(v[0]);
        d.Add(0, v[0]);
        
        Assert.True(l.GetCountNoNull() == d.Count);
        Assert.True(l.Capacity == 128);

        foreach (var keyvalue in d)
        {
            var key = keyvalue.Key;
            if (key >= 128) break;

            if (keyvalue.Value != l[key])
            {
                Assert.Fail();
            }
        }
        
        l[0] = new Audio();
        
        Assert.True(l[0] != v[0]);
        
        var ll = new ListAudio<Audio>(l.ToArray());
        //Assert.All(ll, (Audio a, int i) => Assert.True(a == l[i]));
        for (int i = 0; i < 128; i++)
        {
            Assert.True(ll[i] == l[i]);
        }
    }

    [Fact]
    public void CheckIndexOf()
    {
        var l = new ListAudio<Audio>(64);
        Fill(l, 64);
        var v = new Audio();
        var c = new Mock<IAudioClip>();
        v.Init(0, AudioType.Sound, c.Object, true, true, 1f, 1f, 1f, new Mock<IAudioSource>().Object);

        var i = l.GetFreeIndex();
        l.Add(v);
        Assert.True(l.IndexOf(v) == i);
        Assert.True(l.IndexOf(c.Object) == i);
        Assert.True(l.IndexOf((a) => a?.Clip == c.Object) == i);
        Assert.True(l.Contains(v));
        
        v = new Audio(); 
        c = new Mock<IAudioClip>();
        v.Init(0, AudioType.Sound, c.Object, true, true, 1f, 1f, 1f, new Mock<IAudioSource>().Object);
        
        Assert.True(l.IndexOf(v) == -1);
        Assert.True(l.IndexOf(c.Object) == -1);
        Assert.True(l.IndexOf((a) => a?.Clip == c.Object) == -1);
    }

    [Fact]
    public void CheckIndexing()
    {
        var l = new ListAudio<Audio>(64);
        Fill(l, 64);
        
        l.Remove(0);
        Assert.True(l.GetFreeIndex() == 0);
        Assert.True(l.GetCountNoNull() == l.Count - 1);
        l.Remove(32);
        l.Add(new Audio());
        l.Add(new Audio());
        
        var c = l.GetCountNoNull();
        Assert.True(c != 0 && c != -1);
        l.Clear();
        Assert.True(l.GetCountNoNull() == 0);
    }

    public void Copy()
    {
        var l = new ListAudio<Audio>(64);
        Fill(l, 64);
        
        Audio[] a = new Audio[64];
        l.CopyTo(a, 0);
    }

    [Fact]
    public void Other()
    {
        var l = new ListAudio<Audio>(64);
        Fill(l, 64);
        foreach (var a in l)
        {
            Assert.NotNull(a);
        }
        
        var la = l.GetPrivateArray();

        Assert.True(la.IsSynchronized == l.IsSynchronized);
        Assert.True(la.IsFixedSize != l.IsFixedSize);
        Assert.True(la.IsReadOnly == l.IsReadOnly);
        Assert.Equal(la.SyncRoot, l.SyncRoot);

        l = new();
        Assert.Equal(l.Capacity, l.Count);
        Assert.Equal(l.Capacity, new ListAudio<Audio>().Capacity);
    }

    public class TListAudioAutoGen()
    {
        
    [Fact]
    public void Add_ShouldIncreaseCount()
    {
        var collection = new List<Audio>();
        var audio = new Audio();

        collection.Add(audio);

        Assert.Equal(1, collection.Count);
        Assert.Contains(audio, collection);
    }

    [Fact]
    public void Remove_ShouldDecreaseCount()
    {
        var collection = new List<Audio>();
        var audio = new Audio();

        collection.Add(audio);
        bool removed = collection.Remove(audio);

        Assert.True(removed);
        Assert.Equal(0, collection.Count);
        Assert.DoesNotContain(audio, collection);
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        var collection = new ListAudio<Audio>();
        collection.Add(new Audio());
        collection.Add(new Audio());

        collection.Clear();

        Assert.Empty(collection);
    }

    [Fact]
    public void Contains_ShouldReturnTrueIfItemExists()
    {
        var collection = new ListAudio<Audio>();
        var audio = new Audio();

        collection.Add(audio);

        Assert.True(collection.Contains(audio));
    }

    [Fact]
    public void CopyTo_ShouldCopyElementsToArray()
    {
        var collection = new ListAudio<Audio>(2);
        var audio1 = new Audio();
        var audio2 = new Audio();
        collection.Add(audio1);
        collection.Add(audio2);

        var array = new Audio[2];
        collection.CopyTo(array, 0);

        Assert.Equal(audio1, array[0]);
        Assert.Equal(audio2, array[1]);
    }

    [Fact]
    public void GetEnumerator_ShouldEnumerateAllItems()
    {
        var collection = new ListAudio<Audio>();
        var audio1 = new Audio();
        var audio2 = new Audio();
        collection.Add(audio1);
        collection.Add(audio2);

        var items = new List<Audio>();
        foreach (var item in collection)
        {
            items.Add(item);
        }

        Assert.Equal(2, items.Count);
        Assert.Contains(audio1, items);
        Assert.Contains(audio2, items);
    }
    }
}