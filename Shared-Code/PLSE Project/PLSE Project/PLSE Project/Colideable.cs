using Microsoft.Xna.Framework;

namespace PLSE_Project
{
    //Insures objects used for collisions have appropirate methods
    interface Colideable
    {
        bool intersects(Rectangle rect);
        bool intersects(Colideable obj);
        Rectangle getRect();
    }
}
