using DeviceManager.API.Models;
using DeviceManager.API.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace DeviceManager.API.Services
{
    public class DeviceService
    {
        private readonly IMongoCollection<DispositivoMongo> _dispositivos;
        private readonly ILogger<DeviceService> _logger;

        public DeviceService(IOptions<MongoDbSettings> settings, ILogger<DeviceService> logger)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _dispositivos = database.GetCollection<DispositivoMongo>(settings.Value.DispositivosCollectionName);
            _logger = logger;
        }

        public async Task<List<Dispositivo>> GetAsync()
        {
            var dispositivos = await _dispositivos.Find(_ => true).ToListAsync();
            return dispositivos.Select(d => new Dispositivo
            {
                Id = d.Id.ToString(),
                Descricao = d.Descricao,
                CodigoReferencia = d.CodigoReferencia,
                DataCriacao = d.DataCriacao,
                DataAtualizacao = d.DataAtualizacao
            }).ToList();
        }

        public async Task<Dispositivo> GetByIdAsync(string id)
        {
            try
            {
                // Se o ID for um GUID, buscar por string
                if (Guid.TryParse(id, out _))
                {
                    var dispositivo = await _dispositivos.Find(d => d.Id.ToString() == id).FirstOrDefaultAsync();
                    if (dispositivo == null) return null;
                    return MapToDispositivo(dispositivo);
                }
                else
                {
                    // Se não for GUID, tentar converter para ObjectId
                    var objectId = ObjectId.Parse(id);
                    var dispositivo = await _dispositivos.Find(d => d.Id == objectId).FirstOrDefaultAsync();
                    if (dispositivo == null) return null;
                    return MapToDispositivo(dispositivo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting device with ID {Id}", id);
                throw;
            }
        }

        public async Task<Dispositivo> GetByCodigoReferenciaAsync(string codigoReferencia)
        {
            try
            {
                var dispositivo = await _dispositivos.Find(d => d.CodigoReferencia == codigoReferencia).FirstOrDefaultAsync();
                if (dispositivo == null) return null;
                return MapToDispositivo(dispositivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting device with reference code {CodigoReferencia}", codigoReferencia);
                throw;
            }
        }

        private Dispositivo MapToDispositivo(DispositivoMongo dispositivo)
        {
            return new Dispositivo
            {
                Id = dispositivo.Id.ToString(),
                Descricao = dispositivo.Descricao,
                CodigoReferencia = dispositivo.CodigoReferencia,
                DataCriacao = dispositivo.DataCriacao,
                DataAtualizacao = dispositivo.DataAtualizacao
            };
        }

        public async Task CreateAsync(Dispositivo dispositivo)
        {
            try
            {
                // Verificar se já existe um dispositivo com o mesmo ID
                var existingDevice = await _dispositivos.Find(d => d.Id.ToString() == dispositivo.Id).FirstOrDefaultAsync();
                if (existingDevice != null)
                {
                    throw new Exception($"Device with ID {dispositivo.Id} already exists");
                }

                var dispositivoMongo = new DispositivoMongo
                {
                    Id = ObjectId.Parse(dispositivo.Id),
                    Descricao = dispositivo.Descricao,
                    CodigoReferencia = dispositivo.CodigoReferencia,
                    DataCriacao = dispositivo.DataCriacao,
                    DataAtualizacao = dispositivo.DataAtualizacao
                };

                await _dispositivos.InsertOneAsync(dispositivoMongo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating device");
                throw;
            }
        }

        public async Task UpdateAsync(string id, Dispositivo dispositivo)
        {
            try
            {
                ObjectId objectId;
                if (Guid.TryParse(id, out _))
                {
                    // Se for GUID, buscar o documento pelo ID como string
                    var existingDevice = await _dispositivos.Find(d => d.Id.ToString() == id).FirstOrDefaultAsync();
                    if (existingDevice == null)
                        throw new Exception($"Device with ID {id} not found");
                    objectId = existingDevice.Id;
                }
                else
                {
                    // Se não for GUID, tentar converter para ObjectId
                    objectId = ObjectId.Parse(id);
                }

                var dispositivoMongo = new DispositivoMongo
                {
                    Id = objectId,
                    Descricao = dispositivo.Descricao,
                    CodigoReferencia = dispositivo.CodigoReferencia,
                    DataCriacao = dispositivo.DataCriacao,
                    DataAtualizacao = dispositivo.DataAtualizacao
                };

                await _dispositivos.ReplaceOneAsync(d => d.Id == objectId, dispositivoMongo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating device with ID {Id}", id);
                throw;
            }
        }

        public async Task RemoveAsync(string id)
        {
            try
            {
                if (Guid.TryParse(id, out _))
                {
                    // Se for GUID, buscar o documento pelo ID como string
                    var existingDevice = await _dispositivos.Find(d => d.Id.ToString() == id).FirstOrDefaultAsync();
                    if (existingDevice == null)
                        throw new Exception($"Device with ID {id} not found");
                    
                    await _dispositivos.DeleteOneAsync(d => d.Id == existingDevice.Id);
                }
                else
                {
                    // Se não for GUID, tentar converter para ObjectId
                    var objectId = ObjectId.Parse(id);
                    await _dispositivos.DeleteOneAsync(d => d.Id == objectId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing device with ID {Id}", id);
                throw;
            }
        }
    }

    public class DispositivoMongo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("Descricao")]
        public string Descricao { get; set; }

        [BsonElement("CodigoReferencia")]
        public string CodigoReferencia { get; set; }

        [BsonElement("DataCriacao")]
        public DateTime DataCriacao { get; set; }

        [BsonElement("DataAtualizacao")]
        public DateTime? DataAtualizacao { get; set; }
    }
}
