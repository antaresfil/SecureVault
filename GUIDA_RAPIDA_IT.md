# 🚀 Guida Rapida - SecureVault

## Installazione

### Prerequisiti
1. **Windows 10/11** (64-bit)
2. **.NET 8 Runtime** - Scarica da: https://dotnet.microsoft.com/download/dotnet/8.0

### Compilazione

```bash
# Apri il prompt dei comandi nella cartella SecureVault
cd SecureVault

# Compila il progetto
dotnet build --configuration Release

# Esegui l'applicazione
dotnet run
```

Oppure usa il file `build.bat` per una compilazione automatica.

## Primo Utilizzo

### 1. Crittografare un File

**Passo 1**: Apri SecureVault.exe

**Passo 2**: Clicca su "Browse..." e seleziona il file da crittografare

**Passo 3**: Scegli i fattori di autenticazione:

#### ✅ Password (Consigliato per iniziare)
- Spunta "Use Password"
- Inserisci una password forte (minimo 12 caratteri)
- **IMPORTANTE**: Ricorda questa password! Non può essere recuperata!

#### 📱 TOTP - Opzionale (per sicurezza extra)
- Spunta "Use TOTP"
- Clicca "Generate New" per creare un segreto
- Installa Google Authenticator sul telefono
- Aggiungi l'account inserendo il segreto manualmente
- **SALVA IL SEGRETO** in un posto sicuro (password manager)
- Inserisci il codice a 6 cifre dall'app

#### 🔑 YubiKey - Opzionale (massima sicurezza)
- Inserisci YubiKey nella porta USB
- Spunta "Use YubiKey"
- Clicca "Detect YubiKey"
- Verifica che sia rilevato

**Passo 4**: Clicca "🔒 Execute"

**Risultato**: Verrà creato un file `.svlt` crittografato

### 2. Decrittografare un File

**Passo 1**: Seleziona il file `.svlt`

**Passo 2**: Inserisci gli **STESSI** fattori di autenticazione usati per crittografare:
- Stessa password
- Stesso segreto TOTP + codice attuale
- Stessa YubiKey

**Passo 3**: Seleziona "Decrypt File"

**Passo 4**: Clicca "🔒 Execute"

**Risultato**: Il file originale verrà ripristinato

## 🔐 Sicurezza

### Livelli di Sicurezza Consigliati

**File Normali**:
- ✅ Solo Password

**File Sensibili**:
- ✅ Password + TOTP

**File Critici**:
- ✅ Password + TOTP + YubiKey

### ⚠️ Avvisi Importanti

1. **Non perdere le credenziali!**
   - Se perdi password/TOTP/YubiKey, il file è **IRRECUPERABILE**
   - Salva tutto in un password manager

2. **Testa sempre la decrittografia**
   - Dopo aver crittografato, prova subito a decrittografare
   - Verifica che tutto funzioni

3. **Backup delle credenziali**
   - Salva il segreto TOTP
   - Annota quale password hai usato
   - Tieni una YubiKey di backup (opzionale)

## 🛠️ Risoluzione Problemi

### "YubiKey Non Rilevato"
- Verifica che YubiKey sia inserita
- Prova una porta USB diversa
- Installa YubiKey Manager

### "Codice TOTP Invalido"
- Verifica che l'ora del PC sia corretta
- Aspetta il prossimo codice (30 secondi)
- Controlla di aver usato il segreto corretto

### "Decrittografia Fallita"
- Hai usato gli stessi fattori di autenticazione?
- Password corretta? (maiuscole/minuscole contano)
- Stesso segreto TOTP?
- Stessa YubiKey?

## 📊 Caratteristiche Tecniche

- **Crittografia**: AES-256-GCM (grado militare)
- **Key Derivation**: Argon2id (standard OWASP)
- **TOTP**: RFC 6238 (compatibile con Google Authenticator)
- **Cancellazione Sicura**: Sovrascrittura 3-pass

## 📚 Risorse

- `README.md` - Documentazione completa in inglese
- `SECURITY_WHITEPAPER.md` - Dettagli tecnici di sicurezza
- Setup Guide in app - Clicca "📖 Setup Guide"

## 💡 Esempio Pratico

### Scenario: Crittografare documenti personali

```
1. File: documenti_personali.pdf
2. Fattori: Password + TOTP
3. Password: "MiaPassword2024!Sicura"
4. TOTP: Generato e salvato in LastPass
5. Output: documenti_personali.pdf.svlt
6. Originale: Cancellato in modo sicuro (opzionale)

Per decrittografare:
- Stessa password: "MiaPassword2024!Sicura"
- Codice TOTP attuale da Google Authenticator
```

## ✅ Checklist Prima di Usare

- [ ] .NET 8 installato
- [ ] Password manager pronto (per salvare credenziali)
- [ ] Google Authenticator installato (se usi TOTP)
- [ ] YubiKey configurata (se usi hardware token)
- [ ] Backup delle credenziali pianificato
- [ ] Test di crittografia/decrittografia eseguito

## 🎯 Best Practice

1. **Inizia Semplice**: Prova prima con solo password
2. **Aggiungi TOTP**: Per file importanti
3. **YubiKey**: Solo per file critici
4. **Testa Sempre**: Decrittografa subito dopo crittografare
5. **Backup**: Salva tutte le credenziali
6. **Mai Condividere**: Non dare le tue credenziali a nessuno

---

**Hai bisogno di aiuto?** Consulta `README.md` per la documentazione completa.

**Buona crittografia! 🔒**
