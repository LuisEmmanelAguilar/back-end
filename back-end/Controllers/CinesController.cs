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
    [Route("api/cines")]
    [ApiController]
    public class CinesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CinesController(
            ApplicationDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<CineDTO>>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            var queryable = _context.Cines.AsQueryable();
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable);
            var cines = await queryable
                            .OrderBy(x => x.Nombre)
                            .Paginar(paginacionDTO).ToListAsync();
            return _mapper.Map<List<CineDTO>>(cines);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CineDTO>> Get(int id)
        {
            var cine = await _context.Cines.FirstOrDefaultAsync(x => x.Id == id);

            if(cine == null)
            {
                return NotFound();
            }

            return _mapper.Map<CineDTO>(cine);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CineCreacionDTO cineCreacionDTO)
        {
            var cine = _mapper.Map<Cine>(cineCreacionDTO);
            _context.Add(cine);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put([FromBody] CineCreacionDTO cineCreacionDTO, int id)
        {
            var cine = await _context.Cines.FirstOrDefaultAsync(x => x.Id == id);

            if(cine == null)
            {
                return NotFound();
            }

            cine = _mapper.Map(cineCreacionDTO, cine);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await _context.Cines.AnyAsync(x => x.Id == id);

            if(!existe)
            {
                return NotFound();
            }

            _context.Remove(new Cine() { Id = id });
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
