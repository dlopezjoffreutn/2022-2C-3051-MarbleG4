using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using TGC.MonoGame.TP.Elements;
using TGC.MonoGame.TP.Geometries;

namespace TGC.MonoGame.TP
{
	public class Player
	{
		/// Constantes
		public float KAmbientGoma = 0.8f;
		public float KDiffuseGoma = 0.6f;
		public float KSpecularGoma = 0.8f;

		private float Gravity = 0.7f;
		public float MoveForceVariation = 0f;
		private float MoveForce = 2f;
		private float MoveForceAir = 0.5f;
		private float JumpForce = 15f;
		private float friction = 0.05f;
		public float Reflection = 1f;

		public Vector3 Ks = new Vector3(0.7f, 0.6f, 0.3f); //Ambient, Diffuse, Specular

		/// Flags
		public bool flag_fall { get; set; } = false;
		public bool flag_longfall { get; set; } = false;
		public bool grounded = false;
		public bool Initialized = false;

		public Vector3 VectorSpeed { get; set; }

		private Vector3 scale = new Vector3(5, 5, 5);

		public Sphere Body { get; set; }
		public SpherePrimitiveTex drawnBody { get; set; }

		public Sphere JumpLine { get; set; }
		public Vector3 JumpLinePos = new Vector3(0, -3f, 0);
		public Vector3 JumpLineScale = new Vector3(5, 5, 5);
		public Vector3 Position { get; set; }

		public Vector3 PreFallPosition { get; set; }

		/// Texturas
		public Texture2D Texture1 { get; set; }
		private Texture2D Texture2 { get; set; }
		private Texture2D Texture3 { get; set; }

		public Texture2D PlayerTexture { get; set; }
		public Effect PlayerEffect { get; set; }

		private Model Model { get; set; }

		private GraphicsDevice currentGraphics { get; set; }
		private RenderTargetCube EnvironmentMapRenderTarget { get; set; }

		public Quaternion playerRotation;

		public RenderTarget2D noShadowsRender;
		public RenderTarget2D noEnviromentRender;

		public Player(GraphicsDevice graphics, ContentManager content, Effect Effect, Color color)
		{
			Model = content.Load<Model>("Models/geometries/sphere");

			//Texture1 = content.Load<Texture2D>("Textures/" + "water");
			currentGraphics = graphics;
			PlayerEffect = content.Load<Effect>("Effects/BasicShader");
			//Texture2 = content.Load<Texture2D>("Textures/" + "texture2");
			//Texture3 = content.Load<Texture2D>("Textures/" + "texture3");
			PlayerTexture = Texture1;
			PlayerEffect.Parameters["ModelTexture"]?.SetValue(PlayerTexture);
			PlayerEffect.Parameters["ambientColor"]?.SetValue(Color.White.ToVector3());
			PlayerEffect.Parameters["diffuseColor"]?.SetValue(Color.White.ToVector3());
			PlayerEffect.Parameters["specularColor"]?.SetValue(Color.White.ToVector3());

			Body = new Sphere(graphics, content, 1f, 16, color);
			drawnBody = new SpherePrimitiveTex(graphics, 1f, 16);
			Body.WorldUpdate(scale, new Vector3(0, 15, 0), Quaternion.Identity);
			Position = Body.Position;
			JumpLine = new Sphere(graphics, content, 1f, 10, new Color(0f, 1f, 0f, 0.3f));
			JumpLine.WorldUpdate(JumpLineScale, Position + JumpLinePos, Quaternion.Identity);
			foreach (var meshPart in Model.Meshes.SelectMany(mesh => mesh.MeshParts))
				meshPart.Effect = PlayerEffect;

			noShadowsRender = new RenderTarget2D(graphics, 1, 1, false,
				SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);

			noEnviromentRender = new RenderTarget2D(graphics, 1, 1, false,
				SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);

			graphics.SetRenderTarget(noShadowsRender);
			graphics.Clear(Color.White);

			graphics.SetRenderTarget(noEnviromentRender);
			graphics.Clear(Color.Transparent);

			graphics.SetRenderTarget(null);

			playerRotation = Quaternion.Identity;

		}

		public void Init()
		{
			PlayerEffect.Parameters["Reflection"]?.SetValue(Reflection);
			PlayerEffect.Parameters["KAmbient"]?.SetValue(Ks.X);
			PlayerEffect.Parameters["KDiffuse"]?.SetValue(Ks.Y);
			PlayerEffect.Parameters["KSpecular"]?.SetValue(Ks.Z);
			PlayerEffect.Parameters["ModelTexture"]?.SetValue(PlayerTexture);
			Initialized = true;
		}

		public void Draw(Matrix world, Matrix view, Matrix projection)
		{
			if (!Initialized) Init();
			// Set BasicEffect parameters.
			Vector3 cameraPosition = new Vector3(-10, 10, 0);
			Vector3 LightPosition = new Vector3(-10, 10, 0);

			var playerWorld = this.Body.World;

			PlayerEffect.CurrentTechnique = PlayerEffect.Techniques["BasicColorDrawing"];
			PlayerEffect.Parameters["environmentMap"]?.SetValue(noEnviromentRender);
			PlayerEffect.Parameters["lightPosition"].SetValue(LightPosition);

			Matrix InverseTransposeWorld = Matrix.Transpose(Matrix.Invert(world));
			PlayerEffect.Parameters["InverseTransposeWorld"].SetValue(InverseTransposeWorld);

			PlayerEffect.Parameters["World"].SetValue(world);
			PlayerEffect.Parameters["View"].SetValue(view);
			PlayerEffect.Parameters["Projection"].SetValue(projection);
			PlayerEffect.Parameters["eyePosition"]?.SetValue(cameraPosition);

			PlayerEffect.Parameters["shadowMapSize"]?.SetValue(Vector2.One * 1);
			PlayerEffect.Parameters["shadowMap"]?.SetValue(noShadowsRender);
			PlayerEffect.Parameters["LightViewProjection"]?.SetValue(Matrix.Identity);

			drawnBody.Draw(playerWorld, view, projection, PlayerEffect);
		}

		public void Update(GameTime gameTime)
		{
			var elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

			grounded = true;
			//grounded = CanJump(objects);
			if (!grounded)
				VectorSpeed += Vector3.Down * Gravity;
			else
				VectorSpeed -= VectorSpeed * friction;
			Vector3 scaledSpeed = VectorSpeed * elapsedTime;

			playerRotation = Quaternion.CreateFromAxisAngle(Vector3.Forward, VectorSpeed.X / 100) * Quaternion.CreateFromAxisAngle(Vector3.Right, VectorSpeed.Z / 100) * playerRotation;

			Body.WorldUpdate(scale, Position + scaledSpeed, Matrix.CreateFromQuaternion(playerRotation));
			Position = Body.Position;

			JumpLine.WorldUpdate(new Vector3(1, 1f, 1), Position + JumpLinePos, Quaternion.Identity);

			if (Position.Y > 0) PreFallPosition = Position;
			else if (Position.Y < -10 && Position.Y > -200)
			{
				if (!flag_fall)
				{
					flag_fall = true;
				}
			}
		}

		public void Move(Vector3 direction)
		{
			if (grounded)
				VectorSpeed += direction * (MoveForce + MoveForceVariation);
			else
				VectorSpeed += direction * (MoveForceAir + MoveForceVariation);
		}

		public void Jump()
		{
			var flag_jump = false;
			if (grounded)
			{
				VectorSpeed += Vector3.Up * JumpForce;
				if (!flag_jump)
				{
					flag_jump = true;
				}
				flag_jump = false;
			}
		}
	}
}
