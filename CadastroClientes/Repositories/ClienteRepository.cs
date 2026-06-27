using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Models;

namespace Repositories
{
    public class ClienteRepository
    {
        private readonly string connectionString = "Data Source=clientes.db";

        public ClienteRepository()
        {
            // Garante que as tabelas existam e que a coluna Id esteja correta
            EnsureSchema();
        }

        /// <summary>
        /// Garante esquema: cria tabelas necessárias e, se a tabela Clientes não tiver
        /// uma coluna Id INTEGER PRIMARY KEY AUTOINCREMENT, realiza migração segura.
        /// </summary>
        private void EnsureSchema()
        {
            using var conn = new SqliteConnection(connectionString);
            conn.Open();

            // Cria tabela Stats se não existir
            var sqlStats = @"CREATE TABLE IF NOT EXISTS Stats (
                                Id INTEGER PRIMARY KEY CHECK (Id = 1),
                                Cadastrados INTEGER NOT NULL DEFAULT 0,
                                Editados INTEGER NOT NULL DEFAULT 0,
                                Deletados INTEGER NOT NULL DEFAULT 0
                             );";
            using (var cmd = new SqliteCommand(sqlStats, conn))
            {
                cmd.ExecuteNonQuery();
            }
            using (var cmd = new SqliteCommand("INSERT OR IGNORE INTO Stats (Id, Cadastrados, Editados, Deletados) VALUES (1,0,0,0);", conn))
            {
                cmd.ExecuteNonQuery();
            }

            // Verifica se a tabela Clientes existe
            using (var cmd = new SqliteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='Clientes';", conn))
            {
                var exists = cmd.ExecuteScalar();
                if (exists == null)
                {
                    // Cria tabela com Id autoincremental
                    var create = @"CREATE TABLE Clientes (
                                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                        Nome TEXT NOT NULL,
                                        Email TEXT,
                                        Telefone TEXT
                                   );";
                    using var c2 = new SqliteCommand(create, conn);
                    c2.ExecuteNonQuery();
                    return;
                }
            }

            // Se a tabela existe, verificar se já tem coluna Id que é PRIMARY KEY
            bool hasIdPrimaryKey = false;
            using (var cmd = new SqliteCommand("PRAGMA table_info('Clientes');", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var colName = reader.GetString(1); // name
                    var pk = reader.GetInt32(5); // pk (0 = not pk, 1 = pk)
                    if (string.Equals(colName, "Id", StringComparison.OrdinalIgnoreCase) && pk == 1)
                    {
                        hasIdPrimaryKey = true;
                        break;
                    }
                }
            }

            if (!hasIdPrimaryKey)
            {
                // Migração segura:
                // 1) Cria tabela temporária com Id autoincrement
                // 2) Copia dados da tabela antiga para a nova (Id será gerado)
                // 3) Remove tabela antiga
                // 4) Renomeia tabela temporária para Clientes
                // Observação: preserva os dados das colunas Nome, Email, Telefone

                using var transaction = conn.BeginTransaction();

                // 1) cria tabela temporária
                var createTemp = @"CREATE TABLE IF NOT EXISTS Clientes_new (
                                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                        Nome TEXT NOT NULL,
                                        Email TEXT,
                                        Telefone TEXT
                                   );";
                using (var cmd = new SqliteCommand(createTemp, conn, transaction))
                {
                    cmd.ExecuteNonQuery();
                }

