# Pendlerapp - Gruppe16

En full stack web applikasjon for norske pendlere. Lagre dine faste reiseruter og se sanntids avgangstider fra Entur.

# Teknologi

- ASP.NET Core MVC (.NET 10)
- SQLite med Entity Framework Core
- Tailwind CSS
- ASP.NET Core Identity
- Entur JourneyPlanner API (GraphQL)
- xUnit
- git clone https://github.com/Gruppe16-FullStack/Eksamen2026.git
- Åpne nettleseren på `http://localhost:5111`

# Testbruker

For å teste applikasjonen, bruk følgende innloggingsinformasjon:

- **E-post:** test@gruppe16.no
- **Passord:** Test123!

# Entur API

Applikasjonen bruker Entur sitt offentlige API for sanntids avgangsinformasjon.

- **Base URL:** `https://api.entur.io/journey-planner/v3/graphql`
- **Geocoder URL:** `https://api.entur.io/geocoder/v1/autocomplete`
- Ingen API-nøkkel kreves
- Client-name header: `gruppe16-pendlerapp`

# Kjente begrensninger

- Forsinkelsesdata lagres kun ved valg av avgang, ikke oppdatert i ettertid
- Krever internettilgang for Entur API-kall
