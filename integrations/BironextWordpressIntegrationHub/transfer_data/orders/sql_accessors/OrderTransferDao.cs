using Dapper;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System;
using validator;
using BiroWoocommerceHubTests;
using System.Threading.Tasks;
using ApiClient.utils;

namespace transfer_data.orders.sql_accessors
{

    public class OrderTransferDao
    {
        private string _connectionString;
        private string tableNameAppendix;
        public OrderTransferDao(string connectionString, string tableNameAppendix = "")
        {
            _connectionString = connectionString;
            this.tableNameAppendix = tableNameAppendix;
            EnsureDatabase();

            SqlMapper.AddTypeHandler(new BirokratDocumentTypeHandler());
            SqlMapper.AddTypeHandler(new OrderTransferStatusHandler());
        }

        private string getTableName()
        {
            return $"OrderTransfer{tableNameAppendix ?? ""}";
        }

        private void EnsureDatabase()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                // Check if table exists
                var tableExists = db.ExecuteScalar<int>($"SELECT count(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{getTableName()}'");

                if (tableExists == 0)
                {
                    db.Execute(@$"
                        CREATE TABLE {getTableName()}
                        (
                            OrderId NVARCHAR(60) NOT NULL,
                            OrderStatus NVARCHAR(25) NOT NULL,
                            OrderTransferStatus NVARCHAR(25) NOT NULL,
                            Error NVARCHAR(1000) NULL,
                            BirokratDocType NVARCHAR(25) NOT NULL,
                            BirokratDocNum NVARCHAR(30) NULL,
                            DateCreated DATETIME DEFAULT GETUTCDATE() NOT NULL,
                            DateLastModified DATETIME,
                            DateValidated DATETIME,
                            IntegrationId INT FOREIGN KEY REFERENCES Integration(Id),
                            VersionId INT FOREIGN KEY REFERENCES IntegrationVersion(Id),
                            PRIMARY KEY (IntegrationId, OrderId, OrderStatus)
                        )");
                }
            }
        }

