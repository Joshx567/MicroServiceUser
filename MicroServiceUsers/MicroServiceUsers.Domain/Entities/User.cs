using System;

namespace ServiceUser.Domain.Entities
{
    public class User
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public string first_lastname { get; set; } = string.Empty;
        public string? second_lastname { get; set; }
        public DateTime date_birth { get; set; }
        public string ci { get; set; } = string.Empty;

        public string role { get; set; } = string.Empty;
        public DateTime? hire_date { get; set; }
        public decimal? monthly_salary { get; set; }
        public string? specialization { get; set; }

        public string email { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public bool must_change_password { get; set; }
        public bool is_active { get; set; }

        public DateTime created_at { get; set; }
        public string created_by { get; set; } = string.Empty;
        public DateTime? last_modification { get; set; }
        public string? last_modified_by { get; set; }

        public string? jwt_token { get; set; }           // Guardar el token
        public DateTime? token_expires_at { get; set; } // Fecha de expiración del token
    }
}
