using System;
using System.Collections.Generic;
using System.Linq;
using StreamingPlatform.Models;
using StreamingPlatform.Interfaces;

namespace StreamingPlatform.Composite
{
    // ============================================================
    // COMPOSITE PATTERN
    // ============================================================
    // Problema: Vrem să organizăm conținut media în ierarhii:
    // playlisturi care conțin filme, seriale, dar și alte playlisturi.
    // Vrem să tratăm un film individual și un playlist cu 50 de filme
    // în ACELAȘI mod — GetTotalDuration(), GetItemCount(), Display().
    //
    // Soluția:
    // - IMediaComponent — interfața comună
    // - MediaLeaf — element individual (wrapper peste Movie/Series/Documentary)
    // - MediaPlaylist — colecție care conține alte IMediaComponent
    //   (poate conține atât frunze cât și alte playlisturi)
    // ============================================================

    // ============================================================
    // LEAF — Element individual (wrappează un MediaContent existent)
    // ============================================================
    public class MediaLeaf : IMediaComponent
    {
        private readonly MediaContent _content;

        public MediaLeaf(MediaContent content)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public MediaContent Content => _content;

        public string GetName() => _content.Title;

        public string Display(string indent = "")
        {
            string type = _content switch
            {
                Movie m => $"Film ({m.DurationMinutes} min)",
                Series s => $"Serial ({s.SeasonsCount} sez, {s.EpisodesCount} ep)",
                Documentary d => $"Documentar ({d.DurationMinutes} min)",
                _ => "Conținut"
            };
            return $"{indent}🎬 {_content.Title} [{type}] - Rating: {_content.AverageRating}/5";
        }

        public int GetTotalDuration() => _content.GetDuration();

        public int GetItemCount() => 1;

        public double GetAverageRating() => _content.AverageRating;

        public List<IMediaComponent> GetChildren() => new List<IMediaComponent>();
    }

    // ============================================================
    // COMPOSITE — Colecție (Playlist / Categorie)
    // Conține alte IMediaComponent (frunze SAU alte playlisturi)
    // ============================================================
    public class MediaPlaylist : IMediaComponent
    {
        private readonly string _name;
        private readonly string _description;
        private readonly List<IMediaComponent> _children = new List<IMediaComponent>();

        public MediaPlaylist(string name, string description = "")
        {
            _name = name;
            _description = description;
        }

        // --- Gestionare copii ---

        public void Add(IMediaComponent component)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));
            if (component == this)
                throw new InvalidOperationException("Nu poți adăuga un playlist în el însuși.");
            _children.Add(component);
        }

        public void Remove(IMediaComponent component)
        {
            _children.Remove(component);
        }

        public void Clear()
        {
            _children.Clear();
        }

        // --- Implementare IMediaComponent ---

        public string GetName() => _name;

        public string Display(string indent = "")
        {
            string result = $"{indent}📁 {_name}";
            if (!string.IsNullOrEmpty(_description))
                result += $" — {_description}";
            result += $" [{GetItemCount()} elemente, {GetTotalDuration()} min total]";

            foreach (var child in _children)
            {
                result += "\n" + child.Display(indent + "   ");
            }

            return result;
        }

        /// <summary>
        /// Calculează durata totală RECURSIV — trece prin toate nivelurile ierarhiei.
        /// </summary>
        public int GetTotalDuration()
        {
            int total = 0;
            foreach (var child in _children)
            {
                total += child.GetTotalDuration();
            }
            return total;
        }

        /// <summary>
        /// Numără toate elementele individuale RECURSIV.
        /// Un sub-playlist cu 5 filme contează ca 5, nu ca 1.
        /// </summary>
        public int GetItemCount()
        {
            int count = 0;
            foreach (var child in _children)
            {
                count += child.GetItemCount();
            }
            return count;
        }

        /// <summary>
        /// Calculează media rating-urilor tuturor elementelor din ierarhie.
        /// </summary>
        public double GetAverageRating()
        {
            var allLeaves = GetAllLeaves();
            if (allLeaves.Count == 0) return 0;
            return Math.Round(allLeaves.Average(l => l.GetAverageRating()), 1);
        }

        public List<IMediaComponent> GetChildren() => new List<IMediaComponent>(_children);

        // --- Metode helper ---

        private List<IMediaComponent> GetAllLeaves()
        {
            var leaves = new List<IMediaComponent>();
            foreach (var child in _children)
            {
                if (child is MediaLeaf)
                    leaves.Add(child);
                else if (child is MediaPlaylist playlist)
                    leaves.AddRange(playlist.GetAllLeaves());
            }
            return leaves;
        }
    }
}