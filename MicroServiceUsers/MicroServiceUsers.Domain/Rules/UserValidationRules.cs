using ServiceUser.Application.Common;
using ServiceUser.Domain.Entities;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ServiceUser.Domain.Rules
{
    internal static class UserValidationRules
    {
        // Nombre completo obligatorio, mínimo 2 letras, solo letras y espacios
        public static Result<string> ValidarNombreCompleto(string? nombreCompleto)
        {
            if (string.IsNullOrWhiteSpace(nombreCompleto))
                return Result<string>.Failure("El nombre completo es obligatorio.");

            if (nombreCompleto.Length < 2)
                return Result<string>.Failure("El nombre completo debe tener al menos 2 caracteres.");

            if (!Regex.IsMatch(nombreCompleto, @"^[A-Za-zÁÉÍÓÚáéíóúÑñ ]+$"))
                return Result<string>.Failure("El nombre solo puede contener letras y espacios.");

            return Result<string>.Success(nombreCompleto);
        }

        // CI solo números o letras, obligatorio y longitud entre 6 y 15
        public static Result<string> ValidarCi(string? ci)
        {
            if (string.IsNullOrWhiteSpace(ci))
                return Result<string>.Failure("El CI es obligatorio.");

            if (!Regex.IsMatch(ci, @"^[0-9A-Za-z]{6,15}$"))
                return Result<string>.Failure("El CI debe contener solo letras y números, entre 6 y 15 caracteres.");

            return Result<string>.Success(ci);
        }

        // Fecha de nacimiento no futura y ≥ 18 años
        public static Result<DateTime?> ValidarFechaNacimiento(DateTime? fecha)
        {
            if (!fecha.HasValue)
                return Result<DateTime?>.Failure("La fecha de nacimiento es obligatoria.");

            if (fecha > DateTime.Today)
                return Result<DateTime?>.Failure("La fecha de nacimiento no puede ser futura.");

            var edad = DateTime.Today.Year - fecha.Value.Year;
            if (fecha.Value.AddYears(edad) > DateTime.Today) edad--;

            if (edad < 18)
                return Result<DateTime?>.Failure("El usuario debe tener al menos 18 años.");

            return Result<DateTime?>.Success(fecha);
        }

        // Fecha de contratación no futura y ≥ fecha nacimiento + 18 años
        public static Result<DateTime?> ValidarFechaContratacion(DateTime? hireDate, DateTime? dateBirth)
        {
            if (!hireDate.HasValue || !dateBirth.HasValue)
                return Result<DateTime?>.Failure("Las fechas de contratación y nacimiento son obligatorias.");

            if (hireDate > DateTime.Today)
                return Result<DateTime?>.Failure("La fecha de contratación no puede ser futura.");

            if (hireDate <= dateBirth)
                return Result<DateTime?>.Failure("La fecha de contratación debe ser posterior a la fecha de nacimiento.");

            int edad = hireDate.Value.Year - dateBirth.Value.Year;
            if (hireDate.Value < dateBirth.Value.AddYears(edad)) edad--;

            if (edad < 18)
                return Result<DateTime?>.Failure("El empleado debe tener al menos 18 años al ser contratado.");

            return Result<DateTime?>.Success(hireDate);
        }

        // Rol válido
        public static Result<string> ValidarRol(string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return Result<string>.Failure("El rol es obligatorio.");

            string[] rolesValidos = { "Instructor", "Admin" };
            if (!rolesValidos.Contains(role, StringComparer.OrdinalIgnoreCase))
                return Result<string>.Failure("El rol debe ser Instructor o Admin.");

            return Result<string>.Success(role);
        }

        // Especialización mínima 3 caracteres
        public static Result<string> ValidarEspecializacion(string? especializacion)
        {
            if (string.IsNullOrWhiteSpace(especializacion))
                return Result<string>.Failure("La especialización es obligatoria.");

            if (especializacion.Length < 3)
                return Result<string>.Failure("La especialización debe tener al menos 3 caracteres.");

            return Result<string>.Success(especializacion);
        }

        // Salario obligatorio ≥ 0
        public static Result<decimal> ValidarSalario(decimal? salario)
        {
            if (!salario.HasValue)
                return Result<decimal>.Failure("El salario es obligatorio.");

            if (salario.Value < 0)
                return Result<decimal>.Failure("El salario no puede ser negativo.");

            return Result<decimal>.Success(salario.Value);
        }

        // Email obligatorio y formato válido
        public static Result<string> ValidarEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result<string>.Failure("El correo electrónico es obligatorio.");

            string patronEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(email, patronEmail))
                return Result<string>.Failure("El formato del correo electrónico no es válido.");

            return Result<string>.Success(email);
        }
    }
}