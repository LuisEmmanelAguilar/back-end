using AutoMapper;
using back_end.Data;
using back_end.DTOs;
using back_end.Entidades;
using back_end.Utilidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace back_end.Controllers
{
    [Route("api/generos")]
    [ApiController]
    public class GenerosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<GenerosController> _logger;

        public GenerosController(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<GenerosController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<GeneroDTO>>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            var queryable = _context.Generos.AsQueryable();
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable);
            var generos = await queryable.
                                OrderBy(x => x.Nombre)
                                .Paginar(paginacionDTO)
                                .ToListAsync();

            return _mapper.Map<List<GeneroDTO>>(generos);
        }

        [HttpGet("todos")]
        public async Task<ActionResult<List<GeneroDTO>>> Todos()
        {
            var generos = await _context.Generos.ToListAsync();
            return _mapper.Map<List<GeneroDTO>>(generos);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<GeneroDTO>> Get(int id)
        {
            var genero = await _context.Generos.FirstOrDefaultAsync(x => x.Id == id);

            if(genero == null)
            {
                return NotFound();
            }

            return _mapper.Map<GeneroDTO>(genero);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] GeneroCreacionDTO generoCreacionDTO)
        {
            var genero = _mapper.Map<Genero>(generoCreacionDTO);
            _context.Add(genero);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put([FromBody] GeneroCreacionDTO generoCreacionDTO, int id)
        {
            var genero = await _context.Generos.FirstOrDefaultAsync(x => x.Id == id);

            if(genero == null)
            {
                return NotFound();
            }

            genero = _mapper.Map(generoCreacionDTO, genero);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await _context.Generos.AnyAsync(x => x.Id == id);

            if(!existe)
            {
                return NotFound();
            }

            _context.Remove(new Genero() { Id = id });
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
