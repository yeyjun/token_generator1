using MySqlConnector;

namespace TokenGenerator.Data
{
    public class DatabaseService
    {
        static readonly string[] DefaultBusinesses = { "Baclig Minimart", "Baclig Farms" };

        public MySqlConnection CreateConnection() => new(DatabaseSettings.ConnectionString);

        public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);
            return connection.State == System.Data.ConnectionState.Open;
        }

        public async Task EnsureSchemaAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using (var create = new MySqlCommand(@"
CREATE TABLE IF NOT EXISTS saved_texts (
  id INT AUTO_INCREMENT PRIMARY KEY,
  content TEXT NOT NULL,
  cipher_text TEXT NULL,
  cipher_shift INT NULL,
  source VARCHAR(20) NOT NULL DEFAULT 'manual',
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);", connection))
            {
                await create.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (var alter = new MySqlCommand(@"
ALTER TABLE saved_texts
  ADD COLUMN IF NOT EXISTS cipher_text TEXT NULL,
  ADD COLUMN IF NOT EXISTS cipher_shift INT NULL;", connection))
            {
                await alter.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (var createTokens = new MySqlCommand(@"
CREATE TABLE IF NOT EXISTS tokens (
  id INT AUTO_INCREMENT PRIMARY KEY,
  token VARCHAR(64) NOT NULL,
  business VARCHAR(50) NULL,
  is_active TINYINT(1) NOT NULL DEFAULT 1,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);", connection))
            {
                await createTokens.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (var alterTokens = new MySqlCommand(@"
ALTER TABLE tokens
  ADD COLUMN IF NOT EXISTS business VARCHAR(50) NULL,
  ADD COLUMN IF NOT EXISTS is_active TINYINT(1) NOT NULL DEFAULT 1;", connection))
            {
                await alterTokens.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (var createBusinesses = new MySqlCommand(@"
CREATE TABLE IF NOT EXISTS businesses (
  id INT AUTO_INCREMENT PRIMARY KEY,
  name VARCHAR(50) NOT NULL UNIQUE,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);", connection))
            {
                await createBusinesses.ExecuteNonQueryAsync(cancellationToken);
            }

            foreach (var business in DefaultBusinesses)
            {
                await using var seed = new MySqlCommand(
                    "INSERT IGNORE INTO businesses (name) VALUES (@name);", connection);
                seed.Parameters.AddWithValue("@name", business);
                await seed.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        public async Task<List<string>> GetBusinessesAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = new MySqlCommand("SELECT name FROM businesses ORDER BY name;", connection);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var businesses = new List<string>();
            while (await reader.ReadAsync(cancellationToken))
            {
                businesses.Add(reader.GetString(0));
            }

            return businesses;
        }

        public async Task AddBusinessAsync(string name, CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = new MySqlCommand(
                "INSERT IGNORE INTO businesses (name) VALUES (@name);", connection);
            command.Parameters.AddWithValue("@name", name);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task SaveTokenAsync(string token, string? business, CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);

            object? existingId;
            await using (var select = new MySqlCommand(
                business is null
                    ? "SELECT id FROM tokens WHERE business IS NULL ORDER BY id LIMIT 1;"
                    : "SELECT id FROM tokens WHERE business = @business ORDER BY id LIMIT 1;", connection))
            {
                if (business is not null)
                {
                    select.Parameters.AddWithValue("@business", business);
                }

                existingId = await select.ExecuteScalarAsync(cancellationToken);
            }

            if (existingId is not null)
            {
                await using var update = new MySqlCommand(
                    "UPDATE tokens SET token = @token, is_active = 1, created_at = CURRENT_TIMESTAMP WHERE id = @id;", connection);
                update.Parameters.AddWithValue("@token", token);
                update.Parameters.AddWithValue("@id", existingId);
                await update.ExecuteNonQueryAsync(cancellationToken);
                return;
            }

            await using var insert = new MySqlCommand(
                "INSERT INTO tokens (token, business, is_active) VALUES (@token, @business, 1);", connection);
            insert.Parameters.AddWithValue("@token", token);
            insert.Parameters.AddWithValue("@business", (object?)business ?? DBNull.Value);
            await insert.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task<(string Token, bool IsActive)?> GetTokenAsync(string? business, CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var select = new MySqlCommand(
                business is null
                    ? "SELECT token, is_active FROM tokens WHERE business IS NULL ORDER BY id LIMIT 1;"
                    : "SELECT token, is_active FROM tokens WHERE business = @business ORDER BY id LIMIT 1;", connection);

            if (business is not null)
            {
                select.Parameters.AddWithValue("@business", business);
            }

            await using var reader = await select.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            return (reader.GetString(0), reader.GetBoolean(1));
        }

        public async Task SetTokenActiveAsync(string? business, bool isActive, CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var update = new MySqlCommand(
                business is null
                    ? "UPDATE tokens SET is_active = @isActive WHERE business IS NULL;"
                    : "UPDATE tokens SET is_active = @isActive WHERE business = @business;", connection);
            update.Parameters.AddWithValue("@isActive", isActive);

            if (business is not null)
            {
                update.Parameters.AddWithValue("@business", business);
            }

            await update.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task<bool> ContentExistsAsync(string content, CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = new MySqlCommand(
                "SELECT 1 FROM saved_texts WHERE content = @content LIMIT 1;", connection);
            command.Parameters.AddWithValue("@content", content);
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result is not null;
        }

        public async Task SaveTextAsync(string content, string cipherText, int cipherShift, string source, CancellationToken cancellationToken = default)
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = new MySqlCommand(
                "INSERT INTO saved_texts (content, cipher_text, cipher_shift, source) VALUES (@content, @cipherText, @cipherShift, @source);", connection);
            command.Parameters.AddWithValue("@content", content);
            command.Parameters.AddWithValue("@cipherText", cipherText);
            command.Parameters.AddWithValue("@cipherShift", cipherShift);
            command.Parameters.AddWithValue("@source", source);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
