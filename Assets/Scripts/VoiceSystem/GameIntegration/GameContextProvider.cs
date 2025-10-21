using UnityEngine;
using VoiceSystem.Core.Interfaces;
using VoiceSystem.Core.Data;

namespace VoiceSystem.GameIntegration
{
    /// <summary>
    /// Provides game context to the AI system
    /// </summary>
    public class GameContextProvider : MonoBehaviour, IGameContextProvider
    {
        [Header("Demo Configuration")]
        public bool useDemoData = true;
        
        [Header("Game References")]
        public EventManager eventManager;
        
        // Current context
        private GameContext currentContext;
        
        // Demo data for testing
        private string[] demoLocations = {
            "Sala Principal",
            "Biblioteca", 
            "Comedor",
            "Cocina",
            "Habitación Principal",
            "Habitación de los Niños",
            "Baño",
            "Sótano"
        };
        
        private int currentLocationIndex = 0;
        
        private void Awake()
        {
            // Initialize context
            currentContext = new GameContext();
            
            // Set up demo data
            if (useDemoData)
            {
                SetupDemoContext();
            }
        }
        
        private void SetupDemoContext()
        {
            currentContext.currentLocation = demoLocations[currentLocationIndex];
            currentContext.health = 5;
            currentContext.maxHealth = 5;
            currentContext.fatigue = 0;
            currentContext.actions = 10;
            currentContext.maxActions = 10;
            currentContext.gameTime = "2:00 AM";
            currentContext.timeRemaining = 12f;
            
            // Demo inventory
            currentContext.inventory.Clear();
            currentContext.inventory.Add("comida");
            currentContext.inventory.Add("agua");
            
            // Demo nearby objects based on location
            UpdateNearbyObjects();
            
            // Demo recent events
            currentContext.recentEvents.Clear();
            currentContext.AddEvent("Llegaste a la casa");
            currentContext.AddEvent("El reloj marca las 2 AM");
            
            Debug.Log($"[GameContext] Demo context set up for location: {currentContext.currentLocation}");
        }
        
        private void UpdateNearbyObjects()
        {
            currentContext.nearbyObjects.Clear();
            
            switch (currentContext.currentLocation)
            {
                case "Sala Principal":
                    currentContext.nearbyObjects.AddRange(new string[] { "cuadro familiar", "piano", "reloj" });
                    break;
                case "Biblioteca":
                    currentContext.nearbyObjects.AddRange(new string[] { "libros", "estantes", "mesa" });
                    break;
                case "Comedor":
                    currentContext.nearbyObjects.AddRange(new string[] { "mesa", "sillas", "vajilla" });
                    break;
                case "Cocina":
                    currentContext.nearbyObjects.AddRange(new string[] { "estufa", "refrigerador", "utensilios" });
                    break;
                case "Habitación Principal":
                    currentContext.nearbyObjects.AddRange(new string[] { "cama", "espejo", "flor marchita" });
                    break;
                case "Habitación de los Niños":
                    currentContext.nearbyObjects.AddRange(new string[] { "camas", "juguetes", "oso rojo" });
                    break;
                case "Baño":
                    currentContext.nearbyObjects.AddRange(new string[] { "bañera", "espejo", "lavabo" });
                    break;
                case "Sótano":
                    currentContext.nearbyObjects.AddRange(new string[] { "puerta", "cerrojo" });
                    break;
            }
        }
        
        public GameContext GetCurrentContext()
        {
            return currentContext;
        }
        
        public void UpdateContext(GameContext context)
        {
            currentContext = context;
            Debug.Log($"[GameContext] Context updated: {context.currentLocation}");
        }
        
        public bool IsCommandValid(string commandId)
        {
            if (currentContext == null)
                return false;
                
            return currentContext.availableCommands.Contains(commandId) && 
                   currentContext.CanPerformAction();
        }
        
        public void ExecuteCommand(string commandId)
        {
            if (!IsCommandValid(commandId))
            {
                Debug.LogWarning($"[GameContext] Invalid command: {commandId}");
                return;
            }
            
            Debug.Log($"[GameContext] Executing command: {commandId}");
            
            // Consume actions
            currentContext.ConsumeActions();
            
            // Add event
            currentContext.AddEvent($"Ejecutaste: {commandId}");
            
            // Handle specific commands
            switch (commandId)
            {
                case "adelante":
                    MoveToNextLocation();
                    break;
                case "atrás":
                    MoveToPreviousLocation();
                    break;
                case "comer":
                    EatFood();
                    break;
                case "inspeccionar":
                    InspectLocation();
                    break;
                case "tomar":
                    TakeObject();
                    break;
                case "usar":
                    UseObject();
                    break;
                case "leer":
                    ReadObject();
                    break;
                case "dar":
                    GiveObject();
                    break;
                case "renacer":
                    AttemptRebirth();
                    break;
                case "despertar":
                    AttemptAwakening();
                    break;
            }
            
            // Update time
            UpdateGameTime();
        }
        
