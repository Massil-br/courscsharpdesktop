// namespace calculatrice;
// On importe les bibliothèques nécessaires
using System;
using System.Drawing;          // Pour les couleurs et les positions
using System.Windows.Forms;    // Pour les contrôles WinForms

namespace calculatrice
{
    public partial class Form1 : Form
    {
        // === ÉTAT DE LA CALCULATRICE ===
        // Ces variables gardent en mémoire ce qui se passe
        private TextBox afficheur = null!;         // L'écran de la calculatrice
        private string operandePrecedent = "";     // Le nombre avant l'opérateur (ex: "5" dans 5+3)
        private string operateur = "";             // L'opérateur choisi (+, -, ×, ÷)
        private bool nouvelleEntree = true;        // Vrai = le prochain chiffre remplace l'afficheur
        private List<string> history  = [];
        private FlowLayoutPanel historyPanel = null!;
        public Form1()
        {
            InitializeComponent();

            // Configuration de la fenêtre principale
            Text = "Calculatrice";                                 // Titre de la fenêtre
            Size = new Size(320, 700);                             // Taille : 320 x 480 pixels
            StartPosition = FormStartPosition.CenterScreen;       // Centrer à l'écran
            FormBorderStyle = FormBorderStyle.FixedSingle;        // Empêcher le redimensionnement
            MaximizeBox = false;                                   // Pas de bouton agrandir
            BackColor = Color.FromArgb(30, 40, 56);               // Fond sombre (bleu-gris foncé)

            // Créer tous les éléments de l'interface
            CreerInterface();
        }

        private void CreerInterface()
        {
            // =============================================
            // 1. L'AFFICHEUR (en haut de la fenêtre)
            // =============================================
            // C'est un TextBox en lecture seule, aligné à droite
            // Comme l'écran d'une vraie calculatrice
            afficheur = new TextBox
            {
                Text = "0",                                          // Affiche 0 au démarrage
                Font = new Font("Segoe UI", 32, FontStyle.Bold),    // Grande police
                ForeColor = Color.White,                             // Texte blanc
                BackColor = Color.FromArgb(40, 52, 71),              // Fond un peu plus clair que la fenêtre
                TextAlign = HorizontalAlignment.Right,               // Chiffres alignés à droite
                ReadOnly = true,                                     // L'utilisateur ne peut pas taper dedans
                Dock = DockStyle.Top,                                // Collé en haut de la fenêtre
                Height = 80,                                         // 80 pixels de haut
                BorderStyle = BorderStyle.None                       // Pas de bordure
            };

            // =============================================
            // 2. LA GRILLE DE BOUTONS
            // =============================================
            // TableLayoutPanel = comme CSS Grid !
            // On crée une grille de 4 colonnes × 5 lignes
            var grille = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,      // Remplit tout l'espace restant sous l'afficheur
                ColumnCount = 4,            // 4 colonnes
                RowCount = 5,               // 5 lignes
                Padding = new Padding(5),   // 5px de marge interne
                BackColor = Color.FromArgb(30, 40, 56)  // Même fond que la fenêtre
            };

            // Chaque colonne fait 25% de la largeur (4 × 25% = 100%)
            for (int i = 0; i < 4; i++)
                grille.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            // Chaque ligne fait 20% de la hauteur (5 × 20% = 100%)
            for (int i = 0; i < 5; i++)
                grille.RowStyles.Add(new RowStyle(SizeType.Percent, 20));

            // =============================================
            // 3. DISPOSITION DES BOUTONS
            // =============================================
            // Tableau 2D qui définit le texte de chaque bouton
            // Ligne par ligne, de haut en bas
            string[,] boutons = {
                { "C",  "±",  "%",  "÷" },    // Ligne 1 : fonctions + division
                { "7",  "8",  "9",  "×" },    // Ligne 2 : 7, 8, 9, multiplication
                { "4",  "5",  "6",  "-" },    // Ligne 3 : 4, 5, 6, soustraction
                { "1",  "2",  "3",  "+" },    // Ligne 4 : 1, 2, 3, addition
                { "0",  ".",  "⌫",  "=" }     // Ligne 5 : 0, point, effacer, égal
            };

