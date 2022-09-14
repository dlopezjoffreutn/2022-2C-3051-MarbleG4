using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TGC.MonoGame.TP.Geometries;

namespace TGC.MonoGame.TP.Elements
{

    public abstract class Object
    {
        public Matrix World;

        public Vector3 Position { get; set; }
        public static Vector3 InitialPosition { get; set; }

        public GeometricPrimitive Body { get; set; }

        public virtual void Draw(Matrix view, Matrix projection)
        {
            Body.Draw(World, view, projection);
        }
        public virtual void Draw(Matrix view, Matrix projection, Effect effect)
        {
            Body.Draw(World, view, projection, effect);
        }
        public virtual void Draw(Effect effect)
        {
            Body.Draw(effect);
        }

        public virtual void WorldUpdate(Vector3 scale, Vector3 newPosition, Quaternion rotation)
        {
            World = Matrix.CreateScale(scale) * Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(newPosition);
            Position = newPosition;
        }


        public virtual void WorldUpdate(Vector3 scale, Quaternion rotation)
        {
            World = Matrix.CreateScale(scale) * Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(Position);
        }

        public virtual void WorldUpdate(Vector3 scale, Vector3 newPosition, Matrix rotationMatrix)
        {
            World = Matrix.CreateScale(scale) * rotationMatrix * Matrix.CreateTranslation(newPosition);
            Position = newPosition;
        }

        public abstract bool Intersects(Sphere s);

        public abstract Vector3 GetDirectionFromCollision(Sphere s);
    }

    public class Sphere : Object
    {
        public BoundingSphere Collider { get; set; }
        public BoundingSphere InitialCollider { get; set; }
        private SpherePrimitive currentBody { get; set; }

        public Sphere(GraphicsDevice graphicsDevice, ContentManager content, float diameter, int tessellation, Color color)
        {
            Collider = new BoundingSphere(Vector3.Zero, 1f);

            currentBody = new SpherePrimitive(graphicsDevice, content, diameter, tessellation, color);
            Body = currentBody;
        }

        public Sphere(GraphicsDevice graphicsDevice, ContentManager content, float diameter, int tessellation)
        {
            Collider = new BoundingSphere(Vector3.Zero, 1f);
            Body = new SpherePrimitive(graphicsDevice, content, diameter, tessellation);
        }

        public override void WorldUpdate(Vector3 scale, Vector3 newPosition, Quaternion rotation)
        {
            base.WorldUpdate(scale, newPosition, rotation);
            BoundingSphere collider = Collider;
            collider.Radius = scale.X / 2;
            collider.Center = newPosition;
            Collider = collider;
        }

        public override void WorldUpdate(Vector3 scale, Vector3 newPosition, Matrix rotationMatrix)
        {
            base.WorldUpdate(scale, newPosition, rotationMatrix);
            BoundingSphere collider = Collider;
            collider.Radius = scale.X / 2;
            collider.Center = newPosition;
            Collider = collider;
        }

        public override bool Intersects(Sphere s)
        {
            var boundingSphere = new BoundingSphere(Position, currentBody.diameter / 2);
            return boundingSphere.Intersects(s.Collider);
        }
        public override Vector3 GetDirectionFromCollision(Sphere s)
        {
            return Vector3.One;
        }
    }
}