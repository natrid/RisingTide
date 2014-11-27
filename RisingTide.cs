using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;

/// <summary>
/// Tasohyppelypeli
/// </summary>
public class RisingTide : PhysicsGame
{

    Random r = new Random();

    private const double KENTAN_LEVEYS = 1920;
    private const double KENTAN_KORKEUS = 1080;

    Image taustaKuva = LoadImage("backgroundsky");

    List<PhysicsObject> esineet = new List<PhysicsObject>();
    List<PhysicsObject> pelaajat = new List<PhysicsObject>();
    List<PhysicsObject> tasot = new List<PhysicsObject>();

    private DoubleMeter korkeuslaskuri = new DoubleMeter(0);

    Label korkeusNaytto = new Label();

    private double yhteisKorkeus;
    private double saavutettuYhteiskorkeus = -400;

    private PlatformCharacter pelaaja1;
    private PlatformCharacter pelaaja2;
    private PhysicsObject korkeampiPelaaja;

    private PhysicsObject vesi;


    DoubleMeter voimaMittari; //Mahdollista käyttää tulevaisuudessa...

    double naytonAlareuna;

    private Image[] kuvat = LoadImages("hiiri1", "hiiri2", "vesi1",                                                             //0,1,2
         "EagleFlyingRight", "papukaijaoikealle", "joutsenOikealle", "keltainenLintuOikealle", "merenlintuOikealle",            //3,4,5,6,7
         "", "", "", "", "",                                                                                                    //8,9,10,11,12
        "crate",                                                                                                                //13
        "kivi2",                                                                                                                //14
        "koivunlehti", "lepanlehti", "vaahteranlehti",                                                                          //15,16,17
        "neliapila",                                                                                                            //18
        "wheat",                                                                                                                //19
        "cratetnt");                                                                                                            //20
    private String[] tagit = { "laatikko", "heina", "lintu", "lehti", "tnt" };
    Label aikaNaytto = new Label();
    private ScoreList topLista = new ScoreList(10, false, 0);


    /// <summary>
    /// Peli aloitetaan.
    /// </summary>
    public override void Begin()
    {
        ClearGameObjects();
        ClearControls();
        ClearTimers();
        ClearWidgets();

        Level.Width = KENTAN_LEVEYS;
        Level.Height = KENTAN_KORKEUS;
        Level.Background.Image = taustaKuva;
        Level.Background.FitToLevel();

        //IsFullScreen = true;

        if (DataStorage.Exists("pisteet.xml"))
            topLista = DataStorage.Load<ScoreList>(topLista, "pisteet.xml");
        Mouse.IsCursorVisible = true;
        MultiSelectWindow msw = new MultiSelectWindow("Tervetuloa pelaamaan RisingTidea!", "Yksinpeli", "Kaksinpeli(Coop.)", "Kaksinpeli(Duel)", "Parhaiden pisteiden lista", "Tietoa tekijästä", "Lopeta");
        Add(msw);
        msw.ItemSelected += AlkuvalikkoaPainettu;


    }


    /// <summary>
    /// Valitaan toiminto napin painalluksesta.
    /// </summary>
    /// <param name="valittuNappi">Painettu nappi.</param>
    private void AlkuvalikkoaPainettu(int valittuNappi)
    {
        MessageWindow eiKaytossa = new MessageWindow("Työn alla...");
        eiKaytossa.Closed += delegate { Begin(); };

        switch (valittuNappi)
        {
            case 0:
                AloitaUusiPeli();
                break;
            case 1:

                Add(eiKaytossa);
                //MessageDisplay.Add("Työn alla...");
                break;
            case 2:
                Add(eiKaytossa);
                //MessageDisplay.Add("Työn alla...");
                break;
            case 3:
                HighScoreWindow topIkkuna = new HighScoreWindow(
                              "Parhaat pisteet",
                              topLista);
                topIkkuna.Closed += delegate { Begin(); };
                Add(topIkkuna);
                break;
            case 4:
                MessageWindow tiedot = new MessageWindow("Tämän ohjelman on ohjelmoinut Daniel Nieminen käyttäen Jyväskylän Yliopiston Jypeli kirjastoa.");
                tiedot.Closed += delegate { Begin(); };
                Add(tiedot);
                break;
            case 5:
                Exit();
                break;

        }
    }


