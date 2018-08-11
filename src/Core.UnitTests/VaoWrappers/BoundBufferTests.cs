namespace GLHDN.Core.UnitTests.VaoWrappers
{
    using FluentAssertions;
    using Moq;
    using OpenGL;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Numerics;
    using Xunit;

    public class BoundBufferTests
    {
        private ObservableCollection<Element> collection;
        private BoundBuffer<Element, Vertex> target;
        private MockVertexArrayObject vao;

        public BoundBufferTests()
        {
            this.collection = new ObservableCollection<Element>();
            this.target = new BoundBuffer<Element, Vertex>(
                collection,
                PrimitiveType.Points,
                2,
                5,
                a => new[] { new Vertex(a.center - Vector3.UnitX), new Vertex(a.center + Vector3.UnitX) },
                new[] { 0, 1, 2 },
                (p, a, i) => this.vao = new MockVertexArrayObject(p, a, i));
        }

        [Fact]
        public void Test1()
        {
            this.collection.Add(new Element(Vector3.Zero));
            this.vao.AttributeBuffers[0].Contents.Should().BeEquivalentTo(
                new Vertex(new Vector3(-1f, 0f, 0f)),
                new Vertex(new Vector3(1f, 0f, 0f)));
            this.vao.IndexBuffer.Contents.Should().BeEquivalentTo(
                0, 1, 2);
        }

        private class Element : INotifyPropertyChanged
        {
            public readonly Vector3 center;

            public Element(Vector3 center)
            {
                this.center = center;
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        private struct Vertex
        {
            public readonly Vector3 position;

            public Vertex(Vector3 position)
            {
                this.position = position;
            }
        }
    }
}
