﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Geometries;
namespace TGC.MonoGame.TP
{
/*
*    - escaleras
*    - rampas para bajar y tomar impulso
*    - distintos pisos
*    - saltar el "vacio"
*    - caminito tipo zigzag sin barandas
*    - barrera/puerta que se mueve 
*
*    *obstaculos*
*    - pinchos
*    - "cajas" que se mueven
*    - "cajas" que caigan del cielo
*/

    /// <summary>
    ///     Esta es la clase principal  del juego.
    ///     Inicialmente puede ser renombrado o copiado para hacer más ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar la clase que ejecuta Program <see cref="Program.Main()" /> linea 10.
    /// </summary>
    public class TGCGame : Game
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";

        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        public TGCGame()
        {
            // Maneja la configuracion y la administracion del dispositivo grafico.
            Graphics = new GraphicsDeviceManager(this);
            // Descomentar para que el juego sea pantalla completa.
            // Graphics.IsFullScreen = true;
            // Carpeta raiz donde va a estar toda la Media.
            Content.RootDirectory = "Content";
            // Hace que el mouse sea visible.
            IsMouseVisible = true;
        }

        private Point screenSize { get; set; } 
        private Player Player { get; set; }
        private Camera Camera { get; set; }

        private GraphicsDeviceManager Graphics { get; }
        private SpriteBatch SpriteBatch { get; set; }
        private Model Model { get; set; }
        private Effect Effect { get; set; }
        private float Rotation { get; set; }
        private Matrix World { get; set; }
        private Matrix View { get; set; }
        private Matrix Projection { get; set; }

         // World matrices
        private Matrix BoxWorld { get; set; }
        private Matrix ConoMatrix { get; set; }
        private Matrix FloorMatrix { get; set; }
        private Matrix BarrilMatrix { get; set; }

        private Matrix SphereMatrix { get; set; }
        private Model Cono { get; set; }
        private Model Barril { get; set; }
        private Model Sphere { get; set; }
        private Model Mundo { get; set; }

        private List<Texture2D> ListaTexturas { get; set; }
        private List<Texture2D> ConoTexturas { get; set; }
        private Texture2D EsferaTexturas { get; set; }
        private List<Texture2D> BarrilTexturas { get; set; }

        // Effect for the objects and boxes
            private BasicEffect BoxesEffect { get; set; }
            private BasicEffect ObjEffect { get; set; }
            private BasicEffect PointEffect { get; set; }

        //Coliciones
        private BoundingBox[] Colliders { get; set; }
       
            
        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void Initialize()
        {
            // La logica de inicializacion que no depende del contenido se recomienda poner en este metodo.

            // Apago el backface culling.
            // Esto se hace por un problema en el diseno del modelo del logo de la materia.
            // Una vez que empiecen su juego, esto no es mas necesario y lo pueden sacar.
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;
            // Seria hasta aca.

            screenSize = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
           
            // Inicializacion del Player
            //Player = new Player(GraphicsDevice, Content, null, Color.Green);
            
            // Inicialización lista de texturas donde se van a guardar todas las texturas que vamos a usar 
           // ListaTexturas = new List<Texture2D>

            // Inicializacion de la camara que sigue al Player
            Camera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio, Player.Position, screenSize);
            Camera.FrontDirection = Vector3.Normalize(new Vector3(Player.Position.X - Camera.Position.X, 0, Player.Position.Z - Camera.Position.Z));
            Camera.RightDirection = Vector3.Normalize(Vector3.Cross(Camera.FrontDirection, Vector3.Up));
            

