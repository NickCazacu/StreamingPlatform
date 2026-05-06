-- =====================================================================
-- StreamZoneDB — Actualizare PosterUrl pentru toate filmele/serialele/documentarele
-- Rulează în SSMS pe baza de date StreamZoneDB
-- =====================================================================

USE StreamZoneDB;
GO

-- ─── Filme moldovenești ──────────────────────────────────────────────
UPDATE dbo.Movies SET PosterUrl = N'https://m.media-amazon.com/images/M/MV5BNjI4Mzk1NzUyMV5BMl5BanBnXkFtZTgwODg1NTM5NjE@._V1_FMjpg_UX1000_.jpg'
    WHERE Title = N'Maria Mirabela';

UPDATE dbo.Movies SET PosterUrl = N'https://image.pmgstatic.com/cache/resized/w663/files/images/film/posters/162/493/162493597_45e348.jpg'
    WHERE Title = N'Nunta în Basarabia';

UPDATE dbo.Movies SET PosterUrl = N'https://placehold.co/600x900/2a1118/d4a853?text=Tatal+meu+dictatorul&font=playfair'
    WHERE Title = N'Tatăl meu, dictatorul';

UPDATE dbo.Movies SET PosterUrl = N'https://film.md/uploads/movies/big/ce_lume_minunata_online_film_md.png'
    WHERE Title = N'Ce lume minunată';

UPDATE dbo.Movies SET PosterUrl = N'https://m.media-amazon.com/images/M/MV5BNGZkZjZmZGMtN2RjZS00ZTAxLTg2OWMtODY1YWExZTQxYTg0XkEyXkFqcGc@._V1_FMjpg_UX1000_.jpg'
    WHERE Title = N'La limita de jos a cerului';

UPDATE dbo.Movies SET PosterUrl = N'https://m.media-amazon.com/images/M/MV5BODAyYjI1ZTktNGZiYy00NmY4LTkwNWYtNGZhNWE4MDc5NGQxXkEyXkFqcGc@._V1_.jpg'
    WHERE Title = N'Dimitrie Cantemir';

UPDATE dbo.Movies SET PosterUrl = N'https://placehold.co/600x900/1a3d28/d4a853?text=CODRU&font=playfair'
    WHERE Title = N'Codru';

-- ─── Filme internaționale ────────────────────────────────────────────
UPDATE dbo.Movies SET PosterUrl = N'https://m.media-amazon.com/images/M/MV5BMjAxMzY3NjcxNF5BMl5BanBnXkFtZTcwNTI5OTM0Mw@@._V1_.jpg'
    WHERE Title = N'Inception';

UPDATE dbo.Movies SET PosterUrl = N'https://atthemovies.uk/cdn/shop/products/DarkKnight2008adv1sht27x40in350.jpg?v=1621381289&width=1090'
    WHERE Title = N'The Dark Knight';

UPDATE dbo.Movies SET PosterUrl = N'https://m.media-amazon.com/images/I/91obuWzA3XL.jpg'
    WHERE Title = N'Interstellar';

UPDATE dbo.Movies SET PosterUrl = N'https://m.media-amazon.com/images/I/911USrdQtPL.jpg'
    WHERE Title = N'The Shawshank Redemption';

-- ─── Seriale ─────────────────────────────────────────────────────────
UPDATE dbo.Series SET PosterUrl = N'https://carturesti.md/img-prod/230085293-0.png'
    WHERE Title = N'Toate pânzele sus!';

UPDATE dbo.Series SET PosterUrl = N'https://placehold.co/600x900/2a1118/d4a853?text=Cumpenele+Familiei&font=playfair'
    WHERE Title = N'Cumpenele Familiei';

UPDATE dbo.Series SET PosterUrl = N'https://i.ebayimg.com/images/g/HZ0AAOSwolNkPYqj/s-l1200.jpg'
    WHERE Title = N'Breaking Bad';

