using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Input;
using System.Drawing;
using osum.GameModes;
using osum.Support;
using osum.Audio;
using osum.Helpers;
using osum.Graphics.Skins;
using osum.GameplayElements;
using osum.GameplayElements.Scoring;
using System.Runtime.InteropServices;
using osum.Input.Sources;

namespace osum
{
    public class GameWindowDesktop : GameWindow
    {
        /// <summary>Creates a 1024x768 window with the specified title.</summary>
        public GameWindowDesktop()
            : base(960, 640, GraphicsMode.Default, "osu!stream")
        {
            VSync = VSyncMode.Off;
            //this.Location = new Point(1920, 0);

#if !DEBUG
            WindowState = WindowState.Fullscreen;
#endif
            //GameBase.WindowSize = new Size(960,640);
        }

        [DllImport("user32.dll")]
        static extern int ShowCursor(bool bShow);

        /// <summary>Load resources here.</summary>
        /// <param name="e">Not used.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            MakeCurrent();

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.Blend);

            GameBase.Instance.Initialize();

            if (InputManager.RegisteredSources.Count > 0 && InputManager.RegisteredSources[0] is InputSourceBaanto)
                ShowCursor(false);

            KeyPress += new EventHandler<KeyPressEventArgs>(GameWindowDesktop_KeyPress);
        }

        void GameWindowDesktop_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case 'a':
                    Player.Autoplay = !Player.Autoplay;
                    break;
                case 'r':
                    Director.ChangeMode(Director.CurrentOsuMode);
                    break;
                case 'd':
                    TextureManager.PurgeUnusedTexture();
                    break;
                case 's':
                    TextureManager.ReloadAll();
                    break;
                case 'x':
                    TextureManager.DisposeAll();
                    break;
                case 'o':
                    Director.ChangeMode(OsuMode.Options);
                    break;
                case 'v':
                    Director.ChangeMode(OsuMode.VideoPreview, new FadeTransition(1000, 1500));
                    return;
                case 'e':
                    DifficultyScoreInfo bmi = BeatmapDatabase.GetDifficultyInfo(Player.Beatmap, Difficulty.Normal);
                    if (bmi == null) break;
                    if (bmi.HighScore == null)
                    {
                        GameBase.Notify("Unlocked expert");
                        bmi.HighScore = new Score();
                        bmi.HighScore.comboBonusScore = 1000000;
                    }
                    break;
                case 'k':
                    Director.ChangeMode(OsuMode.PositioningTest);
                    break;
                case 'h':
                    if (ClientSize.Width == 960)
                        ClientSize = new Size(480, 320);
                    else
                        ClientSize = new Size(960, 640);
                    break;
                case 'i':
                    ClientSize = new Size(1024, 768);
                    break;
                case '5':
                    ClientSize = new Size(1136, 640);
                    break;
                case 'p':
                    {
                        if (Director.CurrentMode is Player)
                        {
                            Player p = Director.CurrentMode as Player;
                            if (!p.IsPaused)
                                p.Pause();
                        }
                        else
                        {
                            Director.ChangeMode(OsuMode.SongSelect);
                        }
                    }
                    break;
                case 'j':
                    {
                        Player p = Director.CurrentMode as Player;
                        if (p != null)
                        {
                            Results.RankableScore = p.CurrentScore;
                            Director.ChangeMode(OsuMode.Results, new ResultTransition());
                        }
                    }
                    break;
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (Director.CurrentOsuMode == OsuMode.PlayTest || Director.CurrentOsuMode == OsuMode.PositioningTest)
            {
                e.Cancel = true;
                Director.ChangeMode(OsuMode.PositioningTest, null);
                return;
            }

            if (Director.CurrentOsuMode != OsuMode.MainMenu)
            {
                e.Cancel = true;
                Director.ChangeMode(OsuMode.MainMenu, new FadeTransition(200, 400));
            }

            GameBase.Instance.Dispose();

            GameBase.Config.SaveConfig();

            base.OnClosing(e);
        }

        /// <summary>
        /// Called when your window is resized. Set your viewport here. It is also
        /// a good place to set up your projection matrix (which probably changes
        /// along when the aspect ratio of your window).
        /// </summary>
        /// <param name="e">Not used.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (GameBase.Instance != null) GameBase.Instance.SetupScreen();

        }

        /// <summary>
        /// Called when it is time to setup the next frame. Add you game logic here.
        /// </summary>
        /// <param name="e">Contains timing information for framerate independent logic.</param>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (Keyboard[Key.Escape])
                Exit();
            if (Keyboard[Key.Right])
                AudioEngine.Music.SeekTo(Clock.AudioTime + 500);
            if (Keyboard[Key.T])
                Director.ChangeMode(OsuMode.Tutorial);

            //todo: make update happen from here.
            if (GameBase.Instance != null) GameBase.Instance.Update();
        }

        /// <summary>
        /// Called when it is time to render the next frame. Add your rendering code here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            if (GameBase.Instance != null) GameBase.Instance.Draw();

            // display
            SwapBuffers();
        }
    }
}