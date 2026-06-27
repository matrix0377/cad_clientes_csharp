using System.Collections.Generic;
using System.Data.SQLite;

public class ClienteRepository
{
    private string connectionString = "Data Source=clientes.db;Version=3;";

    public ClienteRepository()
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = @"CREATE TABLE IF NOT EXISTS Clientes (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Nome TEXT NOT NULL,
                            Email TEXT,
                            Telefone TEXT)";
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }

    public void Adicionar(Cliente cliente)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "INSERT INTO Clientes (Nome, Email, Telefone) VALUES (@Nome, @Email, @Telefone)";
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Nome", cliente.Nome);
                cmd.Parameters.AddWithValue("@Email", cliente.Email);
                cmd.Parameters.AddWithValue("@Telefone", cliente.Telefone);
                cmd.ExecuteNonQuery();
            }
        }
    }

    public List<Cliente> Listar()
    {
        var lista = new List<Cliente>();
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "SELECT * FROM Clientes";
            using (var cmd = new SQLiteCommand(sql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new Cliente
                    {
                        Id = reader.GetInt32(0),
                        Nome = reader.GetString(1),
                        Email = reader.GetString(2),
                        Telefone = reader.GetString(3)
                    });
                }
            }
        }
        return lista;
    }
}
