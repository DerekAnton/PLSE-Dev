using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace PLSE_Project
{

    public class PhysicsPlayground : Microsoft.Xna.Framework.Game
    {

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        KeyboardState keyState;
        KeyboardState oldKeyState;

        Viewport viewport;
        Rectangle viewportRect;

        Hero hero = new Hero();

        Platform floor;
        Platform obstacle;

        GroundPatroller groundPatroller;

        BloatSack bloatSack;


        Song backgroundMusic;
        
        public PhysicsPlayground()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            graphics.IsFullScreen = true;
            this.IsMouseVisible = true;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {

            base.Initialize();
        }

        protected override void LoadContent()
        {

            backgroundMusic = Content.Load<Song>("Sounds\\maidensword");
            MediaPlayer.IsRepeating = true;
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            viewport = graphics.GraphicsDevice.Viewport;
            viewportRect = viewport.Bounds;

            CameraManager.setViewportRect(viewportRect);

            EnemyManager.load();
       
            hero.load(Content, 100, 100);
            floor = new Platform(Content, "platform");
            obstacle = new Platform(Content, "obstacle");
            hero.passPlatform(floor, obstacle);


            LevelReader.loadLevel(Content, 1);
            ObstacleManager.finishedLoading();
            //groundPatroller = new GroundPatroller(700, -190, Content);
            //bloatSack = new BloatSack(1000, 1000, Content);
            ProjectileManager.loadProjectiles(Content);
            UIManager.load(Content);
        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (!MediaPlayer.State.Equals(MediaState.Playing))
                MediaPlayer.Play(backgroundMusic);

            oldKeyState = keyState;
            keyState = Keyboard.GetState();

            MouseState mouseState = Mouse.GetState();

            if (keyState.IsKeyDown(Keys.Escape)) //  allows for game to exit //
                this.Exit();

            ProjectileManager.update();
            ObstacleManager.update();
            UIManager.update();
            hero.update(gameTime.ElapsedGameTime.Milliseconds, keyState, oldKeyState, viewportRect, gameTime, Content); // Passed GameTime, The first parameter is only the first 16 miliseconds of the game that never updates...
            floor.update();
            //groundPatroller.update(gameTime.ElapsedGameTime.Milliseconds);
            //bloatSack.update(gameTime.ElapsedGameTime.Milliseconds);

            EnemyManager.update(gameTime.ElapsedGameTime.Milliseconds);
            //Console.Out.WriteLine(CameraManager.getYOffset());
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.MintCream);

            spriteBatch.Begin();

            hero.updateHitboxes();
            ObstacleManager.drawBackground(spriteBatch);
            ObstacleManager.drawMidground(spriteBatch);;
            //floor.draw(spriteBatch);
            //obstacle.draw(spriteBatch);
            hero.draw(spriteBatch);

            //groundPatroller.draw(spriteBatch);
            //bloatSack.draw(spriteBatch);
            EnemyManager.draw(spriteBatch);

            ObstacleManager.drawForeground(spriteBatch);
            ProjectileManager.draw(spriteBatch);
            UIManager.draw(spriteBatch);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
