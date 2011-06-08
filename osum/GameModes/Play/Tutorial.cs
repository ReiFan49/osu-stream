﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.GameModes.SongSelect;
using osum.GameplayElements.Beatmaps;
using osum.GameplayElements;
using osum.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;
using osum.Helpers;
using osum.Graphics.Renderers;
using osum.Support;
using osum.GameplayElements.Scoring;
using osum.GameModes.Play.Components;
using osum.Audio;

namespace osum.GameModes.Play
{
    class Tutorial : Player
    {
        BackButton backButton;
        pText touchToContinueText;

        internal override void Initialize()
        {
            Difficulty = Difficulty.None;
            Beatmap = null;

            MainMenu.InitializeBgm();

            touchToContinueText = new pText("Tap to continue!", 30, new Vector2(0, 20), 1, true, Color4.YellowGreen)
            {
                TextBounds = new Vector2(GameBase.BaseSize.Width * 0.8f, 0),
                Field = FieldTypes.StandardSnapBottomCentre,
                TextAlignment = TextAlignment.Centre,
                TextShadow = true,
                Bold = true,
                Origin = OriginTypes.BottomCentre
            };
            spriteManager.Add(touchToContinueText);

            backButton = new BackButton(delegate { Director.ChangeMode(OsuMode.MainMenu); });
            spriteManager.Add(backButton);

            base.Initialize();

            loadNextSegment();
        }

        TutorialSegments currentSegment;
        TutorialSegments nextSegment = TutorialSegments.Introduction_1;

        VoidDelegate currentSegmentDelegate;


        bool touchToContinue = true;
        private void showTouchToContinue()
        {
            backButton.Alpha = 1;

            touchToContinue = true;
            touchToContinueText.Transformations.Clear();
            touchToContinueText.Transform(new TransformationBounce(Clock.ModeTime, Clock.ModeTime + 300, 1, -0.2f, 1));
            touchToContinueText.Transform(new Transformation(TransformationType.Fade, 1, 0, Clock.ModeTime, Clock.ModeTime + 800, EasingTypes.In) { LoopDelay = 600, Looping = true });
        }

        protected override void InputManager_OnDown(InputSource source, TrackingPoint point)
        {
            if (touchToContinue && !backButton.IsHovering)
            {
                loadNextSegment();
                return;
            }

            base.InputManager_OnDown(source, point);
        }

        private void showText(string text, float verticalOffset = 0)
        {
            pText pt = new pText(text, 30, new Vector2(0, verticalOffset), 1, true, Color4.White)
            {
                TextBounds = new Vector2(GameBase.BaseSize.Width * 0.95f, 0),
                Field = FieldTypes.StandardSnapCentre,
                TextAlignment = TextAlignment.Centre,
                TextShadow = true,
                Origin = OriginTypes.Centre
            };

            pt.ScaleScalar = 1.4f;
            pt.FadeIn(300);
            pt.ScaleTo(1, 400, EasingTypes.In);
            currentSegmentSprites.Add(pt);
        }

        public override void Update()
        {
            if (currentSegmentDelegate != null) currentSegmentDelegate();

            base.Update();
        }

        protected override void initializeUIElements()
        {
            //base.initializeUIElements();
        }

        enum TutorialSegments
        {
            None,
            Introduction_1,
            Introduction_2,
            Introduction_3,
            Healthbar_1,
            Healthbar_2,
            Healthbar_3,
            Healthbar_4,
            Healthbar_5,
            Healthbar_End,
            Score_1,
            Score_2,
            Score_3,
            Score_4,
            HitCircle_1,
            HitCircle_2,
            HitCircle_3,
            End,
        }

        List<pDrawable> currentSegmentSprites = new List<pDrawable>();

        private void loadNextSegment()
        {
            loadSegment(nextSegment);
        }

