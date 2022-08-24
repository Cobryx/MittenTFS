using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Mitten
{
    [Serializable]
    public class Zone
    {
        bool active;
        Collision a;
        Collision b;
        DamageData alpha;
        DamageData beta;
        Rectangle area;
        List<IEntity> entitiesContained;
        int q;
        float penetration_depth;
        Vector2 distanceVector;
        List<int> roomContained;
        List<int> waypointContained;


        public Zone(Rectangle area)
        {
            active = false;
            this.area = area;
            entitiesContained = new List<IEntity>();
            roomContained = new List<int>();
            waypointContained = new List<int>();
            distanceVector = new Vector2();
        }

        #region properties

        public bool Status
        {
            get { return this.active; }
        }

        public Rectangle Area
        {
            get { return this.area; }
        }

        public List<IEntity> Entities
        {
            get { return this.entitiesContained; }
        }

        /// <summary>
        /// Restituisce gli indici delle stanze contenute nella cella
        /// </summary>
        public List<int> Rooms
        {
            get { return this.roomContained; }
        }

        /// <summary>
        /// Restituisce gli indici dei waypoint contenuti nella cella
        /// </summary>
        public List<int> Waypoints
        {
            get { return this.waypointContained; }
        }

        #endregion

        public void CollisionCheck()
        {
            
            q = entitiesContained.Count;
            float distance_threshold;
            float d;
            
            //implementare Icollider e separare il collisionCheck con il DamageCheck
            if (q < 2)
                return;
            else
            {
                for (int i = 0; i < q; i++)
                {
                    for (int j = i + 1; j < q; j++)
                    {
                        distance_threshold = entitiesContained[i].getBoundingCircle.Radius + entitiesContained[j].getBoundingCircle.Radius;
                        distanceVector = entitiesContained[i].getPosition - entitiesContained[j].getPosition;
                        d = distanceVector.Length(); //to be optimized - do not use square root when not necessary
                        penetration_depth = distance_threshold - d;

                        //collisioni fra i corpi principali delle entità
                        if (OBB.Intersects(entitiesContained[i].getBoundingBox, entitiesContained[j].getBoundingBox))
                        {
                            a.collided = true;
                            b.collided = true;
                            /*
                            if (entitiesContained[i] is Shiftable)
                            {
                                ((Shiftable)entitiesContained[i]).ExternalShift(entitiesContained[j].getDirection);
                            }
                            if (entitiesContained[j] is Shiftable)
                            {
                                ((Shiftable)entitiesContained[j]).ExternalShift(entitiesContained[i].getDirection);
                            }*/
                        }
                        else
                        {
                            a.collided = false;
                            b.collided = false;/*
                            if (entitiesContained[i] is Shiftable)
                            {
                                ((Shiftable)entitiesContained[i]).ExternalShift(Vector2.Zero);
                            }
                            if (entitiesContained[j] is Shiftable)
                            {
                                ((Shiftable)entitiesContained[j]).ExternalShift(Vector2.Zero);
                            }*/
                        }

                        a.distance = d;
                        a.position = entitiesContained[j].getPosition;
                        a.direction = entitiesContained[j].getDirection;
                        a.name = entitiesContained[j].getName;
                        a.type = entitiesContained[j].getType;
                        a.penetration_depth = penetration_depth;
                        a.factionId = entitiesContained[j].Faction;
                        a.id = entitiesContained[j].getId;
                        a.boundingBox = entitiesContained[j].getBoundingBox;
                        a.depth = entitiesContained[j].getDepth;
                        a.subtype = entitiesContained[j].getSubtype;
                        a.generic = entitiesContained[j].generic;
                        a.entity = entitiesContained[j];
                        a.axis = entitiesContained[j].getAxis;
                        a.damage_done = false;

                        b.distance = d;
                        b.position = entitiesContained[i].getPosition;
                        b.direction = entitiesContained[i].getDirection;
                        b.name = entitiesContained[i].getName;
                        b.type = entitiesContained[i].getType;
                        b.penetration_depth = penetration_depth;
                        b.factionId = entitiesContained[i].Faction;
                        b.id = entitiesContained[i].getId;
                        b.boundingBox = entitiesContained[i].getBoundingBox;
                        b.depth = entitiesContained[i].getDepth;
                        b.subtype = entitiesContained[i].getSubtype;
                        b.generic = entitiesContained[i].generic;
                        b.entity = entitiesContained[i];
                        b.axis = entitiesContained[i].getAxis;
                        b.damage_done = false;

                        //entitiesContained[i].SetCollisionData(a);
                        //entitiesContained[j].SetCollisionData(b);

                        //controllo delle entità danneggianti e danneggiabili
                        if (((entitiesContained[i] is IAttacker) && !(entitiesContained[j] is IDamageble)) || ((entitiesContained[j] is IAttacker) && !(entitiesContained[i] is IDamageble)))
                        {
                            if ((!(entitiesContained[i] is IAttacker) && (entitiesContained[j] is IDamageble)) || (!(entitiesContained[j] is IAttacker) && (entitiesContained[i] is IDamageble)))
                            { } //    return; 
                        }

                        b.damage_done = false;
                        if (entitiesContained[i] is IDamageble && entitiesContained[j] is IAttacker && OBB.Intersects(entitiesContained[i].getBoundingBox, ((IAttacker)entitiesContained[j]).getDamageDealt.oArea))
                        {
                            alpha = ((IAttacker)entitiesContained[j]).getDamageDealt;
                            ((IDamageble)entitiesContained[i]).SetDamageData(alpha);
                            b.damage_done = true;
                        }

                        a.damage_done = false;
                        if (entitiesContained[j] is IDamageble && entitiesContained[i] is IAttacker && OBB.Intersects(entitiesContained[j].getBoundingBox, ((IAttacker)entitiesContained[i]).getDamageDealt.oArea))
                        {
                            beta = ((IAttacker)entitiesContained[i]).getDamageDealt;
                            ((IDamageble)entitiesContained[j]).SetDamageData(beta);
                            a.damage_done = true;
                        }

                        a.distance = d;
                        a.position = entitiesContained[j].getPosition;
                        a.direction = entitiesContained[j].getDirection;
                        a.name = entitiesContained[j].getName;
                        a.type = entitiesContained[j].getType;
                        a.penetration_depth = penetration_depth;
                        a.factionId = entitiesContained[j].Faction;
                        a.id = entitiesContained[j].getId;
                        a.boundingBox = entitiesContained[j].getBoundingBox;
                        a.depth = entitiesContained[j].getDepth;
                        a.subtype = entitiesContained[j].getSubtype;
                        a.generic = entitiesContained[j].generic;
                        a.entity = entitiesContained[j];
                        a.axis = entitiesContained[j].getAxis;


                        b.distance = d;
                        b.position = entitiesContained[i].getPosition;
                        b.direction = entitiesContained[i].getDirection;
                        b.name = entitiesContained[i].getName;
                        b.type = entitiesContained[i].getType;
                        b.penetration_depth = penetration_depth;
                        b.factionId = entitiesContained[i].Faction;
                        b.id = entitiesContained[i].getId;
                        b.boundingBox = entitiesContained[i].getBoundingBox;
                        b.depth = entitiesContained[i].getDepth;
                        b.subtype = entitiesContained[i].getSubtype;
                        b.generic = entitiesContained[i].generic;
                        b.entity = entitiesContained[i];
                        b.axis = entitiesContained[i].getAxis;


                        /*if (entitiesContained[j] is IAttacker)
                            alpha = ((IAttacker)entitiesContained[j]).getDamageDealt;
                        if( entitiesContained[i] is IAttacker)
                            beta = ((IAttacker)entitiesContained[i]).getDamageDealt;*/

                        entitiesContained[i].SetCollisionData(a);
                        entitiesContained[j].SetCollisionData(b);

                    }
                }
            }
        }

        public void AddAnEntity(ref IEntity entity)
        {
            entitiesContained.Add(entity);
        }

        public void Clear()
        {
            entitiesContained.Clear();
        }

        public bool isEmpty()
        {
            if (entitiesContained.Count == 0)
                return true;
            else return false;
        }

        public void RegisterRoom(int roomNumber)
        {
            if(!roomContained.Contains(roomNumber))
                roomContained.Add(roomNumber);
        }

        public void RegisterWaypoint(int waypointNumber)
        {
            waypointContained.Add(waypointNumber);
        }
    }
}
