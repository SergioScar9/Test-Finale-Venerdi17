using System;
using System.Collections.Generic;

#region  Conf globale

class AppContext
{
    private static AppContext istanza = null;
    
    public string Valuta { get; set; }
    public double IVA { get; set; }
    public double ScontoBase { get; set; }
    private List<string> log;
    
    private AppContext()
    {
        Valuta = "EUR";
        IVA = 0.22;
        ScontoBase = 0.0;
        log = new List<string>();
    }
    
    public static AppContext Instance
    {
        get
        {
            if (istanza == null)
                istanza = new AppContext();
            return istanza;
        }
    }
    
    public void AggiungiLog(string messaggio)
    {
        log.Add($"[{DateTime.Now:HH:mm:ss}] {messaggio}");
    }
    
    public void MostraLog()
    {
        Console.WriteLine("LOG DI SISTEMA");
        foreach (string entry in log)
        {
            Console.WriteLine(entry);
        }
    }
}

#endregion
#region Interfaccia prodotto
interface IProdotto
{
    string Descrizione();
    double PrezzoBase();
    string Codice();
}
#endregion

#region  Prodotti
class TShirt : IProdotto
{
    public string Descrizione() { return "T-Shirt"; }
    public double PrezzoBase() { return 20.00; }
    public string Codice() { return "TSHIRT"; }
}

class Mug : IProdotto
{
    public string Descrizione() { return "Tazza"; }
    public double PrezzoBase() { return 8.00; }
    public string Codice() { return "MUG"; }
}

class PhoneSkin : IProdotto
{
    public string Descrizione() { return "Skin per Smartphone"; }
    public double PrezzoBase() { return 15.00; }
    public string Codice() { return "SKIN"; }
}

class Gadget : IProdotto
{
    public string Descrizione() { return "Gadget Digitale"; }
    public double PrezzoBase() { return 12.00; }
    public string Codice() { return "GADGET"; }
}

#endregion

static class ProductFactory
{
    public static IProdotto Crea(string codice)
    {
        AppContext.Instance.AggiungiLog($"Factory: Creazione prodotto '{codice}'");
        
        switch (codice.ToUpper())
        {
            case "TSHIRT": return new TShirt();
            case "MUG": return new Mug();
            case "SKIN": return new PhoneSkin();
            case "GADGET": return new Gadget();
            default: return null;
        }
    }
}



abstract class AddonProdotto : IProdotto
{
    protected IProdotto prodotto;
    
    public AddonProdotto(IProdotto prodotto)
    {
        this.prodotto = prodotto;
    }
    
    public abstract string Descrizione();
    public abstract double PrezzoBase();
    public string Codice() { return prodotto.Codice(); }
}

class StampaFronte : AddonProdotto
{
    public StampaFronte(IProdotto prodotto) : base(prodotto) { }
    public override string Descrizione() { return prodotto.Descrizione() + " + Stampa Fronte"; }
    public override double PrezzoBase() { return prodotto.PrezzoBase() + 5.00; }
}

class StampaRetro : AddonProdotto
{
    public StampaRetro(IProdotto prodotto) : base(prodotto) { }
    public override string Descrizione() { return prodotto.Descrizione() + " + Stampa Retro"; }
    public override double PrezzoBase() { return prodotto.PrezzoBase() + 5.00; }
}

class ConfezioneRegalo : AddonProdotto
{
    public ConfezioneRegalo(IProdotto prodotto) : base(prodotto) { }
    public override string Descrizione() { return prodotto.Descrizione() + " + Confezione Regalo"; }
    public override double PrezzoBase() { return prodotto.PrezzoBase() + 3.00; }
}

class EstensioneGaranzia : AddonProdotto
{
    public EstensioneGaranzia(IProdotto prodotto) : base(prodotto) { }
    public override string Descrizione() { return prodotto.Descrizione() + " + Garanzia Estesa"; }
    public override double PrezzoBase() { return prodotto.PrezzoBase() + 10.00; }
}

class Incisione : AddonProdotto
{
    public Incisione(IProdotto prodotto) : base(prodotto) { }
    public override string Descrizione() { return prodotto.Descrizione() + " + Incisione Personalizzata"; }
    public override double PrezzoBase() { return prodotto.PrezzoBase() + 8.00; }
}



interface IPricingStrategy
{
    double CalcolaPrezzo(double prezzoBase);
    string Nome();
}

class StandardPricing : IPricingStrategy
{
    public double CalcolaPrezzo(double prezzoBase)
    {
        AppContext ctx = AppContext.Instance;
        return prezzoBase * (1 + ctx.IVA) * (1 - ctx.ScontoBase);
    }
    public string Nome() { return "Standard (+ IVA)"; }
}

