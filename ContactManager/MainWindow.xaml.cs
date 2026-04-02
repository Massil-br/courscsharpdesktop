using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ContactManager.Data;
using ContactManager.Models;

namespace ContactManager
{
    public partial class MainWindow : Window
    {
        // === DÉPENDANCES ===
        // Le repository gère l'accès à la base de données
        private readonly ContactRepository _repo = new();

        // ObservableCollection = une liste qui NOTIFIE L'UI quand on ajoute/supprime
        // C'est comme un state React : quand ça change, le rendu se met à jour
        private ObservableCollection<Contact> _contacts = new();

        // Le contact actuellement sélectionné dans le DataGrid
        private Contact? _selectedContact;

        // =============================================
        // CONSTRUCTEUR : s'exécute au démarrage
        // =============================================
        public MainWindow()
        {
            InitializeComponent();  // Charge le XAML
            ChargerContacts();      // Charge les contacts depuis la BDD
        }

        // =============================================
        // CHARGER : Récupérer tous les contacts depuis la BDD
        // =============================================
        private void ChargerContacts()
        {                                                                                                                                                                                    
            var liste = _repo.GetAll();
            _contacts = new ObservableCollection<Contact>(liste);

            // Lier la liste au DataGrid (comme itemsSource={contacts} en React)
            dgContacts.ItemsSource = _contacts;

            // Mettre à jour le compteur
            lblCount.Text = $"{_contacts.Count} contact(s)";
        }

        // =============================================
        // AJOUTER : Créer un nouveau contact
        // =============================================
        private void BtnAjouter_Click(object sender, RoutedEventArgs e)
        {
            // Validation : le nom est obligatoire
            if (string.IsNullOrWhiteSpace(txtNom.Text))
            {
                MessageBox.Show(
                    "Le nom est obligatoire !",
                    "Champ manquant",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                txtNom.Focus();  // Mettre le curseur dans le champ nom
                return;
            }

            // Créer l'objet Contact
            var contact = new Contact
            {
                Name = txtNom.Text.Trim(),        // Trim() enlève les espaces avant/après
                Email = txtEmail.Text.Trim(),
                Phone = txtTel.Text.Trim()
            };

            // Sauvegarder dans la base de données
            _repo.Add(contact);

            // Ajouter à la liste affichée (le DataGrid se met à jour automatiquement)
            _contacts.Add(contact);

            // Vider le formulaire et mettre à jour le compteur
            ViderFormulaire();
            lblCount.Text = $"{_contacts.Count} contact(s)";
        }

        // =============================================
        // MODIFIER : Mettre à jour un contact existant
        // =============================================
        private void BtnModifier_Click(object sender, RoutedEventArgs e)
        {
            // Vérifier qu'un contact est sélectionné
            if (_selectedContact == null)
            {
                MessageBox.Show(
                    "Sélectionne un contact dans la liste d'abord !",
                    "Aucune sélection",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // Validation
            if (string.IsNullOrWhiteSpace(txtNom.Text))
            {
                MessageBox.Show("Le nom est obligatoire !", "Champ manquant",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Mettre à jour les propriétés du contact sélectionné
            _selectedContact.Name = txtNom.Text.Trim();
            _selectedContact.Email = txtEmail.Text.Trim();
            _selectedContact.Phone = txtTel.Text.Trim();

            // Sauvegarder dans la BDD
            _repo.Update(_selectedContact);

            // Rafraîchir la liste (nécessaire car Contact n'implémente pas INotifyPropertyChanged)
            ChargerContacts();
            ViderFormulaire();
        }

        // =============================================
        // SUPPRIMER : Effacer un contact
        // =============================================
        private void BtnSupprimer_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedContact == null)
            {
                MessageBox.Show(
                    "Sélectionne un contact dans la liste d'abord !",
                    "Aucune sélection",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // Demander confirmation (comme confirm() en JavaScript)
            var result = MessageBox.Show(
                $"Supprimer le contact \"{_selectedContact.Name}\" ?",
                "Confirmer la suppression",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Supprimer de la BDD
                _repo.Delete(_selectedContact.Id);

                // Supprimer de la liste affichée (le DataGrid se met à jour)
                _contacts.Remove(_selectedContact);

                ViderFormulaire();
                lblCount.Text = $"{_contacts.Count} contact(s)";
            }
        }

        // =============================================
        // SÉLECTION : Quand l'utilisateur clique sur un contact dans le DataGrid
        // =============================================
        private void DgContacts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Récupérer le contact sélectionné
            _selectedContact = dgContacts.SelectedItem as Contact;

            if (_selectedContact != null)
            {
                // Remplir le formulaire avec les données du contact sélectionné
                txtNom.Text = _selectedContact.Name;
                txtEmail.Text = _selectedContact.Email;
                txtTel.Text = _selectedContact.Phone;
            }
        }

        // =============================================
        // RECHERCHE : Filtrer les contacts en temps réel
        // =============================================
        private void TxtRecherche_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = txtRecherche.Text.Trim();

            // Si la recherche est vide, afficher tous les contacts
            // Sinon, filtrer avec la méthode Search du repository
            var resultats = string.IsNullOrEmpty(query)
                ? _repo.GetAll()
                : _repo.Search(query);

            _contacts = new ObservableCollection<Contact>(resultats);
            dgContacts.ItemsSource = _contacts;
            lblCount.Text = $"{_contacts.Count} contact(s)";
        }

        // =============================================
        // VIDER : Remettre le formulaire à zéro
        // =============================================
        private void BtnVider_Click(object sender, RoutedEventArgs e)
        {
            ViderFormulaire();
        }

        /// <summary>
        /// Vide tous les champs du formulaire et désélectionne le contact.
        /// </summary>
        private void ViderFormulaire()
        {
            txtNom.Text = "";
            txtEmail.Text = "";
            txtTel.Text = "";
            _selectedContact = null;
            dgContacts.SelectedItem = null;
        }
    }
}