UPDATE dbo.Series SET PosterUrl = N'https://lh4.googleusercontent.com/proxy/g1rGi-gQZqLcHtLiIYVFLi3_vym9dS2JkF4LmD-Kxb-vBUj58-Xyg1fVvHrXq6BnEFWl2U0HP8Iqad0u7tkch_OxoGFoDG7r2g'
    WHERE Title = N'House M.D.';

UPDATE dbo.Series SET PosterUrl = N'https://i.ebayimg.com/00/s/MTYwMFgxMDgw/z/nyoAAOSw0P9hz5Vh/$_57.JPG?set_id=8800005007'
    WHERE Title = N'The Witcher';

UPDATE dbo.Series SET PosterUrl = N'https://m.media-amazon.com/images/I/81U0-cRG34S._AC_UF894,1000_QL80_.jpg'
    WHERE Title = N'Stranger Things';

-- ─── Documentare ─────────────────────────────────────────────────────
UPDATE dbo.Documentaries SET PosterUrl = N'https://placehold.co/600x900/1a3d28/d4a853?text=Codrii+Moldovei&font=playfair'
    WHERE Title = N'Codrii Moldovei';

UPDATE dbo.Documentaries SET PosterUrl = N'https://placehold.co/600x900/3d2415/d4a853?text=Stefan+cel+Mare&font=playfair'
    WHERE Title = N'Ștefan cel Mare — Legenda Moldovei';

UPDATE dbo.Documentaries SET PosterUrl = N'https://placehold.co/600x900/3d2415/d4a853?text=Manastirile+Basarabiei&font=playfair'
    WHERE Title = N'Mănăstirile Basarabiei';

UPDATE dbo.Documentaries SET PosterUrl = N'https://placehold.co/600x900/0a2840/d4a853?text=Our+Planet&font=playfair'
    WHERE Title = N'Our Planet';

-- ─── Sincronizare cu MoldovanContent ─────────────────────────────────
-- (PosterUrl există și aici — actualizăm de la tabelele de origine)
UPDATE mc
SET PosterUrl = src.PosterUrl
FROM dbo.MoldovanContent mc
INNER JOIN dbo.Movies src ON src.MovieId = mc.OriginalRefId AND mc.ContentType = 'Movie';

UPDATE mc
SET PosterUrl = src.PosterUrl
FROM dbo.MoldovanContent mc
INNER JOIN dbo.Series src ON src.SeriesId = mc.OriginalRefId AND mc.ContentType = 'Series';

UPDATE mc
SET PosterUrl = src.PosterUrl
FROM dbo.MoldovanContent mc
INNER JOIN dbo.Documentaries src ON src.DocumentaryId = mc.OriginalRefId AND mc.ContentType = 'Documentary';
GO

-- ─── Verificare ──────────────────────────────────────────────────────
SELECT 'Movies' AS Tabel,
       COUNT(*) AS Total,
       SUM(CASE WHEN PosterUrl IS NOT NULL AND PosterUrl <> '' THEN 1 ELSE 0 END) AS CuPoster
FROM dbo.Movies
UNION ALL
SELECT 'Series', COUNT(*),
       SUM(CASE WHEN PosterUrl IS NOT NULL AND PosterUrl <> '' THEN 1 ELSE 0 END)
FROM dbo.Series
UNION ALL
SELECT 'Documentaries', COUNT(*),
       SUM(CASE WHEN PosterUrl IS NOT NULL AND PosterUrl <> '' THEN 1 ELSE 0 END)
FROM dbo.Documentaries
UNION ALL
SELECT 'MoldovanContent', COUNT(*),
       SUM(CASE WHEN PosterUrl IS NOT NULL AND PosterUrl <> '' THEN 1 ELSE 0 END)
FROM dbo.MoldovanContent;
GO

PRINT '✓ Postere actualizate în BD pentru toate cele 29 titluri.';
GO
