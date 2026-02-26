using dbproba;  
using Microsoft.EntityFrameworkCore;

// ===============================================================================
// ENTITY FRAMEWORK CORE 
// ===============================================================================
// Teljes CRUD (Create, Read, Update, Delete) műveleti példa
// Entity Framework Core használatával SQL Server LocalDB adatbázison.
// ===============================================================================

Console.WriteLine("Entity Framework Core - Könyvek");

// ===============================================================================
// 1. ADATBÁZIS KONTEXTUS LÉTREHOZÁSA
// ===============================================================================
// A "using var" egy C# 8.0-tól elérhető funkció, amely automatikus erőforrás-kezelést biztosít.
// 
// FONTOS MŰKÖDÉS:
// - A "using" blokk végén automatikusan meghívja a Dispose() metódust
// - A Dispose() lezárja az adatbázis-kapcsolatot és felszabadítja az erőforrásokat
// - Az érvényességi kör után (a fájl végén) a cn objektum már NEM használható
// 
// MIÉRT HASZNOS:
// - Elkerüli a memóriaszivárgást
// - Biztosítja, hogy a kapcsolat mindig lezáródik, még kivétel esetén is
// - Nem kell manuálisan meghívni a Close() vagy Dispose() metódusokat
using var cn = new cnModel();

// ===============================================================================
// 2. ADATBÁZIS SÉMA LÉTREHOZÁSA
// ===============================================================================
// Az EnsureCreated() metódus:
// - Ellenőrzi, hogy létezik-e az adatbázis
// - Ha NEM létezik, létrehozza az adatbázist és a táblákat a modell osztályok alapján
// - Ha már létezik, NEM csinál semmit (nem frissíti a sémát!)
cn.Database.EnsureCreated();

// ===============================================================================
// 3. KEZDETI ADATOK BESZÚRÁSA (SEED DATA)
// ===============================================================================
// Az Any() metódus ellenőrzi, hogy van-e már bármilyen rekord a táblában
// Ha üres a tábla (Any() = false), akkor feltöltjük kezdeti adatokkal
if (!cn.Könyvek.Any())
{
  try
  {
    // ---------------------------------------------------------------------------
    // ADATOK HOZZÁADÁSA A CHANGE TRACKER-HEZ
    // ---------------------------------------------------------------------------
    // FONTOS: Az Add() metódus NEM ír azonnal az adatbázisba!
    // Csak a memóriában lévő "change tracker"-be kerülnek az entitások.
    // 
    // A change tracker:
    // - Nyomon követi az entitások változásait
    // - A SaveChanges() hívásakor generálja az INSERT SQL utasításokat
    // - Lehetővé teszi a batch műveletet (egyszerre több INSERT)
    
    // Magyar klasszikus irodalmi művek hozzáadása
    cn.Könyvek.Add(new Könyv { Cím = "Egri csillagok", Szerzők = "Gárdonyi Géza", KiadásÉve = 1901 });
    cn.Könyvek.Add(new Könyv { Cím = "Az ember tragédiája", Szerzők = "Madách Imre", KiadásÉve = 1862 });
    cn.Könyvek.Add(new Könyv { Cím = "Tüskevár", Szerzők = "Fekete István", KiadásÉve = 1957 });
    cn.Könyvek.Add(new Könyv { Cím = "A Pál utcai fiúk", Szerzők = "Molnár Ferenc", KiadásÉve = 1906 });
    cn.Könyvek.Add(new Könyv { Cím = "Abigél", Szerzők = "Szabó Magda", KiadásÉve = 1970 });
    cn.Könyvek.Add(new Könyv { Cím = "A kőszívű ember fiai", Szerzők = "Jókai Mór", KiadásÉve = 1869 });
    cn.Könyvek.Add(new Könyv { Cím = "Légy jó mindhalálig", Szerzők = "Móricz Zsigmond", KiadásÉve = 1920 });
    
    // ---------------------------------------------------------------------------
    // VÁLTOZÁSOK MENTÉSE AZ ADATBÁZISBA
    // ---------------------------------------------------------------------------
    // A SaveChanges() metódus:
    // 1. Végignézi a change tracker-ben lévő összes változást
    // 2. Generál SQL utasításokat (INSERT, UPDATE, DELETE)
    // 3. Tranzakcióban végrehajtja őket
    // 4. Ha minden sikeres, commit-ol, különben rollback
    // 5. Visszaadja az érintett sorok számát
    // 
    // ANALÓGIA: Mint a bevásárlókosár - az Add() beteszi a termékeket,
    // a SaveChanges() pedig kifizeti őket a pénztárnál.
    cn.SaveChanges();
    Console.WriteLine("Adatok sikeresen mentve az adatbázisba.");
  }
  catch (Exception ex)
  {
    // Hibakezelés: Ha bármilyen hiba történik (pl. kapcsolódási probléma, séma hiba)
    // kiírjuk a hibaüzenetet és kilépünk a programból
    Console.WriteLine($"Hiba történt az adatok mentése során: {ex.Message}");
    return; // Kilépés a Main metódusból
  }
}