                // 2) copia dados da tabela antiga para a nova
                // tenta mapear colunas existentes Nome, Email, Telefone; se alguma não existir, usa valor vazio
                // Para ser robusto, vamos verificar colunas existentes
                var existingCols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var cmd = new SqliteCommand("PRAGMA table_info('Clientes');", conn, transaction))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        existingCols.Add(reader.GetString(1));
                    }
                }

                // Monta SELECT dinâmico para copiar apenas colunas existentes
                var selectCols = new List<string>();
                if (existingCols.Contains("Nome")) selectCols.Add("Nome");
                else selectCols.Add("'' AS Nome");

                if (existingCols.Contains("Email")) selectCols.Add("Email");
                else selectCols.Add("'' AS Email");

                if (existingCols.Contains("Telefone")) selectCols.Add("Telefone");
                else selectCols.Add("'' AS Telefone");

                var copySql = $"INSERT INTO Clientes_new (Nome, Email, Telefone) SELECT {string.Join(", ", selectCols)} FROM Clientes;";
                using (var cmd = new SqliteCommand(copySql, conn, transaction))
                {
                    cmd.ExecuteNonQuery();
                }

                // 3) renomeia tabela antiga para backup e renomeia a nova para Clientes
                using (var cmd = new SqliteCommand("ALTER TABLE Clientes RENAME TO Clientes_backup;", conn, transaction))
                {
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new SqliteCommand("ALTER TABLE Clientes_new RENAME TO Clientes;", conn, transaction))
                {
                    cmd.ExecuteNonQuery();
                }

                // 4) opcional: manter backup (Clientes_backup) ou remover. Aqui mantemos como backup.
                transaction.Commit();
            }
            // se já tem Id PK, nada a fazer
        }

        public void Adicionar(Cliente cliente)
        {
            using var conn = new SqliteConnection(connectionString);
            conn.Open();

            var sql = "INSERT INTO Clientes (Nome, Email, Telefone) VALUES (@Nome, @Email, @Telefone);";
            using (var cmd = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Nome", cliente.Nome ?? "");
                cmd.Parameters.AddWithValue("@Email", cliente.Email ?? "");
                cmd.Parameters.AddWithValue("@Telefone", cliente.Telefone ?? "");
                cmd.ExecuteNonQuery();
            }

            IncrementarStat("Cadastrados");
        }

        public void Atualizar(Cliente cliente)
        {
            using var conn = new SqliteConnection(connectionString);
            conn.Open();

            var sql = "UPDATE Clientes SET Nome = @Nome, Email = @Email, Telefone = @Telefone WHERE Id = @Id;";
            using (var cmd = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Nome", cliente.Nome ?? "");
                cmd.Parameters.AddWithValue("@Email", cliente.Email ?? "");
                cmd.Parameters.AddWithValue("@Telefone", cliente.Telefone ?? "");
                cmd.Parameters.AddWithValue("@Id", cliente.Id);
                cmd.ExecuteNonQuery();
            }

            IncrementarStat("Editados");
        }

        public void Remover(int id)
        {
            using var conn = new SqliteConnection(connectionString);
            conn.Open();

            var sql = "DELETE FROM Clientes WHERE Id = @Id;";
            using (var cmd = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }

            IncrementarStat("Deletados");
        }

        public List<Cliente> Listar()
        {
            var lista = new List<Cliente>();
            using var conn = new SqliteConnection(connectionString);
            conn.Open();

            var sql = "SELECT Id, Nome, Email, Telefone FROM Clientes ORDER BY Id;";
            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Cliente
                {
                    Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    Nome = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Email = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Telefone = reader.IsDBNull(3) ? "" : reader.GetString(3)
                });
            }

            return lista;
        }

        private void IncrementarStat(string coluna)
        {
            using var conn = new SqliteConnection(connectionString);
            conn.Open();

            var sql = $"UPDATE Stats SET {coluna} = {coluna} + 1 WHERE Id = 1;";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        public (int Cadastrados, int Editados, int Deletados) ObterStats()
        {
            using var conn = new SqliteConnection(connectionString);
            conn.Open();

            var sql = "SELECT Cadastrados, Editados, Deletados FROM Stats WHERE Id = 1;";
            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return (reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2));
            }

            return (0, 0, 0);
        }

        /// <summary>
        /// Método público para forçar a criação de IDs em registros existentes.
        /// Útil se você quiser executar manualmente após alterações.
        /// </summary>
        public void ForcarCriacaoIdsExistentes()
        {
            // Apenas chama EnsureSchema novamente (já faz a migração se necessário)
            EnsureSchema();
        }
    }
}
