/*
 * Description:     A basic PONG simulator
 * Author: Ewan Peterson          
 * Date: February 7 2022
 */

#region libraries

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Media;

#endregion

namespace Pong
{
    public partial class Form1 : Form
    {
        #region global values

        //graphics objects for drawing
        SolidBrush drawBrush = new SolidBrush(Color.White);
        Font drawFont = new Font("Comic Sans MS", 25);

        // Sounds for game
        SoundPlayer scoreSound = new SoundPlayer(Properties.Resources.score);
        SoundPlayer collisionSound = new SoundPlayer(Properties.Resources.collision);

        //determines whether a key is being pressed or not
        Boolean aKeyDown, zKeyDown, kKeyDown, mKeyDown;

        // check to see if a new game can be started
        Boolean newGameOk = true;

        //ball directions, speed, and rectangle
        Boolean ballMoveRight = true;
        Boolean ballMoveDown = false;
        int BALL_SPEED = 4;
        int BallSpeedCount = 0;

        //paddle speeds and rectangles
        const int PADDLE_SPEED = 4;
        int pHeight, pWidth;

        Rectangle p1 = new Rectangle(20, 195, 10, 60);
        Rectangle p2 = new Rectangle(586, 195, 10, 60);
        Rectangle ball = new Rectangle(295, 195, 10, 10);

        //player and game scores
        int player1Score = 0;
        int player2Score = 0;
        int gameWinScore = 3;  // number of points needed to win game

        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        // -- YOU DO NOT NEED TO MAKE CHANGES TO THIS METHOD
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //check to see if a key is pressed and set is KeyDown value to true if it has
            switch (e.KeyCode)
            {
                case Keys.A:
                    aKeyDown = true;
                    break;
                case Keys.Z:
                    zKeyDown = true;
                    break;
                case Keys.K:
                    kKeyDown = true;
                    break;
                case Keys.M:
                    mKeyDown = true;
                    break;
                case Keys.Y:
                case Keys.Space:
                    if (newGameOk)
                    {
                        SetParameters();
                    }
                    break;
                case Keys.N:
                    if (newGameOk)
                    {
                        Close();
                    }
                    break;
            }
        }

        // -- YOU DO NOT NEED TO MAKE CHANGES TO THIS METHOD
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            //check to see if a key has been released and set its KeyDown value to false if it has
            switch (e.KeyCode)
            {
                case Keys.A:
                    aKeyDown = false;
                    break;
                case Keys.Z:
                    zKeyDown = false;
                    break;
                case Keys.K:
                    kKeyDown = false;
                    break;
                case Keys.M:
                    mKeyDown = false;
                    break;
            }
        }

        /// <summary>
        /// sets the ball and paddle positions for game start
        /// </summary>
        private void SetParameters()
        {
            if (newGameOk)
            {
                player1Score = player2Score = 0;
                newGameOk = false;
                startLabel.Visible = false;
                gameUpdateLoop.Start();
            }

            //set starting position for paddles on new game and point scored 
            const int PADDLE_EDGE = 20;  // buffer distance between screen edge and paddle            

            pWidth = 10;    //width for paddles
            pHeight = 60;   //height for paddles

            //p1 starting position
            p1.X = PADDLE_EDGE;
            p1.Y = this.Height / 2 - pHeight / 2;

            //p2 starting position
            p2.X = this.Width - PADDLE_EDGE - pWidth;
            p2.Y = this.Height / 2 - pHeight / 2;

            //ball start point
            ball.X = 295;
            ball.Y = 195;

            BALL_SPEED = 4;
        }

        /// <summary>
        /// This method is the game engine loop that updates the position of all elements
        /// and checks for collisions.
        /// </summary>
        private void gameUpdateLoop_Tick(object sender, EventArgs e)
        {
            #region update ball position

            // move ball either left or right based on ballMoveRight and using BALL_SPEED
            if (ballMoveRight == true)
            {
                ball.X += BALL_SPEED;
            }
            else
            {
                ball.X -= BALL_SPEED;
            }

            // Move paddles using Keyup and Keydown events

            if (ballMoveDown == true)
            {
                ball.Y += BALL_SPEED;
            }
            else
            {
                ball.Y -= BALL_SPEED;
            }

            #endregion

            #region update paddle positions

            //move paddle when keys are pressed
            if (aKeyDown == true && p1.Y > 0)
            {
                p1.Y -= PADDLE_SPEED;
            }
            if (zKeyDown == true && p1.Y < this.Height - pHeight)
            {
                p1.Y += PADDLE_SPEED;
            }
            if (kKeyDown == true && p2.Y > 0)
            {
                p2.Y -= PADDLE_SPEED;
            }
            if (mKeyDown == true && p2.Y < this.Height - pHeight)
            {
                p2.Y += PADDLE_SPEED;
            }

            #endregion

            #region ball collision with top and bottom lines

            if (ball.Y < 0 || ball.Y > this.Height - ball.Height) // if ball hits top or bottom line
            {
                ballMoveDown =! ballMoveDown;
            }
           
            #endregion

            #region ball collision with paddles

            //check for paddle collision with ball
            if (p1.IntersectsWith(ball) || p2.IntersectsWith(ball))
            {
                //switch direction and play hit sound
                ballMoveRight = !ballMoveRight;
                collisionSound.Play();
                BallSpeedCount++;
            }

            if (BallSpeedCount >= 3)
            {
                BallSpeedCount = 0;
                BALL_SPEED++;
            }

            /*  ENRICHMENT
             *  Instead of using two if statments as noted above see if you can create one
             *  if statement with multiple conditions to play a sound and change direction
             */

            #endregion

            #region ball collision with side walls (point scored)

            if (ball.X < 0)  // ball hits left wall 
            {
                //reset game position
                player2Score++;
                scoreSound.Play();
                SetParameters();
                ballMoveRight = !ballMoveRight;

                //check for wiin
                if (player2Score >= gameWinScore)
                {
                    GameOver("Player 2");
                }
            }

            //ball hits right wall
            if (ball.X > this.Width - ball.Width)
            {
                //reset game position
                player1Score++;
                scoreSound.Play();
                SetParameters();
                ballMoveRight = !ballMoveRight;
                if (player1Score >= gameWinScore)
                //check for win
                {
                    GameOver("Player 1");
                }
            }

            #endregion

            //refresh the screen, which causes the Form1_Paint method to run
            this.Refresh();
        }

        /// <summary>
        /// Displays a message for the winner when the game is over and allows the user to either select
        /// to play again or end the program
        /// </summary>
        /// <param name="winner">The player name to be shown as the winner</param>
        private void GameOver(string winner)
        {
            //show winner, and ask to play again
            startLabel.Visible = true;
            startLabel.Text = $"  {winner} has won!";
            gameUpdateLoop.Stop();
            Refresh();
            Thread.Sleep(2000);
            startLabel.Text += "  \n Play again? (Y / N)";
            newGameOk = true;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //draw paddles and ball and score while game is running
            if (gameUpdateLoop.Enabled == true)
            {
                e.Graphics.FillRectangle(drawBrush, p1);
                e.Graphics.FillRectangle(drawBrush, p2);
                e.Graphics.FillRectangle(drawBrush, ball);

                e.Graphics.DrawString(player1Score.ToString(), drawFont, drawBrush, 100, 30);
                e.Graphics.DrawString(player2Score.ToString(), drawFont, drawBrush, 475, 30);
            }
        }
    }
}
