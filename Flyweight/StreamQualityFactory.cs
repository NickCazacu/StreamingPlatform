using System;
using System.Collections.Generic;

namespace StreamingPlatform.Flyweight
{
    /// <summary>
    /// FLYWEIGHT FACTORY — Garantează că fiecare calitate există o SINGURĂ dată.
    /// Dacă calitatea e deja în pool → returnează instanța existentă.
    /// Dacă nu există → creează una nouă și o adaugă în pool.
    ///
    /// Clienții NICIODATĂ nu creează StreamQuality direct —
    /// folosesc exclusiv această fabrică.
    /// </summary>
    public static class StreamQualityFactory
    {
        // Pool-ul de obiecte partajate — cheia e rezoluția
        private static readonly Dictionary<string, StreamQuality> _pool = new();
        private static int _totalRequests = 0;
        private static int _cacheHits     = 0;

        // Pre-populăm pool-ul cu calitățile standard ale platformei
        static StreamQualityFactory()
        {
            _pool["360p"]  = new StreamQuality("360p",   400,   "H.264", "HLS",  false, 30);
            _pool["480p"]  = new StreamQuality("480p",   800,   "H.264", "HLS",  false, 30);
            _pool["720p"]  = new StreamQuality("720p",   2500,  "H.264", "HLS",  false, 60);
            _pool["1080p"] = new StreamQuality("1080p",  5000,  "H.265", "HLS",  false, 60);
            _pool["4K"]    = new StreamQuality("4K",     15000, "H.265", "DASH", true,  60);
            _pool["4KUHD"] = new StreamQuality("4KUHD",  25000, "AV1",   "DASH", true,  120);
        }

        /// <summary>
        /// Returnează instanța flyweight pentru calitatea cerută.
        /// GARANTEAZĂ că pentru aceeași rezoluție se returnează MEREU același obiect.
        /// </summary>
        public static StreamQuality GetQuality(string resolution)
        {
            _totalRequests++;

            if (_pool.TryGetValue(resolution, out StreamQuality? existing))
            {
                _cacheHits++;
                return existing; // Returnăm instanța EXISTENTĂ — zero alocare nouă!
            }

            // Calitate personalizată — creată o singură dată și adăugată în pool
            var custom = new StreamQuality(resolution, 3000, "H.265", "HLS", false, 60);
            _pool[resolution] = custom;
            Console.WriteLine($"      [Flyweight Factory] Calitate nouă adăugată în pool: {resolution}");
            return custom;
        }

        public static int GetPoolSize()      => _pool.Count;
        public static int GetTotalRequests() => _totalRequests;
        public static int GetCacheHits()     => _cacheHits;

        public static IReadOnlyDictionary<string, StreamQuality> GetPool() => _pool;

        public static string GetPoolReport()
        {
            double hitRate = _totalRequests > 0
                ? Math.Round((double)_cacheHits / _totalRequests * 100, 1)
                : 0;

            return $"Pool size: {_pool.Count} obiecte | " +
                   $"Cereri totale: {_totalRequests} | " +
                   $"Cache hits: {_cacheHits} ({hitRate}%)";
        }
    }
}