        public async Task DropTableAsync()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                await db.ExecuteAsync($"DROP TABLE IF EXISTS {getTableName()}");
            }
        }


        public async Task<int> Insert(OrderTransfer orderTransfer)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.ExecuteAsync(@$"
                    INSERT INTO {getTableName()} 
                    (OrderId, OrderStatus, OrderTransferStatus, Error, BirokratDocType, BirokratDocNum, DateCreated, IntegrationId, VersionId)
                    VALUES 
                    (@OrderId, @OrderStatus, @OrderTransferStatus, @Error, @BirokratDocType, @BirokratDocNum, @DateCreated, @IntegrationId, @VersionId)
                    ", orderTransfer);
            }
        }

        public async Task Update(OrderTransfer orderTransfer)
        {

            if (orderTransfer.DateValidated == DateTime.MinValue)
                orderTransfer.DateValidated = null;
            if (orderTransfer.DateLastModified == DateTime.MinValue)
                orderTransfer.DateLastModified = null;

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                await db.ExecuteAsync(@$"
                    UPDATE {getTableName()}
                    SET OrderTransferStatus = @OrderTransferStatus,
                        Error = @Error,
                        BirokratDocType = @BirokratDocType,
                        BirokratDocNum = @BirokratDocNum,
                        DateLastModified = @DateLastModified,
                        DateValidated = @DateValidated
                    WHERE IntegrationId = @IntegrationId AND OrderId = @OrderId AND OrderStatus = @OrderStatus
                ", orderTransfer);
            }
        }

        public async Task Upsert(OrderTransfer orderTransfer)
        {
            if (orderTransfer.DateValidated == DateTime.MinValue)
                orderTransfer.DateValidated = null;
            if (orderTransfer.DateLastModified == DateTime.MinValue)
                orderTransfer.DateLastModified = null;

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string mergeSql = @$"
            MERGE INTO {getTableName()} AS target
            USING (SELECT @IntegrationId AS IntegrationId, @OrderId AS OrderId, @OrderStatus AS OrderStatus) AS source
            ON target.IntegrationId = source.IntegrationId AND target.OrderId = source.OrderId AND target.OrderStatus = source.OrderStatus
            WHEN MATCHED THEN
                UPDATE SET 
                    OrderTransferStatus = @OrderTransferStatus,
                    Error = @Error,
                    BirokratDocType = @BirokratDocType,
                    BirokratDocNum = @BirokratDocNum,
                    DateLastModified = @DateLastModified,
                    DateValidated = @DateValidated
            WHEN NOT MATCHED THEN
                INSERT (OrderId, OrderStatus, OrderTransferStatus, Error, BirokratDocType, BirokratDocNum, DateCreated, IntegrationId, VersionId)
                VALUES (@OrderId, @OrderStatus, @OrderTransferStatus, @Error, @BirokratDocType, @BirokratDocNum, @DateCreated, @IntegrationId, @VersionId);";

                await db.ExecuteAsync(mergeSql, new
                {
                    orderTransfer.OrderId,
                    orderTransfer.OrderStatus,
                    orderTransfer.OrderTransferStatus,
                    orderTransfer.Error,
                    orderTransfer.BirokratDocType,
                    orderTransfer.BirokratDocNum,
                    orderTransfer.DateCreated,
                    orderTransfer.DateLastModified,
                    orderTransfer.DateValidated,
                    orderTransfer.IntegrationId,
                    orderTransfer.VersionId
                });
            }
        }

        public async Task Delete(int integrationId, string orderId, string orderStatus)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                await db.ExecuteAsync($"DELETE FROM {getTableName()} WHERE IntegrationId = @IntegrationId AND OrderId = @OrderId AND OrderStatus = @OrderStatus",
                    new { IntegrationId = integrationId, OrderId = orderId, OrderStatus = orderStatus });
            }
        }

        public async Task<OrderTransfer> Get(int integrationId, string orderId, string orderStatus)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.QueryFirstOrDefaultAsync<OrderTransfer>($"SELECT * FROM {getTableName()} WHERE IntegrationId = @IntegrationId AND OrderId = @OrderId AND OrderStatus = @OrderStatus",
                    new { IntegrationId = integrationId, OrderId = orderId, OrderStatus = orderStatus });
            }
        }

        public async Task<List<OrderTransfer>> GetAllByIntegrationId(int integrationId, int pageIndex = 0, int pageSize = 1000)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return (await db.QueryAsync<OrderTransfer>(
                    $"SELECT * FROM {getTableName()} WHERE IntegrationId = @IntegrationId" +
                    $" ORDER BY OrderId, DateCreated OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
                    new { IntegrationId = integrationId, Offset = pageIndex * pageSize, PageSize = pageSize })).AsList();
            }
        }

        public async Task<DateTime?> GetDateOfLatestOrderInOrderTransfers(int integrationId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return await db.ExecuteScalarAsync<DateTime?>(@$"
            SELECT MAX(MinDateCreated) 
            FROM 
            (
                SELECT OrderId, MIN(DateCreated) AS MinDateCreated
                FROM {getTableName()} 
                WHERE IntegrationId = @IntegrationId
                GROUP BY OrderId
            ) AS GroupedResult",
                    new { IntegrationId = integrationId });
            }
        }


    }

    public class BirokratDocumentTypeHandler : SqlMapper.TypeHandler<BirokratDocumentType>
    {
        public override BirokratDocumentType Parse(object value)
        {
            return Enum.TryParse(typeof(BirokratDocumentType), value.ToString(), out var result)
                ? (BirokratDocumentType)result
                : BirokratDocumentType.UNASSIGNED; // Default if parsing fails
        }

        public override void SetValue(IDbDataParameter parameter, BirokratDocumentType value)
        {
            parameter.Value = value.ToString();
        }
    }

    public class OrderTransferStatusHandler : SqlMapper.TypeHandler<OrderTransferStatus>
    {
        public override OrderTransferStatus Parse(object value)
        {
            return Enum.TryParse(typeof(OrderTransferStatus), value.ToString(), out var result)
                ? (OrderTransferStatus)result
                : OrderTransferStatus.NO_EVENT; // Default if parsing fails
        }

        public override void SetValue(IDbDataParameter parameter, OrderTransferStatus value)
        {
            parameter.Value = value.ToString();
        }
    }
}