// ===============================================================================
// 4. ÖSSZES ADAT LEKÉRDEZÉSE ÉS MEGJELENÍTÉSE (READ - CRUD)
// ===============================================================================
// A ToList() metódus:
// - SQL SELECT * FROM Könyvek utasítást generál és végrehajtja
// - Az összes rekordot memóriába tölti egy List<Könyv> kollekcióba
// - Ez az "eager loading" - minden adat azonnal betöltődik
// 
// FIGYELEM: Nagy táblák esetén (pl. 100,000+ rekord) ez lassú lehet!
// Ilyenkor használj:
// - Take() és Skip() metódusokat lapozáshoz
// - Where() szűrést csak a szükséges adatok lekérdezéséhez
var könyvek = cn.Könyvek.ToList();

Console.WriteLine("\nKönyvek a könyvtárban:");
// Végigiterálunk az összes könyvön és kiírjuk az adataikat formázott szöveggel
foreach (var könyv in könyvek)
{
  // String interpoláció ($"...") használata a szép formázáshoz
  Console.WriteLine($"Cím: {könyv.Cím}, Szerzők: {könyv.Szerzők}, Kiadás éve: {könyv.KiadásÉve}");
}

// ===============================================================================
// 5. SZŰRT LEKÉRDEZÉS LINQ HASZNÁLATÁVAL (READ - SZŰRÉS)
// ===============================================================================
// LINQ (Language Integrated Query) - a C# nyelv része
// Lehetővé teszi adatbázis-lekérdezések írását C# szintaxissal
// 
// A Where() metódus működése:
// 1. Lambda kifejezést vár paraméterként: k => k.KiadásÉve < 1950
//    - "k" a lambda paraméter (egy Könyv objektum)
//    - "k.KiadásÉve < 1950" a szűrési feltétel
// 2. EF Core SQL-re fordítja: WHERE KiadásÉve < 1950
// 3. A ToList() végrehajtja a lekérdezést (lazy evaluation előtt csak expression tree!)
// 
// MEGJEGYZÉS: A komment "1900 előtti" könyveket ír, de a kód 1950-et használ!
// Ez valószínűleg elírás - a kommentet vagy a kódot javítani kellene.
var régiek = cn.Könyvek
               .Where(k => k.KiadásÉve < 1950)
               .ToList();

Console.WriteLine("\n1900 előtti könyvek a könyvtárban:");
foreach (var könyv in régiek)
{
  Console.WriteLine($"Cím: {könyv.Cím}, Szerzők: {könyv.Szerzők}, Kiadás éve: {könyv.KiadásÉve}");
}

// ===============================================================================
// 6. REKORD MÓDOSÍTÁSA (UPDATE - CRUD)
// ===============================================================================
// A FirstOrDefault() metódus:
// - Visszaadja az első egyező elemet, vagy null-t ha nincs találat
// - SQL: SELECT TOP 1 * FROM Könyvek WHERE Cím = 'Abigél'
// - ALTERNATÍVA: First() - kivételt dob ha nincs találat (kockázatosabb)
// - ALTERNATÍVA: Single() - kivételt dob ha több találat van (szigorúbb)
var konyv = cn.Könyvek.FirstOrDefault(k => k.Cím == "Abigél");

// Null ellenőrzés - kötelező a NullReferenceException elkerülésére!
if (konyv != null)
{
  // ---------------------------------------------------------------------------
  // AUTOMATIKUS VÁLTOZÁS KÖVETÉS
  // ---------------------------------------------------------------------------
  // FONTOS: Nem kell külön "Update()" metódust hívni!
  // Az EF Core automatikusan figyeli az entitás tulajdonságainak változásait.
  // 
  // Amikor meghívjuk a SaveChanges()-t:
  // 1. EF Core észreveszi, hogy a KiadásÉve tulajdonság megváltozott
  // 2. Generál egy UPDATE SQL utasítást
  // 3. Csak a módosult mezőket frissíti az adatbázisban
  konyv.KiadásÉve = 2024;
  
  // A SaveChanges() most egy UPDATE SQL-t generál és hajt végre
  cn.SaveChanges();
  Console.WriteLine($"\nKönyv frissítve: {konyv.Cím}, Új kiadás éve: {konyv.KiadásÉve}");
}
else
{
  // Ha nem találtuk meg a könyvet, tájékoztatjuk a felhasználót
  Console.WriteLine("\nAz Abigél című könyv nem található.");
}

// ===============================================================================
// 7. REKORD TÖRLÉSE (DELETE - CRUD)
// ===============================================================================
// Ismét FirstOrDefault()-ot használunk a törlendő rekord megkereséséhez
var törlendő = cn.Könyvek.FirstOrDefault(k => k.Cím == "A kőszívű ember fiai");

