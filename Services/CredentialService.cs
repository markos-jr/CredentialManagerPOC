using CredentialManagement;

namespace CredentialManagerPOC.Services
{
    public interface ICredentialService
    {
        string GetConnectionString(string targetName);
    }

    public class CredentialService : ICredentialService
    {
        public string GetConnectionString(string targetName)
        {
            try
            {
                using (var cred = new Credential())
                {
                    cred.Target = targetName;
                    if (cred.Load())
                    {
                        return cred.Password; // Retorna a senha armazenada que representa a connection string
                    }
                    else
                    {
                        return null; // Credencial não encontrada
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao acessar credencial: {ex.Message}");
                throw; // Relança a exceção para tratamento
            }
        }
    }
}
