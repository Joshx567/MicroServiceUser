using ServiceUser.Application.Common;
using System.Text.RegularExpressions;

namespace ServiceUser.Domain.Rules
{
    public static class PasswordRules
    {
        public static Result<string> Validar(string? password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return Result<string>.Failure("La contraseña es obligatoria.");

            if (password.Length < 8)
                return Result<string>.Failure("La contraseña debe tener al menos 8 caracteres.");

            if (!Regex.IsMatch(password, @"[A-Z]"))
                return Result<string>.Failure("La contraseña debe contener al menos una letra mayúscula.");

            if (!Regex.IsMatch(password, @"[a-z]"))
                return Result<string>.Failure("La contraseña debe contener al menos una letra minúscula.");

            if (!Regex.IsMatch(password, @"[0-9]"))
                return Result<string>.Failure("La contraseña debe contener al menos un número.");

            return Result<string>.Success(password);
        }
    }
}
