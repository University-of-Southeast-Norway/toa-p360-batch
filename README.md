# Oversikt
Løsningen **Batch-vis overføring av avtaler fra TOA til P360**, heretter referert til som **løsningen**, overfører, som navnet indikerer, avtaler batch-vis fra TOA til P360. En batch er konkret alle avtaler som er registrert mellom to datoer i TOA. Løsningen vil forsøke å gjenbruke eksisterende person og sak i P360 dersom løsningen får match på dette i P360. Det er også lagt inn en sikring som gjør at samme avtale ikke vil lastes opp på nytt dersom den tidligere er lastet opp via denne løsningen.
Følgende data hentes fra TOA og benyttes i løsningen:
-	Avtalenummer
-	Avtale-fil (PDF)
-	Fødselsnummer
-	Navn
-	Adresse
-	Mobilnummer
-	E-port adresse

Persondata blir brukt dersom løsningen ser at det er behov for å opprette ny person i P360. Fødselsnummer brukes til å gjenfinne personen i P360.
Avtalenummer benyttes til å definere filnavnet til selve avtale-filen (PDF) i P360. Avtale-filen lastes opp som den er på et nytt dokument.
Metadata på sak og dokument i P360 defineres i egne definisjonsfiler som kommer med løsningen. Disse settes opp en gang og gjenbrukes for hver batch-kjøring. Disse definisjonsfilene kommer med et standard oppsett, men det vil også være behov for et tilpasset oppsett for hver institusjon.

# Definisjonsfiler
## Teknisk
Disse filene finner man i mappen Definitions under hoved-mappen til løsningen. Dette er JSON filer, og hver fil kommer i to versjoner: Filen som blir brukt ved kjøring, og en _template_ fil som inneholder fullstendig oppsett. Filen som blir brukt ved kjøring inneholder også et forslag til oppsett slik at man har et utgangspunkt.

## Oppsett
Selv om filene potensielt kan inneholde mange definisjoner er det noen grunnleggende data som et er verdt å merke seg. Enkelte felter er nødvendig, samt at enkelte felter også medfører noe logikk i selve løsningen dersom spesifikke verdier angis.
Felles for alle filene er at de settes opp med feltet _ADContextUser_, som angir brukeren som definerer at endringene i P360 er gjort av en maskin og ikke en reell bruker.

### Finne eksisterende person
_get_private_persons.json_

Søker etter eksisterende personer basert på metadata gitt i definisjonsfilen og fødselsnummer fra TOA. Metadata som settes i denne filen må matche metadata som er definert i _synchronize_private_person.json_ for at gjenbruk skal fungere optimalt.

### Opprette ny person
_synchronize_private_person.json_

### Finne eksisterende sak
_get_cases.json_

Søker etter saker basert på metadata gitt i definisjonsfilen og recno fra eksisterende person. Metadata som settes opp i denne filen må matche metadata som er definert i _create_case.json_ for at gjenbruk skal fungere optimalt.

### Opprette ny sak
_create_case.json_

Dersom feltet Contacts er satt opp settes feltet _ReferenceNumber_ automatisk lik _'recno:\<recno\>'_ der recno er recno til eksisterende eller opprettet privat person.
Følgende felter må settes:
- Title
- Status
- AccessCode
- AccessGroup

### Sjekke om avtalen er lastet opp tidligere
_get_documents.json_
  
Hver gang løsningen laster opp en avtale til P360 settes det en unik verdi i _Notat_ feltet tilknyttet avtale-filen i P360. Dette feltet benyttes til å kontrollere om filen er lastet opp tidligere. Dersom man ønsker ytterligere begrensninger kan dette settes i definisjonsfilen. Det er ellers ingen spesifikke behov i denne filen utover _ADContextUser_.

### Opprette nytt dokument
_create_document.json_

Dersom feltet Contacts er satt opp med Role lik «Avsender» settes _ReferenceNumber_ automatisk lik 'recno:\<recno\>', der _\<recno\>_ er recno til eksisterende eller opprettet privat person.
Følgende felter må settes:
- Title
- Status
- AccessCode
- AccessGroup

### Laste opp filen til dokument
_update_document.json_

Det er ingen spesifikke behov i denne filen utover _ADContextUser_.

### Avslutte dokument
_sign_off_document.json_

Følgende felter må settes:
- ResponseCode

# Teknisk oppsett
## Eksekvering av kjøring
Løsningen kan kjøres på både Windows og Linux. Selve programmet kan eksekveres fra kommandolinje med eller uten inndata parametere. Dersom det ikke oppgis parametere blir man bedt om å oppgi fra og til-dato. Fra og til-dato kan også oppgis som parametere. Dersom man oppgir en dato, anses denne datoen for å være fra-dato, og til-dato settes til dagens dato. Dersom man oppgir to datoer anses disse å være fra og til-dato. Dato-format som skal benyttes er YYYYMMDD.
### Alternativer for parametere:
Skriver ut en liten hjelpe-tekst:

``` sh
DfoToa.BatchRun.exe -h
```

Fra oppgitt dato til og med dagen før gjeldende dato:
``` sh
DfoToa.BatchRun.exe -f 20220901
```

Fra oppgitt dato til oppgitt dato:
``` sh
DfoToa.BatchRun.exe -f 20220701 -t 20220901
```

Samme som over men vil ikke spørre om bekreftelse fra bruker (til bruk ved for eksempel planlagt kjøring):
``` sh
DfoToa.BatchRun.exe -f 20220701 -t 20220901 -s
```

## Oppsett av miljøkonfigurasjon
Konfigurasjon av miljø settes opp i filen _JSON/\_general.config_. Det medfølger en _JSON/\_general.template.config_ mal-fil som man endrer navnet på til _\_general.config_.

### Parametere:
_inProductionDate_ - Dette feltet brukes til å hindre at man gjenbruker gamle saker som matcher på feltene som er satt opp i _get_cases.json_. Dette kan være saker som er lagt inn manuelt på en person i P360 og som det tilfeldigvis oppstår en match på. For å unngå at en gammel sak gjenbrukes kan man sette dette feltet til datoen for første kjøring av denne løsningen, slik at man ved fremtidige første og fremtidige kjøringer kun bruker saken som er opprettet av denne løsningen. Man kan også bruke dette feltet dersom man ønsker å tvinge opprettelse av en ny sak på person ved overføring av nye avtaler ved en senere anledning.

_p360ApiKey_ – institusjonens unike API-nøkkel

_maskinporten_ –
- path: angi katalog-sti til plassering av institusjonens sertifikat (p12 fil)
- password: passord til sertifikatet

_issuer_ – unik identifikator hentet fra samarbeidsportalen

_logFolder_ – mappe der logger lagres

_reportFolder_ – mappe der rapport på overførte avtaler lagres

_stateFolder_ – mappe som brukes til å holde state på overføring av avtaler. Innhold i denne mappen **bør slettes** etter en tid i etterkant av en overføring. Dersom man ser behov for å kjøre en periode på nytt bør man avvente med å slette innhold i denne mappen til dette er utført.

_api_keys_ - som et alternativ til Maskinporten kan man sette opp autentisering med API-nøkler, for eksempel om man har satt opp en gateway mot DFØ-API i IntArk

_scope_ - det må oppgis header og API-nøkkel for dfo:ansatte dfo:ansatte/infokontrakter dfo:infokontrakter/filer
  - Som angitt i _general.example.json setter man opp en nøkkel for hvert scope, men det er mulig å gjenbruke samme nøkkel dersom man har valgt å pakke scope sammen i gateway (IntArk)
 
