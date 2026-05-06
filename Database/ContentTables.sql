-- =====================================================================
-- StreamZoneDB — Tabele pentru conținutul platformei
-- Filme, Seriale, Documentare + tabel separat pentru conținut moldovenesc
-- Rulează în SSMS pe baza de date StreamZoneDB
-- =====================================================================

USE StreamZoneDB;
GO

-- ─── Drop tabele existente (în ordine inversă FK-urilor) ─────────────
IF OBJECT_ID('dbo.MoldovanContent', 'U') IS NOT NULL DROP TABLE dbo.MoldovanContent;
IF OBJECT_ID('dbo.Movies',          'U') IS NOT NULL DROP TABLE dbo.Movies;
IF OBJECT_ID('dbo.Series',          'U') IS NOT NULL DROP TABLE dbo.Series;
IF OBJECT_ID('dbo.Documentaries',   'U') IS NOT NULL DROP TABLE dbo.Documentaries;
GO

-- =====================================================================
-- TABEL 1: Movies — toate filmele de pe platformă
-- =====================================================================
CREATE TABLE dbo.Movies (
    MovieId         INT IDENTITY(1,1) PRIMARY KEY,
    Title           NVARCHAR(200)  NOT NULL,
    Description     NVARCHAR(1000) NOT NULL,
    Genre           NVARCHAR(50)   NOT NULL,
    ContentRating   NVARCHAR(10)   NOT NULL,    -- G, PG, PG13, R
    DurationMinutes INT            NOT NULL,
    Director        NVARCHAR(150)  NOT NULL,
    AverageRating   DECIMAL(3,2)   NOT NULL DEFAULT 0,
    IsMoldovan      BIT            NOT NULL DEFAULT 0,
    PosterUrl       NVARCHAR(500)  NULL,
    CreatedAt       DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- =====================================================================
-- TABEL 2: Series — toate serialele de pe platformă
-- =====================================================================
CREATE TABLE dbo.Series (
    SeriesId        INT IDENTITY(1,1) PRIMARY KEY,
    Title           NVARCHAR(200)  NOT NULL,
    Description     NVARCHAR(1000) NOT NULL,
    Genre           NVARCHAR(50)   NOT NULL,
    ContentRating   NVARCHAR(10)   NOT NULL,
    Creator         NVARCHAR(150)  NOT NULL,
    SeasonsCount    INT            NOT NULL,
    EpisodesCount   INT            NOT NULL,
    EpisodeDuration INT            NOT NULL,    -- minute / episod
    IsCompleted     BIT            NOT NULL DEFAULT 0,
    AverageRating   DECIMAL(3,2)   NOT NULL DEFAULT 0,
    IsMoldovan      BIT            NOT NULL DEFAULT 0,
    PosterUrl       NVARCHAR(500)  NULL,
    CreatedAt       DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- =====================================================================
-- TABEL 3: Documentaries — toate documentarele de pe platformă
-- =====================================================================
CREATE TABLE dbo.Documentaries (
    DocumentaryId   INT IDENTITY(1,1) PRIMARY KEY,
    Title           NVARCHAR(200)  NOT NULL,
    Description     NVARCHAR(1000) NOT NULL,
    Genre           NVARCHAR(50)   NOT NULL DEFAULT 'Documentary',
    ContentRating   NVARCHAR(10)   NOT NULL,
    DurationMinutes INT            NOT NULL,
    Topic           NVARCHAR(100)  NOT NULL,
    Narrator        NVARCHAR(150)  NOT NULL,
    IsEducational   BIT            NOT NULL DEFAULT 1,
    AverageRating   DECIMAL(3,2)   NOT NULL DEFAULT 0,
    IsMoldovan      BIT            NOT NULL DEFAULT 0,
    PosterUrl       NVARCHAR(500)  NULL,
    CreatedAt       DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- =====================================================================
-- TABEL 4: MoldovanContent — TABEL SEPARAT cu DOAR conținutul moldovenesc
-- (filme + seriale + documentare unificate într-o singură listă)
-- =====================================================================
CREATE TABLE dbo.MoldovanContent (
    ContentId       INT IDENTITY(1,1) PRIMARY KEY,
    ContentType     NVARCHAR(20)   NOT NULL,    -- 'Movie' | 'Series' | 'Documentary'
    Title           NVARCHAR(200)  NOT NULL,
    Description     NVARCHAR(1000) NOT NULL,
    Genre           NVARCHAR(50)   NOT NULL,
    ContentRating   NVARCHAR(10)   NOT NULL,
    DurationMinutes INT            NOT NULL,    -- pentru seriale = sezoane*episoade*durata
    AuthorOrCreator NVARCHAR(150)  NOT NULL,    -- regizor / creator / narrator
    AverageRating   DECIMAL(3,2)   NOT NULL DEFAULT 0,
    Region          NVARCHAR(50)   NOT NULL DEFAULT 'Moldova',
    PosterUrl       NVARCHAR(500)  NULL,
    OriginalRefId   INT            NULL,        -- referință opțională la tabelul de origine
    CreatedAt       DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_MoldovanContent_Type CHECK (ContentType IN ('Movie','Series','Documentary'))
);
GO

-- =====================================================================
-- DATE — INSERĂRI ÎN Movies
-- =====================================================================
INSERT INTO dbo.Movies (Title, Description, Genre, ContentRating, DurationMinutes, Director, AverageRating, IsMoldovan, PosterUrl) VALUES
-- ── Filme moldovenești (12) ─────────────────────────────────────────
(N'Carbon',                        N'Un tânăr inginer descoperă că fabrica unde lucrează ascunde secrete toxice periculoase. O dramă contemporană din nordul Moldovei.', N'Drama',   N'PG13', 102, N'Ion Borș',          4.23, 1, N'https://m.media-amazon.com/images/M/MV5BODY1OGIxOTItM2Q4OC00ZGQyLWIwOWUtMWU4NzZkODI2MDJhXkEyXkFqcGc@._V1_UX800_.jpg'),
(N'Hotarul',                       N'Povestea unui sat moldovenesc aflat la frontiera dintre două lumi, în primii ani postbelici. Un film clasic al cinematografiei naționale.', N'Drama', N'PG',  88,  N'Vasile Pascaru',    4.80, 1, N'https://m.media-amazon.com/images/M/MV5BMTZhN2FlMDItMWY4ZS00OTY3LTk0ZTYtYmU0MDUzYWI5YzMyXkEyXkFqcGc@._V1_UX800_.jpg'),
(N'Abis',                          N'Un actor din Chișinău se luptă cu demonii interiori pe scena vieții și a teatrului. O poveste despre cădere și redempțiune în Moldova contemporană.', N'Drama', N'R', 96, N'Vlad Druc',     4.10, 1, N'https://cineplex.md/posters/Abis%20RO.jpg'),
(N'Puterea Probabilitatii',        N'Drumurile a trei personaje din Moldova se încrucișează într-un joc al destinului și al alegerilor. O reflecție despre șansă, vinovăție și iertare.', N'Drama', N'PG13', 110, N'Anatol Durbală', 4.23, 1, N'https://m.media-amazon.com/images/M/MV5BMDA0Yjk5ZjQtYjY4MC00N2RjLWFhYjctNDZhYjRkYWZjOWYyXkEyXkFqcGc@._V1_UX800.jpg'),
(N'Lăutarii',                      N'Povestea lui Toma Alimoș, lăutar din Basarabia, între dragoste, libertate și muzica sufletului. Capodopera lui Emil Loteanu, premiată la Moscova.',     N'Drama', N'PG',  87,  N'Emil Loteanu',     4.90, 1, N'https://m.media-amazon.com/images/M/MV5BNjkwZDdiYzQtYmY3OS00NmI3LWIzZTMtMzMyMzhjYjk3NGI1XkEyXkFqcGc@._V1_.jpg'),
(N'Maria Mirabela',                N'Capodopera de animație româno-sovietică a lui Ion Popescu-Gopo. O fetiță pornește într-o călătorie magică prin pădure pentru a-și salva prietenii.', N'Fantasy', N'G', 64,  N'Ion Popescu-Gopo', 4.73, 1, NULL),
(N'Nunta în Basarabia',            N'Un cuplu mixt — el bucureștean, ea din Chișinău — descoperă că o nuntă în Basarabia poate uni sau dezbina două lumi. Comedie romantică despre identitate.', N'Comedy', N'PG13', 95,  N'Napoleon Helmis',  4.07, 1, NULL),
(N'Tatăl meu, dictatorul',         N'Adolescenta Lia se confruntă cu autoritatea tatălui său într-o Moldovă post-sovietică plină de contraste. Dramă intimistă despre libertate, familie.',  N'Drama',  N'PG13', 105, N'Igor Cobileanski', 4.23, 1, NULL),
(N'Ce lume minunată',              N'Trei generații dintr-o familie din Chișinău se reîntâlnesc la o petrecere care scoate la iveală secretele vechi. O comedie tristă despre Moldova de astăzi.', N'Drama', N'PG13', 98,  N'Anatol Durbală',   3.93, 1, NULL),
(N'La limita de jos a cerului',    N'Doi frați dintr-un sat din nordul Moldovei visează să evadeze din rutina existenței rurale. O metaforă vizuală despre dor, iertare și destin.',           N'Drama',  N'PG13', 92,  N'Igor Cobileanski', 4.20, 1, NULL),
(N'Dimitrie Cantemir',             N'Portretul savantului-domnitor moldovean Dimitrie Cantemir, învățat enciclopedic și prinț al Moldovei. Frescă istorică cu accent pe dilemele politice.',  N'Drama',  N'PG',   118, N'Vlad Iovita',      4.47, 1, NULL),
(N'Codru',                         N'Un pădurar din codrii Orheiului descoperă o rețea de tăieri ilegale. Thriller ecologic care îmbină frumusețea peisajului basarabean cu mizele morale.', N'Drama',  N'PG13', 101, N'Eugen Damaschin',  3.87, 1, NULL),
-- ── Filme internaționale (4) ───────────────────────────────────────
(N'Inception',                     N'Un hoț specializat în furtul informațiilor din vise primește o ultimă misiune: implantarea unei idei în mintea cuiva. Thriller SF labirintic.',          N'SciFi',  N'PG13', 148, N'Christopher Nolan', 4.80, 0, NULL),
(N'The Dark Knight',               N'Batman se confruntă cu Joker, un criminal care vrea să arunce Gotham-ul în haos. Magnum opus al genului super-erou.',                                  N'Action', N'PG13', 152, N'Christopher Nolan', 4.90, 0, NULL),
(N'Interstellar',                  N'Pământul moare. Un grup de astronauți trece printr-o gaură de vierme în căutarea unei planete locuibile pentru omenire.',                              N'SciFi',  N'PG13', 169, N'Christopher Nolan', 4.60, 0, NULL),
(N'The Shawshank Redemption',      N'Andy Dufresne, condamnat pe nedrept la închisoare pe viață, își păstrează speranța alături de prietenul său Red. Cea mai votată dramă a tuturor timpurilor.', N'Drama', N'R', 142, N'Frank Darabont',  4.97, 0, NULL);
GO

-- =====================================================================
-- DATE — INSERĂRI ÎN Series
-- =====================================================================
INSERT INTO dbo.Series (Title, Description, Genre, ContentRating, Creator, SeasonsCount, EpisodesCount, EpisodeDuration, IsCompleted, AverageRating, IsMoldovan, PosterUrl) VALUES
-- ── Seriale moldovenești ────────────────────────────────────────────
(N'Plaha',              N'O dramă despre crima organizată, traficul de droguri și oameni prinși între loialitate și supraviețuire. Inspirat din realitățile spațiului post-sovietic.', N'Drama',  N'R',    N'Iuri Moroz',         1, 8,   52, 1, 4.87, 1, N'https://images.kinorium.com/movie/1080/11994379.jpg?1759733447'),
(N'Cumpenele Familiei', N'Trei generații dintr-o familie din Bălți încearcă să-și păstreze casa părintească într-o Moldovă care se schimbă rapid. Dramă socială cu accente de comedie.', N'Drama', N'PG13', N'Anatol Durbală',     2, 20,  45, 1, 4.17, 1, NULL),
-- ── Seriale internaționale ──────────────────────────────────────────
(N'Toate pânzele sus!', N'Aventurile echipajului bricului Speranța în secolul XIX, după romanul lui Radu Tudoran. Clasic al televiziunii românești despre prietenie, curaj și descoperire.', N'Action', N'PG', N'Mircea Mureșan', 1, 12,  48, 1, 4.80, 0, NULL),
(N'Breaking Bad',       N'Un profesor de chimie diagnosticat cu cancer începe să producă metamfetamină pentru a-și asigura familia. Una dintre cele mai aclamate serii TV din toate timpurile.', N'Drama', N'R', N'Vince Gilligan',  5, 62,  49, 1, 4.93, 0, NULL),
(N'House M.D.',         N'Dr. Gregory House, geniu medical antisocial, rezolvă cazuri imposibile cu echipa sa de la Princeton-Plainsboro. Mister medical cu suspans și umor caustic.',         N'Drama',   N'PG13', N'David Shore',     8, 177, 44, 1, 4.70, 0, NULL),
(N'The Witcher',        N'Geralt din Rivia, vânător mutant de monștri, traversează un continent fantastic în căutarea destinului său. Fantasy epic bazat pe romanele lui Andrzej Sapkowski.',  N'Fantasy', N'R',    N'Lauren S. Hissrich',3, 24,  60, 0, 4.00, 0, NULL),
(N'Stranger Things',    N'În anii 80, un grup de copii din Hawkins descoperă un univers paralel și forțe supranaturale. Omagiu nostalgic adus filmelor SF/horror din acea perioadă.',         N'SciFi',   N'PG13', N'Duffer Brothers',  4, 34,  55, 0, 4.50, 0, NULL);
GO

-- =====================================================================
-- DATE — INSERĂRI ÎN Documentaries
-- =====================================================================
INSERT INTO dbo.Documentaries (Title, Description, Genre, ContentRating, DurationMinutes, Topic, Narrator, IsEducational, AverageRating, IsMoldovan, PosterUrl) VALUES
-- ── Documentare moldovenești (5) ────────────────────────────────────
(N'Moldova: Inima de Vin',              N'Un portret al viticulturii moldovenești — oameni, podgorii și tradiții care fac din Moldova unul dintre cei mai mari producători de vin din lume.', N'Documentary', N'G',  52, N'Cultură', N'Nicolae Jelescu',    1, 4.80, 1, N'https://images.unsplash.com/photo-1510812431401-41d2bd2722f3?w=300&h=450&fit=crop'),
(N'Chișinău — Memorii',                 N'Mărturii ale locuitorilor bătrâni ai Chișinăului și arhivă vizuală rară care reconturează istoria orașului de-a lungul unui secol de schimbări.',  N'Documentary', N'G',  48, N'Istorie', N'Mihai Cimpoi',       1, 4.53, 1, N'https://images.unsplash.com/photo-1486325212027-8081e485255e?w=300&h=450&fit=crop'),
(N'Codrii Moldovei',                    N'Pădurile centrale ale Moldovei — Codrii — sunt explorate prin obiectivul biologilor și pădurarilor. De la stejarii seculari la fauna sălbatică.',     N'Documentary', N'G',  58, N'Natură',  N'Valeriu Munteanu',   1, 4.50, 1, NULL),
(N'Ștefan cel Mare — Legenda Moldovei', N'Recompunere documentară a vieții marelui domnitor Ștefan III al Moldovei, învingător în 34 de bătălii și ctitor a peste 40 de mănăstiri.',           N'Documentary', N'PG', 72, N'Istorie', N'Ion Țurcanu',        1, 4.80, 1, NULL),
(N'Mănăstirile Basarabiei',             N'Călătorie prin mănăstirile rupestre și de zid ale Moldovei — Țipova, Saharna, Curchi, Căpriana. Spiritualitate, arhitectură și istorie.',            N'Documentary', N'G',  64, N'Cultură', N'Mihai Cimpoi',       1, 4.40, 1, NULL),
-- ── Documentare internaționale (1) ──────────────────────────────────
(N'Our Planet',                          N'Documentar epic Netflix despre frumusețea și fragilitatea planetei. De la junglele tropicale la deșerturile reci, narat de David Attenborough.',     N'Documentary', N'G',  50, N'Natură',  N'David Attenborough', 1, 4.93, 0, NULL);
GO

-- =====================================================================
-- DATE — INSERĂRI ÎN MoldovanContent
-- (populat AUTOMAT din celelalte tabele unde IsMoldovan = 1)
-- =====================================================================
INSERT INTO dbo.MoldovanContent (ContentType, Title, Description, Genre, ContentRating, DurationMinutes, AuthorOrCreator, AverageRating, Region, PosterUrl, OriginalRefId)
SELECT N'Movie',       Title, Description, Genre, ContentRating, DurationMinutes,                Director, AverageRating, N'Moldova', PosterUrl, MovieId
FROM   dbo.Movies        WHERE IsMoldovan = 1
UNION ALL
SELECT N'Series',      Title, Description, Genre, ContentRating, SeasonsCount * EpisodesCount * EpisodeDuration, Creator,  AverageRating, N'Moldova', PosterUrl, SeriesId
FROM   dbo.Series        WHERE IsMoldovan = 1
UNION ALL
SELECT N'Documentary', Title, Description, Genre, ContentRating, DurationMinutes,                Narrator, AverageRating, N'Moldova', PosterUrl, DocumentaryId
FROM   dbo.Documentaries WHERE IsMoldovan = 1;
GO

-- =====================================================================
-- VERIFICARE — Numără și afișează rezultate
-- =====================================================================
SELECT 'Movies'           AS Tabel, COUNT(*) AS Total, SUM(CAST(IsMoldovan AS INT)) AS Moldovenesti FROM dbo.Movies
UNION ALL SELECT 'Series',          COUNT(*),          SUM(CAST(IsMoldovan AS INT))                FROM dbo.Series
UNION ALL SELECT 'Documentaries',   COUNT(*),          SUM(CAST(IsMoldovan AS INT))                FROM dbo.Documentaries
UNION ALL SELECT 'MoldovanContent', COUNT(*),          COUNT(*)                                    FROM dbo.MoldovanContent;
GO

SELECT ContentType, COUNT(*) AS Cate FROM dbo.MoldovanContent GROUP BY ContentType ORDER BY ContentType;
GO

PRINT '✓ Tabele create și populate cu succes.';
GO
