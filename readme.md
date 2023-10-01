# Általános információk

Az alkalmazás elmenti a bejelentkezett felhasználót és a beállított workdirt a config.json fileba

---

# Parancsok

- dotnet run --workdir (csv-ket tartalmazó mappa útvonala \\-vel a végén)
- dotnet run login --username=username --password=password
- dotnet run register --username=username --password=password --email=email --firstname=firstname --lastname=lastname
- dotnet run logout
- dotnet run list
- dotnet run add --username=username --password=password --website=website