using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.TP.Geometries
{

	/// <summary>
	///     Geometric primitive class for drawing spheres.
	/// </summary>
	public class SpherePrimitiveTex
	{
		private VertexBuffer VertexBuffer { get; set; }
		private IndexBuffer IndexBuffer { get; set; }

		public BasicEffect Effect { get; set; }

		/// <summary>
		///     Constructs a new sphere primitive, with the specified size, tessellation level and color.
		/// </summary>
		/// <param name="graphicsDevice">Used to initialize and control the presentation of the graphics device.</param>
		/// <param name="diameter">Diameter of the sphere.</param>
		/// <param name="tessellation">The number of times the surface triangles are subdivided.</param>
		/// <param name="color">Color of the sphere.</param>
		public SpherePrimitiveTex(GraphicsDevice graphicsDevice, float diameter, int tessellation)
		{
			if (tessellation < 3)
				throw new ArgumentOutOfRangeException("Tessellation value for the sphere is less than expected");

			var verticalSegments = tessellation;
			var horizontalSegments = tessellation * 2;

			var radius = diameter / 2;

			CreateVertexBuffer(graphicsDevice, radius, verticalSegments, horizontalSegments);

			CreateIndexBuffer(graphicsDevice, verticalSegments, horizontalSegments);


			// Create a BasicEffect, which will be used to render the primitive.
			Effect = new BasicEffect(graphicsDevice);
			Effect.VertexColorEnabled = false;
			Effect.TextureEnabled = true;
		}

		private void CreateVertexBuffer(GraphicsDevice graphicsDevice, float radius, int vertical, int horizontal)
		{
			VertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionNormalTexture.VertexDeclaration, ((vertical - 1) * horizontal) + 2, BufferUsage.WriteOnly);

			VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[((vertical - 1) * horizontal) + 2];

			var currentVertex = 0;

			// Start with a single vertex at the bottom of the sphere.
			AddVertex(vertices, Vector3.Down, radius, ref currentVertex);

			// Create rings of vertices at progressively higher latitudes.
			for (var i = 0; i < vertical - 1; i++)
			{
				var latitude = (i + 1) * MathHelper.Pi /
					vertical - MathHelper.PiOver2;

				var dy = (float)Math.Sin(latitude);
				var dxz = (float)Math.Cos(latitude);

				// Create a single ring of vertices at this latitude.
				for (var j = 0; j < horizontal; j++)
				{
					var longitude = j * MathHelper.TwoPi / horizontal + 0.5f;

					var dx = (float)Math.Cos(longitude) * dxz;
					var dz = (float)Math.Sin(longitude) * dxz;

					var normal = new Vector3(dx, dy, dz);
					normal.Normalize();
					AddVertex(vertices, normal, radius, ref currentVertex);
				}
			}

			// Finish with a single vertex at the top of the sphere.
			AddVertex(vertices, Vector3.Up, radius, ref currentVertex);

			VertexBuffer.SetData(vertices);
		}

		private void AddVertex(VertexPositionNormalTexture[] vertices, Vector3 position, float radius, ref int index)
		{
			Vector2 coordinates;
			coordinates.X = MathF.Asin(position.X) / MathF.PI + 0.5f;
			coordinates.Y = MathF.Asin(position.Y) / MathF.PI + 0.5f;

			VertexPositionNormalTexture vertex = new VertexPositionNormalTexture(position * radius, position, coordinates);
			vertices[index] = vertex;
			index++;
		}

		private void CreateIndexBuffer(GraphicsDevice graphicsDevice, int vertical, int horizontal)
		{
			var indexCount = (horizontal + ((vertical - 2) * horizontal)) * 6;
			IndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indexCount, BufferUsage.WriteOnly);

			ushort[] indices = new ushort[indexCount];

			int currentIndex = 0;
			// Create a fan connecting the bottom vertex to the bottom latitude ring.
			for (var i = 0; i < horizontal; i++)
			{
				AddIndex(indices, 0, ref currentIndex);
				AddIndex(indices, 1 + (i + 1) % horizontal, ref currentIndex);
				AddIndex(indices, 1 + i, ref currentIndex);
			}

			// Fill the sphere body with triangles joining each pair of latitude rings.
			for (var i = 0; i < vertical - 2; i++)
				for (var j = 0; j < horizontal; j++)
				{
					var nextI = i + 1;
					var nextJ = (j + 1) % horizontal;

					AddIndex(indices, 1 + i * horizontal + j, ref currentIndex);
					AddIndex(indices, 1 + i * horizontal + nextJ, ref currentIndex);
					AddIndex(indices, 1 + nextI * horizontal + j, ref currentIndex);

					AddIndex(indices, 1 + i * horizontal + nextJ, ref currentIndex);
					AddIndex(indices, 1 + nextI * horizontal + nextJ, ref currentIndex);
					AddIndex(indices, 1 + nextI * horizontal + j, ref currentIndex);
				}

			var vertexCount = ((vertical - 1) * horizontal) + 2;
			// Create a fan connecting the top vertex to the top latitude ring.
			for (var i = 0; i < horizontal; i++)
			{
				AddIndex(indices, vertexCount - 1, ref currentIndex);
				AddIndex(indices, vertexCount - 2 - (i + 1) % horizontal, ref currentIndex);
				AddIndex(indices, vertexCount - 2 - i, ref currentIndex);
			}

			IndexBuffer.SetData(indices);
		}

		private void AddIndex(ushort[] indices, int index, ref int arrayIndex)
		{
			indices[arrayIndex] = (ushort)index;
			arrayIndex++;
		}

		public void Draw(Matrix World, Matrix View, Matrix Projection)
		{
			var graphicsDevice = Effect.GraphicsDevice;

			// Set our vertex declaration, vertex buffer, and index buffer.
			graphicsDevice.SetVertexBuffer(VertexBuffer);
			graphicsDevice.Indices = IndexBuffer;

			Effect.World = World;
			Effect.View = View;
			Effect.Projection = Projection;

			foreach (var effectPass in Effect.CurrentTechnique.Passes)
			{
				effectPass.Apply();

				var primitiveCount = IndexBuffer.IndexCount / 3;
				graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
			}
		}

		public void Draw(Matrix World, Matrix View, Matrix Projection, Effect effect)
		{
			var graphicsDevice = Effect.GraphicsDevice;

			// Set our vertex declaration, vertex buffer, and index buffer.
			graphicsDevice.SetVertexBuffer(VertexBuffer);
			graphicsDevice.Indices = IndexBuffer;

			Effect.World = World;
			Effect.View = View;
			Effect.Projection = Projection;

			foreach (var effectPass in effect.CurrentTechnique.Passes)
			{
				effectPass.Apply();

				var primitiveCount = IndexBuffer.IndexCount / 3;
				graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
			}
		}

		public void Draw(Matrix World, Matrix View, Matrix Projection, Texture2D texture)
		{
			Effect.Parameters["Texture"].SetValue(texture);
			Draw(World, View, Projection);
		}

		public void Dispose()
		{
			VertexBuffer.Dispose();
			IndexBuffer.Dispose();
			Effect.Dispose();
		}
	}
}