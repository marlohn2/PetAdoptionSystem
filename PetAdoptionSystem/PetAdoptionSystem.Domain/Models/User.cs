﻿namespace PetAdoptionSystem.Domain.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } //HASH IT
        public string Role { get; set; }
    }
}