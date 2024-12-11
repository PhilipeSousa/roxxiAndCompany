using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoxxiWaiting.Components
{
    public class Sprite

    {
        public Texture2D MainAtlasAsset {get; set;}
        public int X_InitialPositionOnTheMainAtlas {get; set;}
        public int Y_InitialPositionOnTheMainAtlas { get; set; }
        public int Sprite_Width_OntheMainAtlas { get; set; }
        public int Sprite_Height_OntheMainAtlas { get; set; }
        public Vector2 Position { get; set; }
        public Rectangle RectangleSprite;
        public SpriteEffects SpriteEffects_Flip {get; set; } = SpriteEffects.None;
        public Color color {get; set;} = Color.White;
        public float Rotation {get; set;} = 0f;
        public Vector2 CenterOfRotation {get; set;} = Vector2.Zero;
        public float Layer {get; set;} = 0f;
        public float Scale {get; set;} = 1f;
     


        public Sprite(Texture2D mainAtlasAsset, int x_InitialPositionOnTheMainAtlas, int y_InitialPositionOnTheMainAtlas,
                int sprite_Width_OntheMainAtlas, int sprite_Height_OntheMainAtlas, Vector2 intialPosition
        )
        {
            MainAtlasAsset = mainAtlasAsset;
            X_InitialPositionOnTheMainAtlas = x_InitialPositionOnTheMainAtlas;
            Y_InitialPositionOnTheMainAtlas = y_InitialPositionOnTheMainAtlas;
            Sprite_Width_OntheMainAtlas = sprite_Width_OntheMainAtlas;
            Sprite_Height_OntheMainAtlas = sprite_Height_OntheMainAtlas;
            Position = intialPosition;

            RectangleSprite = new Rectangle(X_InitialPositionOnTheMainAtlas, Y_InitialPositionOnTheMainAtlas, 
                        Sprite_Width_OntheMainAtlas, Sprite_Height_OntheMainAtlas);

        }


        public Rectangle GetCollisionRectangle()
        {
            return new Rectangle(
                (int)Position.X, 
                (int)Position.Y, 
                RectangleSprite.Width, 
                RectangleSprite.Height
            );
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(MainAtlasAsset, Position, RectangleSprite, color, Rotation, CenterOfRotation, Scale, SpriteEffects_Flip, Layer);
        }

    }
}