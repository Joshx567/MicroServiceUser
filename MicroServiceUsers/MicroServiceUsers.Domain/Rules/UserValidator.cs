using ServiceUser.Application.Common;
using ServiceUser.Domain.Entities;
using System;
using System.Text.RegularExpressions;

namespace ServiceUser.Domain.Rules
{
    public static class UserValidator
    {
        public static Result<User> Validar(User user)
        {
            if (user == null)
                return Result<User>.Failure("El usuario no puede ser nulo.");

            var nombreResult = UserValidationRules.ValidarNombreCompleto(user.Name);
            if (nombreResult.IsFailure) return Result<User>.Failure(nombreResult.Error);

            var primerApellidoResult = UserValidationRules.ValidarNombreCompleto(user.FirstLastname);
            if (primerApellidoResult.IsFailure) return Result<User>.Failure("Primer apellido: " + primerApellidoResult.Error);

            if (!string.IsNullOrWhiteSpace(user.SecondLastname))
            {
                var segundoApellidoResult = UserValidationRules.ValidarNombreCompleto(user.SecondLastname);
                if (segundoApellidoResult.IsFailure) return Result<User>.Failure("Segundo apellido: " + segundoApellidoResult.Error);
            }

            var ciResult = UserValidationRules.ValidarCi(user.Ci);
            if (ciResult.IsFailure) return Result<User>.Failure(ciResult.Error);

            var fechaNacResult = UserValidationRules.ValidarFechaNacimiento(user.DateBirth);
            if (fechaNacResult.IsFailure) return Result<User>.Failure(fechaNacResult.Error);

            var rolResult = UserValidationRules.ValidarRol(user.Role);
            if (rolResult.IsFailure) return Result<User>.Failure(rolResult.Error);

            if (user.HireDate.HasValue)
            {
                var fechaContratacionResult = UserValidationRules.ValidarFechaContratacion(user.HireDate, user.DateBirth);
                if (fechaContratacionResult.IsFailure) return Result<User>.Failure(fechaContratacionResult.Error);
            }

            if (!string.IsNullOrWhiteSpace(user.Role) &&
                (user.Role.Equals("Instructor", StringComparison.OrdinalIgnoreCase) ||
                 user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase)))
            {
                var salarioResult = UserValidationRules.ValidarSalario(user.MonthlySalary);
                if (salarioResult.IsFailure) return Result<User>.Failure(salarioResult.Error);
            }

            if (user.Role?.Equals("Instructor", StringComparison.OrdinalIgnoreCase) == true)
            {
                var espResult = UserValidationRules.ValidarEspecializacion(user.Specialization);
                if (espResult.IsFailure) return Result<User>.Failure(espResult.Error);
            }

            var emailResult = UserValidationRules.ValidarEmail(user.Email);
            if (emailResult.IsFailure) return Result<User>.Failure(emailResult.Error);

            return Result<User>.Success(user);
        }
    }
}