using Dapper;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace pdf_handling
{
    public class PdfDataDao
    {
        private readonly string _connectionString;

        public PdfDataDao(string connectionString)
        {
            _connectionString = connectionString;
            EnsureDatabase();
        }


        private void EnsureDatabase()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var tableExists = db.ExecuteScalar<int>("SELECT count(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PdfData'");

                if (tableExists == 0)
                {
                    db.Execute(@"
                        CREATE TABLE PdfData
                        (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            TaskId INT NOT NULL REFERENCES AssignedTasks(Id),
                            Content NVARCHAR(MAX),
                            BirokratDocType NVARCHAR(20),
                            BirokratDocNum NVARCHAR(20)
                        )");
                }
            }
        }

        public async Task<int> InsertAsync(PdfData pdfData)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.ExecuteAsync(@"
                    INSERT INTO PdfData (TaskId, Content, BirokratDocType, BirokratDocNum) 
                    VALUES (@TaskId, @Content, @BirokratDocType, @BirokratDocNum)",
                    pdfData);
            }
        }

        public async Task UpdateAsync(PdfData pdfData)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                await db.ExecuteAsync(@"
                    UPDATE PdfData 
                    SET Content = @Content, 
                        BirokratDocType = @BirokratDocType, 
                        BirokratDocNum = @BirokratDocNum 
                    WHERE Id = @Id",
                    pdfData);
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                await db.ExecuteAsync("DELETE FROM PdfData WHERE Id = @Id", new { Id = id });
            }
        }

        public async Task<PdfData> GetAsync(int id)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.QueryFirstOrDefaultAsync<PdfData>("SELECT * FROM PdfData WHERE Id = @Id", new { Id = id });
            }
        }

        public async Task<List<PdfData>> GetAsyncByTaskId(int taskId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return (await db.QueryAsync<PdfData>("SELECT * FROM PdfData WHERE TaskId = @TaskId", new { TaskId = taskId })).AsList();
            }
        }

        public async Task<PdfData> GetAsync(int taskId, string birokratDocType, string birokratDocNum)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var result = await db.QueryFirstOrDefaultAsync<PdfData>(
                    "SELECT * FROM PdfData WHERE TaskId = @TaskId AND BirokratDocType = @BirokratDocType AND BirokratDocNum = @BirokratDocNum",
                    new { TaskId = taskId, BirokratDocType = birokratDocType, BirokratDocNum = birokratDocNum });
                return result;
            }
        }

        public async Task<List<PdfData>> GetAllAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return (await db.QueryAsync<PdfData>("SELECT * FROM PdfData")).AsList();
            }
        }

        public async Task DropTableAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                // It's important to check if the table exists before trying to drop it to avoid an error
                var tableExists = await db.ExecuteScalarAsync<int>(
                    "SELECT count(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PdfData'");

                if (tableExists > 0)
                {
                    await db.ExecuteAsync("DROP TABLE PdfData");
                }
            }
        }
    }

    public class PdfData
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string Content { get; set; }  // Base64 encoded PDF content
        public string BirokratDocType { get; set; }
        public string BirokratDocNum { get; set; }
    }
}
