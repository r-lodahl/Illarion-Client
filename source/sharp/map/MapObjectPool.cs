using System.Collections.Concurrent;

namespace Illarion.Client.Map
{
    public class MapObjectPool 
    {
        private readonly ConcurrentBag<MapObject> items = new ConcurrentBag<MapObject>();
        private int counter;
        private const int max = 2500;
        
        public void Release(MapObject item)
        {
            if (counter > max) return;

            items.Add(item);
            counter++;
        }

        public MapObject Get() 
        {
            MapObject item;
            
            if (items.TryTake(out item))
            {
                counter--;
                return item;
            } 

            item = new MapObject();
            counter++;
            return item;
        }

    }
}