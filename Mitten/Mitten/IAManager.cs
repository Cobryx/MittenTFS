using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Mitten
{
    public class IAManager
    {
        Player[] players;
        List<IEntity>[] entities;
        Dungeon[] dungeons;
        Player target = null;
        Player nearTarget = null;

        int previousWaypoint=-1;
        int currentWaypoint=-1;

        int currentDungeonIndex;

        public IAManager(ref Player[] players, ref List<IEntity>[] entities, ref Dungeon[] dungeons)
        {
            this.players=players;
            this.entities=entities;
            this.dungeons=dungeons;
        }

        public void Target()
        {
                float distance = float.PositiveInfinity;
                float distanceWeak = 320;
                float hp = 100;
                target = null;
                nearTarget = null;
                foreach (Monster m in entities[currentDungeonIndex].OfType<Monster>().Where(x => x.Alive))
                {
                    
                    foreach (Player p in players.Where(x => x.Alive))
                    {
                      //  if (m is Zombie || m is Banshee)
                        {
                            if (Vector2.Distance(p.getPosition, m.getPosition) < distance)
                            {
                                    target = p;
                                    distance = Vector2.Distance(p.getPosition, m.getPosition);

                            }
                            nearTarget = p;
                        }
                     /* else
                        {
                            if(Vector2.Distance(p.getPosition, m.getPosition) < distance)
                            {
                                target = p;
                                hp = p.getHealth;
                            }
                            else
                                target = nearTarget;
                        }*/
                    }
                    hp = 100;
                    distance = 10000;
                    distanceWeak = 320;

                    if (m.requiredPath && target != null)
                    {
                        m.AssignPath(null);//debug / video
                        //m.AssignPath(dungeons[currentDungeonIndex].FindShortestPath(m.getPosition, target.getPosition));
                    }
                    else
                        m.AssignPath(null);
                    m.LookAt(target);
                  /*  if (m.requiredAlternativePath && m.obstructive is IBypass)
                    {
                        IBypass b = (IBypass)m.obstructive;
                        List<Circle> cList = new List<Circle>();
                        float d = float.PositiveInfinity;   //distanze
                        float e = float.PositiveInfinity;
                        float z;
                        int n = 0;  //indici dei waypoint più vicini
                        int o = 0;
                        for (int i = 0; i < b.Alternative.Count; i++)
                        {
                            z = Vector2.Distance(b.Alternative[i].c.Center, m.getPosition);
                            if (z < d)
                            {
                                d = z;
                                n = i;
                            }
                            else if (z < e)
                            {
                                e = z;
                                o = i;
                            }
                        }
                        m.sensorBox.HalfWidths = new Vector2(Vector2.Distance(m.obstructive.getPosition, m.getPosition), 16);
                        cList.Add(b.Alternative[n].c);
                        cList.Add(b.Alternative[o].c);
                        /*if (m.requiredAlternativePath) //debug
                        {
                            foreach (Circle c in dungeons[currentDungeonIndex].FindShortestPath(m.getPosition, target.getPosition))
                            {
                                cList.Add(c);
                            }
                            m.AssignPath(cList);
                        }
                        else m.AssignPath(null);

                    }*/

                 //   m.requiredPath = false;
                    
                   // m.requiredAlternativePath = false;
                }
        }

        //fondere weakplaere e nearplayer in modo da scorrere solo una voltta la lista
        /*public void WeakPlayer()
        {
            
            foreach (Monster m in entities[currentDungeonIndex].OfType<Monster>())
            {
                foreach (Player p in players)
                {

                    if (p.getHealth < hp && Vector2.Distance(p.getPosition, m.getPosition) < distance)
                    {
                        weakPlayer = p;
                        hp = p.getHealth;
                    }
                }
                if (weakPlayer != null)
                    m.LookAt(weakPlayer);
                else
                    NearPlayer();
            }
        }*/

        public List<Circle> GetPath(Vector2 position, IEntity target)
        {
                return dungeons[currentDungeonIndex].FindShortestPath(position, target.getPosition);
        }
        
        public void Update(GameTime gameTime, int currentDungeonIndex)
        {
            Target();
       
            this.currentDungeonIndex = currentDungeonIndex;
            //non ottimizzato, ovviamente da rifare integralmente
            previousWaypoint = currentWaypoint;
            currentWaypoint=dungeons[currentDungeonIndex].FindNearestWaypoint(players[0].getPosition);
           /* foreach(Monster m in entities[currentDungeonIndex].OfType<Monster>())
            {
                switch (m.getType)
                {
                    case (int)entityTypes.zombie:
                        if (nearTarget != null)
                            m.LookAt(nearTarget); 
                        break;
                    case (int)entityTypes.spiderbot:
                        if (nearTarget != null)
                        {
                            //if (Circle.intersect(m.getSeekingCircle, nearPlayer.getBoundingCircle))
                                m.AssignPath(null);
                            if (currentWaypoint != previousWaypoint)
                                m.AssignPath(dungeons[currentDungeonIndex].FindShortestPath(m.getPosition, nearTarget.getPosition));
                        }
                        break;
                    case (int)entityTypes.wizard:
                        if (nearTarget != null)
                        {
                            //if (Circle.intersect(m.getSeekingCircle, nearPlayer.getBoundingCircle))
                                m.AssignPath(null);
                            if (currentWaypoint != previousWaypoint)
                                m.AssignPath(dungeons[currentDungeonIndex].FindShortestPath(m.getPosition, nearTarget.getPosition));
                        }
                        break;
                    case (int)entityTypes.banshee:
                        if (nearTarget != null)
                            m.LookAt(nearTarget); 
                        break;
                }
            }*/
        }


        public void Intensity(IEntity p)
        {
            float distance = 320;
            Color tone = Color.White;
            foreach (Torch t in entities[currentDungeonIndex].OfType <Torch>())
            {
                if (Vector2.Distance(p.getPosition,t.getPosition)<distance)
                {
                    tone = t.Color*t.Intensity;
                    distance = Vector2.Distance(p.getPosition,t.getPosition);
                }
            }
            float lightPercent =2/((distance  / 320)+1); //potenza dell'illuminazione tra 1 e 2
            tone = new Color(tone.R ,tone.G, tone.B);
            if (distance >= 320)
                p.Color = Globals.krypton.AmbientColor;
            else
                p.Color = new Color((int)(Globals.krypton.AmbientColor.R + (float)tone.R * (lightPercent - 1)), (int)(Globals.krypton.AmbientColor.G + (float)tone.G * (lightPercent - 1)), (int)(Globals.krypton.AmbientColor.B + (float)tone.B * (lightPercent - 1)));
          
        }
    }
}