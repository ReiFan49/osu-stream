using System;
using osum.Graphics.Sprites;
using osum.GameplayElements.Beatmaps;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using osum.Helpers;
using System.Text.RegularExpressions;
using osum.Graphics.Drawables;
using osum.Graphics.Renderers;
using osum.Graphics.Skins;
using osum.Audio;
using osum.Graphics;
using osum.GameplayElements;
using System.Collections.Generic;
namespace osum.GameModes.SongSelect
{
    internal class BeatmapPanel : pSpriteCollection
    {
        internal Beatmap Beatmap;

        internal pSprite s_BackingPlate;
        internal pSprite s_BackingPlate2;
        internal pText s_Text;
        internal pText s_TextArtist;
        internal pText s_TextCreator;
        internal pSprite s_Thumbnail;

        float base_depth = 0.4f;

        static Color4 colourNormal = new Color4(50, 50, 50, 255);
        static Color4 colourHover = new Color4(28, 139, 242, 255);

        internal const int PANEL_HEIGHT = 60;
        public static Color4 BACKGROUND_COLOUR = new Color4(255, 255, 255, 240);
        private pSprite s_Star;

        internal BeatmapPanel(Beatmap beatmap, SongSelectMode select, int index)
        {
            base_depth += 0.001f * index;

            s_BackingPlate = new pSprite(TextureManager.Load(OsuTexture.songselect_panel), Vector2.Zero)
            {
                DrawDepth = base_depth,
                Colour = new Color4(255, 255, 255, 170)
            };

            Sprites.Add(s_BackingPlate);

            Beatmap = beatmap;

            s_BackingPlate.OnClick += delegate { select.onSongSelected(this, null); };

            s_BackingPlate.HandleClickOnUp = true;

            s_BackingPlate.OnHover += delegate
            {
                s_BackingPlate.FadeOut(100, 0.01f);
                s_BackingPlate2.FadeColour(BeatmapPanel.BACKGROUND_COLOUR, 80);
            };
            s_BackingPlate.OnHoverLost += delegate
            {
                s_BackingPlate.FadeIn(60);
                s_BackingPlate2.FadeColour(Color4.Transparent, 100);
            };

            s_Text = new pText(string.Empty, 26, Vector2.Zero, Vector2.Zero, 0.5f, true, Color4.White, false);
            s_Text.Bold = true;
            s_Text.Offset = new Vector2(100, 0);
            Sprites.Add(s_Text);

            s_TextArtist = new pText(string.Empty, 26, Vector2.Zero, Vector2.Zero, 0.51f, true, Color4.OrangeRed, false);
            //s_TextArtist.TextAlignment = TextAlignment.Right;
            //s_TextArtist.Origin = OriginTypes.TopRight;
            //s_TextArtist.Field = FieldTypes.StandardSnapRight;
            s_TextArtist.Offset = new Vector2(100, 29);
            Sprites.Add(s_TextArtist);

#if !DIST && !iOS
            s_TextCreator = new pText(string.Empty, 14, Vector2.Zero, Vector2.Zero, 0.52f, true, BACKGROUND_COLOUR, false);
            s_TextCreator.Origin = OriginTypes.TopRight;
            s_TextCreator.Field = FieldTypes.StandardSnapRight;
            Sprites.Add(s_TextCreator);
#endif

            pTexture thumb = null;
            
            int starCount = 0;

            if (beatmap != null)
            {
                //string filename = Path.GetFileNameWithoutExtension(beatmap.ContainerFilename);

                //Regex r = new Regex(@"(.*) - (.*) \((.*)\)");
                //Match m = r.Match(filename);
                //s_Text.Text = m.Groups[2].Value;
                //s_TextArtist.Text = m.Groups[1].Value;
                //s_TextCreator.Text = m.Groups[3].Value;

                try
                {
                    s_Text.Text = beatmap.Title;
                    s_TextArtist.Text = beatmap.Artist;
                    if (s_TextCreator != null) s_TextCreator.Text = beatmap.Creator;
                    starCount = (int)beatmap.DifficultyStars;
                }
                catch
                {
                    //could fail due to corrupt package.
                }
            }
            else
            {
                thumb = TextureManager.Load(OsuTexture.songselect_thumb_dl);
            }

            if (thumb != null)
                s_Thumbnail = new pSprite(thumb, Vector2.Zero) { DrawDepth = base_depth + 0.02f };
            else
                s_Thumbnail = new pSpriteDynamic() { LoadDelegate = GetThumbnail, DrawDepth = 0.49f };
            
            s_Thumbnail.AlphaBlend = false;
            s_Thumbnail.Offset = new Vector2(8.5f, 3.8f);
            Sprites.Add(s_Thumbnail);

            s_BackingPlate2 = new pSprite(TextureManager.Load(OsuTexture.songselect_panel_selected), Vector2.Zero)
            {
                DrawDepth = base_depth + 0.01f,
                Colour = new Color4(255, 255, 255, 0)
            };

            Sprites.Add(s_BackingPlate2);


            if (beatmap != null)
            {
                Color4 col = Color4.YellowGreen;
                if (starCount > 4)
                    col = Color4.Red;
                else if (starCount > 2)
                    col = Color4.BlueViolet;

                for (int i = 0; i < starCount; i++)
                {
                    s_Star = new pSprite(TextureManager.Load(OsuTexture.songselect_star), Vector2.Zero)
                    {
                        Origin = OriginTypes.BottomCentre,
                        Field = FieldTypes.StandardSnapTopCentre,
                        DrawDepth = base_depth + 0.06f,
                        Colour = col,
                        Offset = new Vector2(-(15 * starCount / 2f) + i * 15, PANEL_HEIGHT)
                    };
                    Sprites.Add(s_Star);
                }

                BeatmapInfo info = Beatmap.BeatmapInfo;

                foreach (DifficultyScoreInfo diffInfo in info.DifficultyScores.Values)
                {
                    if (diffInfo.HighScore != null)
                    {
                        int offset = 0;
                        switch (diffInfo.difficulty)
                        {
                            case Difficulty.Easy:
                                offset = 150;
                                break;
                            case Difficulty.Normal:
                                offset = 90;
                                break;
                            case Difficulty.Expert:
                                offset = 30;
                                break;
                        }

                        pSprite rankingSprite = new pSprite(diffInfo.HighScore.RankingTextureSmall, Vector2.Zero)
                        {
                            Origin = OriginTypes.Centre,
                            Field = FieldTypes.StandardSnapRight,
                            DrawDepth = base_depth + 0.06f,
                            Offset = new Vector2(offset, PANEL_HEIGHT / 2)
                        };
                        Sprites.Add(rankingSprite);
                        rankSprites.Add(rankingSprite);
                    }
                }
            }
        }

        List<pDrawable> rankSprites = new List<pDrawable>();

        private pTexture GetThumbnail()
        {
            pTexture thumb = null;
            byte[] bytes = Beatmap.GetFileBytes("thumb-128.jpg");
            if (bytes != null)
                thumb = pTexture.FromBytes(bytes);
            return thumb;
        }

        internal void HideRankings()
        {
            foreach (pDrawable p in rankSprites)
                p.FadeOut(300);
        }

        internal void ShowRankings()
        {
            foreach (pDrawable p in rankSprites)
                p.FadeIn(200);
        }
    }
}

