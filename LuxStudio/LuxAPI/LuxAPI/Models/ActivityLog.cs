using System;

namespace LuxAPI.Models
{
    public class ActivityLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Ajoute cette propriété pour stocker l'ID de l'utilisateur (nullable)
        public Guid? UserId { get; set; }

        public string Action { get; set; } = string.Empty;

        // Nom ou email de la personne ayant effectué l'action
        public string PerformedBy { get; set; } = string.Empty;

        public string Details { get; set; } = string.Empty;

        // Ajoute la propriété Timestamp pour la date/heure de l'action
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
    }
}
