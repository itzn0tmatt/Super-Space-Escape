using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;


namespace ArcadeRPG
{
    public class TimeExpired
    {
        public enum State { SHOW, HIDE };
        private State state; // ^^ being displayed or not
        private Color color;
        private Texture2D timeoutpic; // graphic for menu
        private Vector2 timeoutpos;
        private Vector2 offset;
        public Boolean play_again;
        private TimeSpan timeLeft = TimeSpan.FromSeconds(10.0); // grant the player a certain time to decide if they want to play again
        private Vector2 d_time_pos;
        private SpriteFont font;


        public TimeExpired()
        {
            state = State.HIDE; // hidden by default until shown explicitly
            color = Color.Crimson; // the color of defeat....
            offset = new Vector2(0, 0);
            d_time_pos = new Vector2(373, 250); // formatting
            play_again = false;
        }

        public void Show(SpriteBatch spritebatch)
        {
            state = State.SHOW;
            spritebatch.Draw(timeoutpic, timeoutpos, null, Color.White, 0 , offset, 1.0f, SpriteEffects.None, 0); // draw the time expired screen
            spritebatch.DrawString(font, String.Format("{0:0.0}", timeLeft.TotalSeconds), d_time_pos, Color.White);
        }

        public Boolean isRunning()
        {
            return timeLeft.TotalSeconds > 0;
        }

        public void reset()
        {
            play_again = false;
            timeLeft = TimeSpan.FromSeconds(10.0);
        }

        public void Hide()
        {
            if (state == State.HIDE) { return; } // if already hiding, no need to hide again, just return
            else
            {
                timeoutpic.ToString().Remove(0); // "undraws" screen
                state = State.HIDE;
            }
        }

        public void loadContent(ContentManager contman) // load placeholder menu for time expired
        {
            timeoutpic = contman.Load<Texture2D>("OutOfTime");
            timeoutpos = new Vector2(0, 0);
            font = contman.Load<SpriteFont>("TimeExpired");
        }


        public Boolean isShowing()
        {
            return (state == State.SHOW); // is the time expired screen showing?
        }


        public void update(GameTime gt)
        {
            // have a "timer" count down from arbitrary time (10 seconds for this menu)
            //if user doesnt click to play again in that amount of time, auto exit

            timeLeft -= gt.ElapsedGameTime;
            if (timeLeft.TotalSeconds <= 0)
            {
                shutDown();
                return;
            }
            else
            {
                TouchCollection tc = TouchPanel.GetState();
                foreach (TouchLocation tl in tc)
                {
                    if (tl.State == TouchLocationState.Pressed)
                    {
                        play_again = true;
                        return;
                    }
                }
            }
        } // end update


        public void shutDown() // send here when timer runs out, game over screen and shut down
        {
            play_again = false;
            return;
        }
    }
}
