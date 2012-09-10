using Microsoft.Xna.Framework;

namespace PLSE_Project
{
    //Insures objects used for collisions have appropirate methods
    interface Colideable
    {
         bool intersects(Rectangle rect);
         //{
         //   return rect.Intersects(rectangle);
         //}

        //Allows objects that implement colidable to be compared to one another easily
        bool intersects(Colideable obj);
        //{
        //    return rect.Intersects(obj.getRect());
        //}

        Rectangle getRect();
        //{
        //    return rect;
        //} 
    }
}
