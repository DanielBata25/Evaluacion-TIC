using Business.Services;
using Entity.context;
using Entity.DTO;
using Entity.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Utilities;

namespace Web.Controllers
{
    /// <summary>
    /// Controlador para la gestión de roles en el sistema
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    public class RolController : ControllerBase
    {
        private readonly RolService _rolBusiness;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RolController> _logger;

        /// <summary>
        /// Constructor del controlador de roles
        /// </summary>
        public RolController(RolService _rolBusiness, ApplicationDbContext context ,ILogger<RolController> _logger)
        {
            this._rolBusiness = _rolBusiness;
            _context = context;
            this._logger = _logger;
        }

        /// <summary>
        /// Obtiene todos los roles del sistema
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RolDTO>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllRols()
        {
            try
            {
                var roles = await _rolBusiness.GetAllAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener roles");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("jwt")]
        [ProducesResponseType(typeof(IEnumerable<RolDTO>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllRolJWT()
        {
            try
            {
                var userClaims = HttpContext.User;

                // Verificar si el usuario tiene el rol "Administrador"
                if (userClaims.IsInRole("Administrador"))
                {
                    // Lógica si es administrador (ejecuta método original)
                    var usersAll = await _context.Set<Rol>()
                        .ToListAsync();

                    return Ok(usersAll);
                }
                else
                {
                    // Si NO es administrador, ejecuta lógica alternativa
                    var usersLimited = await _rolBusiness.GetAllAsync();
                    return Ok(usersLimited);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los Usuarios con JWT");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un rol específico por su ID
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetRolById(int id)
        {
            try
            {
                var rol = await _rolBusiness.GetByIdAsync(id);
                return Ok(rol);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida para el rol con ID:" + id);
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogInformation(ex, "Rol no encontrado con ID: {RolId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al obtener rol con ID: {RolId}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo rol en el sistema
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(RolDTO), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateRol([FromBody] RolDTO rolDTO)
        {
            try
            {
                var createRol = await _rolBusiness.CreateAsync(rolDTO);
                return CreatedAtAction(nameof(GetRolById), new { id = createRol.Id }, createRol);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida");
                return BadRequest(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al crear el rol");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un rol existente
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(RolDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateRol([FromBody] RolDTO rolDTO)
        {
            try
            {
                if (rolDTO == null || rolDTO.Id <= 0)
                {
                    return BadRequest(new { message = "El ID del rol debe ser mayor que cero y no nulo" });
                }

                var updatedRol = await _rolBusiness.UpdateAsync(rolDTO);
                return Ok(updatedRol);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al actualizar el rol");
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogInformation(ex, "Rol no encontrado con ID: {RolId}", rolDTO.Id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al actualizar el rol con ID: {RolId}", rolDTO.Id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un rol por su ID (eliminación permanente)
        /// </summary>
        [HttpDelete("permanent/{id:int}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteRol(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "El ID del rol debe ser mayor que cero" });
                }

                await _rolBusiness.DeletePermanentAsync(id);
                return Ok(new { message = "Rol eliminado correctamente" });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogInformation(ex, "Rol no encontrado con ID: {RolId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al eliminar el rol con ID: {RolId}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Elimina lógicamente un rol por su ID
        /// </summary>
        [HttpPut("Logico/{id:int}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteRolLogical(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "El ID del rol debe ser mayor que cero" });
                }

                await _rolBusiness.DeleteLogicalAsync(id);
                return Ok(new { message = "Rol eliminado lógico correctamente" });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogInformation(ex, "Rol no encontrado con ID: " + id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al eliminar lógicamente el rol con ID:" + id);
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
