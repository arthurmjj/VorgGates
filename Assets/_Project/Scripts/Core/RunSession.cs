namespace VorgGates.Core
{
    /// <summary>
    /// Session-only campaign state. A checkpoint is written only by CommitCheckpoint,
    /// after the next dungeon has been built successfully.
    /// </summary>
    public sealed class RunSession
    {
        public const int DungeonCount = 3;
        private const int NoTransition = -1;

        private readonly InventoryState inventory;
        private InventorySnapshot checkpoint;

        public RunSession(InventoryState inventoryState)
        {
            inventory = inventoryState;
            checkpoint = inventory.Capture();
        }

        public int DungeonIndex { get; private set; }
        public int Attempt { get; private set; }
        public int Deaths { get; private set; }
        public int PendingDungeon { get; private set; } = NoTransition;
        public bool IsTransitioning => PendingDungeon != NoTransition;
        public bool IsFinalDungeon => DungeonIndex == DungeonCount - 1;

        public bool BeginTransition()
        {
            if (IsTransitioning || IsFinalDungeon) return false;
            PendingDungeon = DungeonIndex + 1;
            return true;
        }

        public bool CommitCheckpoint()
        {
            if (!IsTransitioning) return false;
            DungeonIndex = PendingDungeon;
            PendingDungeon = NoTransition;
            Attempt = 0;
            checkpoint = inventory.Capture();
            return true;
        }

        public void AbortTransition() => PendingDungeon = NoTransition;

        /// <summary>Restores the inventory held when the current dungeon was entered.</summary>
        public bool RegisterDeath()
        {
            if (IsTransitioning) return false;
            Deaths++;
            Attempt++;
            inventory.Restore(checkpoint);
            return true;
        }

        /// <summary>Returns to the current checkpoint without counting a death.</summary>
        public void RestoreCheckpoint()
        {
            PendingDungeon = NoTransition;
            inventory.Restore(checkpoint);
        }

        public void RestartCampaign()
        {
            inventory.Reset();
            checkpoint = inventory.Capture();
            DungeonIndex = 0;
            Attempt = 0;
            Deaths = 0;
            PendingDungeon = NoTransition;
        }

        /// <summary>Places the session at a dungeon with the current inventory, for tests and capture sessions.</summary>
        public void JumpTo(int dungeonIndex)
        {
            DungeonIndex = dungeonIndex;
            PendingDungeon = NoTransition;
            Attempt = 0;
            checkpoint = inventory.Capture();
        }
    }
}
