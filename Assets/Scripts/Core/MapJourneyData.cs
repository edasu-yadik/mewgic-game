using System;

namespace Mewgic.Core
{
    [Serializable]
    public sealed class MapJourneyData
    {
        public bool initialized;
        public int displayedLevel = 1;
        public int pendingFrom, pendingTo, lastShownLevel;
        public bool Pending => pendingFrom > 0 && pendingTo > pendingFrom && pendingTo > lastShownLevel;
        public MapJourneyData Copy() => (MapJourneyData)MemberwiseClone();
    }

    public static class MapJourneyRules
    {
        public static void Initialize(ProgressData data)
        {
            var journey = data.worldMap.journey ?? (data.worldMap.journey = new MapJourneyData());
            int current = WorldMapRules.CurrentLevel(data);
            // Old saves start at their existing frontier, without replaying historical victories.
            if (!journey.initialized)
            {
                journey.initialized = true;
                journey.displayedLevel = current;
                journey.lastShownLevel = current;
                journey.pendingFrom = journey.pendingTo = 0;
            }
            if (journey.displayedLevel < 1 || journey.displayedLevel > current ||
                journey.lastShownLevel < 0 || journey.lastShownLevel > current ||
                journey.pendingFrom < 0 || journey.pendingTo < 0 || journey.pendingTo > current ||
                ((journey.pendingFrom != 0 || journey.pendingTo != 0) && !journey.Pending))
                throw new ArgumentException("Geçersiz harita yolculuğu kaydı");
        }

        public static void Queue(ProgressData data, int completed)
        {
            var journey = data.worldMap.journey;
            if (!journey.Pending) journey.pendingFrom = completed;
            // Consecutive Next victories become one short journey to the latest frontier.
            journey.pendingTo = WorldMapRules.CurrentLevel(data);
        }
    }

    public sealed partial class ProgressLedger
    {
        public MapJourneyData MapJourney => state.worldMap.journey.Copy();
        public int CurrentUnlockedLevel => WorldMapRules.CurrentLevel(state);
        public int CurrentCompletedLevel => CurrentUnlockedLevel - 1;
        public bool PeekMapJourney(out int from, out int to)
        {
            var journey = state.worldMap.journey;
            from = journey.pendingFrom;
            to = journey.pendingTo;
            return journey.Pending;
        }
        public bool AcknowledgeMapJourney(int target)
        {
            var journey = state.worldMap.journey;
            if (!journey.Pending) return journey.lastShownLevel >= target;
            if (target != journey.pendingTo) return false;
            var copy = state.Copy();
            copy.worldMap.journey.displayedLevel = target;
            copy.worldMap.journey.lastShownLevel = target;
            copy.worldMap.journey.pendingFrom = copy.worldMap.journey.pendingTo = 0;
            return Commit(copy);
        }
        // Explicit QA/legacy caller only; entering or restarting a level must never discard pending work.
        public void ClearMapJourney() { if (MapJourney.Pending) AcknowledgeMapJourney(MapJourney.pendingTo); }
    }
}
