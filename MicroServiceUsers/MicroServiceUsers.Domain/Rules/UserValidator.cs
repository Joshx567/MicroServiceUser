using ServiceUser.Application.Common;
using ServiceUser.Domain.Entities;
using System;

namespace ServiceUser.Domain.Rules
{
    public static class UserValidator
    {
        public static Result<User> Validar(User user)
        {
            if (user == null)
                return Result<User>.Failure("El usuario no puede ser nulo.");

            var nombreResult = UserValidationRules.ValidarNombreCompleto(user.name);
            if (nombreResult.IsFailure) return Result<User>.Failure(nombreResult.Error);

            var primerApellidoResult = UserValidationRules.ValidarNombreCompleto(user.first_lastname);
            if (primerApellidoResult.IsFailure) return Result<User>.Failure("Primer apellido: " + primerApellidoResult.Error);

            if (!string.IsNullOrWhiteSpace(user.second_lastname))
            {
                var segundoApellidoResult = UserValidationRules.ValidarNombreCompleto(user.second_lastname);
                if (segundoApellidoResult.IsFailure) return Result<User>.Failure("Segundo apellido: " + segundoApellidoResult.Error);
            }

            var ciResult = UserValidationRules.ValidarCi(user.ci);
            if (ciResult.IsFailure) return Result<User>.Failure(ciResult.Error);

            var fechaNacResult = UserValidationRules.ValidarFechaNacimiento(user.date_birth);
            if (fechaNacResult.IsFailure) return Result<User>.Failure(fechaNacResult.Error);

            var rolResult = UserValidationRules.ValidarRol(user.role);
            if (rolResult.IsFailure) return Result<User>.Failure(rolResult.Error);

            if (user.hire_date.HasValue)
            {
                var fechaContratacionResult = UserValidationRules.ValidarFechaContratacion(user.hire_date, user.date_birth);
                if (fechaContratacionResult.IsFailure) return Result<User>.Failure(fechaContratacionResult.Error);
            }

            if (!string.IsNullOrWhiteSpace(user.role) &&
                (user.role.Equals("Instructor", StringComparison.OrdinalIgnoreCase) ||
                 user.role.Equals("Admin", StringComparison.OrdinalIgnoreCase)))
            {
                var salarioResult = UserValidationRules.ValidarSalario(user.monthly_salary);
                if (salarioResult.IsFailure) return Result<User>.Failure(salarioResult.Error);
            }

            if (user.role?.Equals("Instructor", StringComparison.OrdinalIgnoreCase) == true)
            {
                var espResult = UserValidationRules.ValidarEspecializacion(user.specialization);
                if (espResult.IsFailure) return Result<User>.Failure(espResult.Error);
            }

            var emailResult = UserValidationRules.ValidarEmail(user.email);
            if (emailResult.IsFailure) return Result<User>.Failure(emailResult.Error);

            return Result<User>.Success(user);
        }
    }
}
