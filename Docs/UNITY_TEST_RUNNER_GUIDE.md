# Unity Test Runner — QA rokasgrāmata

Šī rokasgrāmata ir domāta Reviewer / QA + Build lomai, arī vienkāršākiem
modeļiem. Tās mērķis ir palaist un pierakstīt Unity testus, netraucējot cilvēkam
vai Core izstrādātājam, kas strādā tajā pašā projektā.

## Stingrie noteikumi

- Testus Unity redaktorā drīkst palaist tikai Reviewer / QA + Build pēc tam,
  kad kandidāts ir iesaldēts un Architect ir devis statiskās pārbaudes atļauju.
- **Nekad nelieto UnityCLI, `-runTests`, batch mode vai citu Unity komandrindu.**
  Šajā projektā tas var sabojāt vai aizvērt atvērto Unity sesiju.
- **Nekad neaizver, nerestartē un nepārstartē Unity**, lai mēģinātu palaist
  testus. Ja Unity nav gatavs, apstājies un ziņo uzdevuma devējam.
- Neizmaini kodu, testus, Project Settings vai ainas, kamēr esi QA lomā.
- Ja cilvēks strādā Unity logā, neaiztiec viņa logus. Test Runner var palikt
  neliels; nav jāmaina tā izmērs, lai palaistu visu komplektu.

## Īsā procedūra

1. Pārliecinies, ka šis ir QA uzdevums, diff ir iesaldēts un nav jaunu koda vai
   testa izmaiņu kopš pēdējās pilnās zaļās palaišanas.
2. Unity redaktorā atver **Window → General → Test Runner**. Ja logs jau ir
   atvērts, izmanto to; neveido jaunu Unity procesu.
3. Vispirms palaid mazāko uzdevumā prasīto fokusēto testu vai testa klasi.
   Izvēlies **EditMode** vai **PlayMode** atbilstoši uzdevumam un spied
   **Run Selected**. Negaidi manuālu spēles testu, ja automatizētais tests jau
   ir pietiekams konkrētajai pārbaudei.
4. Ja fokusētais tests ir zaļš un kandidāts kopš tā nav mainīts, palaid vienu
   pilno komplektu: **EditMode → Run All**, pēc tam **PlayMode → Run All**.
   Šajā projektā pilnais **Run All** izpildās pietiekami ātri.
5. Pieraksti tikai redzamo rezultātu: nokārtoto/kopējo skaitu, kļūmju skaitu un
   to, vai pēc gala komplektiem ir mainījies kods vai testi. Aktuālo pieņemto
   baseline skaties `Docs/PROTOTYPE_STATUS.md`; testa skaits var pieaugt ar
   nākamajiem uzdevumiem.
6. Zaļus pilnos komplektus neatkārto. Atkārto tikai tad, ja pēc tiem mainījās
   kandidāts, imports vai tests, vai ja atradi konkrētu testu problēmu.

## Ja kaut kas neizdodas

- Ja Test Runner nav atrodams, Unity kompilē, logs ir pārāk mazs, vai UI nav
  droši vadāms: **neizmanto komandrindu, neaizver Unity un nesāc restartu**.
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
Changed after final suite: yes | no
Commit/push: not performed
```

Nesaki, ka veikts manuāls vai fiziskas ierīces smoke tests, ja tas nav tieši
novērots. Ja lietotājs pats apstiprina, ka abi komplekti ir zaļi, to drīkst
ziņot kā lietotāja sniegtu rezultātu, nevis kā paša QA palaistu pārbaudi.
