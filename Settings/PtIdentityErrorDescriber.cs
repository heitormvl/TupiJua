using Microsoft.AspNetCore.Identity;

namespace TupiJua.Settings
{
    public class PtIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DuplicateUserName(string userName)
        {
            return new IdentityError
            {
                Code = nameof(DuplicateUserName),
                Description = $"O nome de usuário '{userName}' já está sendo utilizado."
            };
        }

        public override IdentityError DuplicateEmail(string email)
        {
            return new IdentityError
            {
                Code = nameof(DuplicateEmail),
                Description = $"O e-mail '{email}' já está cadastrado."
            };
        }

        public override IdentityError PasswordTooShort(int length)
        {
            return new IdentityError
            {
                Code = nameof(PasswordTooShort),
                Description = $"A senha deve ter no mínimo {length} caracteres."
            };
        }

        // Você pode sobrescrever outros métodos conforme a necessidade (InvalidToken, PasswordRequiresDigit, etc)
    }
}