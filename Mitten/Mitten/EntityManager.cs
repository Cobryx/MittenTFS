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

namespace Mitten
{
    public class EntityManager
    {
        private int[] state;
        private bool[] oldState;
        private int[] stateTime;
        private int[] st_duration;
        private List<int>[] dependencies;
        private bool[] justFinished;
        private bool[] perpetual;

        private int na;
        private int ns;

        private int[] frame;
        private int[] frameChange;
        private int[] an_duration;
        private bool[] active;
        SpriteSheet sheet;
        private int currentAnimation;

        //nuove - introdotte per il binding tra animazioni e stati
        private int[] boundAnimation;
        private bool[] loopable;
        private int[] previousState;    //questo potrebbe o dovrebbe sostituire anche oldState[]
        private int[] previousAnimation;


        /// <summary>
        /// Costruttore del gestore entità.
        /// </summary>
        /// <param name="states">Numero degli stati previsti per questa entità.</param>
        /// <param name="animations">Numero delle animazioni previste per questa entità.</param>
        public EntityManager(int states, int animations, ref SpriteSheet spriteSheet)
        {
            ns = states;
            state = new int[ns];
            oldState = new bool[ns];
            stateTime = new int[ns];
            st_duration = new int[ns];
            dependencies = new List<int>[ns];
            justFinished = new bool[ns];
            perpetual = new bool[ns];

            boundAnimation = new int[ns];
            previousState = new int[ns];

            for (int i = 0; i < ns; i++)
            {
                state[i] = 0;
                oldState[i] = false;
                st_duration[i] = 0;
                dependencies[i] = new List<int>();
                justFinished[i] = false;
            }

            na = animations;
            sheet = spriteSheet;
            frame = new int[na];
            frameChange = new int[na];
            an_duration = new int[na];
            active = new bool[na];

            loopable = new bool[na];
            previousAnimation = new int[na];
        }

        public EntityManager(EntityManager e)
        {
            ns = e.ns;
            state = new int[ns];
            oldState = new bool[ns];
            stateTime = new int[ns];
            st_duration = new int[ns];
            dependencies = new List<int>[ns];
            justFinished = new bool[ns];
            perpetual = new bool[ns];

            boundAnimation = new int[ns];
            previousState = new int[ns];

            for (int i = 0; i < ns; i++)
            {
                state[i] = e.state[i];
                oldState[i] = e.oldState[i];
                stateTime[i] = e.stateTime[i];
                st_duration[i] = e.st_duration[i];

                dependencies[i] = new List<int>();
                dependencies[i].AddRange(e.dependencies[i]);

                justFinished[i] = e.justFinished[i];
                perpetual[i] = e.perpetual[i];
                boundAnimation[i] = e.boundAnimation[i];
                previousState[i] = e.previousState[i];
            }

            na = e.na;
            sheet = e.sheet;
            frame = new int[na];
            frameChange = new int[na];
            an_duration = new int[na];
            active = new bool[na];

            loopable = new bool[na];
            previousAnimation = new int[na];

            for (int i = 0; i < na; i++)
            {
                frame[i] = e.frame[i];
                frameChange[i] = e.frameChange[i];
                e.an_duration[i] = e.an_duration[i];
                active[i] = e.active[i];
                loopable[i] = e.loopable[i];
                previousAnimation[i] = e.previousAnimation[i];
            }
        }

        public void AutoOff()
        {
            for (int i = 0; i < ns; i++)
            {
                if (stateTime[i] < 0 && !perpetual[i])
                {
                    SetOff(i);
                }
            }
        }

        public void Bind(int state, int animation, bool loopable, bool setCurrent, bool reset=true)
        {
            boundAnimation[state] = animation;
            StartAnimation(animation, loopable, setCurrent, reset);
        }

        /// <summary>
        /// Ottiene o imposta l'indice dell'animazione corrente.
        /// </summary>
        public int CurrentAnimation
        {
            get { return currentAnimation; }
            set
            {
                currentAnimation = value;
                if (!active[value])
                {
                    active[value] = true;
                }
            }
        }

        /// <summary>
        /// Ottiene il frame corrente per l'animazione corrente.
        /// </summary>
        /// <returns>Restituisce l'indice del frame corrente.</returns>
        public int GetCurrentFrame()
        {
            return frame[currentAnimation];
        }

        public int GetFrameTime(int animation)
        {
            return frameChange[animation];
        }

        public void ResetAnimation(int animation)
        {
            frame[animation] = 0;
            frameChange[animation] = 0;
            an_duration[animation] = 0;
        }

        public int TimeLeft(int s)
        {
            return stateTime[s];
        }

        public int TimeOn(int s)
        {
            return st_duration[s];
        }

        public bool Finished(int s)
        {
            return justFinished[s];
        }

        public bool IsOn(int s)
        {
            if (state[s] == 1)
                return true;
            else return false;
        }

