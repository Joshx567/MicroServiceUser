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

        // ----------------------------
        // Implementación de IRepository<User>
        // ----------------------------

        public async Task<IEnumerable<User>> GetAllAsync() => await GetAllUsersAsync();

        public async Task<User> GetByIdAsync(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);

            const string sql = @"
                SELECT 
                    p.id AS Id,
                    p.name AS Name,
                    p.first_lastname AS FirstLastname,
                    p.second_lastname AS SecondLastname,
                    p.date_birth AS DateBirth,
                    p.ci AS Ci,
                    u.role AS Role,
                    u.hire_date AS HireDate,
                    u.monthly_salary AS MonthlySalary,
                    u.specialization AS Specialization,
                    u.email AS Email,
                    u.password AS Password,
                    u.must_change_password AS MustChangePassword
                FROM person p
                LEFT JOIN ""user"" u ON p.id = u.id_person
                WHERE p.id = @Id;";

            return await conn.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<User> CreateAsync(User entity)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                entity.LastModification = DateTime.UtcNow;
                entity.IsActive = true;

                const string personSql = @"
                    INSERT INTO person (name, first_lastname, second_lastname, date_birth, ci, created_at, last_modification, is_active)
                    VALUES (@Name, @FirstLastname, @SecondLastname, @DateBirth, @Ci, @CreatedAt, @LastModification, @IsActive)
                    RETURNING id;";

                entity.Id = await conn.ExecuteScalarAsync<int>(personSql, entity, transaction);

                const string userSql = @"
                    INSERT INTO ""user"" (id_person, role, hire_date, monthly_salary, specialization, email, password, must_change_password)
                    VALUES (@Id, @Role, @HireDate, @MonthlySalary, @Specialization, @Email, @Password, @MustChangePassword);";

                await conn.ExecuteAsync(userSql, entity, transaction);

                await transaction.CommitAsync();
                return entity;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error creando usuario {entity.Name}");
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
                // 1️⃣ Obtener el rol existente si el rol nuevo es null o vacío
                if (string.IsNullOrWhiteSpace(entity.Role))
                {
                    const string existingRoleSql = @"SELECT role FROM ""user"" WHERE id_person = @Id;";
                    var existingRole = await conn.ExecuteScalarAsync<string>(existingRoleSql, new { Id = entity.Id }, transaction);
                    entity.Role = existingRole; // conservar el rol
                }

                // 2️⃣ Actualizar tabla person
                entity.LastModification = DateTime.UtcNow;

                const string personSql = @"
            UPDATE person
            SET name = @Name,
                first_lastname = @FirstLastname,
                second_lastname = @SecondLastname,
                date_birth = @DateBirth,
                ci = @Ci,
                last_modification = @LastModification,
                is_active = @IsActive
            WHERE id = @Id;"; 

                await conn.ExecuteAsync(personSql, entity, transaction);

                // 3️⃣ Actualizar tabla user
                const string userSql = @"
            UPDATE ""user""
            SET role = @Role,
                hire_date = @HireDate,
                monthly_salary = @MonthlySalary,
                specialization = @Specialization,
                email = @Email
            WHERE id_person = @Id;";

                await conn.ExecuteAsync(userSql, entity, transaction);

                // 4️⃣ Commit
                await transaction.CommitAsync();
                return entity;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error actualizando usuario {entity.Id}");
                throw;
            }
        }


        public async Task<bool> DeleteByIdAsync(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);

            const string sql = @"
                UPDATE person
                SET is_active = false, last_modification = @LastModification
                WHERE id = @Id;";

            var affectedRows = await conn.ExecuteAsync(sql, new { Id = id, LastModification = DateTime.UtcNow });
            return affectedRows > 0;
        }


        public async Task<User> GetByEmailAsync(string email)
        {
            using var conn = new NpgsqlConnection(_connectionString);

            const string sql = @"
                SELECT 
                    p.id AS Id,
                    p.name AS Name,
                    p.first_lastname AS FirstLastname,
                    p.second_lastname AS SecondLastname,
                    p.date_birth AS DateBirth,
                    p.ci AS Ci,
                    u.role AS Role,
                    u.hire_date AS HireDate,
                    u.monthly_salary AS MonthlySalary,
                    u.specialization AS Specialization,
                    u.email AS Email,
                    u.password AS Password,
                    u.must_change_password AS MustChangePassword
                FROM person p
                JOIN ""user"" u ON p.id = u.id_person
                WHERE u.email = @Email;";

            return await conn.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<bool> UpdatePasswordAsync(int id, string password)
        {
            using var conn = new NpgsqlConnection(_connectionString);

            const string sql = @"
                UPDATE ""user""
                SET password = @Password,
                    must_change_password = false
                WHERE id_person = @IdUser;";

            var affectedRows = await conn.ExecuteAsync(sql, new { IdUser = id, Password = password });
            return affectedRows > 0;
        }

        private async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            using var conn = new NpgsqlConnection(_connectionString);

            // Trae solo usuarios que tienen rol (Instructor/Admin/Otros) y no clientes
            const string sql = @"
                SELECT 
                    p.id AS Id,
                    p.name AS Name,
                    p.first_lastname AS FirstLastname,
                    p.second_lastname AS SecondLastname,
                    p.date_birth AS DateBirth,
                    p.ci AS Ci,
                    u.role AS Role,
                    u.hire_date AS HireDate,
                    u.monthly_salary AS MonthlySalary,
                    u.specialization AS Specialization,
                    u.email AS Email,
                    u.password AS Password,
                    u.must_change_password AS MustChangePassword
                FROM ""user"" u
                JOIN person p ON p.id = u.id_person
                WHERE p.is_active = true;";

            return await conn.QueryAsync<User>(sql);
        }
    }
}