if (törlendő != null)
{
  // ---------------------------------------------------------------------------
  // TÖRLÉS MŰVELETE
  // ---------------------------------------------------------------------------
  // A Remove() metódus:
  // - Megjelöli az entitást törlésre a change tracker-ben
  // - NEM törli azonnal az adatbázisból!
  // - A SaveChanges() híváskor generálódik a DELETE SQL utasítás
  // 
  // SQL ekvivalens: DELETE FROM Könyvek WHERE Id = <törlendő.Id>
  cn.Könyvek.Remove(törlendő);
  
  // Most ténylegesen végrehajtjuk a törlést az adatbázisban
  cn.SaveChanges();
  Console.WriteLine($"\nKönyv törölve: {törlendő.Cím}");
}
else
{
  Console.WriteLine("\nA 'A kőszívű ember fiai' című könyv nem található.");
}

// ===============================================================================
// 8. ADATBÁZIS LEVÁLASZTÁSA SQL SERVER-RŐL (SPECIÁLIS MŰVELET)
// ===============================================================================
// Ez egy haladó funkció, amely SQL Server LocalDB-vel használatos.
// 
// MIÉRT VAN RÁ SZÜKSÉG:
// - SQL Server LocalDB zárolva tartja az adatbázisfájlokat (.mdf, .ldf)
// - Amíg csatlakozik hozzá, nem lehet átnevezni, áthelyezni vagy törölni őket
// - A leválasztás (detach) feloldja ezt a zárolást
// 
// HASZNÁLATI ESETEK:
// - Adatbázis fájlok biztonsági mentése
// - Fájlok áthelyezése másik helyre
// - Fájlok átnevezése
// - Adatbázis másolása másik szerverre
try
{
    // ---------------------------------------------------------------------------
    // 8.1. AKTÍV KAPCSOLAT LEZÁRÁSA
    // ---------------------------------------------------------------------------
    // Először le kell zárni az EF Core kapcsolatot
    // Különben az adatbázis továbbra is használatban van
    cn.Database.GetDbConnection().Close();
    
    // ---------------------------------------------------------------------------
    // 8.2. ADATBÁZIS NEVÉNEK LEKÉRDEZÉSE
    // ---------------------------------------------------------------------------
    // A connection string-ből kiolvassuk az adatbázis nevét
    // Ezt fogjuk használni a master adatbázis parancsokban
    var db = cn.Database.GetDbConnection().Database;
    
    // ---------------------------------------------------------------------------
    // 8.3. MASTER ADATBÁZISHOZ KAPCSOLÓDÁS
    // ---------------------------------------------------------------------------
    // A "master" egy rendszer-adatbázis SQL Server-ben
    // Ezen keresztül lehet adatbázis-szintű műveleteket végrehajtani (create, drop, detach)
    // 
    // A connection string módosítása:
    // - Az eredeti connection string-et vesszük
    // - Lecseréljük benne a Database paramétert "master"-re
    var mastercs = cn.Database.GetDbConnection()
        .ConnectionString.Replace($"Database={db}", "Database=master");
    
    // Új kapcsolat létrehozása a master adatbázishoz
    using var masterconn = new Microsoft.Data.SqlClient.SqlConnection(mastercs);
    masterconn.Open();
    
    // ---------------------------------------------------------------------------
    // 8.4. ADATBÁZIS SINGLE-USER MÓDBA ÁLLÍTÁSA
    // ---------------------------------------------------------------------------
    // Ez kényszerítően lezár minden más aktív kapcsolatot az adatbázishoz
    // 
    // ALTER DATABASE paraméterek:
    // - SET SINGLE_USER: Egyfelhasználós módba kapcsol
    // - WITH ROLLBACK IMMEDIATE: Azonnal megszakít minden aktív tranzakciót
    // 
    // FIGYELEM: Ez destructív művelet! Minden más felhasználó kiesik!
    using (var cmd = masterconn.CreateCommand())
    {
        cmd.CommandText = $"ALTER DATABASE [{db}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
        cmd.ExecuteNonQuery(); // SQL parancs végrehajtása
    }
    
    // ---------------------------------------------------------------------------
    // 8.5. ADATBÁZIS LEVÁLASZTÁSA
    // ---------------------------------------------------------------------------
    // Az sp_detach_db egy rendszer tárolt eljárás (stored procedure)
    // Leválasztja az adatbázist az SQL Server példányról
    // 
    // EREDMÉNY:
    // - Az .mdf és .ldf fájlok felszabadulnak
    // - Szabadon mozgathatók, átnevezhetők, törölhetők
    // - Az adatbázis nem lesz elérhető, amíg újra nem csatolod (sp_attach_db)
    using (var cmd = masterconn.CreateCommand())
    {
        cmd.CommandText = $"EXEC sp_detach_db '{db}'";
        cmd.ExecuteNonQuery();
    }
    
    Console.WriteLine($"\nAdatbázis '{db}' sikeresen leválasztva. A fájlok szabadon mozgathatók.");
}
catch (Exception ex)
{
    // Hibakezelés - ha bármilyen hiba történik a leválasztás során
    // (pl. másik alkalmazás használja az adatbázist, nincs jogosultság, stb.)
    Console.WriteLine($"\nHiba az adatbázis leválasztása során: {ex.Message}");
}

// ===============================================================================
// PROGRAM VÉGE
// ===============================================================================
// A using var cn automatikusan meghívja itt a Dispose()-t
// A kapcsolat lezáródik és az erőforrások felszabadulnak


