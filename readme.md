# Általános információk

---

A feladat kiírásnak megfelelő megvalósításával 10 pontot lehet szerezni.
A feladat célja a C#-{I, II, III} óra anyagainak gyakorlása és közelebbi ismerettség szerzése a fejlesztőkörnyezettel.

A megoldást tömörítve a Coospace-re kell feltölteni **NEPTUN.zip** formátumban az 1. beadandóhoz létrehozott beadási felületen, mindenkinek a saját gyakorlati színterébe.
Az archívum tartalmazza az egész Solution-t (.sln és .csproj) fájlokat is, azaz kicsomagolás után betölthető és futtatható legyen Visual Studioban.
A `bin` és `obj` mappák a tömörítés előtt törlendők, a kiadott CSV-ket ne tartalmazza.

A felhasznált .NET verzió .NET 6!
**A beadott megoldásnak fordulnia és futnia kell.**
A projekt mappában a `dotnet run <argumentumok>` parancs hatására induljon el az alkalmazás.
Ellenkező esetben a projektmunkára automatikusan 0 pont jár.

Részpontok léteznek, azaz ha nem sikerül valamely feladatot teljes mértékben megoldani, attól nem kell kitörölni (persze forduljon-fusson a projekt).

Esetleges megjegyzések egy readme.md fájlba kerüljenek a .sln fájl mellé.

Amennyiben a feladat másképp nem szól:

---

