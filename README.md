# 🏢 BusinessRoomBooking

**Un système de gestion de réservation de salles de réunion pour entreprise.**

Backend API en ASP.NET Core (.NET 10) permettant de gérer des salles, des employés, des équipements et des réservations, avec une architecture en couches et une couverture de tests unitaires.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![xUnit](https://img.shields.io/badge/tests-xUnit-informational)
![License](https://img.shields.io/badge/license-non%20d%C3%A9finie-lightgrey)

---

## Sommaire

- [Présentation](#présentation)
- [Fonctionnalités](#fonctionnalités)
- [Captures d'écran](#captures-décran)
- [Démonstration](#démonstration)
- [Architecture](#architecture)
- [Arborescence du projet](#arborescence-du-projet)
- [Technologies](#technologies)
- [Packages NuGet](#packages-nuget)
- [Prérequis](#prérequis)
- [Installation](#installation)
- [Variables d'environnement](#variables-denvironnement)
- [Configuration](#configuration)
- [Utilisation](#utilisation)
- [Documentation API](#documentation-api)
- [Base de données](#base-de-données)
- [Qualité du code](#qualité-du-code)
- [Tests](#tests)
- [Docker](#docker)
- [GitHub Actions / CI-CD](#github-actions--ci-cd)
- [Déploiement](#déploiement)
- [Performance](#performance)
- [Sécurité](#sécurité)
- [Roadmap](#roadmap)
- [Bonnes pratiques](#bonnes-pratiques)
- [Contribution](#contribution)
- [Versioning](#versioning)
- [Licence](#licence)
- [Auteur](#auteur)
- [Remerciements](#remerciements)

---

## Présentation

**BusinessRoomBooking** est une API backend destinée à gérer la réservation de salles de réunion au sein d'une entreprise. Elle permet de :

- gérer un catalogue de **salles** (nom, capacité maximale, localisation) ;
- gérer les **employés** (`Worker`) susceptibles de réserver une salle ;
- gérer des **équipements** (`Equipment`) et les associer à des salles ;
- créer, consulter, modifier et annuler des **réservations** (`Booking`), en garantissant l'absence de chevauchement horaire et le respect de la capacité de la salle.

Le projet s'adresse aux équipes qui souhaitent disposer d'un backend robuste pour un outil interne de gestion de salles, consommable par un frontend séparé (les entêtes CORS indiquent un client Angular, voir [Sécurité](#sécurité)).

---

## Fonctionnalités

### Gestion des salles (`Room`)
- Création, consultation d'une salle (nom, capacité maximale, localisation)
- Recherche des salles disponibles
- Association d'équipements à une salle

### Gestion des employés (`Worker`)
- Création et consultation d'un employé (nom, prénom, email)
- Contrôle d'unicité de l'email (exception métier dédiée)

### Gestion des équipements (`Equipment` / `RoomEquipment`)
- Association d'un équipement à une salle (relation many-to-many via `RoomEquipment`)
- Contrôle des doublons d'affectation

### Gestion des réservations (`Booking`)
- Création d'une réservation (salle, employé, date de début/fin, nombre de participants)
- Contrôle des chevauchements de réservation sur une même salle
- Contrôle du nombre de participants par rapport à la capacité de la salle
- Contrôle des dates déjà passées
- Consultation des réservations à venir par salle
- Annulation / mise à jour de réservation

### Transverse
- Gestion centralisée des erreurs métier via un middleware d'exception (`ExceptionMiddleware`) traduisant les exceptions de domaine en codes HTTP (400 / 404 / 409)
- Documentation OpenAPI générée nativement par ASP.NET Core

> Cette liste est déduite des services, exceptions et contrôleurs présents dans le code (`BusinessRoomBooking.Core`, `BusinessRoomBooking.API`). Les libellés exacts des endpoints sont détaillés dans [Utilisation](#utilisation).

---

## Captures d'écran

<!-- TODO: aucune capture d'écran n'est présente dans le dépôt actuellement. -->
Aucune image ni capture d'écran n'a été trouvée dans le dépôt. À ajouter ici une fois l'interface ou la documentation Swagger disponible.

---

## Démonstration

<!-- TODO: aucune démo hébergée détectée dans le dépôt -->
Aucune URL de démonstration, environnement de staging ou frontend public n'est référencé dans ce dépôt.

- **API** : exécutable localement, voir [Installation](#installation) et [Utilisation](#utilisation).
- **Frontend** : non présent dans ce dépôt (la politique CORS `AllowAngularDev` suggère un client Angular développé séparément, consommant cette API sur `http://localhost:4200`).

---

## Architecture

Le projet suit une **architecture en couches** proche d'une Clean/Onion Architecture simplifiée, avec séparation stricte des responsabilités entre projets .NET :

```
┌─────────────────────────────────────────────┐
│           BusinessRoomBooking.API             │  Contrôleurs, Middleware, DI, OpenAPI
│  (Controllers, ExceptionMiddleware, Program)  │
└───────────────────┬───────────────────────────┘
                     │ dépend de
┌───────────────────▼───────────────────────────┐
│          BusinessRoomBooking.Core              │  Application : DTOs, Mappers,
│  (Services, Mappers, DTOs, Interfaces)         │  Services métier, interfaces Repository
└───────────────────┬───────────────────────────┘
                     │ dépend de
┌───────────────────▼───────────────────────────┐
│        BusinessRoomBooking.Infrastructure      │  EF Core DbContext, Repositories,
│  (DbContext, Repositories, Migrations)         │  Migrations, accès SQL Server
└───────────────────┬───────────────────────────┘
                     │ dépend de
┌───────────────────▼───────────────────────────┐
│          BusinessRoomBooking.Domain            │  Entités métier pures (POCO)
│  (Room, Worker, Booking, Equipment, ...)       │  aucune dépendance externe
└─────────────────────────────────────────────────┘

┌─────────────────────────────────────────────┐
│           BusinessRoomBooking.Tests           │  xUnit + NSubstitute
│  (Controllers, Mappers, Services tests)       │  teste API + Core
└─────────────────────────────────────────────────┘
```

**Patterns identifiés dans le code :**

| Pattern | Où | Détail |
| --- | --- | --- |
| Repository générique | `Infrastructure/Repositories/BaseRepository.cs` | Repository de base générique, spécialisé par entité (`RoomRepository`, `BookingRepository`, `WorkerRepository`, `RoomEquipmentRepository`) |
| Injection de dépendances | `Program.cs`, `ServiceExtensions`, `InfrastructureExtensions` | Enregistrement des services et repositories via des méthodes d'extension `AddApplicationServices()` / `AddInfrastructure()` |
| Exceptions métier dédiées | `Core/Exceptions/*` | Une hiérarchie d'exceptions par domaine (`BookingExceptions`, `EquipmentExceptions`, `RoomExceptions`, `WorkerExceptions`) traduites en codes HTTP par le middleware |
| Mappers par extension | `Core/Mappers/*` | Méthodes d'extension (ex. `room.ToRoomResponseDto()`) pour convertir entités ↔ DTOs |
| DTOs séparés par usage | `Core/Dtos/*/{Request,Response,Summaries,Projections,Queries}` | Séparation claire entre modèles d'entrée, de sortie, de résumé et de requête |

Aucun usage de CQRS/MediatR, ni de Unit of Work formalisé, ni de Result Pattern n'a été détecté : chaque repository expose directement `SaveChangesAsync`.

---

## Arborescence du projet

```text
BusinessRoomBooking.sln
BusinessRoomBooking.API/            # Point d'entrée HTTP (ASP.NET Core Web API)
  Controllers/                      # BookingController, RoomController, WorkerController
  Extensions/                       # CorsExtensions
  MiddleWare/                       # ExceptionMiddleware (mapping exceptions -> HTTP)
  Properties/launchSettings.json    # Profils de lancement (http/https)
  Program.cs                        # Bootstrap, pipeline middleware, DI
  appsettings*.json                 # Configuration (connection string, logging)

BusinessRoomBooking.Core/           # Couche Application
  Dtos/                             # DTOs par entité (Booking, Room, RoomEquipment, Worker)
  Exceptions/                       # Exceptions métier par domaine
  Extensions/ServiceExtensions.cs   # Enregistrement DI des services applicatifs
  Interfaces/                       # Interfaces Repository et Services
  Mappers/                          # Extensions de mapping entité <-> DTO
  Services/                         # BookingService, RoomService, WorkerService

BusinessRoomBooking.Domain/         # Entités métier pures (POCO), aucune dépendance
  Booking.cs, Room.cs, Worker.cs, Equipment.cs, RoomEquipment.cs

BusinessRoomBooking.Infrastructure/ # Accès aux données
  DataBase/Configuration/           # Fluent API EF Core par entité
  DataBase/Context/                 # BusinessRoomBookingContext (DbContext)
  Extensions/InfrastructureExtensions.cs
  Migrations/                       # InitialCreate, AddRoomName
  Repositories/                     # BaseRepository + repositories spécifiques

BusinessRoomBooking.Tests/          # Tests unitaires (xUnit + NSubstitute)
  Controllers/, Mappers/, Services/
```

---

## Technologies

| Technologie | Version | Usage |
| --- | --- | --- |
| .NET | 10.0 | Framework cible de tous les projets (`net10.0`) |
| ASP.NET Core | 10.0 | API Web (contrôleurs, middleware) |
| Entity Framework Core | 10.0.9 | ORM d'accès aux données |
| EF Core SqlServer | 10.0.9 | Fournisseur SQL Server pour EF Core |
| Microsoft.AspNetCore.OpenApi | 10.0.1 | Génération de la documentation OpenAPI native |
| Microsoft.OpenApi | 2.7.5 | Modèles OpenAPI sous-jacents |
| xUnit | 2.9.3 | Framework de tests unitaires |
| NSubstitute | 6.0.0 | Mocking pour les tests |
| coverlet.collector | 6.0.4 | Collecte de couverture de code |

---

## Packages NuGet

| Projet | Package | Rôle |
| --- | --- | --- |
| API | `Microsoft.AspNetCore.OpenApi` | Génère le document OpenAPI natif (`/openapi`) |
| API | `Microsoft.OpenApi` | Modèles de spécification OpenAPI |
| API | `Microsoft.EntityFrameworkCore.Design` | Outils de design-time EF Core (migrations via CLI) |
| Core | `Microsoft.Extensions.DependencyInjection.Abstractions` | Abstractions DI pour l'enregistrement des services applicatifs |
| Infrastructure | `Microsoft.EntityFrameworkCore.SqlServer` | Fournisseur SQL Server pour EF Core |
| Infrastructure | `Microsoft.EntityFrameworkCore.Tools` | Commandes `dotnet ef` (migrations, update database) |
| Tests | `xunit` / `xunit.runner.visualstudio` | Framework et runner de tests |
| Tests | `NSubstitute` | Création de mocks/substituts |
| Tests | `coverlet.collector` | Couverture de code |
| Tests | `Microsoft.NET.Test.Sdk` | SDK d'exécution des tests .NET |

---

## Prérequis

| Outil | Version | Remarque |
| --- | --- | --- |
| [.NET SDK](https://dotnet.microsoft.com/) | 10.0 | Requis pour build/run/test (cible `net10.0` de tous les projets) |
| SQL Server (ou LocalDB) | — | Base de données relationnelle utilisée par EF Core |
| Git | — | Gestion de versions |
| IDE recommandé | — | Rider ou Visual Studio (fichiers `.sln`, `.DotSettings.user` présents) |

Docker n'est pas requis : aucun `Dockerfile` n'est présent dans ce dépôt (voir [Docker](#docker)).

---

## Installation

```bash
# 1. Cloner le dépôt
git clone <url-du-depot>
cd BusinessRoomBooking

# 2. Restaurer les dépendances
dotnet restore

# 3. Compiler la solution
dotnet build
```

### Configuration de la base de données

1. Renseigner une chaîne de connexion SQL Server dans `BusinessRoomBooking.API/appsettings.Development.json` (fichier ignoré par git, voir [Configuration](#configuration)) :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=BusinessRoomBookingDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

2. Appliquer les migrations EF Core :

```bash
dotnet ef database update --project BusinessRoomBooking.Infrastructure --startup-project BusinessRoomBooking.API
```

### Lancement de l'API

```bash
dotnet run --project BusinessRoomBooking.API
```

L'API démarre selon les profils définis dans `launchSettings.json` (voir [Configuration](#configuration)).

### Frontend

Aucun frontend n'est présent dans ce dépôt.

---

## Variables d'environnement

Aucun fichier `.env` n'est utilisé ; la configuration passe par `appsettings*.json` (standard ASP.NET Core). Variables/clés détectées :

| Clé | Fichier | Détecté | Description |
| --- | --- | --- | --- |
| `ConnectionStrings:DefaultConnection` | `appsettings.json` / `appsettings.Development.json` | Oui (valeur vide dans `appsettings.json`) | Chaîne de connexion vers la base SQL Server |
| `Logging:LogLevel:Default` | `appsettings.json` | Oui (`Information`) | Niveau de log par défaut |
| `Logging:LogLevel:Microsoft.AspNetCore` | `appsettings.json` | Oui (`Warning`) | Niveau de log pour le framework |
| `AllowedHosts` | `appsettings.json` | Oui (`*`) | Hôtes autorisés |
| `ASPNETCORE_ENVIRONMENT` | `launchSettings.json` | Oui (`Development`) | Environnement d'exécution (profils http/https) |

> ⚠️ Aucune valeur secrète n'est reproduite ici. `appsettings.Development.json` est volontairement exclu du dépôt via `.gitignore` — configurez-le localement.

---

## Configuration

- **`appsettings.json`** : configuration de base (logging, hosts autorisés, connection string vide).
- **`appsettings.Development.json`** : configuration locale de développement (ignorée par Git), contient la connection string réelle vers la base locale.
- **`launchSettings.json`** (`BusinessRoomBooking.API/Properties/`) : deux profils de lancement :

| Profil | URL(s) | Environnement |
| --- | --- | --- |
| `http` | `http://localhost:5215` | Development |
| `https` | `https://localhost:7076;http://localhost:5215` | Development |

- **CORS** : une politique nommée `AllowAngularDev` (voir `Extensions/CorsExtensions.cs`) autorise l'origine `http://localhost:4200`, suggérant un client Angular développé séparément.
- **Docker / Angular environment files** : non présents dans ce dépôt.

---

## Utilisation

Exemples d'appels HTTP vers les contrôleurs exposés par l'API (voir `BusinessRoomBooking.API/Controllers/` et le fichier `BusinessRoomBooking.http` fourni pour des exemples exécutables directement depuis Rider/VS Code) :

```http
### Récupérer une salle
GET http://localhost:5215/api/room/{id}

### Créer un employé
POST http://localhost:5215/api/worker
Content-Type: application/json

{
  "firstName": "Jean",
  "lastName": "Dupont",
  "email": "jean.dupont@example.com"
}

### Créer une réservation
POST http://localhost:5215/api/booking
Content-Type: application/json

{
  "roomId": "{roomId}",
  "workerId": "{workerId}",
  "startDate": "2026-08-10T09:00:00",
  "endDate": "2026-08-10T10:00:00",
  "numberOfParticipants": 4
}
```

> Les noms exacts des routes dépendent des attributs de route définis dans `BookingControlers.cs`, `RoomController.cs` et `WorkerController.cs` — se référer au fichier `.http` du projet ou à la documentation OpenAPI (voir ci-dessous) pour la liste exhaustive et à jour.

---

## Documentation API

L'API expose une spécification **OpenAPI générée nativement par ASP.NET Core** (`AddOpenApi()` / `MapOpenApi()` dans `Program.cs`), disponible uniquement en environnement **Development**, typiquement sur `/openapi/v1.json`.

Aucun package Swagger UI (`Swashbuckle`, `NSwag`, etc.) n'est référencé : il n'y a donc pas d'interface graphique Swagger intégrée par défaut — le document OpenAPI brut peut être importé dans un outil externe (Postman, Scalar, etc.) si besoin.

---

## Base de données

| Élément | Détail |
| --- | --- |
| Moteur | SQL Server (`Microsoft.EntityFrameworkCore.SqlServer`) |
| ORM | Entity Framework Core 10.0.9 |
| DbContext | `BusinessRoomBookingContext` (`BusinessRoomBooking.Infrastructure/DataBase/Context/`) |
| Configuration des entités | Fluent API, une classe de configuration par entité (`RoomConfiguration`, `WorkerConfiguration`, `BookingConfiguration`, `EquipmentConfiguration`, `RoomEquipmentConfiguration`) |
| Migrations | `InitialCreate`, `AddRoomName` (`BusinessRoomBooking.Infrastructure/Migrations/`) |

Modèle relationnel (déduit des entités et configurations) :

- `Room` (1) — (n) `Booking`
- `Worker` (1) — (n) `Booking`
- `Room` (n) — (n) `Equipment` via la table de jointure `RoomEquipment`

Commandes utiles :

```bash
# Ajouter une nouvelle migration
dotnet ef migrations add <NomMigration> --project BusinessRoomBooking.Infrastructure --startup-project BusinessRoomBooking.API

# Appliquer les migrations
dotnet ef database update --project BusinessRoomBooking.Infrastructure --startup-project BusinessRoomBooking.API
```

---

## Qualité du code

Conventions et patterns réellement identifiés dans le code :

- **Séparation des responsabilités** par projet (Domain / Core / Infrastructure / API), proche d'une Clean Architecture simplifiée.
- **Repository Pattern** : `BaseRepository<T>` générique, spécialisé par entité.
- **Dependency Injection** native ASP.NET Core, avec méthodes d'extension dédiées (`AddApplicationServices`, `AddInfrastructure`).
- **Exceptions métier typées** par domaine, centralisées et traduites en réponses HTTP via `ExceptionMiddleware`.
- **Mappers en extension methods** pour isoler la conversion entité ↔ DTO du reste de la logique métier.
- **DTOs séparés par usage** (Request / Response / Summaries / Projections / Queries), évitant l'exposition directe des entités du domaine.

Aucun `.editorconfig` n'est présent dans le dépôt ; les conventions de nommage observées (`PascalCase` pour les classes, suffixes `Service`/`Repository`/`Exception`/`Configuration`) sont cohérentes avec les conventions .NET standard.

---

## Tests

- **Framework** : xUnit 2.9.3, avec `NSubstitute` pour les mocks et `coverlet.collector` pour la couverture.
- **Organisation** (`BusinessRoomBooking.Tests/`) :
  - `Controllers/` — ex. `RoomControllerTests.cs`
  - `Mappers/` — un fichier de test par mapper (`BookingMapperTests`, `RoomEquipmentTest`, `RoomMapperTests`, `WorkerMapperTests`)
  - `Services/` — un fichier de test par service (`BookingServiceTests`, `RoomServiceTests`, `WorkerServiceTests`)
- **Convention de nommage** : `MethodeTestee_ShouldComportementAttendu` (ex. `GetBookingById_ShouldReturnBooking`).

Lancer les tests :

```bash
dotnet test
```

Avec couverture de code :

```bash
dotnet test --collect:"XPlat Code Coverage"
```

> Aucun rapport de couverture n'est actuellement publié ou versionné dans le dépôt — le pourcentage de couverture réel n'est donc pas déterminable sans exécuter la commande ci-dessus.

---

## Docker

<!-- TODO: aucun Dockerfile ni docker-compose détecté dans ce dépôt -->
Aucun `Dockerfile` ni fichier `docker-compose*.yml` n'est présent actuellement. La conteneurisation reste à mettre en place si nécessaire.

---

## GitHub Actions / CI-CD

<!-- TODO: aucun workflow détecté -->
Aucun dossier `.github/workflows` n'est présent dans ce dépôt : il n'existe actuellement aucun pipeline d'intégration ou de déploiement continu configuré.

---

## Déploiement

<!-- TODO: aucune méthode de déploiement détectée -->
Aucune configuration de déploiement (Docker, scripts, pipeline CI/CD, IIS, Azure, etc.) n'a été trouvée dans le dépôt. Le déploiement reste à définir selon l'environnement cible.

---

## Performance

<!-- TODO: aucune optimisation spécifique détectée -->
Aucune optimisation de performance particulière (mise en cache, pagination explicite, requêtes projetées avancées, etc.) au-delà des pratiques EF Core standard n'a été identifiée dans le code actuel.

---

## Sécurité

| Aspect | État constaté |
| --- | --- |
| Authentification | Non implémentée — aucun middleware d'authentification détecté dans `Program.cs` |
| Autorisation | Non implémentée — aucun attribut `[Authorize]` détecté |
| JWT | Absent — aucun package ni configuration JWT trouvé |
| HTTPS | `UseHttpsRedirection()` activé dans `Program.cs`, profil `https` disponible dans `launchSettings.json` |
| CORS | Politique `AllowAngularDev` restreinte à l'origine `http://localhost:4200` (`Extensions/CorsExtensions.cs`) |
| Secrets | `appsettings.Development.json` (contenant la chaîne de connexion locale) est exclu du contrôle de version via `.gitignore` ; un `UserSecretsId` est également configuré sur le projet API |

> ⚠️ En l'état, l'API ne dispose d'aucun mécanisme d'authentification/autorisation : à considérer avant toute exposition hors environnement local.

---

## Roadmap

Éléments visiblement en place ou manquants, à titre indicatif :

- [x] Domaine métier (Room, Worker, Booking, Equipment)
- [x] Couche d'accès aux données EF Core + migrations
- [x] Services applicatifs et gestion des exceptions métier
- [x] API REST avec contrôleurs par ressource
- [x] Documentation OpenAPI native
- [x] Suite de tests unitaires (Controllers, Mappers, Services)
- [ ] Authentification / autorisation
- [ ] Conteneurisation (Docker / docker-compose)
- [ ] Pipeline CI/CD (GitHub Actions)
- [ ] Documentation Swagger UI / Scalar
- [ ] Licence de projet
- [ ] Frontend Angular (référencé implicitement via CORS, non présent dans ce dépôt)

---

## Bonnes pratiques

- Respecter la séparation des couches existante : ne pas référencer `Infrastructure` depuis `Domain`, ni exposer les entités du `Domain` directement dans les réponses API (passer par les DTOs de `Core/Dtos`).
- Ajouter toute nouvelle règle métier sous forme d'exception dédiée dans `Core/Exceptions`, gérée par `ExceptionMiddleware`.
- Suivre la convention de test existante `MethodeTestee_ShouldComportementAttendu` lors de l'ajout de nouveaux tests.
- Générer une migration EF Core pour toute modification du modèle de données (`Domain` + `DataBase/Configuration`).

---

## Contribution

Les contributions sont les bienvenues. Pour proposer une modification :

1. **Fork** le dépôt.
2. Créez une branche dédiée à votre fonctionnalité ou correctif :
   ```bash
   git checkout -b feature/ma-fonctionnalite
   ```
3. Committez vos changements avec des messages clairs :
   ```bash
   git commit -m "feat: ajoute la fonctionnalité X"
   ```
4. Poussez votre branche et ouvrez une **Pull Request** vers la branche principale.

Aucun fichier `CONTRIBUTING.md` n'existe encore dans ce dépôt — ces règles sont fournies à titre de base et peuvent être formalisées ultérieurement.

---

## Versioning

<!-- TODO: aucun schéma de versioning explicite détecté -->
Aucun fichier de version, tag sémantique ou `CHANGELOG.md` n'a été trouvé dans le dépôt. Un schéma de versioning (ex. [SemVer](https://semver.org/)) reste à définir.

---

## Licence

<!-- TODO: aucun fichier LICENSE présent -->
Aucun fichier `LICENSE` n'est présent dans ce dépôt. La licence de ce projet reste **à définir**.

---

## Auteur

- **Math Peeters**

---

## Remerciements

<!-- TODO: aucune mention/dépendance tierce nécessitant un remerciement explicite n'a été identifiée au-delà des packages listés ci-dessus -->
Merci à l'écosystème .NET / Entity Framework Core / xUnit / NSubstitute dont les bibliothèques sont utilisées dans ce projet (voir [Packages NuGet](#packages-nuget)).
