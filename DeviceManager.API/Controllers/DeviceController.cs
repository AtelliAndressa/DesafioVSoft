using DeviceManager.API.Models;
using DeviceManager.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DeviceController : ControllerBase
    {
        private readonly DeviceService _deviceService;
        private readonly ILogger<DeviceController> _logger;

        public DeviceController(DeviceService deviceService, ILogger<DeviceController> logger)
        {
            _deviceService = deviceService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all devices
        /// </summary>
        /// <returns>List of devices</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<Dispositivo>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Dispositivo>>> Get()
        {
            try
            {
                var devices = await _deviceService.GetAsync();
                return Ok(devices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all devices");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving devices");
            }
        }

        /// <summary>
        /// Gets a device by ID
        /// </summary>
        /// <param name="id">Device ID</param>
        /// <returns>Device information</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Dispositivo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Dispositivo>> Get(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return BadRequest("ID cannot be empty");

                var device = await _deviceService.GetByIdAsync(id);
                if (device == null)
                    return NotFound($"Device with ID {id} not found");

                return Ok(device);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting device with ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the device");
            }
        }

        /// <summary>
        /// Creates a new device
        /// </summary>
        /// <param name="device">Device information</param>
        /// <returns>Created device</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Dispositivo), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Dispositivo>> Create([FromBody] Dispositivo device)
        {
            try
            {
                if (device == null)
                    return BadRequest("Device information is required");

                if (string.IsNullOrEmpty(device.Descricao) || string.IsNullOrEmpty(device.CodigoReferencia))
                    return BadRequest("Description and reference code are required");

                var existingDevice = await _deviceService.GetByCodigoReferenciaAsync(device.CodigoReferencia);
                if (existingDevice != null)
                    return BadRequest($"Device with reference code {device.CodigoReferencia} already exists");

                device.DataCriacao = DateTime.UtcNow;
                await _deviceService.CreateAsync(device);
                return CreatedAtAction(nameof(Get), new { id = device.Id }, device);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating device");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the device");
            }
        }

        /// <summary>
        /// Updates an existing device
        /// </summary>
        /// <param name="id">Device ID</param>
        /// <param name="device">Updated device information</param>
        /// <returns>No content</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(string id, [FromBody] Dispositivo device)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || device == null)
                    return BadRequest("ID and device information are required");

                if (string.IsNullOrEmpty(device.Descricao) || string.IsNullOrEmpty(device.CodigoReferencia))
                    return BadRequest("Description and reference code are required");

                var existingDevice = await _deviceService.GetByIdAsync(id);
                if (existingDevice == null)
                    return NotFound($"Device with ID {id} not found");

                if (existingDevice.CodigoReferencia != device.CodigoReferencia)
                {
                    var deviceWithSameCode = await _deviceService.GetByCodigoReferenciaAsync(device.CodigoReferencia);
                    if (deviceWithSameCode != null)
                        return BadRequest($"Device with reference code {device.CodigoReferencia} already exists");
                }

                device.Id = id;
                device.DataAtualizacao = DateTime.UtcNow;
                await _deviceService.UpdateAsync(id, device);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating device with ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the device");
            }
        }

        /// <summary>
        /// Deletes a device
        /// </summary>
        /// <param name="id">Device ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return BadRequest("ID cannot be empty");

                var device = await _deviceService.GetByIdAsync(id);
                if (device == null)
                    return NotFound($"Device with ID {id} not found");

                await _deviceService.RemoveAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting device with ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the device");
            }
        }

        /// <summary>
        /// Synchronizes devices from mobile app
        /// </summary>
        /// <param name="request">Sync request containing device operations</param>
        /// <returns>Sync result</returns>
        [HttpPost("sync")]
        [ProducesResponseType(typeof(SyncResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SyncResult>> Sync([FromBody] SyncRequest request)
        {
            try
            {
                if (request?.Items == null || !request.Items.Any())
                    return BadRequest("Sync request must contain at least one item");

                var result = new SyncResult
                {
                    Success = true,
                    SyncedItems = new List<SyncItem>()
                };

                foreach (var item in request.Items)
                {
                    try
                    {
                        if (item.Device == null)
                            throw new ArgumentException("Device information is required");

                        switch (item.Operation?.ToLower())
                        {
                            case "create":
                                await _deviceService.CreateAsync(item.Device);
                                break;
                            case "update":
                                await _deviceService.UpdateAsync(item.Device.Id, item.Device);
                                break;
                            case "delete":
                                await _deviceService.RemoveAsync(item.Device.Id);
                                break;
                            default:
                                throw new ArgumentException($"Invalid operation: {item.Operation}");
                        }

                        result.SyncedItems.Add(new SyncItem
                        {
                            DeviceId = item.Device.Id,
                            Success = true
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error syncing device {DeviceId}", item.Device?.Id);
                        result.SyncedItems.Add(new SyncItem
                        {
                            DeviceId = item.Device?.Id,
                            Success = false,
                            Error = ex.Message
                        });
                        result.Success = false;
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sync operation");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during sync operation");
            }
        }
    }

    public class SyncRequest
    {
        public List<SyncItemRequest> Items { get; set; }
    }

    public class SyncItemRequest
    {
        public string Operation { get; set; }
        public Dispositivo Device { get; set; }
    }

    public class SyncResult
    {
        public bool Success { get; set; }
        public List<SyncItem> SyncedItems { get; set; }
    }

    public class SyncItem
    {
        public string DeviceId { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }
} 