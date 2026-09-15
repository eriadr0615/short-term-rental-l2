using System.Security.Cryptography;

namespace Inmobiliaria.Models
{
    public static class ServicioHash
    {
        private const int Iteraciones = 100000;
        private const int TamanioSal = 16;
        private const int TamanioHash = 32;

        public static string Crear(string clave)
        {
            byte[] sal = RandomNumberGenerator.GetBytes(TamanioSal);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                clave,
                sal,
                Iteraciones,
                HashAlgorithmName.SHA256,
                TamanioHash);

            return $"PBKDF2${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
        }

        public static bool Verificar(string clave, string valorGuardado)
        {
            try
            {
                string[] partes = valorGuardado.Split('$');
                if (partes.Length != 4 || partes[0] != "PBKDF2")
                {
                    return false;
                }

                int iteraciones = Convert.ToInt32(partes[1]);
                byte[] sal = Convert.FromBase64String(partes[2]);
                byte[] hashGuardado = Convert.FromBase64String(partes[3]);
                byte[] hashIngresado = Rfc2898DeriveBytes.Pbkdf2(
                    clave,
                    sal,
                    iteraciones,
                    HashAlgorithmName.SHA256,
                    hashGuardado.Length);

                return CryptographicOperations.FixedTimeEquals(hashIngresado, hashGuardado);
            }
            catch (FormatException)
            {
                return false;
            }
            catch (OverflowException)
            {
                return false;
            }
        }
    }
}
