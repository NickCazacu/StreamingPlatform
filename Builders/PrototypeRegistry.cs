using System;
using System.Collections.Generic;
using StreamingPlatform.Models;

namespace StreamingPlatform.Builders
{
    public class PrototypeRegistry
    {
        private readonly Dictionary<string, Movie> _moviePrototypes = new Dictionary<string, Movie>();
        private readonly Dictionary<string, Series> _seriesPrototypes = new Dictionary<string, Series>();
        private readonly Dictionary<string, Documentary> _docPrototypes = new Dictionary<string, Documentary>();


        public void RegisterMovie(string key, Movie prototype)
        {
            _moviePrototypes[key] = prototype;
        }

        public void RegisterSeries(string key, Series prototype)
        {
            _seriesPrototypes[key] = prototype;
        }

        public void RegisterDocumentary(string key, Documentary prototype)
        {
            _docPrototypes[key] = prototype;
        }

        public Movie CloneMovie(string key)
        {
            if (!_moviePrototypes.ContainsKey(key))
                throw new InvalidOperationException($"Prototipul '{key}' nu există pentru Movie.");
            return _moviePrototypes[key].DeepClone();
        }

        public Series CloneSeries(string key)
        {
            if (!_seriesPrototypes.ContainsKey(key))
                throw new InvalidOperationException($"Prototipul '{key}' nu există pentru Series.");
            return _seriesPrototypes[key].DeepClone();
        }

        public Documentary CloneDocumentary(string key)
        {
            if (!_docPrototypes.ContainsKey(key))
                throw new InvalidOperationException($"Prototipul '{key}' nu există pentru Documentary.");
            return _docPrototypes[key].DeepClone();
        }

        public IEnumerable<string> GetMoviePrototypes() => _moviePrototypes.Keys;
        public IEnumerable<string> GetSeriesPrototypes() => _seriesPrototypes.Keys;
        public IEnumerable<string> GetDocPrototypes() => _docPrototypes.Keys;
    }
}