class PromoPricing : IPricingStrategy
{
    public double CalcolaPrezzo(double prezzoBase)
    {
        AppContext ctx = AppContext.Instance;
        double sconto = 0.20;
        return prezzoBase * (1 + ctx.IVA) * (1 - sconto);
    }
    public string Nome() { return "Promo (-20%)"; }
}

class WholesalePricing : IPricingStrategy
{
    public double CalcolaPrezzo(double prezzoBase)
    {
        double sconto = 0.35;
        return prezzoBase * (1 - sconto);
    }
    public string Nome() { return "Wholesale (-35%, no IVA)"; }
}

class DynamicPricing : IPricingStrategy
{
    private double fattore;
    
    public DynamicPricing(double fattore)
    {
        this.fattore = fattore;
    }
    
    public double CalcolaPrezzo(double prezzoBase)
    {
        AppContext ctx = AppContext.Instance;
        return prezzoBase * fattore * (1 + ctx.IVA);
    }
    public string Nome() { return $"Dynamic (x{fattore})"; }
}


interface IObserverOrdine
{
    void Aggiorna(string evento, Ordine ordine);
}

class UINotifier : IObserverOrdine
{
    public void Aggiorna(string evento, Ordine ordine)
    {
        Console.WriteLine($"\n[UI] {evento} - Ordine {ordine.Id}");
    }
}

class LogNotifier : IObserverOrdine
{
    public void Aggiorna(string evento, Ordine ordine)
    {
        AppContext.Instance.AggiungiLog($"Ordine {ordine.Id}: {evento}");
    }
}

class EmailNotifier : IObserverOrdine
{
    public void Aggiorna(string evento, Ordine ordine)
    {
        Console.WriteLine($"\n[Email Mock] Inviata notifica: {evento}");
    }
}


class Ordine
{
    public int Id { get; private set; }
    public List<IProdotto> Prodotti { get; private set; }
    public IPricingStrategy Strategia { get; set; }
    private List<IObserverOrdine> observers;
    public string Stato { get; private set; }
    
    public Ordine(int id)
    {
        Id = id;
        Prodotti = new List<IProdotto>();
        Strategia = new StandardPricing();
        observers = new List<IObserverOrdine>();
        Stato = "In preparazione";
    }
    
    public void AggiungiObserver(IObserverOrdine observer)
    {
        observers.Add(observer);
    }
    
    public void AggiungiProdotto(IProdotto prodotto)
    {
        Prodotti.Add(prodotto);
        Notifica($"Prodotto aggiunto: {prodotto.Descrizione()}");
    }
    
    public void RimuoviProdotto(int indice)
    {
        if (indice >= 0 && indice < Prodotti.Count)
        {
            string desc = Prodotti[indice].Descrizione();
            Prodotti.RemoveAt(indice);
            Notifica($"Prodotto rimosso: {desc}");
        }
    }
    
    public void CambiaStrategia(IPricingStrategy nuovaStrategia)
    {
        Strategia = nuovaStrategia;
        Notifica($"Strategia cambiata: {nuovaStrategia.Nome()}");
    }
    
    public double CalcolaTotale()
    {
        double totale = 0;
        foreach (IProdotto p in Prodotti)
        {
            totale += Strategia.CalcolaPrezzo(p.PrezzoBase());
        }
        return totale;
    }
    
    public void Checkout()
    {
        Stato = "Completato";
        Notifica("Ordine completato e pagato!");
    }
    
    private void Notifica(string evento)
    {
        foreach (IObserverOrdine obs in observers)
        {
            obs.Aggiorna(evento, this);
        }
    }
    
    public void MostraDettagli()
    {
        Console.WriteLine($"\nOrdine {Id} ");
        Console.WriteLine($"Stato: {Stato}");
        Console.WriteLine($"Strategia: {Strategia.Nome()}");
        Console.WriteLine("Prodotti:");
        
        for (int i = 0; i < Prodotti.Count; i++)
        {
            IProdotto p = Prodotti[i];
            double prezzo = Strategia.CalcolaPrezzo(p.PrezzoBase());
            Console.WriteLine($"│   {i + 1}. {p.Descrizione()}");
            Console.WriteLine($"│      Prezzo base: €{p.PrezzoBase():F2}");
            Console.WriteLine($"│      Prezzo finale: €{prezzo:F2}");
        }
        
        Console.WriteLine($"TOTALE: €{CalcolaTotale():F2}");

    }
}



class GestoreOrdini
{
    private List<Ordine> ordini;
    private int prossimoId;
    
    public GestoreOrdini()
    {
        ordini = new List<Ordine>();
        prossimoId = 1;
    }
    
