// Data/ContactRepository.cs
// Le Repository gère toutes les opérations sur la base de données.
// C'est l'équivalent d'un fichier "contactService.js" ou "contactDAO.java".
// Il parle à SQLite et retourne des objets Contact.

using Microsoft.Data.Sqlite;
using ContactManager.Models;

namespace ContactManager.Data
{
    public class ContactRepository
    {
        // La chaîne de connexion indique le chemin du fichier SQLite
        // "Data Source=contacts.db" → crée/ouvre le fichier contacts.db
        private readonly string _connectionString;

        /// <summary>
        /// Constructeur : initialise la connexion et crée la table si elle n'existe pas
        /// </summary>
        /// <param name="dbPath">Chemin du fichier de base de données (par défaut : contacts.db)</param>
        public ContactRepository(string dbPath = "contacts.db")
        {
            _connectionString = $"Data Source={dbPath}";
            InitDB();
        }

        /// <summary>
        /// Crée la table "contacts" si elle n'existe pas encore.
        /// C'est comme un "CREATE TABLE IF NOT EXISTS" en SQL classique.
        /// </summary>
        private void InitDB()
        {
            // "using" ferme automatiquement la connexion à la fin du bloc
            // C'est comme un try/finally en Java, ou un "defer" en Go
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS contacts (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    nom TEXT NOT NULL,
                    email TEXT NOT NULL DEFAULT '',
                    telephone TEXT NOT NULL DEFAULT ''
                )";
            cmd.ExecuteNonQuery();
        }

        // =============================================
        // READ : Lire tous les contacts
        // =============================================
        /// <summary>
        /// Récupère tous les contacts de la base, triés par nom.
        /// C'est comme un SELECT * FROM contacts ORDER BY nom.
        /// </summary>
        public List<Contact> GetAll()
        {
            var contacts = new List<Contact>();

            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, nom, email, telephone FROM contacts ORDER BY nom";

            // ExecuteReader() exécute la requête et retourne un "curseur"
            // On lit ligne par ligne avec reader.Read()
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                contacts.Add(new Contact
                {
                    Id = reader.GetInt32(0),          // Colonne 0 = id
                    Name = reader.GetString(1),        // Colonne 1 = nom
                    Email = reader.GetString(2),      // Colonne 2 = email
                    Phone = reader.GetString(3)   // Colonne 3 = telephone
                });
            }

            return contacts;
        }

        // =============================================
        // CREATE : Ajouter un contact
        // =============================================
        /// <summary>
        /// Ajoute un nouveau contact dans la base de données.
        /// Utilise des paramètres SQL ($nom, $email...) pour éviter l'injection SQL.
        /// </summary>
        public void Add(Contact contact)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();

            // IMPORTANT : on utilise des PARAMÈTRES ($nom, $email, $tel)
            // et JAMAIS de la concaténation de chaînes !
            // Mauvais : $"INSERT ... VALUES ('{contact.Nom}')"  ← INJECTION SQL !
            // Bon     : "INSERT ... VALUES ($nom)" + AddWithValue  ← SÉCURISÉ
            cmd.CommandText = @"
                INSERT INTO contacts (nom, email, telephone)
                VALUES ($nom, $email, $tel)";
            cmd.Parameters.AddWithValue("$nom", contact.Name);
            cmd.Parameters.AddWithValue("$email", contact.Email);
            cmd.Parameters.AddWithValue("$tel", contact.Phone);
            cmd.ExecuteNonQuery();

            // Récupérer l'ID auto-généré par SQLite
            cmd.CommandText = "SELECT last_insert_rowid()";
            contact.Id = Convert.ToInt32(cmd.ExecuteScalar());
        }

        // =============================================
        // UPDATE : Modifier un contact existant
        // =============================================
        /// <summary>
        /// Met à jour un contact existant dans la base de données.
        /// On identifie le contact par son Id.
        /// </summary>
        public void Update(Contact contact)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE contacts 
                SET nom = $nom, email = $email, telephone = $tel
                WHERE id = $id";
            cmd.Parameters.AddWithValue("$id", contact.Id);
            cmd.Parameters.AddWithValue("$nom", contact.Name);
            cmd.Parameters.AddWithValue("$email", contact.Email);
            cmd.Parameters.AddWithValue("$tel", contact.Phone);
            cmd.ExecuteNonQuery();
        }

        // =============================================
        // DELETE : Supprimer un contact
        // =============================================
        /// <summary>
        /// Supprime un contact de la base de données par son Id.
        /// </summary>
        public void Delete(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM contacts WHERE id = $id";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        // =============================================
        // SEARCH : Rechercher des contacts
        // =============================================
        /// <summary>
        /// Recherche des contacts par nom, email ou téléphone.
        /// Utilise LIKE pour une recherche partielle (contient).
        /// </summary>
        public List<Contact> Search(string query)
        {
            var contacts = new List<Contact>();

            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            // LIKE '%query%' → cherche "query" n'importe où dans le texte
            // Le || est l'opérateur de concaténation en SQL (pas le OR logique !)
            cmd.CommandText = @"
                SELECT id, nom, email, telephone FROM contacts
                WHERE nom LIKE '%' || $q || '%'
                   OR email LIKE '%' || $q || '%'
                   OR telephone LIKE '%' || $q || '%'
                ORDER BY nom";
            cmd.Parameters.AddWithValue("$q", query);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                contacts.Add(new Contact
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2),
                    Phone = reader.GetString(3)
                });
            }

            return contacts;
        }
    }
}