A CSV-k a [Kaggleről](https://www.kaggle.com/) is származhatnak, tartalmazhatnak hibákat.
A projekt munka célja nem a CSV-k hibáinak kézzel történő javítása, így arra nem jár pont.
A kiértékelés a kiadott CSV-kkel törétnik az alábbi módon.
Parancssori argumentumokként várja az alkalmazás a CSV-k elérési útvonalait.

```csahrp
static void Main(string[] args) {
  foreach (var arg in args)
    Console.WriteLine(arg); // 1 2 3
}
// dotnet run 1 2 3
```

Amennyiben a feladat úgy kéri, a program pontosan annyi CSV-t várjon amennyit a feladat említ, minden más esetben hibát jelezzen
(pl. ha 1 csv van akkor csak 1 csv-t lehessen megadni, 0, vagy 2 esetén hiba). Az evvel kapcsolatos elvárások változhatnak, amennyiben a feladat ezt explicit módon kéri.

---

Visual Studioban is beállíthatók a parancssori argumentumok a következő beállítások segítségével:

- Solution Explorer-ben jobb kattintás a projekt nevére, `Properties`
- `Debug` fül balról
- `Application arguments`-be kell beleírni szóközzel elválasztva.

## Hasznos hivatkozások

- [How to read from a text file](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-read-from-a-text-file)
- [How to write to a text file](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-write-to-a-text-file)


# 1. Projektmunka

---

# Password Manager

A feledat egy jelszó kezelő alkalmazás létrehozása.
Mivel a gyakorlat anyaga nem terjed ki egy biztonságosnak tekinthető szoftver készítésére,
ezért itt most megelégszünk egy naív megoldással is.

A programunk különböző felhasználók adatainak tárolására,
valamint említett felhasználók jelszavainak tárolására kell alkalmas legyen.

Az adatok tárolása csv fájlok használatával történik,
de nyugodtan használhatunk tetszőleges adatbázist is a megoldás során,
annyi kitétellel, hogy a választott *db* binárisa is elérhető kell legyen a
*NuGet Package Manager* segítségével (pl. SQLite).<br />
A .csv fájlokra példát a [resources/db](./resources/db) mappa alatt találhatsz:
 - felhasználók [users.csv](./resources/db/users.csv)
 - tárolt jelszavak [vault.csv](./resources/db/vault.csv)


## Kritériumok

Egyik kritérium, hogy szenzitív adatok (pl. jelszavak) ne kerülhessenek *plain text* formátumban az adatbázisba.

 - A felhasználói jelszavakat egy megfelelő *hash* függvénnyel kell titkosítani.
 - A tárolt jelszavakat (*vault entries*) pedig enkriptált formában kell tárolni úgy,
   hogy azok csak az adott felhasználó számára legyenek visszafejthetők.

A fentiek megvalósításához ajánlott beépített metódusok, valamint meglévő *package*-ek használata.

Egy tárolt jelszó az órán látottak alapján legyen összekapcsolva a felhasználóval olyan módon,
hogy maga a kapcsolt felhasználó is lekérhető legyen az adott jelszótól,
ám ezen adattag ne legyen része a jelszavakat tároló csv-nek.
A kapcsolás a fő-felhasználónév alapján történjen (a kapcsolatot a UserId mező biztosítja)!

Rövid példa:

```cs
class User
{
    ...
    public string Username { get; set; }
    ...
}

class VaultEntry
{
    ...
    // refers to Username in User
    public string UserId { get; set; }
    public User User
    {
        get
        {
            // get user by UserId from the database
        }
    }
    ...
}
```

## Hasznos linkek:
 - https://riptutorial.com/csharp/example/9345/sha512
 - https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.sha512?view=net-6.0
 - https://github.com/daverayment/Cryptography.Fernet/

A példa csv-ben látható vault entry jelszavak a következő "naív" metódusokkal kódolhatóak, illetve fejthetők vissza,
azonban a csv fájlok, csak szemléltető jellegűek, saját titkosító eljárás is elfogadott.

```cs
class EncryptedType
{
    ...
    ...Key  // user email
    ...Secret  // data to be encrypted
    ...

    EncryptedType Encrypt()
    {
        using var hashing = SHA256.Create();
        byte[] keyHash = hashing.ComputeHash(Encoding.Unicode.GetBytes(Key));
        string key = Base64UrlEncoder.Encode(keyHash);
        string message = Base64UrlEncoder.Encode(Encoding.Unicode.GetBytes(Secret));
        return new(Key, Fernet.Encrypt(key, message));
    }
    
    EncryptedType Decrypt()
    {
        using var hashing = SHA256.Create();
        byte[] keyHash = hashing.ComputeHash(Encoding.Unicode.GetBytes(Key));
        string key = Base64UrlEncoder.Encode(keyHash);
        string encodedSecret = Fernet.Decrypt(key, Secret);
        string message = Encoding.Unicode.GetString(Base64UrlEncoder.DecodeBytes(encodedSecret));
        return new(Key, message);
    }
}
```

Ezen kívül a felhasználói jelszavakat (mester jelszavakat) SHA512 algoritmussal hasheltük,
valamint a kapott bytokat base64 url-safe módon kódoltuk.

## Feladatok

A feladatok megoldása során relatív útvonalakkal dolgozzunk, azaz az útvonalak a jelenlegi `workdir`-hez képest legyenek megadva.
Ez a `workdir` legyen állítható, egy `--workdir=<my/path/to/resources>` parancssori argumentummal.

Fogadjon el továbbá az allkalmazás egyéb parancsokat is, mint pl.
 - `register`: új felhasználót regisztrálhatunk vele,
 - `list`: adott user tárolt jelszavainak listázására legyen alkalmas,
 amihez persze authentikálni kell az adott felhasználót,
 például annak felhasználónevének és jelszavának megkérdezésével,
 avagy ugyanezt megtenni két argumentummal `--username=<user> --password=<pw>`.
 Más esetben a program utasítsaa el a jelszavak listázását!

Ehhez egy kiváló, ám egyelőre *beta* verzióban lévő package nyújthat segítséget
[CommandLine](https://learn.microsoft.com/en-us/dotnet/standard/commandline/get-started-tutorial),
melynek használata opcionális.

## Note

Inkább mentsük a jelszavakat plain-text formában, mint sehogy!
A fő szempont egy működő alkalmazás, a lényegi rész nem a titkosításban van.
