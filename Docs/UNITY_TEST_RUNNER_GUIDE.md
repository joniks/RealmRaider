# Unity Test Runner — QA rokasgrāmata

Šī rokasgrāmata ir domāta Reviewer / QA + Build lomai, arī vienkāršākiem
modeļiem. Tās mērķis ir palaist un pierakstīt Unity testus, netraucējot cilvēkam
vai Core izstrādātājam, kas strādā tajā pašā projektā.

## Stingrie noteikumi

- Testus Unity redaktorā drīkst palaist tikai Reviewer / QA + Build pēc tam,
  kad kandidāts ir iesaldēts un Architect ir devis statiskās pārbaudes atļauju.
- Unity GUI ir noklusētais ceļš, kamēr Editors ir atvērts. **Nekad nepalaid
  UnityCLI pret projektu, kuru jau tur atvērtu Unity process.**
- Architect drīkst lietot repozitorija `Tools/realmraider-unity-cli.sh`, ja
  lietotājs ir iepriekš brīdināts, Unity Editors ir pilnībā aizvērts un process
  ir pārbaudīts. Pirms Editora atkārtotas atvēršanas lietotāju brīdina vēlreiz.
- Unity GUI sesiju neaizver vai nerestartē kā nejaušu Test Runner workaround.
  Pāreja uz CLI ir apzināta QA robeža, nevis reakcija uz vienu neveiksmīgu klikšķi.
- Neizmaini kodu, testus, Project Settings vai ainas, kamēr esi QA lomā.
- Ja cilvēks strādā Unity logā, neaiztiec viņa logus. Test Runner var palikt
  neliels; nav jāmaina tā izmērs, lai palaistu visu komplektu.

## Īsā procedūra

1. Pārliecinies, ka šis ir QA uzdevums, diff ir iesaldēts un nav jaunu koda vai
   testa izmaiņu kopš pēdējās pilnās zaļās palaišanas.
2. Unity redaktorā atver **Window → General → Test Runner**. Ja logs jau ir
   atvērts, vispirms to skaidri pacel un aktivizē; neveido jaunu Unity procesu.
   **Tieši pirms katra `Run Selected` vai `Run All` klikšķa Test Runner logam
   jābūt atvērtam, redzamam un aktīvam.** Nepalaiž testu no nefokusēta vai tikai
   fonā redzama Test Runner loga.
3. Vispirms palaid mazāko uzdevumā prasīto fokusēto testu vai testa klasi.
   Izvēlies **EditMode** vai **PlayMode** atbilstoši uzdevumam un spied
   **Run Selected**. Negaidi manuālu spēles testu, ja automatizētais tests jau
   ir pietiekams konkrētajai pārbaudei.
   Ja iepriekšējais filtrs ir aktīvs, **notīri to ar meklēšanas lauka `×` pogu**
   pirms jebkura `Run All`; citādi `Run All` var palaist tikai filtrēto testu.
4. Ja fokusētais tests ir zaļš un kandidāts kopš tā nav mainīts, palaid vienu
   pilno komplektu: **EditMode → Run All**, pēc tam **PlayMode → Run All**.
   Šajā projektā pilnais **Run All** izpildās pietiekami ātri.
5. Pieraksti tikai redzamo rezultātu: nokārtoto/kopējo skaitu, kļūmju skaitu un
   to, vai pēc gala komplektiem ir mainījies kods vai testi. Aktuālo pieņemto
   baseline skaties `Docs/PROTOTYPE_STATUS.md`; testa skaits var pieaugt ar
   nākamajiem uzdevumiem.
6. Zaļus pilnos komplektus neatkārto. Atkārto tikai tad, ja pēc tiem mainījās
   kandidāts, imports vai tests, vai ja atradi konkrētu testu problēmu.

### Zināms Test Runner gadījums — filtrētais tests paliek sagatavošanā

2026-09-10 filtrēts PlayMode `Run Selected` palika Test Runner sagatavošanas
ainā un neiegāja pašā testa metodē. Tas nebija spēles bezgalīgs cikls, koda
deadlock vai Mac resursu problēma. Pēc palaišanas atcelšanas, meklēšanas filtra
notīrīšanas un viena parasta PlayMode `Run All` viss komplekts pabeidzās ar
`79/79`; sekojošais EditMode `Run All` pabeidzās ar `137/137`.