    /// <summary>
    /// Aloittaa pelin alusta.
    /// </summary>
    private void AloitaUusiPeli()
    {
        ClearControls();
        ClearAll();
        Gravity = new Vector(0, -1000);
        Level.BackgroundColor = Color.Black;
        Level.Width = KENTAN_LEVEYS;
        Camera.ZoomTo(-KENTAN_LEVEYS / 2, -KENTAN_KORKEUS / 2, KENTAN_LEVEYS / 2, KENTAN_KORKEUS / 2);
        Level.Background.CreateGradient(Color.White, Color.Blue);

        //Level.Background.Image = taustaKuva;


        pelaaja1 = LuoPelaaja(-KENTAN_LEVEYS / 2 + 25, -300, 50, 50, "pelaaja1");
        pelaaja1.Image = kuvat[0];
        Add(pelaaja1);
        AsetaOhjaimet(pelaaja1, Key.Left, Key.Right, Key.Up);
        AddCollisionHandler(pelaaja1, PelaajaTormaa);

        //pelaaja2 = LuoPelaaja(+KENTAN_LEVEYS / 2 - 25, -200, 50, 50, "pelaaja2");
        //pelaaja2.Image = kuvat[1];
        //Add(pelaaja2);
        //AsetaOhjaimet(pelaaja2, Key.A, Key.D, Key.W);
        //AddCollisionHandler(pelaaja2, PelaajaTormaa);

        LisaaAjastinLentavilleEsineille();

        double laiturinKorkeus = 100;
        LuoVesi();
        vesi.Position = new Vector(0, -(KENTAN_KORKEUS / 2 + vesi.Height / 2) - laiturinKorkeus);
        PiirraLaatikko(this, KENTAN_LEVEYS / 2, Level.Bottom, laiturinKorkeus, 50, LoadImage("laituriright"));
        PiirraLaatikko(this, -KENTAN_LEVEYS / 2, Level.Bottom, laiturinKorkeus, 50, Image.Mirror(LoadImage("laituriright")));

        KorkeusNaytto();
        //VoimaMittari();
        Laiturit();
        LiikutaVettaTasaisesti();

        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");

    }


    /// <summary>
    /// Peli pistetään poikki, tehdään loppu siivoukset ja valmistellaan uuteen peliin.
    /// Välilyönnillä aloittaa uuden peli.
    /// </summary>
    private void PeliPaattyy()
    {
        MessageDisplay.Add("Peli päättyi!");
        ClearGameObjects();
        esineet.Clear();
        pelaajat.Clear();
        tasot.Clear();

        ClearTimers();
        ClearControls();
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        MessageDisplay.Add("Paina välilyöntiä aloittaaksesi uuden pelin");
        MessageDisplay.Add("Paina 'M' päästäksesi alkuvalikkoon");
        NaytaTopTen();

        Keyboard.Listen(Key.Space, ButtonState.Pressed, AloitaUusiPeli, "Aloita uusi peli");
        Keyboard.Listen(Key.M, ButtonState.Pressed, Begin, "Alkuvalikko");
    }


    /// <summary>
    /// Luodaan PlatformCharacter Olion
    /// </summary>
    /// <param name="x">X-Koordinaatti aloituspaikalle.</param>
    /// <param name="y">Y-Koordinaatti aloituspaikalle.</param>
    /// <param name="leveys">Olion leveys.</param>
    /// <param name="korkeus">Olion korekeus.</param>
    /// <returns>Palauttaa olion.</returns>
    private PlatformCharacter LuoPelaaja(double x, double y, double leveys, double korkeus, String tagi)
    {
        PlatformCharacter character = new PlatformCharacter(leveys, korkeus);
        character.Position = new Vector(x, y);
        character.Tag = tagi;
        character.Shape = Shape.Ellipse;
        //character.MomentOfInertia = 1000;
        character.Restitution = 0.9; // Estää pelaajan juuttumisen
        character.StaticFriction = 500;
        character.MaintainMomentum = true;
        character.KineticFriction = 50;
        character.Mass = 10.0;
        character.LinearDamping = 0.96;
        pelaajat.Add(character);
        return character;
    }


