using System.Collections.Generic;
using UnityEngine;

namespace VoiceSystem.Core.Data
{
    /// <summary>
    /// Represents the current state of the game for AI context
    /// </summary>
    [System.Serializable]
    public class GameContext
    {
        [Header("Location")]
        public string currentLocation = "Sala Principal";
        public List<string> nearbyObjects = new List<string>();
        public List<string> availableExits = new List<string>();
        
        [Header("Player State")]
        public int health = 5;
        public int maxHealth = 5;
        public int fatigue = 0;
        public int maxFatigue = 25;
        public int actions = 10;
        public int maxActions = 10;
        
        [Header("Inventory")]
        public List<string> inventory = new List<string>();
        
        [Header("Time")]
        public string gameTime = "2:00 AM";
        public float timeRemaining = 12f; // hours
        
        [Header("Game State")]
        public List<string> availableCommands = new List<string>();
        public List<string> recentEvents = new List<string>();
        public bool isInCombat = false;
        public bool hasEncounteredChild = false;
        public bool hasLotusFlower = false;
        
        [Header("Room Specific")]
        public Dictionary<string, object> roomData = new Dictionary<string, object>();
        
        public GameContext()
        {
            // Initialize default commands
            availableCommands.AddRange(new string[] 
            {
                "adelante", "atrás", "izquierda", "derecha",
                "inspeccionar", "tomar", "usar", "comer", "leer", "dar",
                "renacer", "despertar", "ayuda", "inventario"
            });
        }
        
        /// <summary>
        /// Add a recent event (keeps only last 5)
        /// </summary>
        public void AddEvent(string eventDescription)
        {
            recentEvents.Insert(0, eventDescription);
            if (recentEvents.Count > 5)
            {
                recentEvents.RemoveAt(recentEvents.Count - 1);
            }
        }
        
        /// <summary>
        /// Check if player can perform an action
        /// </summary>
        public bool CanPerformAction(int actionCost = 1)
        {
            return actions >= actionCost;
        }
        
        /// <summary>
        /// Consume actions
        /// </summary>
        public void ConsumeActions(int amount = 1)
        {
            actions = Mathf.Max(0, actions - amount);
            fatigue += amount;
            
            // Check for fatigue penalty
            if (fatigue >= 5)
            {
                int livesLost = fatigue / 5;
                health = Mathf.Max(0, health - livesLost);
                fatigue = fatigue % 5;
            }
        }
        
        /// <summary>
        /// Restore health (eating)
        /// </summary>
        public void RestoreHealth(int amount = 1)
        {
            health = Mathf.Min(maxHealth, health + amount);
        }
        
        /// <summary>
        /// Get context summary for AI
        /// </summary>
        public string GetContextSummary()
        {
            return $"Ubicación: {currentLocation}\n" +
                   $"Vida: {health}/{maxHealth}\n" +
                   $"Fatiga: {fatigue}\n" +
                   $"Acciones: {actions}\n" +
                   $"Tiempo: {gameTime}\n" +
                   $"Inventario: {(inventory.Count > 0 ? string.Join(", ", inventory) : "vacío")}\n" +
                   $"Objetos cercanos: {(nearbyObjects.Count > 0 ? string.Join(", ", nearbyObjects) : "ninguno")}";
        }
    }
}