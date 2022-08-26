using AutoMapper;
using back_end.DTOs;
using back_end.Entidades;
using NetTopologySuite.Geometries;

namespace back_end.Mapper
{
    public class AutoMapperProfile: Profile
    {
        public AutoMapperProfile(GeometryFactory geometryFactory)
        {
            CreateMap<Genero, GeneroDTO>().ReverseMap();
            CreateMap<GeneroCreacionDTO, Genero>();

            CreateMap<Actor, ActorDTO>().ReverseMap();
            CreateMap<ActorCreacionDTO, Actor>()
                .ForMember(x => x.Foto, options => options.Ignore());

            CreateMap<CineCreacionDTO, Cine>()
                .ForMember(x => x.Ubicacion, x => x.MapFrom(dto => geometryFactory.CreatePoint(new Coordinate(dto.Longitud, dto.Latitud))));

            CreateMap<Cine, CineDTO>()
                .ForMember(x => x.Latitud, dto => dto.MapFrom(campo => campo.Ubicacion.Y))
                .ForMember(x => x.Longitud, dto => dto.MapFrom(campo => campo.Ubicacion.X));

            CreateMap<PeliculaCreacionDTO, Pelicula>()
                .ForMember(x => x.Poster, opciones => opciones.Ignore())
                .ForMember(x => x.PeliculasGeneros, opciones => opciones.MapFrom(MapearPeliculasGeneros))
                .ForMember(x => x.PeliculasCines, opciones => opciones.MapFrom(MapearPeliculasCines))
                .ForMember(x => x.PeliculasActores, opciones => opciones.MapFrom(MapearPeliculasActores));

            CreateMap<Pelicula, PeliculaDTO>()
                .ForMember(x => x.Generos, opciones => opciones.MapFrom(MapearPeliculasGeneros))
                .ForMember(x => x.Cines, opciones => opciones.MapFrom(MapearPeliculasCines))
                .ForMember(x => x.Actores, opciones => opciones.MapFrom(MapearPeliculasActores));
        }

        private List<CineDTO> MapearPeliculasCines(Pelicula pelicula, PeliculaDTO peliculaDTO)
        {
            var resultado = new List<CineDTO>();

            if(pelicula.PeliculasCines != null)
            {
                foreach(var peliculasCines in pelicula.PeliculasCines)
                {
                    resultado.Add(new CineDTO()
                    {
                        Id = peliculasCines.CineId,
                        Nombre = peliculasCines.Cine.Nombre,
                        Latitud = peliculasCines.Cine.Ubicacion.Y,
                        Longitud = peliculasCines.Cine.Ubicacion.X
                    });
                }
            }

            return resultado;
        }

        private List<PeliculaActorDTO> MapearPeliculasActores(Pelicula pelicula, PeliculaDTO peliculaDTO)
        {
            var resultado = new List<PeliculaActorDTO>();

            if(pelicula.PeliculasActores != null)
            {
                foreach(var actorPeliculas in pelicula.PeliculasActores)
                {
                    resultado.Add(new PeliculaActorDTO()
                    {
                        Id = actorPeliculas.ActorId,
                        Nombre = actorPeliculas.Actor.Nombre,
                        Foto = actorPeliculas.Actor.Foto,
                        Orden = actorPeliculas.Orden,
                        Personaje = actorPeliculas.Personaje
                    });
                }
            }

            return resultado;
        }

        private List<PeliculasGeneros> MapearPeliculasGeneros(PeliculaCreacionDTO peliculaCreacionDTO, Pelicula pelicula)
        {
            var resultado = new List<PeliculasGeneros>();

            if(peliculaCreacionDTO.GenerosIds == null)
            {
                return resultado;
            }

            foreach(var id in peliculaCreacionDTO.GenerosIds)
            {
                resultado.Add(new PeliculasGeneros() { GeneroId = id });
            }

            return resultado; 
        }

        private List<PeliculasCines> MapearPeliculasCines(PeliculaCreacionDTO peliculaCreacionDTO, Pelicula pelicula)
        {
            var resultado = new List<PeliculasCines>();

            if(peliculaCreacionDTO.CinesIds == null)
            {
                return resultado;
            }

            foreach(var id in peliculaCreacionDTO.CinesIds)
            {
                resultado.Add(new PeliculasCines() { CineId = id });
            }

            return resultado;
        }

    }
}