    /// <summary>
    /// Asetetaan pelaajalle ohjaimet.
    /// </summary>
    /// <param name="tyyppi">Olio jolle halutaan antaa ohjaimet.</param>
    /// <param name="vasen">Näppäin vasemmalle</param>
    /// <param name="oikea">Näppäin oikealle</param>
    /// <param name="ylos">Näppäin ylös</param>
    private void AsetaOhjaimet(PlatformCharacter tyyppi, Key vasen, Key oikea, Key ylos)
    {
        Keyboard.Listen(vasen, ButtonState.Down, PelaajaWalk, tyyppi.Tag + " liikkuu vasemmalle", tyyppi, new Vector(-500, 0));
        Keyboard.Listen(vasen, ButtonState.Released, PelaajaWalk, null, tyyppi, Vector.Zero);
        Keyboard.Listen(oikea, ButtonState.Down, PelaajaWalk, tyyppi.Tag + " liikkuu oikealle", tyyppi, new Vector(500, 0));
        Keyboard.Listen(oikea, ButtonState.Released, PelaajaWalk, null, tyyppi, Vector.Zero);
        Keyboard.Listen(ylos, ButtonState.Pressed, PelaajaJump, tyyppi.Tag + "hyppää ylöspäin", tyyppi, 1000.0);
    }


    /// <summary>
    /// Pistetään PlatformCharacter kävelemään haluttuun suuntaan.
    /// </summary>
    /// <param name="liikuteltavaOlio">liikuteltava olio.</param>
    /// <param name="suunta">Olion nopeus (suunta).</param>
    private void PelaajaWalk(PlatformCharacter liikuteltavaOlio, Vector suunta)
    {
        #region Reunatarkistukset
        //tässä jos sattuu tarviimaan.
        //if (suunta.X < 0 && (liikuteltavaOlio.X < Level.Left + (liikuteltavaOlio.Width / 2)))
        //{
        //    liikuteltavaOlio.Velocity = Vector.Zero;
        //    return;
        //}

        //if (suunta.X > 0 && (liikuteltavaOlio.X > Level.Right - (liikuteltavaOlio.Width / 2)))
        //{
        //    liikuteltavaOlio.Velocity = Vector.Zero;
        //    return;
        //}

        //if (suunta.Y < 0 && (liikuteltavaOlio.Y < Level.Bottom + (liikuteltavaOlio.Height / 2)))
        //{
        //    liikuteltavaOlio.Velocity = Vector.Zero;
        //    return;
        //}

        //if (suunta.Y > 0 && (liikuteltavaOlio.Y > Level.Top - (liikuteltavaOlio.Height / 2)))
        //{
        //    liikuteltavaOlio.Velocity = Vector.Zero;
        //    return;
        //}
        #endregion

        liikuteltavaOlio.Walk(suunta.X);
    }


    /// <summary>
    /// Pelaaja hyppää.
    /// </summary>
    /// <param name="pelaaja">PlatformCharacter jota halutaan liikauttaa.</param>
    /// <param name="suunta">Mihin suuntaan impulssi kohdistuu.</param>
    public static void PelaajaJump(PlatformCharacter pelaaja, double speed)
    {
        pelaaja.Jump(speed);
    }


    /// <summary>
    /// Tönäistään pelaajaa kentän sivureunojen sisälle, jos se menee rajojen ulkopuolelle.
    /// </summary>
    private void ReunaTonaisy()
    {
        foreach (var pelaaja in pelaajat)
        {
            if (pelaaja.Left < -KENTAN_LEVEYS / 2)
            {

                pelaaja.Hit(new Vector(3000, 0));
            }
            if (pelaaja.Right > KENTAN_LEVEYS / 2)
            {
                pelaaja.Hit(new Vector(-3000, 0));
            }
        }
    }


