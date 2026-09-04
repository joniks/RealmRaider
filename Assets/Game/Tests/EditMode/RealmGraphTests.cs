using System;
using NUnit.Framework;
using RealmRaiders.Realm;

namespace RealmRaiders.Tests
{
    public sealed class RealmGraphTests
    {
        [Test]
        public void Visit_RevealsOnlyConnectedNeighbors()
        {
            var graph = new RealmGraph(); graph.Add("Start"); graph.Add("Next"); graph.Add("Hidden"); graph.Connect("Start", "Next");
            graph.Nodes["Start"].Visit();
            Assert.That(graph.Nodes["Start"].Fog, Is.EqualTo(FogState.Visited));
            Assert.That(graph.Nodes["Next"].Fog, Is.EqualTo(FogState.Discovered));
            Assert.That(graph.Nodes["Hidden"].Fog, Is.EqualTo(FogState.Hidden));
        }

        [Test]
        public void Connections_AreBidirectional()
        {
            var graph = new RealmGraph(); var first = graph.Add("A"); var second = graph.Add("B"); graph.Connect("A", "B");
            Assert.That(first.Neighbors, Does.Contain(second)); Assert.That(second.Neighbors, Does.Contain(first));
        }

        [Test]
        public void DuplicateNodeIds_AreRejected()
        {
            var graph = new RealmGraph(); graph.Add("A");
            Assert.Throws<ArgumentException>(() => graph.Add("A"));
        }
    }
}
