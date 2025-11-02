using System;
using VoiceSystem.Core.Data;

namespace VoiceSystem.Core.Interfaces
{
    /// <summary>
    /// Interface for room system providers
    /// This allows the future room generation module to connect to the voice system
    /// </summary>
    public interface IRoomSystemProvider
    {
        /// <summary>
        /// Event fired when the player moves to a different room
        /// </summary>
        event Action<RoomData> OnRoomChanged;
        
        /// <summary>
        /// Event fired when a door's state changes (locked/unlocked)
        /// </summary>
        event Action<DoorData> OnDoorStateChanged;
        
        /// <summary>
        /// Get the current room the player is in
        /// </summary>
        RoomData GetCurrentRoom();
        
        /// <summary>
        /// Get a room by its ID
        /// </summary>
        /// <param name="roomId">Unique identifier of the room</param>
        /// <returns>Room data or null if not found</returns>
        RoomData GetRoomById(string roomId);
        
        /// <summary>
        /// Try to move through a door
        /// </summary>
        /// <param name="doorId">ID of the door to use</param>
        /// <param name="failureReason">Reason why movement failed (if it failed)</param>
        /// <returns>True if movement was successful, false otherwise</returns>
        bool TryMoveThroughDoor(string doorId, out string failureReason);
        
        /// <summary>
        /// Update the state of a specific room
        /// </summary>
        /// <param name="room">Room data to update</param>
        void UpdateRoomState(RoomData room);
        
        /// <summary>
        /// Unlock a door if player has the required key
        /// </summary>
        /// <param name="doorId">ID of the door to unlock</param>
        /// <param name="keyItemId">ID of the key item being used</param>
        /// <returns>True if door was unlocked, false otherwise</returns>
        bool TryUnlockDoor(string doorId, string keyItemId);
        
        /// <summary>
        /// Get all available doors in the current room
        /// </summary>
        DoorData[] GetCurrentRoomDoors();
        
        /// <summary>
        /// Check if a specific room has been visited
        /// </summary>
        /// <param name="roomId">ID of the room to check</param>
        /// <returns>True if room has been visited, false otherwise</returns>
        bool HasVisitedRoom(string roomId);
        
        /// <summary>
        /// Get the total number of rooms in the game
        /// </summary>
        int GetTotalRoomCount();
        
        /// <summary>
        /// Get the number of rooms the player has visited
        /// </summary>
        int GetVisitedRoomCount();
    }
}