    /// <summary>
    /// Pelaajan törmäykset toteutetaan tässä.
    /// </summary>
    /// <param name="pelaaja">IPhysicsObject joka törmää.</param>
    /// <param name="kohde">IPhysicsObject johon törmätään.</param>
    private void PelaajaTormaa(IPhysicsObject pelaaja, IPhysicsObject kohde)
    {
        if (kohde.Tag == "vesi" && (pelaaja.Bottom - kohde.Top) < 10)
        //if (pelaaja.Bottom < vesi.Y + vesi.Height / 2 + 20 || pelaaja.Y < vedenpinta)
        {
            if (pelaaja != null)
            {
                pelaaja.Destroy();
                //Remove(pelaaja);
                MessageDisplay.Add(pelaaja.Tag + " kuoli!");
                PeliPaattyy();
            }
        }

        if (kohde.Tag == "kotka")
        {
            pelaaja.Destroy();
            //Remove(pelaaja);
            MessageDisplay.Add("Kotka söi " + pelaaja.Tag + ":n!!!");
            PeliPaattyy();
        }

        if (kohde.Tag == "tnt")
        {
            //if (kohde.Y < 0) return;
            //Explosion rajahdys = new Explosion(kohde.Width);
            //rajahdys.Position = kohde.Position;
            int pMax = 200;
            Explosion rajahdys = new Explosion(pMax);
            rajahdys.Position = new Vector(kohde.X, kohde.Y);
            //Add(rajahdys);
            rajahdys.Speed = 500.0;
            rajahdys.Force = 10000;


            kohde.Destroy();
            pelaaja.Hit(new Vector(0, 10000));
        }

        //Kaksinpeliin alla oleva...
        if (pelaaja.Y < Camera.Y - KENTAN_KORKEUS / 2 && pelaajat.Count > 1) 
        {
            //Remove(pelaaja);
            pelaaja.Destroy();
            MessageDisplay.Add(pelaaja.Tag + " kuoli!");
            PeliPaattyy();
        }
    }


    /// <summary>
    /// Aliohjelma piirtää ruutuun yhden laatikon.
    /// </summary>
    /// <param name="peli">Peli, johon neliö piirretään</param>
    /// <param name="x">Neliön keskipisteen x-koordinaatti.</param>
    /// <param name="y">Neliön keskipisteen y-koordinaatti.</param>
    /// <param name="height">Laatikon korkeus.</param>
    /// <param name="width">Laatikon leveys.</param>
    /// <param name="kuva">Laatikolle annettava kuva.</param>
    public void PiirraLaatikko(Game peli, double x, double y, double width, double height, Image kuva)
    {
        // Vector v = new Vector(x, y);
        PhysicsObject nelio = PhysicsObject.CreateStaticObject(width, height, Shape.Rectangle);
        nelio.Position = new Vector(x, y);
        nelio.Image = kuva;
        peli.Add(nelio);
        tasot.Add(nelio);
    }


    /// <summary>
    /// Aliohjelma säätää lähtölaitureita ja pitää ajastinta lähtölaiturin tuhoutumiselle.
    /// </summary>
    private void Laiturit()
    {
        Timer aikaLaskuri = new Timer();
        aikaLaskuri.Interval = 5;
        aikaLaskuri.Timeout += delegate { Remove(aikaNaytto); };
        aikaLaskuri.Timeout += delegate
        {
            foreach (var esine in tasot)
            {
                esine.Destroy();
            }
            MessageDisplay.Add("Laiturit hajosivat...");
        };
        aikaLaskuri.Start(1);

        aikaNaytto.TextColor = Color.White;
        aikaNaytto.DecimalPlaces = 1;
        aikaNaytto.BindTo(aikaLaskuri.SecondCounter);
        Add(aikaNaytto);
    }


    /// <summary>
    /// Lisää ajastimen lentäville esineille.
    /// </summary>
    private void LisaaAjastinLentavilleEsineille()
    {
        Timer ajastin = new Timer();
        ajastin.Interval = 0.5;
        ajastin.Timeout += LentavaEsine;
        ajastin.Start();
    }


    /// <summary>
    /// Luodaan lentävälle esineelle parametrejä.
    /// </summary>
    public void LentavaEsine()
    {
        double lautaY = r.Next(20, 31);
        double lautaX = Jypeli.RandomGen.NextDouble(50, 100);
        double spawnY = pelaaja1.Y + Jypeli.RandomGen.NextDouble(-100, 450); //Jypeli.RandomGen.NextDouble(korkeampiPelaaja.Y - KENTAN_KORKEUS / 2, korkeampiPelaaja.Y + KENTAN_KORKEUS / 2); //korkeampiPelaaja.Y + Jypeli.RandomGen.NextDouble(-100, 350);
        double spawnX = KENTAN_LEVEYS / 2.5;

        double nopeus = r.Next(20, 150);
        string tagi = "";
        int tagienTodennakoisyys = r.Next(0, 10);
        switch (tagienTodennakoisyys)
        {
            case 0:
                tagi += tagit[0];
                break;
            case 1:
                tagi += tagit[0];
                break;
            case 2:
                tagi += tagit[4];
                break;
            case 3:
                tagi += tagit[1];
                break;
            case 4:
                tagi += tagit[1];
                break;
            case 5:
                tagi += tagit[2];
                break;
            case 6:
                tagi += tagit[2];
                break;
            case 7:
                tagi += tagit[2];
                break;
            case 8:
                tagi += tagit[2];
                break;
            case 9:
                tagi += tagit[2];
                break;
            default:
                break;
        }

        double kulma;
        int oikeaVaiVasen = r.Next(0, 2); // 1 tarkoittaa syntymistä oikealle ja 2 vasemmalle
        if (oikeaVaiVasen == 1)
        {
            if (spawnY < Camera.Y)
            {
                kulma = r.Next(165, 180);
            }
            else
            {
                kulma = r.Next(180, 195);
            }
            LuoEsine(lautaY, lautaX, spawnX, spawnY, nopeus, kulma, tagi);
        }
        else
        {
            if (spawnY < Camera.Y)
            {
                kulma = r.Next(0, 15);
            }
            else
            {
                kulma = r.Next(-45, 0);
            }
            LuoEsine(lautaY, lautaX, -spawnX, spawnY, nopeus, kulma, tagi);
        }
    }


