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
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            viewport = graphics.GraphicsDevice.Viewport;
            viewportRect = viewport.Bounds;

            CameraManager.setViewportRect(viewportRect);

            hero.load(Content, 100, 100);
            floor = new Platform(Content, "platform");
            obstacle = new Platform(Content, "obstacle");
            hero.passPlatform(floor, obstacle);

            hero.standingHitbox.X = (int)Hero.body.position.X - 70;
            hero.standingHitbox.Y = (int)Hero.body.position.Y - 43;

            UIManager.load(Content);

            // For Camera logic currently causes errors so is commented out ~Jordan
            //LevelReader.loadLevel(Content, 1);
            //ObstacleManager.finishedLoading();
        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            oldKeyState = keyState;
            keyState = Keyboard.GetState();

            MouseState mouseState = Mouse.GetState();

            if (keyState.IsKeyDown(Keys.Escape)) //  allows for game to exit //
                this.Exit();



            hero.update(gameTime.ElapsedGameTime.Milliseconds, keyState, oldKeyState, viewportRect, gameTime); // Passed GameTime, The first parameter is only the first 16 miliseconds of the game that never updates...
            floor.update();



            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.MintCream);

            spriteBatch.Begin();

            hero.updateHitboxes();

            floor.draw(spriteBatch);
            obstacle.draw(spriteBatch);
            hero.draw(spriteBatch);
            UIManager.draw(spriteBatch);

            // THIS IS FOR DEBUGGING HITBOXES , TAKE ME OUT //
            /*
            Texture2D rect = new Texture2D(graphics.GraphicsDevice, hero.standingHitbox.Width, hero.standingHitbox.Height);
            Color[] data = new Color[hero.standingHitbox.Width * hero.standingHitbox.Height];
            for (int i = 0; i < data.Length; ++i) data[i] = Color.Chocolate;
            rect.SetData(data);
            Vector2 coor = new Vector2(hero.standingHitbox.X, hero.standingHitbox.Y);
            spriteBatch.Draw(rect, coor, Color.Chocolate);
            */
            // THIS IS FOR DEBUGGING HITBOXES , TAKE ME OUT //

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