        private void loadSegment(TutorialSegments segment)
        {
            currentSegment = segment;

            nextSegment = (TutorialSegments)(currentSegment + 1);

            foreach (pDrawable p in currentSegmentSprites)
            {
                p.AlwaysDraw = false;
                p.FadeOut(100);
                p.ScaleTo(0.9f, 400, EasingTypes.In);
            }

            currentSegmentSprites.Clear();
            currentSegmentDelegate = null;
            touchToContinue = true;

            switch (currentSegment)
            {
                case TutorialSegments.Introduction_1:
                    showText("Welcome to the world of osu!.\nThis tutorial will teach you everything you need to know in order to become a rhythm master.");
                    break;
                case TutorialSegments.Introduction_2:
                    showText("osu!stream is a game which requires both rhythmical and positional accuracy.");
                    playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_STANDARD, false);
                    break;
                case TutorialSegments.Introduction_3:
                    showText("You will need to feel the beat, so make sure you are using headphones or playing in quiet surroundings!");
                    break;
                case TutorialSegments.Healthbar_1:
                    showText("The health bar is located at the top-left of your display.");
                    healthBar = new HealthBar();
                    {
                        pDrawable lastFlash = null;
                        currentSegmentDelegate = delegate
                        {
                            if (lastFlash == null || lastFlash.Alpha == 0)
                                lastFlash = healthBar.s_barBg.AdditiveFlash(1000, 1).ScaleTo(healthBar.s_barBg.ScaleScalar * 1.04f, 1000);
                        };
                    }

                    streamSwitchDisplay = new StreamSwitchDisplay();
                    break;
                case TutorialSegments.Healthbar_2:
                    showText("It will go up or down depending on your performance.");
                    break;
                case TutorialSegments.Healthbar_3:
                    showText("In stream mode gameplay, you can jump to the next stream by filling your health bar.", -120);
                    touchToContinue = false;

                    healthBar.SetCurrentHp(100);
                    {
                        float increaseRate = 0;
                        currentSegmentDelegate = delegate
                        {
                            if (touchToContinue) return;

                            if (healthBar.CurrentHp == 200)
                            {
                                if (increaseRate > 20)
                                {
                                    streamSwitchDisplay.EndSwitch();
                                    healthBar.SetCurrentHp(100);
                                    playfieldBackground.ChangeColour(Difficulty.Hard);
                                    showTouchToContinue();
                                }
                                else
                                {
                                    increaseRate += 0.2f;
                                    streamSwitchDisplay.BeginSwitch(true);
                                    playfieldBackground.Move(increaseRate);
                                }
                            }
                            else
                            {
                                healthBar.SetCurrentHp(healthBar.CurrentHp + 1);
                            }
                        };
                    }
                    break;
                case TutorialSegments.Healthbar_4:
                    showText("In a similar manner, if it reaches zero, you will drop down a stream.", -120);
                    touchToContinue = false;
                    {
                        float increaseRate = 0;
                        currentSegmentDelegate = delegate
                        {
                            if (touchToContinue) return;

                            if (healthBar.CurrentHp == 0)
                            {
                                if (increaseRate > 20)
                                {
                                    streamSwitchDisplay.EndSwitch();
                                    healthBar.SetCurrentHp(100);
                                    playfieldBackground.ChangeColour(Difficulty.Normal);
                                    showTouchToContinue();
                                }
                                else
                                {
                                    increaseRate += 0.2f;
                                    streamSwitchDisplay.BeginSwitch(false);
                                    playfieldBackground.Move(-increaseRate);
                                }
                            }
                            else
                            {
                                healthBar.SetCurrentHp(healthBar.CurrentHp - 1);
                            }
                        };
                    }
                    break;
                case TutorialSegments.Healthbar_5:
                    showText("If it hits zero on the lowest stream you will fail instantly, so watch out!", -120);
                    touchToContinue = false;

                    playfieldBackground.ChangeColour(Difficulty.Easy, false);

                    currentSegmentDelegate = delegate
                    {
                        if (playfieldBackground.Velocity == 0)
                            healthBar.SetCurrentHp(healthBar.CurrentHp - 0.5f);
                        if (healthBar.CurrentHp == 0)
                        {

                            if (!touchToContinue)
                            {
                                showTouchToContinue();
                                playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO);
                                showFailSprite();
                                AudioEngine.Music.Pause();
                            }
                        }
                        else if (healthBar.CurrentHp < HealthBar.HP_BAR_MAXIMUM / 3)
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING);
                    };
                    break;
                case TutorialSegments.Healthbar_End:
                    healthBar.SetCurrentHp(100);
                    hideFailSprite();
                    AudioEngine.Music.Play();
                    playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_STANDARD);
                    healthBar.InitialIncrease = true;
                    touchToContinue = false;
                    currentSegmentDelegate = delegate { if (healthBar.DisplayHp > 20) loadNextSegment(); };
                    break;
                case TutorialSegments.Score_1:
                    scoreDisplay = new ScoreDisplay();
                    {
                        pDrawable lastFlash = null;
                        currentSegmentDelegate = delegate
                        {
                            if (lastFlash == null || lastFlash.Alpha == 0)
                                scoreDisplay.spriteManager.Sprites.ForEach(s => lastFlash = s.AdditiveFlash(1000, 1).ScaleTo(s.ScaleScalar * 1.1f, 1000));
                        };
                    }
                    showText("Scoring is based on a your accuracy and combo.");
                    break;
                case TutorialSegments.Score_2:
                    showText("You can also get score bonuses from reaching higher streams, and for spinning spinners fast!");
                    break;
                case TutorialSegments.Score_3:
                    showText("Your current combo can be seen in the bottom-left corner of the screen.");
                    touchToContinue = false;

                    comboCounter = new ComboCounter();
                    comboCounter.SetCombo(35);
                    {
                        pDrawable lastFlash = null;
                        currentSegmentDelegate = delegate
                        {
                            if (comboCounter.displayCombo == 35)
                            {
                                if (!touchToContinue)
                                    showTouchToContinue();

                                if (lastFlash == null || lastFlash.Alpha == 0)
                                    comboCounter.spriteManager.Sprites.ForEach(s => lastFlash = s.AdditiveFlash(1000, 1).ScaleTo(s.ScaleScalar * 1.1f, 1000));
                            }

                            backButton.Alpha = 0;
                        };


                    }
                    break;
                case TutorialSegments.Score_4:
                    touchToContinue = false;
                    comboCounter.SetCombo(0);
                    showText("Your combo will only show up when you are on a streak!");
                    GameBase.Scheduler.Add(delegate { showTouchToContinue(); }, 1500);
                    break;
                case TutorialSegments.End:
                    backButton.HandleInput = false;
                    Director.ChangeMode(OsuMode.MainMenu, new FadeTransition(3000, FadeTransition.DEFAULT_FADE_IN));
                    touchToContinue = false;
                    break;
                case TutorialSegments.HitCircle_1:
                    resetScore();

                    showText("\"Hit Circles\" are the most basic hit type in osu!.");
                    touchToContinue = true;
                    break;
                case TutorialSegments.HitCircle_2:
                    touchToContinue = false;

                    Difficulty = Difficulty.Easy;

                    Beatmap = new Beatmap();
                    Beatmap.ControlPoints.Add(new ControlPoint(2950, 375, TimeSignatures.SimpleQuadruple, SampleSet.Normal, CustomSampleSet.Default, 100, true, false));

                    if (countdown == null) countdown = new CountdownDisplay();

                    firstCountdown = true;
                    AudioEngine.Music.SeekTo(58000);
                    CountdownResume(2950 + 160 * 375, 8);

                    loadBeatmap();

                    const int x1 = 100;
                    const int x2 = 512 - 100;
                    const int y1 = 80;
                    const int y2 = 384 - 80;

                    hitObjectManager.Add(new HitCircle(hitObjectManager, new Vector2(x1, y1), 2950 + 160 * 375, true, 0, HitObjectSoundType.Normal), Difficulty);
                    hitObjectManager.Add(new HitCircle(hitObjectManager, new Vector2(x2, y1), 2950 + 164 * 375, true, 0, HitObjectSoundType.Finish), Difficulty);
                    hitObjectManager.Add(new HitCircle(hitObjectManager, new Vector2(x1, y2), 2950 + 168 * 375, true, 0, HitObjectSoundType.Normal), Difficulty);
                    hitObjectManager.Add(new HitCircle(hitObjectManager, new Vector2(x2, y2), 2950 + 172 * 375, true, 0, HitObjectSoundType.Finish), Difficulty);

                    hitObjectManager.Add(new HitCircle(hitObjectManager, new Vector2(x2, y1), 2950 + 176 * 375, true, 0, HitObjectSoundType.Normal), Difficulty);
                    hitObjectManager.Add(new HitCircle(hitObjectManager, new Vector2(x1, y2), 2950 + 180 * 375, true, 0, HitObjectSoundType.Finish), Difficulty);
                    hitObjectManager.Add(new HitCircle(hitObjectManager, new Vector2(x2, y2), 2950 + 184 * 375, true, 0, HitObjectSoundType.Normal), Difficulty);
                    hitObjectManager.Add(new HitCircle(hitObjectManager, new Vector2(x1, y1), 2950 + 188 * 375, true, 0, HitObjectSoundType.Finish), Difficulty);

                    hitObjectManager.PostProcessing();

                    hitObjectManager.SetActiveStream(Difficulty.Easy);

                    currentSegmentDelegate = delegate
                    {
                        if (!touchToContinue && hitObjectManager.AllNotesHit)
                            loadNextSegment();
                    };
                    break;
                case TutorialSegments.HitCircle_3:
                    if (currentScore.countMiss > 0)
                    {
                        showText("Hmm, looks like we need to practise a bit more. Let's go over this again!");
                        nextSegment = TutorialSegments.HitCircle_1;
                    }
                    else if (currentScore.count50 > 0)
                    {
                        showText("Getting there!\nWatch the approaching circle carefully and listen to the beat. Let's try once more!");
                        nextSegment = TutorialSegments.HitCircle_2;
                    }
                    else if (currentScore.count100 > 0)
                    {
                        showText("That's right!\nFocus on the beat of the song and try to time your taps to get higher accuracy.");
                    }
                    else
                    {
                        showText("Flawless! Great job!");
                    }

                    break;

            }

            if (touchToContinue)
                showTouchToContinue();
            else
            {
                touchToContinueText.Transformations.Clear();
                touchToContinueText.FadeOut(100);
                backButton.Alpha = 0.2f;
            }

            topMostSpriteManager.Add(currentSegmentSprites);
        }

        protected override void CheckForCompletion()
        {
        }

    }
}