    /// <summary>
    /// Luodaan PhysicsObject esine.
    /// </summary>
    /// <param name="peli">Mihin esine lisätään.</param>
    /// <param name="korkeus">Esineen korkeus.</param>
    /// <param name="leveys">Esineen leveys.</param>
    /// <param name="x">Esineen X-koordinaatti.</param>
    /// <param name="y">Esineen Y-koordinaatti.</param>
    /// <param name="nopeus">Nopeus arvo esineen liikkeelle.</param>
    /// <param name="kulma">Kulma johon esineen liike suunnistetaan.</param>
    /// <param name="tagi"></param>
    /// <returns>Ei toistaiseksi mitään.</returns>
    private PhysicsObject LuoEsine(double korkeus, double leveys, double x, double y, double nopeus, double kulma, string tagi)
    {
        PhysicsObject esine;
        if (tagi != "laatikko" && tagi != "tnt")
        {
            esine = PhysicsObject.CreateStaticObject(leveys, korkeus);
            esine.X = x;
            esine.Y = y;
            esine.Tag = tagi;
        }
        else
        {
            esine = new PhysicsObject(leveys, korkeus);
            esine.Tag = tagi;
        }
        if (esine.Tag == "heina")
        {
            esine.Image = kuvat[19];
        }

        if (esine.Tag == "tnt")
        {
            esine.Y = Camera.Y + KENTAN_KORKEUS / 2;
            esine.X = Jypeli.RandomGen.NextDouble(-KENTAN_LEVEYS / 2, KENTAN_LEVEYS / 2);
            esine.Image = kuvat[20];
            esine.Height = r.Next(30, 60);
            esine.Width = esine.Height;
            Add(esine);
            esineet.Add(esine);

        }
        if (esine.Tag == "lehti")
        {
            esine.Image = kuvat[r.Next(15, 18)];
        }
        if (esine.Tag == "laatikko")
        {
            esine.X = Jypeli.RandomGen.NextDouble(-KENTAN_LEVEYS / 2, KENTAN_LEVEYS / 2);
            esine.Y = Camera.Y + KENTAN_KORKEUS / 2;
            Add(esine);
            esine.Height = r.Next(30, 60);
            esine.Width = esine.Height;
            esine.MomentOfInertia = 10000;
            esineet.Add(esine);
            esine.Hit(new Vector(200, 20));
            esine.Image = kuvat[13];
            return null;

        }
        if (esine.Tag == "lintu")
        {
            if (r.Next(0, 10) < 1) // Toteutetaan todennäköisyys kotkan ilmestymiselle
            {
                if (x > 0) esine.Image = Image.Mirror(kuvat[3]);
                else esine.Image = kuvat[3];
                esine.Tag = "kotka";
            }
            else
            {
                if (x > 0) esine.Image = Image.Mirror(kuvat[r.Next(4, 8)]);
                else esine.Image = kuvat[r.Next(4, 8)];
            }
        }
        if (esine.Tag != "tnt" && esine.Tag != "laatikko")
        {
            Angle suunta = Angle.FromDegrees(kulma);
            Vector suuntaVektori = Vector.FromLengthAndAngle(nopeus, suunta);
            esine.Velocity = suuntaVektori;
            Add(esine);
            esineet.Add(esine);
        }
        return null;
    }