            // Configuramos nuestras matrices de la escena.
            World = Matrix.Identity;
            View = Matrix.CreateLookAt(Vector3.UnitZ * 150, Vector3.Zero, Vector3.Up);
            Projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 250);

            var viewMatrix = Matrix.CreateLookAt(new Vector3(0, 0, 50), Vector3.Forward, Vector3.Up);

            BoxWorld = Matrix.CreateScale(30f) * Matrix.CreateTranslation(85f, 15f, zPosition: -15f);
            FloorMatrix = Matrix.CreateScale(200f, 0.001f, 200f);

            BoxesEffect = new BasicEffect(GraphicsDevice);
            
            ObjEffect=new BasicEffect(GraphicsDevice);
            
            PointEffect= new BasicEffect(GraphicsDevice);
            base.Initialize();
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
        ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
        ///     que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void LoadContent()
        {
            Camera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 5, 0), screenSize);

            // Aca es donde deberiamos cargar todos los contenido necesarios antes de iniciar el juego.
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            // Cargo el modelo del logo.
           // Model = Content.Load<Model>(ContentFolder3D + "tgc-logo/tgc-logo");
           
           // Cargo los modelos de los objetos
            Sphere = Content.Load<Model>(ContentFolder3D + "geometries/sphere");
            Cono = Content.Load<Model>(ContentFolder3D + "geometries/Cone");
            Barril = Content.Load<Model>(ContentFolder3D + "barriles/barrels_fbx");
            

           
           
            // Cargo un efecto basico propio declarado en el Content pipeline.
            // En el juego no pueden usar BasicEffect de MG, deben usar siempre efectos propios.
            Effect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
            //PointEffect.TextureEnable = true;
            //BoxesEffect.TextureEnable = true;
            //ObjEffect.TextureEnable = true;
            

            //
            ConoTexturas.Add(Content.Load<Texture2D>(ContentFolderTextures+"Cone/Cone Mat_Base_Color"));
            ConoTexturas.Add(Content.Load<Texture2D>(ContentFolderTextures+"Cone/Cone Mat_Height"));
            ConoTexturas.Add(Content.Load<Texture2D>(ContentFolderTextures+"Cone/Cone Mat_Normal"));
            ConoTexturas.Add(Content.Load<Texture2D>(ContentFolderTextures+"Cone/Cone Mat_Metallic"));

            // Asigno el efecto que cargue a cada parte del mesh.
            // Un modelo puede tener mas de 1 mesh internamente.

            foreach (var mesh in Sphere.Meshes) { 

                // Un mesh puede tener mas de 1 mesh part (cada 1 puede tener su propio efecto).
                //Cargo las texturas 
                foreach (var meshPart in mesh.MeshParts){
                    ObjEffect =((BasicEffect)meshPart.Effect);
                    meshPart.Effect = Effect;
                    };
            }
            foreach (var meshCono in Cono.Meshes)
            {
                foreach (var meshConoPart in meshCono.MeshParts)
                {
                    ObjEffect = ((BasicEffect)meshConoPart.Effect);
                    if (ObjEffect.Texture != null)
                        ListaTexturas.Add(ObjEffect.Texture);
                    meshConoPart.Effect = Effect;
                };
            }
            foreach (var meshBarril in Barril.Meshes)
            {
                foreach (var meshBarrilPart in meshBarril.MeshParts)
                {
                    ObjEffect = ((BasicEffect)meshBarrilPart.Effect);
                }
            }

            base.LoadContent();
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logica de actualizacion del juego.
            var keyboardState = Keyboard.GetState();

            // Capturar Input teclado
            if (Keyboard.GetState().IsKeyDown(Keys.Q))
                //Salgo del juego.
                Exit();

            Camera.UpdatePlayerPosition(Player.Position);
            Camera.Update(gameTime);

            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
            {
                Player.Move(Camera.RightDirection);
            }
            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
            {
                Player.Move(Camera.RightDirection * -1);
            }
            if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))
            {
                Player.Move(Camera.FrontDirection);
            }
            if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S))
            {
                Player.Move(Camera.FrontDirection * -1);
            }
            if (keyboardState.IsKeyDown(Keys.Space))
            {
                Player.Jump();
            }

            // Basado en el tiempo que paso se va generando una rotacion.
            Rotation += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqui el codigo referido al renderizado.
        /// </summary>
        protected void DrawModel(GameTime gameTime, Effect effect){

        } 
        protected override void Draw(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logia de renderizado del juego.
            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            // Para dibujar le modelo necesitamos pasarle informacion que el efecto esta esperando.
            Effect.Parameters["View"].SetValue(View);
            Effect.Parameters["Projection"].SetValue(Projection);
            Effect.Parameters["DiffuseColor"].SetValue(Color.DarkBlue.ToVector3());
           
            var rotationMatrix = Matrix.CreateRotationY(Rotation);
            var indice =0;
            foreach (var mesh in Model.Meshes)
            {
               var bassicEffect = (BasicEffect)mesh.Effects.FirstOrDefault();
                if(bassicEffect.Texture != null ){
                    Effect.Parameters["Texture"]?.SetValue(indice);
                    indice++;
                 };    
                World = mesh.ParentBone.Transform * rotationMatrix;
                Effect.Parameters["World"].SetValue(World);
                mesh.Draw();
            }
            Sphere.Draw(World,Camera.View,Camera.Projection);
           // Cono.Draw(ConoMatrix,)

        }

        /// <summary>
        ///     Libero los recursos que se cargaron en el juego.
        /// </summary>
        protected override void UnloadContent()
        {
            // Libero los recursos.
            Content.Unload();

            base.UnloadContent();
        }
    }
}