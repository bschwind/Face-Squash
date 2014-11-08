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

namespace FaceSquash
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        int screenWidth = 1024;
        int screenHeight = 700;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D testTexture, handleTex;
        List<Vector2> vertexHandles;

        //The vertex and index buffer for the quad
        //which we will draw the distorted image onto
        VertexPositionTexture[] fullScreenQuadVerts;
        int[] indexBuffer;

        //The texture coordinates we will pass into the shader
        Vector2[] textureCoords;

        //I'm lazy, so I'm drawing the distorted image into
        //a rendertarget, the same size as the original texture
        RenderTarget2D distortedTarget;

        //Offset the texture in the window so we can drag the handles
        //to the left and above the image
        Vector2 pictureOffset = new Vector2(65, 150);

        //The shader file to distort the image
        Effect faceSquashEffect;

        MouseState previousMouseState;
        int clickedIndex = -1;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            testTexture = Content.Load<Texture2D>("Textures/brian");
            handleTex = Content.Load<Texture2D>("Textures/Vertex");
            faceSquashEffect = Content.Load<Effect>("FaceSquash");
            distortedTarget = new RenderTarget2D(GraphicsDevice,
                                                 testTexture.Width,
                                                 testTexture.Height,
                                                 false,
                                                 SurfaceFormat.Color,
                                                 DepthFormat.None);

            vertexHandles = new List<Vector2>(4);
            vertexHandles.Add(new Vector2(0, 0) + pictureOffset);
            vertexHandles.Add(new Vector2(testTexture.Width, 0) + pictureOffset);
            vertexHandles.Add(new Vector2(testTexture.Width, testTexture.Height) + pictureOffset);
            vertexHandles.Add(new Vector2(0, testTexture.Height) + pictureOffset);

            fullScreenQuadVerts = new VertexPositionTexture[] { new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0,0)),
                                                     new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1,0)),
                                                     new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1,1)),
                                                     new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0,1))};

            textureCoords = new Vector2[4];

            indexBuffer = new int[] { 1, 2, 0, 2, 3, 0 };
        }

        protected override void UnloadContent()
        {
            
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            MouseState currentMouseState = Mouse.GetState();

            UpdateMousePresses(currentMouseState, previousMouseState);
            UpdateTextureCoordinates();

            previousMouseState = currentMouseState;

            base.Update(gameTime);
        }

        private void UpdateTextureCoordinates()
        {
            for (int i = 0; i < vertexHandles.Count; i++)
            {
                textureCoords[i] = new Vector2((vertexHandles[i].X-pictureOffset.X) / (float)testTexture.Width,
                                         (vertexHandles[i].Y-pictureOffset.Y) / (float)testTexture.Height);
            }
        }

        private void UpdateMousePresses(MouseState currentState, MouseState previousState)
        {
            float clickRadius = 35;

            //See if the user clicks a control handle
            if (IsNewMouseLeftClick(currentState, previousState))
            {
                for (int i = 0; i < vertexHandles.Count; i++)
                {
                    if (Vector2.Distance(vertexHandles[i], new Vector2(currentState.X, currentState.Y)) <= clickRadius)
                    {
                        clickedIndex = i;
                        break;
                    }
                }
            }

            //If they clicked a handle, store the index of the handle,
            //otherwise leave the method
            if (clickedIndex >= 0)
            {
                vertexHandles[clickedIndex] = new Vector2(currentState.X, currentState.Y);
            }
            else
            {
                return;
            }

            //If they released the control handle, set the current index to -1
            if (IsNewMouseLeftRelease(currentState, previousMouseState))
            {
                clickedIndex = -1;
                return;
            }

            //If the mouse button is held down, move the current control handle
            //to the mouse position
            if (IsLeftMouseButtonPressed(currentState))
            {
                vertexHandles[clickedIndex] = new Vector2(currentState.X, currentState.Y);
            }


            //Clamp the control handles to the screen window
            for (int i = 0; i < vertexHandles.Count; i++)
            {
                vertexHandles[i] = Vector2.Clamp(vertexHandles[i], Vector2.Zero, new Vector2(screenWidth, screenHeight));
            }
        }

        private bool IsNewMouseLeftClick(MouseState currentState, MouseState previousState)
        {
            return currentState.LeftButton == ButtonState.Pressed 
                && previousState.LeftButton == ButtonState.Released;
        }

        private bool IsNewMouseLeftRelease(MouseState currentState, MouseState previousState)
        {
            return currentState.LeftButton == ButtonState.Released 
                && previousState.LeftButton == ButtonState.Pressed;
        }

        private bool IsLeftMouseButtonPressed(MouseState currentState)
        {
            return currentState.LeftButton == ButtonState.Pressed;
        }

        protected override void Draw(GameTime gameTime)
        {
            //Draw the distorted images into the render target
            GraphicsDevice.SetRenderTarget(distortedTarget);
            GraphicsDevice.Clear(Color.Black);
            faceSquashEffect.CurrentTechnique.Passes[0].Apply();
            faceSquashEffect.Parameters["tex"].SetValue(testTexture);
            faceSquashEffect.Parameters["corners"].SetValue(textureCoords);
            GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList,
                                                                            fullScreenQuadVerts,
                                                                            0,
                                                                            4,
                                                                            indexBuffer,
                                                                            0,
                                                                            2);


            //Draw the distorted image to the backbuffer, along with the original
            //image and the control handles for the vertices
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            spriteBatch.Draw(distortedTarget, pictureOffset + new Vector2(testTexture.Width + 65, 0), Color.White);
            spriteBatch.Draw(testTexture, pictureOffset, Color.White);

            foreach (Vector2 v in vertexHandles)
            {
                spriteBatch.Draw(handleTex, v - new Vector2(handleTex.Width / 2, handleTex.Height / 2), Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
