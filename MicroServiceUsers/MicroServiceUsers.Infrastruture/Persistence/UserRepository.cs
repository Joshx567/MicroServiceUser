using Dapper;
using Npgsql;
using ServiceUser.Domain.Entities;
using ServiceUser.Domain.Ports;
using ServiceUser.Infrastructure.Provider;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceUser.Infrastructure.Persistence
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(IUserConnectionProvider connectionProvider, ILogger<UserRepository> logger)
        {
            ArgumentNullException.ThrowIfNull(connectionProvider, nameof(connectionProvider));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));

            _connectionString = connectionProvider.GetConnectionString()
                                ?? throw new InvalidOperationException("El connection provider debe entregar una cadena válida.");
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            const string sql = @"SELECT * FROM users WHERE is_active = true;";
            return await conn.QueryAsync<User>(sql);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            const string sql = @"SELECT * FROM users WHERE id = @id;";
            return await conn.QuerySingleOrDefaultAsync<User>(sql, new { id });
        }

        public async Task<User> CreateAsync(User entity)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                entity.created_at = DateTime.UtcNow;
                entity.last_modification = DateTime.UtcNow;
                entity.is_active = true;

                const string sql = @"
                    INSERT INTO users (
                        name, first_lastname, second_lastname, date_birth, ci, is_active,
                        role, email, password, must_change_password, hire_date,
                        monthly_salary, specialization, created_at, created_by, last_modification, last_modified_by
                    )
                    VALUES (
                        @name, @first_lastname, @second_lastname, @date_birth, @ci, @is_active,
                        @role, @email, @password, @must_change_password, @hire_date,
                        @monthly_salary, @specialization, @created_at, @created_by, @last_modification, @last_modified_by
                    )
                    RETURNING id;";

                entity.id = await conn.ExecuteScalarAsync<int>(sql, entity, transaction);
                await transaction.CommitAsync();
                return entity;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error creando usuario {entity.name}");
                throw;
            }
        }

        public async Task<User> UpdateAsync(User entity)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                entity.last_modification = DateTime.UtcNow;

                const string sql = @"
                    UPDATE users
                    SET
                        name = @name,
                        first_lastname = @first_lastname,
                        second_lastname = @second_lastname,
                        date_birth = @date_birth,
                        ci = @ci,
                        is_active = @is_active,
                        role = @role,
                        email = @email,
                        password = @password,
                        must_change_password = @must_change_password,
                        hire_date = @hire_date,
                        monthly_salary = @monthly_salary,
                        specialization = @specialization,
                        last_modification = @last_modification,
                        last_modified_by = @last_modified_by
                    WHERE id = @id;";

                await conn.ExecuteAsync(sql, entity, transaction);
                await transaction.CommitAsync();
                return entity;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error actualizando usuario {entity.id}");
                throw;
            }
        }

        public async Task<bool> DeleteByIdAsync(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            const string sql = @"
                UPDATE users
                SET is_active = false,
                    last_modification = @last_modification
                WHERE id = @id;";

            var affectedRows = await conn.ExecuteAsync(sql, new { id, last_modification = DateTime.UtcNow });
            return affectedRows > 0;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            const string sql = @"SELECT * FROM users WHERE email = @email;";
            return await conn.QuerySingleOrDefaultAsync<User>(sql, new { email });
        }

        public async Task<bool> UpdatePasswordAsync(int id, string password)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            const string sql = @"
                UPDATE users
                SET password = @password,
                    must_change_password = false
                WHERE id = @id;";

            var affectedRows = await conn.ExecuteAsync(sql, new { id, password });
            return affectedRows > 0;
        }
    }
}
