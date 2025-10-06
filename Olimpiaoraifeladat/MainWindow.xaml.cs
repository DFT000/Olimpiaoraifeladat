using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace Olimpiaoraifeladat
{
    public partial class MainWindow : Window
    {
        private List<OlimpiaRecord> adatBazis = new List<OlimpiaRecord>();
        private List<OlimpiaRecord> eredetiLista = new List<OlimpiaRecord>();
        private string adatFajl = "olimpia.txt";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                adatBazis = Beolvas(adatFajl);
                eredetiLista = adatBazis.Select(r => new OlimpiaRecord { Ev = r.Ev, Nev = r.Nev, Orszag = r.Orszag, Sportag = r.Sportag, Pont = r.Pont }).ToList();
                dgRecords.ItemsSource = adatBazis;
                tbStatus.Text = $"Beolvasva: {adatBazis.Count} rekord.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba: " + ex.Message);
            }
        }

        private List<OlimpiaRecord> Beolvas(string path)
        {
            var lista = new List<OlimpiaRecord>();
            if (!File.Exists(path)) throw new FileNotFoundException("Hiányzó fájl: " + path);
            var sorok = File.ReadAllLines(path);
            foreach (var s in sorok)
            {
                if (string.IsNullOrWhiteSpace(s)) continue;
                var parts = s.Split(';');
                if (parts.Length < 5) continue;
                if (!int.TryParse(parts[0].Trim(), out int ev)) continue;
                var rekord = new OlimpiaRecord
                {
                    Ev = ev,
                    Nev = parts[1].Trim(),
                    Orszag = parts[2].Trim(),
                    Sportag = parts[3].Trim(),
                    Pont = int.TryParse(parts[4].Trim(), out int p) ? p : 0
                };
                lista.Add(rekord);
            }
            return lista;
        }

        private void BtnShowAll_Click(object sender, RoutedEventArgs e)
        {
            dgRecords.ItemsSource = adatBazis;
            tbStatus.Text = $"Mutat minden adat ({adatBazis.Count}).";
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            adatBazis = eredetiLista.Select(r => new OlimpiaRecord { Ev = r.Ev, Nev = r.Nev, Orszag = r.Orszag, Sportag = r.Sportag, Pont = r.Pont }).ToList();
            dgRecords.ItemsSource = adatBazis;
            tbStatus.Text = "Visszaállítva.";
        }

        private void BtnShowCounts_Click(object sender, RoutedEventArgs e)
        {
            int ossz = adatBazis.Count;
            int hun = adatBazis.Count(r => r.Orszag.Equals("HUN", StringComparison.OrdinalIgnoreCase));
            double szazalek = ossz == 0 ? 0.0 : (100.0 * hun / ossz);
            MessageBox.Show($"Összes: {ossz}\nMagyar: {hun}\nArány: {szazalek:F2} %");
            dgRecords.ItemsSource = adatBazis.Where(r => r.Orszag.Equals("HUN", StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private void BtnUSAAvg_Click(object sender, RoutedEventArgs e)
        {
            var usa = adatBazis.Where(r => r.Orszag.Equals("USA", StringComparison.OrdinalIgnoreCase)).ToList();
            if (usa.Count == 0) { MessageBox.Show("Nincs USA adat."); return; }
            double avg = usa.Average(r => r.Pont);
            MessageBox.Show($"USA átlag: {avg:F2}");
        }

        private void BtnLatestYear_Click(object sender, RoutedEventArgs e)
        {
            if (adatBazis.Count == 0) { MessageBox.Show("Nincs adat."); return; }
            int legutolso = adatBazis.Max(r => r.Ev);
            var talalatok = adatBazis.Where(r => r.Ev == legutolso).ToList();
            string msg = $"Legutolsó év: {legutolso}\n";
            foreach (var t in talalatok) msg += $"{t.Nev} - {t.Sportag}\n";
            MessageBox.Show(msg);
            dgRecords.ItemsSource = talalatok;
        }

        private void BtnSearchSport_Click(object sender, RoutedEventArgs e)
        {
            string keres = txtSportSearch.Text?.Trim();
            if (string.IsNullOrEmpty(keres)) { MessageBox.Show("Adj meg keresési kifejezést."); return; }
            var talalatok = adatBazis.Where(r => r.Sportag.IndexOf(keres, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            MessageBox.Show($"Találatok: {talalatok.Count}");
            dgRecords.ItemsSource = talalatok;
        }

        private void BtnAggregateCountries_Click(object sender, RoutedEventArgs e)
        {
            var osszeg = adatBazis.GroupBy(r => r.Orszag.ToUpper()).Select(g => new { Orszag = g.Key, OsszPont = g.Sum(r => r.Pont) }).OrderByDescending(x => x.OsszPont).ThenBy(x => x.Orszag).ToList();
            string msg = "";
            foreach (var o in osszeg) msg += $"{o.Orszag}: {o.OsszPont}\n";
            MessageBox.Show(msg);
            dgRecords.ItemsSource = osszeg;
        }
    }
}