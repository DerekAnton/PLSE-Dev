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

namespace PLSE_Project.Interfaces
{

    public class PhysicsPlayground : Microsoft.Xna.Framework.Game
    {

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;


        Viewport viewport;
        Rectangle viewportRect;

        Hero hero = new Hero();

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
            KeyboardState keyState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            //Allows the game to exit
            if (keyState.IsKeyDown(Keys.Escape))
                this.Exit();

            hero.update(gameTime.ElapsedGameTime.Milliseconds, keyState, mouseState, viewportRect, gameTime); // Passed GameTime, The first parameter is only the first 16 miliseconds of the game that never updates...

            UIManager.update(keyState.IsKeyDown(Keys.M));

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.MintCream);

            spriteBatch.Begin();

            hero.draw(spriteBatch);

            UIManager.draw(spriteBatch);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