    /// <summary>
    /// Tuhoaa esineet kun niitä ei tarvita enää.
    /// </summary>
    public void TuhoaEsineet() //kysy actioneista/tapahtumista miten tehdä tapahtuma? halutaa poistaa kaikki kentän ulkopuolelle menevät esineet... (eli ylittää tietyn X-arvon)
    {
        foreach (var esine in esineet)
        {
            if (esine.X > KENTAN_LEVEYS / 2 + esine.Width ||
                esine.X < -KENTAN_LEVEYS / 2 - esine.Width ||
                //esine.Y < Level.Bottom + pelaaja1.Height + esine.Height / 2 || // Jos halutaan että esineet tuhoutuvat alareunaan.
                esine.Y < vesi.Top
                )
            {
                esine.Destroy();
            }
        }
    }


    /// <summary>
    /// Voimamittari esimerkki on tässä, mutta sille ei ole mitään ihmeellistä käyttöä vielä.
    /// </summary>
    private void VoimaMittari()
    {
        voimaMittari = new DoubleMeter(10);
        voimaMittari.MaxValue = 10;
        BarGauge voimaPalkki = new BarGauge(10, 150);
        voimaPalkki.BindTo(voimaMittari);
        Add(voimaPalkki);

        voimaPalkki.X = Screen.Right - 150;
        voimaPalkki.Y = Screen.Top - 20;
        voimaPalkki.BarColor = Color.Green;
        voimaPalkki.BorderColor = Color.White;
        voimaPalkki.Angle = Angle.FromDegrees(90);

        // Kun voima loppuu, kutsutaan VoimaLoppui-aliohjelmaa
        voimaMittari.LowerLimit += VoimaLoppui;

        Keyboard.Listen(Key.Space, ButtonState.Pressed,
                        VahennaVoimia, "Vähennä pelaajan voimia");
    }


    /// <summary>
    /// Vähennetään voimamittarista voimia.
    /// </summary>
    private void VahennaVoimia()
    {
        voimaMittari.Value--;
    }


    /// <summary>
    /// Ilmoittaa voimien loppumisesta.
    /// </summary>
    /// <param name="mittarinArvo">Paljonko voimaa on jäljellä.</param>
    private void VoimaLoppui(double mittarinArvo)
    {
        MessageDisplay.Add("Voimat loppuivat, voi voi.");
    }


    /// <summary>
    /// Jypelin update metodi, johon olen lisänyt suoritettavaa.
    /// </summary>
    protected override void Update(Time time)
    {
        base.Update(time);
        naytonAlareuna = Camera.Y - KENTAN_KORKEUS / 2;

        if (pelaaja1 == null || pelaajat.Count < 1) return;
        korkeampiPelaaja = KorkeinPelaaja(pelaajat);
        KameraSeuraa(korkeampiPelaaja);
        PaivitaPisteet();
        TuhoaEsineet();
        ReunaTonaisy(); //paranteluja voisi tehdä.. 


        if (pelaajat.Count == 1)
        {
            yhteisKorkeus = pelaaja1.Y + 400; //-400 on y-koordinaatti josta aletaan laskea korkeutta, eli nollakohta...
            if (yhteisKorkeus > saavutettuYhteiskorkeus)
            {
                saavutettuYhteiskorkeus = yhteisKorkeus;
            }
        }
        else
        {
            yhteisKorkeus = (pelaaja1.Y + pelaaja2.Y) / 2 + 400;


            if (yhteisKorkeus > saavutettuYhteiskorkeus)
            {
                saavutettuYhteiskorkeus = yhteisKorkeus;
            }
        }

        //OnkoKorkeus(); //Lisätään jos tarvitaan...

        //if(pelaaja1.IsDestroyed == true || pelaaja2.IsDestroyed == true && pelaajat.Count > 0)PeliPaattyy(); // Toinen tapa päättää peli...

        if (vesi == null || vesi.IsDestroyed == true)
        {
            return;
        }
      
        //LiikutaVettaAlareunanMukana(); //Toinen tapa liikuttaa vettä
    }


    /// <summary>
    /// Liikutetaan vettä näytön alareunan mukana.
    /// </summary>
    private void LiikutaVettaAlareunanMukana()
    {
        double vedenEtaisyysAlhaalta = (vesi.Y - (Camera.Y - KENTAN_KORKEUS / 2));
        #region Vesi nousee ikkunan alarajassa mutta ei laske
        if (vesi.Top > naytonAlareuna)
        {
            return;
        }

        if (vedenEtaisyysAlhaalta != 0)
        {
            vesi.Position = new Vector(0, naytonAlareuna - KENTAN_KORKEUS / 2 + 20);
        }
        #endregion
    }