Šajā situācijā **nepārstartē Mac un nerestartē Unity**. Atcel tikai iestrēgušo
testu palaišanu, pacel Test Runner logu, notīri filtru, pārliecinies par pilno
testu skaitu un palaid vienu `Run All`. Restartēšanu apsver tikai pēc īsta
Editor crash vai tad, ja assembly pēc `Assets → Refresh` joprojām ir pierādāmi
novecojis.

## Autorizētais CLI režīms

CLI režīmu drīkst sākt tikai Architect un tikai ar aizvērtu Unity Editoru.
Pirms katras palaišanas skripts atsakās strādāt, ja atrod aktīvu `Unity`
procesu, un saglabā `HEAD`, darba koka statusu, XML un pilno logu zem
`Logs/CLI/<UTC-laiks>/`.

2026-09-10 pilots pierādīja, ka Unity Personal licence ir pieejama arī
`com.unity.editor.headless`, ja Unity saņem aktīvā Hub licences IPC kanālu.
Tieša Unity palaišana bez šī kanāla kļūdaini ziņoja, ka tiesības nav atrastas.
Repozitorija skripts tādēļ prasa atvērtu, ielogotu Unity Hub, droši nolasa tikai
lokālo kanāla nosaukumu un nodod to headless Unity procesam. Unity projektam
jābūt aizvērtam, bet Unity Hub jāpaliek atvērtam visu komandas laiku.

Iebūvētais Test Framework `-runTests` pilots ar derīgu licenci ielādēja projektu,
bet nesāka fokusēto PlayMode testu un neizveidoja XML. Process tika droši
pārtraukts. Tādēļ **CLI testi vēl nav pieņemts QA ceļš**; EditMode un PlayMode
testus turpina palaist ar šīs rokasgrāmatas GUI procedūru. CLI ir autorizēts
tikai zemāk aprakstītajiem platformu eksportiem.

```text
Tools/realmraider-unity-cli.sh export-android
```

Eksporta komanda lieto `-batchmode`, Hub licences IPC, fiksētu `-buildTarget`,
`-executeMethod` un `-quit`. `-quit` šeit ir atļauts, jo sinhronais
`BuildPipeline.BuildPlayer` ir pabeigts pirms `executeMethod` atgriežas.

Pēc katra pieņemta un Architect iecommitota Core izstrādes soļa, kad vairs nav
vēlāku gameplay/build izmaiņu, veselā atvērtā Unity GUI lieto **Realm Raiders →
Build → Export Android Studio Project**. Editoru šim nolūkam neaizver un
nerestartē. `export-android` CLI lieto tikai tad, ja Editors jau ir aizvērts
iepriekš saskaņotā robežā. Tad eksports ir pieņemts tikai ar kodu `0`, veiksmīgu
`Realm Raiders Android export completed` ierakstu un atjaunotu
`Builds/AndroidStudio`.

## Nepārtraukta QA kārtība

- Pēc Architect uzdevuma neapstājies pie starpposma kopsavilkuma: turpini ar
  nākamo jau atļauto QA soli, līdz ir gala rezultāts vai konkrēts bloķētājs.
- Par **katru faktisko** fokusētā testa, pilnā komplekta, manuālā smoke vai UI
  bloķētāja rezultātu uzreiz ziņo Architect (un Core, ja tas ir koda/testa
  defekts). Neziņo izdomātu vai vēl nenolasītu rezultātu.
- Pirms `Run All` pārbaudi, ka meklēšanas lauks ir tukšs un Test Runner rāda
  pilnu attiecīgā režīma testu skaitu.
- Pirms katras palaišanas vēlreiz aktivizē pašu Test Runner logu; tikai pēc tam
  spied `Run Selected` vai `Run All` un nogaidi palaišanas beigas pirms nākamā
  klikšķa.
- Ja kļūme bloķē turpmākos atļautos soļus, nekavējoties nosūti pilno testa
  nosaukumu un kļūdas tekstu Architect; nepaliec klusā gaidīšanas stāvoklī.