    public Ordine CreaOrdine()
    {
        Ordine ordine = new Ordine(prossimoId++);
        
        ordine.AggiungiObserver(new UINotifier());
        ordine.AggiungiObserver(new LogNotifier());
        ordine.AggiungiObserver(new EmailNotifier());
        
        ordini.Add(ordine);
        AppContext.Instance.AggiungiLog($"Nuovo ordine creato: {ordine.Id}");
        
        return ordine;
    }
    
    public void MostraTuttiOrdini()
    {

        Console.WriteLine("TUTTI GLI ORDINI");
        
        if (ordini.Count == 0)
        {
            Console.WriteLine("Nessun ordine presente.");
        }
        else
        {
            foreach (Ordine o in ordini)
            {
                o.MostraDettagli();
            }
        }
     
    }
    
    public Ordine GetOrdine(int id)
    {
        foreach (Ordine o in ordini)
        {
            if (o.Id == id)
                return o;
        }
        return null;
    }
}



class Program
{
    static void Main(string[] args)
    {

        Console.WriteLine("==MODSHOP==");

        GestoreOrdini gestore = new GestoreOrdini();
        Ordine ordineCorrente = null;
        bool continua = true;
        
        while (continua)
        {
            Console.WriteLine("MENU PRINCIPALE");
            Console.WriteLine("1.  Crea nuovo ordine");
            Console.WriteLine("2.  Aggiungi prodotto all'ordine corrente");
            Console.WriteLine("3.  Applica addon al prodotto");
            Console.WriteLine("4.  Cambia strategia pricing");
            Console.WriteLine("5.  Visualizza ordine corrente");
            Console.WriteLine("6.  Completa ordine (checkout)");
            Console.WriteLine("7. Visualizza tutti gli ordini");
            Console.WriteLine("8.  Configura sistema (AppContext)");
            Console.WriteLine("9.  Visualizza log");
            Console.WriteLine("0 Esci");

            
            if (ordineCorrente != null)
            {
                Console.WriteLine($"Ordine corrente: {ordineCorrente.Id}");
            }
            else
            {
                Console.WriteLine("Nessun ordine, crea un nuovo ordine!");
            }
            
            Console.Write("\nScelta: ");
            string scelta = Console.ReadLine();
            Console.WriteLine();
            
            switch (scelta)
            {
                case "1":
                    ordineCorrente = gestore.CreaOrdine();
                    Console.WriteLine($"Ordine {ordineCorrente.Id} creato");
                    break;
                    
                case "2":
                    if (ordineCorrente == null)
                    {
                        Console.WriteLine("Crea prima un ordine!");
                        break;
                    }
                    AggiungiProdotto(ordineCorrente);
                    break;
                    
                case "3":
                    if (ordineCorrente == null || ordineCorrente.Prodotti.Count == 0)
                    {
                        Console.WriteLine("Aggiungi prima dei prodotti");
                        break;
                    }
                    ApplicaAddon(ordineCorrente);
                    break;
                    
                case "4":
                    if (ordineCorrente == null)
                    {
                        Console.WriteLine("Crea prima un ordine");
                        break;
                    }
                    CambiaStrategia(ordineCorrente);
                    break;
                    
                case "5":
                    if (ordineCorrente == null)
                    {
                        Console.WriteLine("Nessun ordine");
                        break;
                    }
                    ordineCorrente.MostraDettagli();
                    break;
                    
                case "6":
                    if (ordineCorrente == null)
                    {
                        Console.WriteLine("Nessun ordine da completare");
                        break;
                    }
                    ordineCorrente.Checkout();
                    ordineCorrente = null;
                    break;
                    
                case "7":
                    gestore.MostraTuttiOrdini();
                    break;
                    
                case "8":
                    ConfiguraSistema();
                    break;
                    
                case "9":
                    AppContext.Instance.MostraLog();
                    break;
                    
                case "0":
                    Console.WriteLine("Arrivederci");
                    continua = false;
                    break;
                    
                default:
                    Console.WriteLine("Scelta non valida!");
                    break;
            }
            
            if (continua && scelta != "7" && scelta != "9")
            {
                Console.WriteLine("\nPremi un tasto per continuare");
                Console.ReadKey();
                Console.WriteLine();
            }
        }
    }
    
    static void AggiungiProdotto(Ordine ordine)
    {

        Console.WriteLine("CATALOGO PRODOTTI");
        Console.WriteLine("1.  TSHIRT (€20.00)");
        Console.WriteLine("2.  MUG (€8.00)");
        Console.WriteLine("3.  SKIN (€15.00)");
        Console.WriteLine("4. GADGET (€12.00)");
        Console.Write("Scelta: ");
        
        string scelta = Console.ReadLine();
        string codice = "";
        
        switch (scelta)
        {
            case "1": codice = "TSHIRT"; break;
            case "2": codice = "MUG"; break;
            case "3": codice = "SKIN"; break;
            case "4": codice = "GADGET"; break;
            default:
                Console.WriteLine("Prodotto non valido!");
                return;
        }
        
        IProdotto prodotto = ProductFactory.Crea(codice);
        ordine.AggiungiProdotto(prodotto);
        Console.WriteLine($"{prodotto.Descrizione()} aggiunto all'ordine!");
    }
    
