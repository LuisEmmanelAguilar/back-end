﻿using AutoMapper;
using back_end.Data;
using back_end.DTOs;
using back_end.Entidades;
using back_end.Interfaces;
using back_end.Utilidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace back_end.Controllers
{
    [Route("api/peliculas")]
    [ApiController]
    public class PeliculasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAlmacenadorArchivos _almacenadorArchivos;
        private readonly string contenedor = "peliculas";

        public PeliculasController(
            ApplicationDbContext context,
            IMapper mapper,
            IAlmacenadorArchivos almacenadorArchivos)
        {
            _context = context;
            _mapper = mapper;
            _almacenadorArchivos = almacenadorArchivos;
        }

        [HttpGet]
        public async Task<ActionResult<LandingPageDTO>> Get()
        {
            var top = 6;
            var hoy = DateTime.Today;

            var proximosEstrenos = await _context.Peliculas
                                        .Where(x => x.FechaLanzamiento > hoy)
                                        .OrderBy(x => x.FechaLanzamiento)
                                        .Take(top)
                                        .ToListAsync();

            var enCines = await _context.Peliculas
                                .Where(x => x.EnCines)
                                .OrderBy(x => x.FechaLanzamiento)
                                .Take(top)
                                .ToListAsync();

            var resultado = new LandingPageDTO();
            resultado.ProximosEstrenos = _mapper.Map<List<PeliculaDTO>>(proximosEstrenos);
            resultado.EnCines = _mapper.Map<List<PeliculaDTO>>(enCines);

            return resultado;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PeliculaDTO>> Get(int id)
        {
            var pelicula = await _context.Peliculas
                                .Include(x => x.PeliculasGeneros).ThenInclude(x => x.Genero)
                                .Include(x => x.PeliculasActores).ThenInclude(x => x.Pelicula)
                                .Include(x => x.PeliculasCines).ThenInclude(x => x.Cine)
                                .FirstOrDefaultAsync(x => x.Id == id);

            if(pelicula == null)
            {
                return NotFound();
            }

            var dto = _mapper.Map<PeliculaDTO>(pelicula);
            dto.Actores = dto.Actores.OrderBy(x => x.Orden).ToList();
            return dto;
        }

        [HttpGet("filtrar")]
        public async Task<ActionResult<List<PeliculaDTO>>> Filtrar([FromQuery] PeliculasFiltrarDTO peliculasFiltrarDTO)
        {
            var peliculasQueryable = _context.Peliculas.AsQueryable();

            if(!string.IsNullOrEmpty(peliculasFiltrarDTO.Titulo))
            {
                peliculasQueryable = peliculasQueryable
                                    .Where(x => x.Titulo.Contains(peliculasFiltrarDTO.Titulo));
            }

            if(peliculasFiltrarDTO.EnCines)
            {
                peliculasQueryable = peliculasQueryable
                                    .Where(x => x.EnCines);
            }

            if(peliculasFiltrarDTO.ProximosEstrenos)
            {
                var hoy = DateTime.Today;
                peliculasQueryable = peliculasQueryable
                                     .Where(x => x.FechaLanzamiento > hoy);
            }

            if(peliculasFiltrarDTO.GeneroId != 0)
            {
                peliculasQueryable = peliculasQueryable
                                    .Where(x => x.PeliculasGeneros.Select(y => y.GeneroId)
                                    .Contains(peliculasFiltrarDTO.GeneroId));
            }

            await HttpContext.InsertarParametrosPaginacionEnCabecera(peliculasQueryable);

            var peliculas = await peliculasQueryable
                                .Paginar(peliculasFiltrarDTO.PaginacionDTO)
                                .ToListAsync();
            return _mapper.Map<List<PeliculaDTO>>(peliculas);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Post([FromForm] PeliculaCreacionDTO peliculaCreacionDTO)
        {
            var pelicula = _mapper.Map<Pelicula>(peliculaCreacionDTO);

            if(peliculaCreacionDTO.Poster != null)
            {
                pelicula.Poster = await _almacenadorArchivos.GuardarArchivo(contenedor, peliculaCreacionDTO.Poster);
            }

            EscribirOrdenActores(pelicula);

            _context.Add(pelicula);
            await _context.SaveChangesAsync();
            return pelicula.Id;
        }

        [HttpGet("PostGet")]
        public async Task<ActionResult<PeliculasPostGetDTO>> PostGet()
        {
            var cines = await _context.Cines.ToListAsync();
            var generos = await _context.Generos.ToListAsync();

            var cinesDTO = _mapper.Map<List<CineDTO>>(cines);
            var generosDTO = _mapper.Map<List<GeneroDTO>>(generos);

            return new PeliculasPostGetDTO() { Cines = cinesDTO, Generos = generosDTO };
        }

        [HttpGet("PutGet/{id:int}")]
        public async Task<ActionResult<PeliculasPutGetDTO>> PutGet(int id)
        {
            var peliculaActionResult = await Get(id);
            if(peliculaActionResult.Result is NotFoundResult) { return NotFound(); }

            var pelicula = peliculaActionResult.Value;

            var generosSeleccionadosIds = pelicula.Generos
                                        .Select(x => x.Id)
                                        .ToList();

            var generosNoSeleccionados = await _context.Generos
                                         .Where(x => !generosSeleccionadosIds.Contains(x.Id))
                                         .ToListAsync();

            var cinesSeleccionadosIds = pelicula.Cines
                                        .Select(x => x.Id)
                                        .ToList();

            var cinesNoSeleccionados = await _context.Cines
                                        .Where(x => !cinesSeleccionadosIds.Contains(x.Id))
                                        .ToListAsync();

            var generosNoSeleccionadosDTO = _mapper.Map<List<GeneroDTO>>(generosNoSeleccionados);
            var cinesNoSeleccionadosDTO = _mapper.Map<List<CineDTO>>(cinesNoSeleccionados);

            var respuesta = new PeliculasPutGetDTO();
            respuesta.Pelicula = pelicula;
            respuesta.GenerosSeleccionados = pelicula.Generos;
            respuesta.GenerosNoSeleccionados = generosNoSeleccionadosDTO;
            respuesta.CinesSeleccionados = pelicula.Cines;
            respuesta.CinesNoSeleccionados = cinesNoSeleccionadosDTO;
            respuesta.Actores = pelicula.Actores;
            return respuesta;
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put([FromForm] PeliculaCreacionDTO peliculaCreacionDTO, int id)
        {
            var pelicula = await _context.Peliculas
                            .Include(x => x.PeliculasActores)
                            .Include(x => x.PeliculasGeneros)
                            .Include(x => x.PeliculasCines)
                            .FirstOrDefaultAsync(x => x.Id == id);
            
            if(pelicula == null)
            {
                return NotFound();
            }

            pelicula = _mapper.Map(peliculaCreacionDTO, pelicula);

            if(peliculaCreacionDTO.Poster != null)
            {
                pelicula.Poster = await _almacenadorArchivos.EditarArchivo(contenedor, peliculaCreacionDTO.Poster, pelicula.Poster);
            }

            EscribirOrdenActores(pelicula);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var pelicula = await _context.Peliculas.FirstOrDefaultAsync(x => x.Id == id);

            if(pelicula == null)
            {
                return NotFound();
            }

            _context.Remove(pelicula);
            await _context.SaveChangesAsync();
            await _almacenadorArchivos.BorrarArchivo(pelicula.Poster, contenedor);

            return NoContent();
        }

        private void EscribirOrdenActores(Pelicula pelicula)
        {
            if(pelicula.PeliculasActores != null)
            {
                for(int i = 0; i < pelicula.PeliculasActores.Count; i++)
                {
                    pelicula.PeliculasActores[i].Orden = i;
                }
            }
        }
    }
}