        public bool IsOff(int s)
        {
            if (state[s] == 0)
                return true;
            else return false;
        }

        public bool IsLocked(int s)
        {
            if (state[s] == 2)
                return true;
            else return false;
        }

        public bool Old(int s)
        {
            return oldState[s];
        }

        public void Prolongate(int s)
        {

        }

        /// <summary>
        /// Prolunga di un dato lasso temporale la permanenza in 'on' dello stato specificato. Non ha effetto su stati bloccati o inattivi.
        /// </summary>
        /// <param name="s">Numero dello stato</param>
        /// <param name="time">Incremento temporale</param>
        public void Prolongate(int s, int time)
        {
            if(state[s]==1)
                stateTime[s] += time;
        }

        public void Prolongate(int s, int time, int boundAnimation, bool loopable, bool setCurrent, bool reset = true)
        {
            if (state[s] == 1)
            {
                stateTime[s] += time;
                this.boundAnimation[s] = boundAnimation;
                StartAnimation(boundAnimation, loopable, setCurrent, reset);
            }
        }


        public void Reiterate(int s)
        {
            
        }

        public void SetOff(int s)
        {
            if (state[s] != 0)
            {
                if (state[s] == 1)
                {
                    oldState[s] = true;
                }
                state[s] = 0;
                stateTime[s] = 0;
                st_duration[s] = 0;
                foreach (int d in dependencies[s])
                {
                    state[d] = 0;
                }

                dependencies[s].Clear();

                if (boundAnimation[s] != -1)
                {
                    StopAnimation(boundAnimation[s]);
                    boundAnimation[s] = -1;
                }

                justFinished[s] = true;
                perpetual[s] = false;
            }
        }


        public void SetOn(int s, int boundAnimation, bool loopable, bool setCurrent, bool reset=true,List<int> dep=null)
        {
            if (state[s] != 1)
            {
                oldState[s] = false;
            }
            perpetual[s] = true;
            state[s] = 1;

            if (dep != null)
            {
                dependencies[s] = dep;
                foreach (int d in dependencies[s])
                {
                    this.SetLock(d);
                }
            }

            this.boundAnimation[s] = boundAnimation;
            StartAnimation(boundAnimation, loopable, setCurrent, reset);
        }

        public void SetOn(int s, int time, int boundAnimation, bool loopable, bool setCurrent, List<int> dep = null,bool reset = true)
        {
            if (state[s] != 1)
            {
                oldState[s] = false;
            }
            if (time >= 0)
            {
                stateTime[s] = time;
            }
            else
            {
                perpetual[s] = true;
            }
            state[s] = 1;

            if (dep != null)
            {
                dependencies[s] = dep;
                foreach (int d in dependencies[s])
                {
                    this.SetLock(d);
                }
            }
            this.boundAnimation[s] = boundAnimation;
            StartAnimation(boundAnimation, loopable, setCurrent, reset);
        }

        public void SetLock(int s)
        {
            SetOff(s);
            state[s] = 2;
        }

        public void StartAnimation(int a, bool loop, bool setCurrent, bool reset)
        {
            active[a] = true;
            loopable[a] = loop;
            if (setCurrent)
            {
                currentAnimation = a;
            }
            if (reset)
            {
                frame[a] = 0;
                frameChange[a] = 0;
                an_duration[a] = 0;
            }
        }

        public void StopAnimation(int a)
        {
            active[a]=false;
            loopable[a]=false;
        }

        public void Unlock(int s)
        {
            if (state[s] == 2)
            {
                oldState[s] = false;
                state[s] = 0;
                st_duration[0] = 0;
            }
        }

        public void UnlockAll()
        {
            for (int i = 0; i < ns; i++)
            {
                if (state[i] == 2)
                {
                    oldState[i] = false;
                    state[i] = 0;
                    stateTime[i] = 0;
                }
            }
        }
                
        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < ns; i++)
            {
                justFinished[i] = false;
                if (state[i] == 1)
                {
                    oldState[i] = true;
                    stateTime[i] -= (int)gameTime.ElapsedGameTime.TotalMilliseconds;
                    st_duration[i] += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
                }
                else
                {
                    oldState[i] = false;
                }
            }
            //AutoOff();

            for (int i = 0; i < na; i++)
            {
                if (active[i])
                {
                    an_duration[i] += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (an_duration[i] > sheet.GetFrameDuration(i, frame[i]))
                    {
                        an_duration[i] %= sheet.GetFrameDuration(i, frame[i]);
                        frame[i]++;
                        if (frame[i] > sheet.GetFrameNumber(i))
                        {
                            if (loopable[i])
                            {
                                frame[i] %= sheet.GetFrameNumber(i);
                            }
                            else
                            {
                                StopAnimation(i);
                            }
                        }
                    }
                }
            }
        }
    }
}
