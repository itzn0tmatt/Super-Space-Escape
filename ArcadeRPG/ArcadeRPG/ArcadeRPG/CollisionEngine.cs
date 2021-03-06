using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcadeRPG
{
    enum ColType { MAP, PLAYER, MONSTER, BULLET };

    class Collision
    {
        public ColType type;
        public int id;

        public Collision(ColType _type, int _id)
        {
            type = _type;
            id = _id;
        }
    };

    class ColToken
    {
        CollisionEngine ce;
        
        int engine_id;
        
        ColType local_type;


        List<ColToken> cols;
        object parent;
        public bool updated_this_frame;

        public ColToken(ColType type, CollisionEngine _ce, int _engine_id, object _parent)
        {
            local_type = type;
            ce = _ce;
            parent = _parent;
            updated_this_frame = false;
            cols = new List<ColToken>();
            engine_id = _engine_id;
        }

        //decided on changing the implementation of this after construction of this function.
        //won't delete it because it is called with parameters in the code
        //doesn't hurt the code, just a little unnecessary
        public void update(int x, int y)
        {
            update();
        }

        public void update()
        {
            if (!updated_this_frame)
            {
                ce.mark(this);
                updated_this_frame = true;
            }
        }

        public int getID()
        {
            return engine_id;
        }



        public void Collision(ColToken tok)
        {
            if (!cols.Contains(tok))
            {
                cols.Add(tok);
            }
        }

        public bool HasCollisions() // does the sprite have a collision?
        {

            if (cols.Count() > 0)
            {
                return true;
            }
            return false;
        }

        public ColType GetLocalType()
        {
            return local_type;
        }

        public object GetParent()
        {
            return parent;
        }

        public List<ColToken> GetCollisions()
        {
            return cols;
        }

        public void ResetCollisions() // collisions handled, reset to allow for new collision instances
        {
            cols.Clear();
        }
    }

    class CollisionEngine
    {
        List<ColToken> all_objects;
        List<ColToken> marked_objects; //Objects that have been updated and need to be checked for collision
        int unique_id;

        GameState game_state;

        public CollisionEngine(GameState _game_state)
        {
            game_state = _game_state;
            unique_id = 0;
            all_objects = new List<ColToken>();
            marked_objects = new List<ColToken>();
        }

        public void mark(ColToken tok) {
            marked_objects.Add(tok);
        }

        public ColToken register_object(object parent, ColType type)
        {
            ColToken new_object = new ColToken(type, this, unique_id++, parent);
            all_objects.Add(new_object);

            return new_object; // make a new object for the game
        }

        public void remove_object(ColToken tok)
        {
            bool test = all_objects.Remove(tok);
            bool test2 = marked_objects.Remove(tok);

            tok = null; // take object out of the game
        }

        public void Update()
        {
            for (int i = 0; i < marked_objects.Count(); ++i) // for every object that has been "marked", or changed/modified via gameplay, check to see if there is collisions
            {
                ColToken obj = marked_objects.ElementAt(i);
                int loc_x = 0, loc_y = 0, width = 0, height = 0;
                switch (obj.GetLocalType())
                {
                    case ColType.PLAYER:
                        Player temp_pl = (Player)obj.GetParent();
                        loc_x = temp_pl.getX();
                        loc_y = temp_pl.getY();
                        width = temp_pl.getWidth();
                        height = temp_pl.getHeight();
                        break;
                    case ColType.MONSTER:
                        Enemy temp_em = (Enemy)obj.GetParent();
                        loc_x = temp_em.getX();
                        loc_y = temp_em.getY();
                        width = temp_em.getWidth();
                        height = temp_em.getHeight();
                        break;
                    case ColType.BULLET:
                        Bullet temp_bu = (Bullet)obj.GetParent();
                        loc_x = temp_bu.x;
                        loc_y = temp_bu.y;
                        width = temp_bu.width;
                        height = temp_bu.height;
                        break;
                }

                if (check_map_col(loc_x, loc_y, width, height))
                {
                    obj.Collision(new ColToken(ColType.MAP, this, unique_id++, null));
                }


                for (int j = 0; j < all_objects.Count(); ++j) // check to see if any object is involved in a collision
                {
                    ColToken other_obj = all_objects.ElementAt(j);
                    if (obj.getID() == other_obj.getID())
                    {
                        continue;
                    }
                    int other_loc_x = 0, other_loc_y = 0, other_width = 0, other_height = 0;
                    switch (other_obj.GetLocalType())
                    {
                        case ColType.PLAYER:
                            Player temp_pl = (Player)other_obj.GetParent();
                            other_loc_x = temp_pl.getX();
                            other_loc_y = temp_pl.getY();
                            other_width = temp_pl.getWidth();
                            other_height = temp_pl.getHeight();
                            break;
                        case ColType.MONSTER:
                            Enemy temp_em = (Enemy)other_obj.GetParent();
                            other_loc_x = temp_em.getX();
                            other_loc_y = temp_em.getY();
                            other_width = temp_em.getWidth();
                            other_height = temp_em.getHeight();
                            break;
                        case ColType.BULLET:
                            Bullet temp_bu = (Bullet)other_obj.GetParent();
                            other_loc_x = temp_bu.x;
                            other_loc_y = temp_bu.y;
                            other_width = temp_bu.width;
                            other_height = temp_bu.height;
                            break;
                    }


                    if(check_col(loc_x, loc_y, other_loc_x, other_loc_y, width, height, other_width, other_height)) {
                        obj.Collision(other_obj);
                    }

                }
                obj.updated_this_frame = false;
            }
            marked_objects.Clear(); // handled all of the modified objects
        }

        bool check_col(int x1, int y1, int x2, int y2, int width1, int height1, int width2, int height2) 
        {
            if (y1 + height1 < y2)
            {
                return false;
            }

            if (y1 > y2 + height2)
            {
                return false;
            }

            if (x1 + width1 < x2)
            {
                return false;
            }

            if (x1 > x2 + width2)
            {
                return false;
            }
            return true;
        }

        public bool check_map_col(int pot_x, int pot_y, int width, int height)
        {
            TileLayer back_layer = game_state.tile_engine.getCurrentMap().getLayer(LayerType.BACKGROUND);
            //Convert from world coords to tile coords

            //Bottom Left
            int tile_x =  (pot_x+5) / game_state.tile_engine.getTileSize();
            int tile_y = (pot_y + height) / game_state.tile_engine.getTileSize();

            Tile t = back_layer.getTile(tile_x, tile_y);
            if (t.hasCollision() == true)
            {
                return true;
            }

            //Bottom Right
            tile_x = (pot_x + width-10) / game_state.tile_engine.getTileSize();
            tile_y = (pot_y + height) / game_state.tile_engine.getTileSize();

            t = back_layer.getTile(tile_x, tile_y);
            if (t.hasCollision() == true)
            {
                return true;
            }
            
            return false;
        }
    }
}