    /// <summary>
    /// Nostaa vettä tietyn määrän.
    /// </summary>
    private void NostaVetta()
    {
        vesi.Y = vesi.Y + 1;
    }


    /// <summary>
    /// Liikutetaan vettä tasaisesti ylös päin Yksinpeliin vaihtoehtona
    /// </summary>
    private void LiikutaVettaTasaisesti()
    {
        Timer ajastin4 = new Timer();
        ajastin4.Interval = 0.05;
        ajastin4.Timeout += NostaVetta;
        ajastin4.Start();
    }


    /// <summary>
    /// Luodaan vesi.
    /// </summary>
    private void LuoVesi()
    {
        vesi = PhysicsObject.CreateStaticObject(KENTAN_LEVEYS, KENTAN_KORKEUS);
        vesi.Tag = "vesi";
        vesi.Image = kuvat[2];
        this.Add(vesi, 1);
    }


    /// <summary>
    /// Kamera seuraa PhysicsObject pelaajaa.
    /// </summary>
    /// <param name="pelaaja">PhysicsObject jota halutaan seurata.</param>
    private void KameraSeuraa(PhysicsObject pelaaja)
    {

        Camera.FollowY(pelaaja);
        //Camera.ZoomFactor = 5;
        //Camera.StayInLevel = true;

    }


    /// <summary>
    /// Määritetään kumpi pelaaja on korkeammalla.
    /// </summary>
    /// <param name="pelaajat">Pelin pelaajat</param>
    /// <returns>Palauttaa korkeamman pelaajan olioviittauksen</returns>
    public PhysicsObject KorkeinPelaaja(List<PhysicsObject> pelaajat)
    {
        if (pelaajat.Count < 1 || pelaajat == null) return null;
        if (pelaajat.Count == 1) return pelaajat[0];

        PhysicsObject korkein = pelaajat[0];
        if (pelaajat[1].Y > pelaajat[0].Y)
        {
            korkein = pelaajat[1];
        }

        return korkein;
    }


    /// <summary>
    /// Tarkoituksena olisi tulostaa hetkellisiä korkeuksia.
    /// </summary>
    private void OnkoKorkeus()
    {
        if (saavutettuYhteiskorkeus % 100 == 0)
        {
            if (MessageDisplay.IsAddedToGame == false)
            {
                MessageDisplay.Add(Convert.ToString(saavutettuYhteiskorkeus));
            }
        }

    }


    /// <summary>
    /// Luo pistelaskurin korkeudesta näyttöön
    /// </summary>
    private void PaivitaPisteet()
    {
        //yhteisKorkeus päivittyy update methodissa
        korkeuslaskuri.Value = yhteisKorkeus;
    }


    /// <summary>
    /// Lisätään näyttö näyttämään korkeutta.
    /// </summary>
    public void KorkeusNaytto()
    {
        korkeusNaytto.X = Screen.Right - 60;
        korkeusNaytto.Y = Screen.Top - 30;
        korkeusNaytto.TextColor = Color.Brown;
        korkeusNaytto.Color = Color.Yellow;
        korkeusNaytto.BorderColor = Color.Black;
        korkeusNaytto.Width = 100;
        korkeusNaytto.BindTo(korkeuslaskuri);
        Add(korkeusNaytto);
    }


    /// <summary>
    /// Tallentaa pisteet pisteet.xml tiedostoon.
    /// </summary>
    /// <param name="sender"></param>
    private void TallennaPisteet(Window sender)
    {
        DataStorage.Save<ScoreList>(topLista, "pisteet.xml");
    }


    /// <summary>
    /// Näyttää pelin TopTen listan ja tarvittaessa lisää nimesi listalle.
    /// </summary>
    private void NaytaTopTen()
    {
        HighScoreWindow topIkkuna = new HighScoreWindow(
                             "Parhaat pisteet",
                             "Onneksi olkoon, pääsit listalle pisteillä %p! Syötä nimesi:",
                             topLista, yhteisKorkeus);
        topIkkuna.Closed += TallennaPisteet;
        Add(topIkkuna);
    }
}
