using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;

namespace HeavenStudio.Games.Scripts_BouncyRoad
{
    using HeavenStudio.Util;
    public class Ball : MonoBehaviour
    {
        [System.NonSerialized] public double startBeat, lengthBeat;
        private double currentBeat;

        [System.NonSerialized] public BezierCurve3D[] curve;
        private BezierCurve3D currentCurve;

        public Color color;
        [System.NonSerialized] public bool goal;

        private bool isMiss;

        private BouncyRoad game;

        public void Init()
        {
            game = BouncyRoad.instance;

            GetComponent<SpriteRenderer>().color = color;
            Bounce();
        }
        void Update()
        {
            var cond = Conductor.instance;
            
            if (currentCurve is not null)
            {
                float curveProg = cond.GetPositionFromBeat(currentBeat, lengthBeat);
                if (isMiss) {
                    curveProg = cond.GetPositionFromBeat(currentBeat, lengthBeat/2);
                } else {
                    // curveProg /= (float)(1 + BouncyRoad.ngLateTime);
                    if (curveProg >= 1) curveProg = 1;
                }
                transform.position = currentCurve.GetPoint(curveProg);
            }
        }

        private void Bounce()
        {
            game.PlayBounceSound(startBeat, lengthBeat);

            var actions = new List<BeatAction.Action>();

            actions.Add(new BeatAction.Action(startBeat - lengthBeat, delegate {
                currentCurve = curve[0];
                currentBeat = startBeat - lengthBeat;
            }));
            for (int i = 0; i < 12 ; i++)
            {
                int currentItr = i;
                actions.Add(new BeatAction.Action(startBeat + currentItr * lengthBeat, delegate {
                    game.ThingsAnim[currentItr].Play("podium", 0, 0);
                    currentCurve = curve[1+currentItr];
                    currentBeat = startBeat + currentItr * lengthBeat;
                }));
            }
            BeatAction.New(game, actions);

            game.ScheduleInput(startBeat + 11 * lengthBeat, lengthBeat, BouncyRoad.InputAction_Right, RightSuccess, RightMiss, Empty); 
        }

        public void RightSuccess(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("bouncyRoad/ballRight");
            game.ThingsAnim[12].Play("podium", 0, 0);
            currentCurve = curve[1+12];
            currentBeat = startBeat + 12 * lengthBeat;

            game.ScheduleInput(startBeat + 12 * lengthBeat, lengthBeat, BouncyRoad.InputAction_Left, LeftSuccess, LeftMiss, Empty); 
        }

        public void RightMiss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame("bouncyRoad/ballBounce");
            currentCurve = curve[^2];
            currentBeat = Conductor.instance.songPositionInBeats;
            isMiss = true;
            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(currentBeat + lengthBeat / 2, delegate
                {
                    Destroy(gameObject);
                }),
            });
        }

        public void LeftSuccess(PlayerActionEvent caller, float state)
        {
            SoundByte.PlayOneShotGame("bouncyRoad/ballLeft");
            game.ThingsAnim[13].Play("podium", 0, 0);
            currentCurve = curve[1+13];
            currentBeat = startBeat + 13 * lengthBeat;
            
            if (goal) SoundByte.PlayOneShotGame("bouncyRoad/goal", startBeat + 14 * lengthBeat);
            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(startBeat + 14 * lengthBeat, delegate
                {
                    game.ThingsAnim[14].Play("podium", 0, 0);
                    currentCurve = curve[1+14];
                    currentBeat = startBeat + 14 * lengthBeat;
                }),
                new BeatAction.Action(startBeat + 15 * lengthBeat, delegate
                {
                    Destroy(gameObject);
                }),
            });
        }

        public void LeftMiss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShotGame("bouncyRoad/ballBounce");
            currentCurve = curve[^1];
            currentBeat = Conductor.instance.songPositionInBeats;
            isMiss = true;
            BeatAction.New(game, new List<BeatAction.Action>()
            {
                new BeatAction.Action(currentBeat + lengthBeat / 2, delegate
                {
                    Destroy(gameObject);
                }),
            });
        }

        public void Empty(PlayerActionEvent caller) { }
    }
}