        private void MoveToNextLocation()
        {
            currentLocationIndex = (currentLocationIndex + 1) % demoLocations.Length;
            currentContext.currentLocation = demoLocations[currentLocationIndex];
            UpdateNearbyObjects();
            currentContext.AddEvent($"Te moviste a {currentContext.currentLocation}");
        }
        
        private void MoveToPreviousLocation()
        {
            currentLocationIndex = (currentLocationIndex - 1 + demoLocations.Length) % demoLocations.Length;
            currentContext.currentLocation = demoLocations[currentLocationIndex];
            UpdateNearbyObjects();
            currentContext.AddEvent($"Te moviste a {currentContext.currentLocation}");
        }
        
        private void EatFood()
        {
            if (currentContext.inventory.Contains("comida"))
            {
                currentContext.inventory.Remove("comida");
                currentContext.RestoreHealth(1);
                currentContext.AddEvent("Comiste comida y recuperaste 1 de vida");
            }
            else
            {
                currentContext.AddEvent("No tienes comida para comer");
            }
        }
        
        private void InspectLocation()
        {
            string inspection = $"Inspeccionas {currentContext.currentLocation}. ";
            
            if (currentContext.nearbyObjects.Count > 0)
            {
                inspection += $"Ves: {string.Join(", ", currentContext.nearbyObjects)}.";
            }
            else
            {
                inspection += "No hay nada notable aquí.";
            }
            
            currentContext.AddEvent(inspection);
        }
        
        private void TakeObject()
        {
            if (currentContext.nearbyObjects.Count > 0)
            {
                string objectToTake = currentContext.nearbyObjects[0];
                currentContext.nearbyObjects.RemoveAt(0);
                currentContext.inventory.Add(objectToTake);
                currentContext.AddEvent($"Tomaste: {objectToTake}");
            }
            else
            {
                currentContext.AddEvent("No hay nada que tomar aquí");
            }
        }
        
        private void UseObject()
        {
            if (currentContext.inventory.Count > 0)
            {
                string objectToUse = currentContext.inventory[0];
                currentContext.AddEvent($"Usaste: {objectToUse}");
            }
            else
            {
                currentContext.AddEvent("No tienes nada que usar");
            }
        }
        
        private void ReadObject()
        {
            if (currentContext.nearbyObjects.Contains("libros"))
            {
                currentContext.AddEvent("Lees un libro. Contiene información sobre la historia de la casa.");
            }
            else
            {
                currentContext.AddEvent("No hay nada que leer aquí");
            }
        }
        
        private void GiveObject()
        {
            if (currentContext.inventory.Count > 0)
            {
                string objectToGive = currentContext.inventory[0];
                currentContext.inventory.RemoveAt(0);
                currentContext.AddEvent($"Diste: {objectToGive}");
            }
            else
            {
                currentContext.AddEvent("No tienes nada que dar");
            }
        }
        
        private void AttemptRebirth()
        {
            if (currentContext.inventory.Contains("flor de loto"))
            {
                currentContext.AddEvent("¡Usaste la flor de loto y lograste renacer! ¡Has ganado!");
                // Game win condition
            }
            else
            {
                currentContext.AddEvent("No tienes la flor de loto necesaria para renacer");
            }
        }
        
        private void AttemptAwakening()
        {
            currentContext.AddEvent("Intentas despertar, pero algo te mantiene atrapado en este lugar");
        }
        
        private void UpdateGameTime()
        {
            // Simulate time passing
            currentContext.timeRemaining -= 0.1f;
            
            if (currentContext.timeRemaining <= 0)
            {
                currentContext.timeRemaining = 0;
                currentContext.AddEvent("¡Se acabó el tiempo! La casa te ha atrapado para siempre.");
                // Game over condition
            }
        }
        
        // Demo methods for testing
        [ContextMenu("Move to Next Location")]
        public void DemoMoveNext()
        {
            MoveToNextLocation();
        }
        
        [ContextMenu("Move to Previous Location")]
        public void DemoMovePrevious()
        {
            MoveToPreviousLocation();
        }
        
        [ContextMenu("Add Food")]
        public void DemoAddFood()
        {
            currentContext.inventory.Add("comida");
            Debug.Log("[GameContext] Added food to inventory");
        }
        
        [ContextMenu("Take Damage")]
        public void DemoTakeDamage()
        {
            currentContext.health = Mathf.Max(0, currentContext.health - 1);
            Debug.Log($"[GameContext] Health reduced to {currentContext.health}");
        }
    }
}