    static void ApplicaAddon(Ordine ordine)
    {
        Console.WriteLine("Seleziona il prodotto da personalizzare:");
        for (int i = 0; i < ordine.Prodotti.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {ordine.Prodotti[i].Descrizione()}");
        }
        Console.Write("Numero prodotto: ");
        
        int indice;
        if (!int.TryParse(Console.ReadLine(), out indice) || indice < 1 || indice > ordine.Prodotti.Count)
        {
            Console.WriteLine("Indice non valido!");
            return;
        }
        indice--;
        
        Console.WriteLine("ADDON DISPONIBILI");
        Console.WriteLine("1. Stampa Fronte (+€5.00)");
        Console.WriteLine("2. Stampa Retro (+€5.00)");
        Console.WriteLine("3. Confezione Regalo (+€3.00)");
        Console.WriteLine("4. Estensione Garanzia (+€10.00)");
        Console.WriteLine("5.  Incisione Personalizzata (+€8.00)");
        Console.Write("Scelta: ");
        
        string scelta = Console.ReadLine();
        IProdotto prodotto = ordine.Prodotti[indice];
        
        switch (scelta)
        {
            case "1":
                prodotto = new StampaFronte(prodotto);
                break;
            case "2":
                prodotto = new StampaRetro(prodotto);
                break;
            case "3":
                prodotto = new ConfezioneRegalo(prodotto);
                break;
            case "4":
                prodotto = new EstensioneGaranzia(prodotto);
                break;
            case "5":
                prodotto = new Incisione(prodotto);
                break;
            default:
                Console.WriteLine("Addon non valido!");
                return;
        }
        
        ordine.Prodotti[indice] = prodotto;
        Console.WriteLine($"Addon applicato Nuovo prodotto: {prodotto.Descrizione()}");
    }
    
    static void CambiaStrategia(Ordine ordine)
    {

        Console.WriteLine("STRATEGIE DI PRICING");
        Console.WriteLine("1.  Standard (prezzo + IVA)");
        Console.WriteLine("2. Promo (sconto 20%)");
        Console.WriteLine("3. Wholesale (sconto 35%, no IVA)");
        Console.WriteLine("4.  Dynamic (fattore personalizzato)");
        Console.Write("Scelta: ");
        
        string scelta = Console.ReadLine();
        IPricingStrategy strategia = null;
        
        switch (scelta)
        {
            case "1":
                strategia = new StandardPricing();
                break;
            case "2":
                strategia = new PromoPricing();
                break;
            case "3":
                strategia = new WholesalePricing();
                break;
            case "4":
                Console.Write("Inserisci fattore moltiplicativo: ");
                double fattore;
                if (double.TryParse(Console.ReadLine(), out fattore))
                {
                    strategia = new DynamicPricing(fattore);
                }
                break;
        }
        
        if (strategia != null)
        {
            ordine.CambiaStrategia(strategia);
            Console.WriteLine($"Strategia cambiata: {strategia.Nome()}");
        }
        else
        {
            Console.WriteLine("Strategia non valida!");
        }
    }
    
    static void ConfiguraSistema()
    {
        AppContext ctx = AppContext.Instance;
        
        Console.WriteLine("CONFIGURAZIONE SISTEMA");
        Console.WriteLine($"Valuta corrente: {ctx.Valuta}");
        Console.WriteLine($"IVA corrente: {ctx.IVA * 100}%");
        Console.WriteLine($"Sconto base: {ctx.ScontoBase * 100}%");
        
        Console.Write("Vuoi modificare le impostazioni? (s/n): ");
        if (Console.ReadLine().ToLower() == "s")
        {
            Console.Write("Nuova valuta (lascia vuoto per non cambiare): ");
            string val = Console.ReadLine();
            if (!string.IsNullOrEmpty(val))
                ctx.Valuta = val;
            
            Console.Write("Nuova IVA in % (lascia vuoto per non cambiare): ");
            string iva = Console.ReadLine();
            if (!string.IsNullOrEmpty(iva) && double.TryParse(iva, out double ivaVal))
                ctx.IVA = ivaVal / 100.0;
            
            Console.Write("Nuovo sconto base in % (lascia vuoto per non cambiare): ");
            string sconto = Console.ReadLine();
            if (!string.IsNullOrEmpty(sconto) && double.TryParse(sconto, out double scontoVal))
                ctx.ScontoBase = scontoVal / 100.0;
            
            Console.WriteLine("Configurazione aggiornata");
            ctx.AggiungiLog("Configurazione sistema modificata");
        }
    }
}