            // Parcourir chaque case de la grille et créer un bouton
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    string texte = boutons[row, col];
                    var btn = CreerBouton(texte);       // Créer le bouton (méthode suivante)
                    grille.Controls.Add(btn, col, row); // L'ajouter à la grille à la position (col, row)
                }
            }

            // =============================================
            // 4. AJOUTER À LA FENÊTRE
            // =============================================
            // IMPORTANT : l'ordre d'ajout compte !
            // D'abord la grille (elle se met en Fill), puis l'afficheur (il se met en Top)
            Controls.Add(grille);
            Controls.Add(afficheur);
        }

        /// <summary>
        /// Crée un bouton de la calculatrice avec le bon style
        /// </summary>
        /// <param name="texte">Le texte affiché sur le bouton (ex: "7", "+", "=")</param>
        /// <returns>Le bouton configuré</returns>
        private Button CreerBouton(string texte)
        {
            var btn = new Button
            {
                Text = texte,
                Dock = DockStyle.Fill,                                // Remplit toute la cellule de la grille
                Font = new Font("Segoe UI", 18, FontStyle.Bold),     // Police assez grande
                FlatStyle = FlatStyle.Flat,                           // Style plat (moderne)
                Margin = new Padding(3),                              // 3px d'espace entre les boutons
                Cursor = Cursors.Hand                                 // Curseur main au survol
            };
            btn.FlatAppearance.BorderSize = 0;  // Pas de bordure

            // === COULEURS SELON LE TYPE DE BOUTON ===
            if (texte == "=")
            {
                // Bouton ÉGAL = vert-teal (couleur d'action principale)
                btn.BackColor = Color.FromArgb(46, 196, 182);   // #2EC4B6
                btn.ForeColor = Color.White;
            }
            else if ("÷×-+".Contains(texte))
            {
                // Boutons OPÉRATEURS = orange
                btn.BackColor = Color.FromArgb(249, 115, 22);   // #F97316
                btn.ForeColor = Color.White;
            }
            else if (texte == "C" || texte == "±" || texte == "%" || texte == "⌫")
            {
                // Boutons FONCTIONS = gris foncé
                btn.BackColor = Color.FromArgb(55, 71, 95);
                btn.ForeColor = Color.White;
            }
            else
            {
                // Boutons CHIFFRES = gris-bleu
                btn.BackColor = Color.FromArgb(70, 90, 120);
                btn.ForeColor = Color.White;
            }

            // Brancher l'événement Click
            // Quand l'utilisateur clique sur CE bouton, la méthode BoutonClick sera appelée
            btn.Click += BoutonClick;

            return btn;
        }

        /// <summary>
        /// Gère le clic sur n'importe quel bouton de la calculatrice.
        /// On récupère le texte du bouton cliqué et on agit en fonction.
        /// </summary>
        private void BoutonClick(object? sender, EventArgs e)
        {
            // Vérifier que c'est bien un bouton qui a déclenché l'événement
            if (sender is not Button btn) return;

            // Récupérer le texte du bouton (ex: "7", "+", "=", "C"...)
            string texte = btn.Text;

            switch (texte)
            {
                // =============================================
                // BOUTON C : Tout effacer (Clear)
                // =============================================
                case "C":
                    operandePrecedent = "";
                    operateur = "";
                    afficheur.Text = "0";
                    nouvelleEntree = true;
                    break;

                // =============================================
                // BOUTON ⌫ : Effacer le dernier caractère (Backspace)
                // =============================================
                case "⌫":
                    if (afficheur.Text.Length > 1)
                    {
                        // Enlever le dernier caractère
                        // "123" → "12"
                        afficheur.Text = afficheur.Text[..^1];
                    }
                    else
                    {
                        // Si un seul chiffre, remettre à 0
                        afficheur.Text = "0";
                    }
                    break;

                // =============================================
                // BOUTON ± : Changer le signe (positif ↔ négatif)
                // =============================================
                case "±":
                    if (afficheur.Text != "0" && afficheur.Text != "")
                    {
                        if (afficheur.Text.StartsWith('-'))
                            afficheur.Text = afficheur.Text[1..];     // "-5" → "5"
                        else
                            afficheur.Text = "-" + afficheur.Text;    // "5" → "-5"
                    }
                    break;

                // =============================================
                // BOUTON % : Calculer le pourcentage
                // =============================================
                case "%":
                    if (double.TryParse(afficheur.Text, out double pct))
                    {
                        afficheur.Text = (pct / 100).ToString();
                    }
                    break;

                // =============================================
                // OPÉRATEURS : +, -, ×, ÷
                // =============================================
                case "+":
                case "-":
                case "×":
                case "÷":
                    // Sauvegarder le nombre actuel et l'opérateur
                    operandePrecedent = afficheur.Text;
                    operateur = texte;
                    nouvelleEntree = true;  // Le prochain chiffre remplacera l'afficheur
                    break;

                // =============================================
                // BOUTON = : Calculer le résultat
                // =============================================
                case "=":
                    Calculer();
                    break;

                // =============================================
                // BOUTON . : Point décimal
                // =============================================
                case ".":
                    if (nouvelleEntree)
                    {
                        afficheur.Text = "0.";
                        nouvelleEntree = false;
                    }
                    else if (!afficheur.Text.Contains('.'))
                    {
                        // N'ajouter le point que s'il n'y en a pas déjà un
                        afficheur.Text += ".";
                    }
                    break;

                // =============================================
                // CHIFFRES : 0, 1, 2, 3, 4, 5, 6, 7, 8, 9
                // =============================================
                default:
                    if (nouvelleEntree)
                    {
                        // Nouvelle saisie → remplacer l'afficheur
                        afficheur.Text = texte;
                        nouvelleEntree = false;
                    }
                    else
                    {
                        // Continuer la saisie → ajouter le chiffre
                        if (afficheur.Text == "0")
                            afficheur.Text = texte;     // Remplacer le 0 initial
                        else
                            afficheur.Text += texte;    // Ajouter à la suite
                    }
                    break;
            }
        }

        /// <summary>
        /// Effectue le calcul entre operandePrecedent et l'afficheur,
        /// selon l'opérateur choisi.
        /// Exemple : si operandePrecedent = "5", operateur = "+", afficheur = "3"
        ///           → résultat = 8
        /// </summary>
        private void Calculer()
        {
            // Vérifier qu'on a bien un opérateur et un nombre précédent
            if (string.IsNullOrEmpty(operateur) || string.IsNullOrEmpty(operandePrecedent))
                return;

            // Convertir les textes en nombres
            // TryParse retourne false si la conversion échoue (texte invalide)
            if (!double.TryParse(operandePrecedent, out double a)
               ||  !double.TryParse(afficheur.Text, out double b))
            {
                afficheur.Text = "Erreur";
                nouvelleEntree = true;
                return;
            }

            // Calculer selon l'opérateur (switch expression C# moderne)
            double resultat = operateur switch
            {
                "+" => a + b,                                  // Addition
                "-" => a - b,                                  // Soustraction
                "×" => a * b,                                  // Multiplication
                "÷" => b != 0 ? a / b : double.NaN,          // Division (attention au zéro !)
                _ => 0                                         // Cas par défaut (ne devrait pas arriver)
            };

            // Vérifier si le résultat est valide
            if (double.IsNaN(resultat) || double.IsInfinity(resultat))
            {
                afficheur.Text = "Erreur";   // Division par zéro ou résultat invalide
            }
            else
            {
                afficheur.Text = resultat.ToString();
            }

            // Réinitialiser pour le prochain calcul
            operateur = "";
            operandePrecedent = "";
            nouvelleEntree = true;
        }

    }
}