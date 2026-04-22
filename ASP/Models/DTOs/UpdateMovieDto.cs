using System;
using System.Collections.Generic;

namespace ConnectDB.Models.DTOs
{
    public class UpdateMovieDto
    {
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public int? Duration { get; set; }

        public string? PosterUrl { get; set; }

        public string? TrailerUrl { get; set; }

        public string? VideoUrl { get; set; }

        public List<int>? GenreIds { get; set; }
    }
}