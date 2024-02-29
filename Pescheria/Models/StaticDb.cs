namespace Pescheria.Models
{
    public static class StaticDb
    {
        private static int _maxId = 3;
        private static List<Fish> _fishes = [
            new Fish(){FishId = 1, Name = "Trota", IsSeaFish = false, Price = 1500},
            new Fish(){FishId = 2, Name = "Salmone", IsSeaFish = false, Price = 3000},
            new Fish(){FishId = 3, Name = "Sgombro", IsSeaFish = true, Price = 800},
        ];

        public static List<Fish> GetAll()
        {
            List<Fish> notDeletedFishes = [];
            foreach(var fish in _fishes)
            {
                if (fish.DeletedAt is null)
                {
                    notDeletedFishes.Add(fish);
                }
            }
            return notDeletedFishes;

            //return _fishes;
        }

        public static List<Fish> GetAllDeleted()
        {
            List<Fish> deletedFishes = [];
            foreach (var fish in _fishes)
            {
                if (fish.DeletedAt is not null)
                {
                    deletedFishes.Add(fish);
                }
            }
            return deletedFishes;

            //return _fishes;
        }

        public static Fish? Recover(int idToRecover)
        {
            int? index = findFishIndex(idToRecover);

            if (index is not null)
            {
                var fishRecovered = _fishes[(int)index];
                fishRecovered.DeletedAt = null;
                return fishRecovered;
            }

            return null;
        }

        public static Fish? GetById(int? id)
        {
            if (id is null) return null;

            for(int i = 0; i < _fishes.Count; i++)
            {
                Fish fish = _fishes[i];
                if (fish.FishId == id)
                {
                    return fish;
                }
            }

            return null;
        }

        public static Fish Add(string name, bool isSeaFish, int price)
        {
            _maxId++;
            var fish = new Fish() { FishId = _maxId, Name = name, IsSeaFish = isSeaFish, Price = price };
            _fishes.Add(fish);
            return fish;
        }

        public static Fish? Modify(Fish fish)
        {
            foreach(var fishInList in _fishes)
            {
                if (fishInList.FishId == fish.FishId)
                {
                    fishInList.Name = fish.Name;
                    fishInList.IsSeaFish = fish.IsSeaFish;
                    fishInList.Price = fish.Price;
                    return fishInList;
                }
            }
            return null;
        }

        public static Fish? SoftDelete(int idToDelete)
        {
            // ha bisogno di un ulteriore campo nella tabella DeletedAt (o null o data di eliminazione)
            int? deletedIndex = findFishIndex(idToDelete);

            if (deletedIndex is not null)
            {
                var fishDeleted = _fishes[(int)deletedIndex];
                fishDeleted.DeletedAt = DateTime.UtcNow;
                return fishDeleted;
            }

            return null;
        }

        public static Fish? HardDelete(int idToDelete)
        {
            // elimina per sempre il dato

            int? deletedIndex = findFishIndex(idToDelete);
            

            if (deletedIndex is not null)
            {
                var fishDeleted = _fishes[(int)deletedIndex];
                _fishes.RemoveAt((int)deletedIndex);
                return fishDeleted;
            }

            return null;
        }

        private static int? findFishIndex(int idToDelete)
        {
            int i;
            bool fishFound = false;
            for (i = 0; i < _fishes.Count; i++)
            {
                if (_fishes[i].FishId == idToDelete)
                {
                    fishFound = true;
                    break;
                }
            }

            if (fishFound) return i;
            return null;
        }
    }
}