## Darba nepārtrauktības konteksts

- Atļauts QA solis ir jāizpilda uzreiz. Nekad nebeidz atbildi tikai ar plānu,
  statusu vai jautājumu, ja nākamais drošais solis jau ir zināms.
- Ja Test Runner filtrs, logs vai rezultātu XML ir neuzticams, vispirms notīri
  filtru un skaties pašu Test Runner. Ja fokusētais saraksts nav praktiski
  lietojams un `Run All` ir atļauts, palaid vienu parasto `Run All`, nevis
  iestrēgsti pie filtra UI.
- Lieto recovery kāpni tieši šādā secībā: filtrs/skaits → `Assets → Refresh`
  → avota, assembly un Console freshness → viens parasts Unity GUI restarts
  tikai tad, ja assembly paliek novecojis bez kompilācijas kļūdas. Pēc katra
  soļa turpini ar nākamo pieejamo pārbaudi vai ziņo precīzu bloķētāju.
- Kamēr Unity GUI ir atvērts, nelieto UnityCLI vai batch mode kā recovery soli.
  Nekad nelieto `Reimport All`, Library dzēšanu, plašu cache reset vai minētus
  klikšķus. Ja izvēlne atver nepareizu logu, aizver
  tikai šo logu, no jauna iegūsti UI stāvokli un mēģini drošo izvēli vēlreiz.
- Test Runner virsrakstjoslu nekad nedubultklikšķina un nemaina loga vietu vai
  izmēru. Kad tests nav aktīvs, Runner minimizē ar tā minimizēšanas pogu.
- Architect ir uzdevuma devējs: nekavējoties sūti viņam faktisko rezultātu,
  atteikumu vai UI bloķētāju. Ja Core deva kandidātu, sūti to pašu arī Core.
- Ziņošana ir obligāta, nevis noslēguma izvēle: par fokusēto testu, katru gala
  komplektu, manuālo smoke un bloķētāju nosūti faktu uzreiz pēc tā nolasīšanas.
  Pēc ziņojuma **neapstājies gaidīt atbildi**, ja ir vēl kāds jau atļauts QA
  solis; turpini līdz gala handoff vai īstam ārējam bloķētājam.
- Gala handoff uz Architect un Core ir jānosūta nekavējoties pēc pēdējās
  pārbaudes, arī tad, ja rezultāts ir sarkans vai manuālais smoke nav palaists.

## Ja kaut kas neizdodas

- Ja Test Runner nav atrodams, Unity kompilē, logs ir pārāk mazs, vai UI nav
  droši vadāms: neaizver Unity slepus un nesāc otru procesu. Ziņo Architect;
  CLI nav pieņemts testa recovery ceļš; turpini ar GUI vai ziņo konkrēto robežu.
- Noformulē konkrēto šķērsli un paziņo personai, no kuras saņēmi uzdevumu.
- Ja vajag palīdzību ar gaidāmo uzvedību vai fokusēto testu, jautā Core
  developerim; viņš pārzina veiksmīgi testēto scenāriju. Core tomēr pats
  nekontrolē Unity un nepalaiž testus.
- Ja tests ir sarkans, pieraksti testa pilno nosaukumu, kļūdas tekstu un vai tas
  bija fokusētais vai pilnais komplekts. Nosūti to Core developerim un
  Architect; nemēģini labot kodu QA lomā.

## Noslēguma ziņojums

Sūti šo sešu rindu formātu Core developerim un Architect:

```text
Review: accepted | rejected with <concrete blocker>
Focused: <result>
Final EditMode / PlayMode: <totals vai not yet run>
Manual smoke: <novērotais rezultāts vai user-owned/not run>
Android export: <GUI commit SHA + success vai CLI commit SHA + log path; not run>
Changed after final suite: yes | no
Commit/push: not performed
```

Nesaki, ka veikts manuāls vai fiziskas ierīces smoke tests, ja tas nav tieši
novērots. Ja lietotājs pats apstiprina, ka abi komplekti ir zaļi, to drīkst
ziņot kā lietotāja sniegtu rezultātu, nevis kā paša QA palaistu pārbaudi.
