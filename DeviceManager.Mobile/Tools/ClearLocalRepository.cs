using DeviceManager.Mobile.Repositories;

public class ClearLocalRepository
{
    public static async Task ClearAsync()
    {
        try
        {
            Console.WriteLine("Iniciando limpeza do repositório local...");
            
            using var deviceRepository = new DeviceRepository();
            await deviceRepository.ClearAllDataAsync();
            
            Console.WriteLine("Repositório local limpo com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
        }
    }
} 