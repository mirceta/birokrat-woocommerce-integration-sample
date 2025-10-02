using ApiClient.utils;
using BiroWoocommerceHubTests;
using Castle.Core;
using System;
using System.Collections.Generic;
using System.Net.Cache;
using System.Threading.Tasks;
using transfer_data_abstractions.orders;
using validator;

namespace transfer_data.orders.sql_accessors
{

    public class SqlOrderTransferAccessor : IOrderTransferAccessor
    {
        OrderTransferDao orderTransferDao;
        int integrationId;
        IOutApiClient outclient;
        public SqlOrderTransferAccessor(string connectionString, int integrationId, IOutApiClient outclient)
        {
            orderTransferDao = new OrderTransferDao(connectionString);
            this.integrationId = integrationId;
            this.outclient = outclient;
        }

        public async Task AddUnaccepted(string orderid, string orderstatus)
        {
            await orderTransferDao.Insert(new OrderTransfer()
            {
                OrderId = orderid,
                OrderStatus = orderstatus,
                BirokratDocType = BirokratDocumentType.UNASSIGNED,
                OrderTransferStatus = OrderTransferStatus.UNACCEPTED,
                DateCreated = DateTime.Now,
                DateLastModified = DateTime.Now,
                DateValidated = DateTime.MinValue,
                Error = "",
                BirokratDocNum = "",
            });
        }

        public async Task Delete(string orderid, string orderstatus)
        {
            await orderTransferDao.Delete(integrationId, orderid, orderstatus);
        }

        public async Task<OrderTransfer> Get(string orderid, string orderstatus)
        {
            return await orderTransferDao.Get(integrationId, orderid, orderstatus);
        }

        public async Task<List<OrderTransfer>> GetByStatus(List<OrderTransferStatus> statuses)
        {
            // TODO: DON'T RETURN ALL DO SOMETHING ELSE???
            return await orderTransferDao.GetAllByIntegrationId(integrationId, 0, 1000);
        }

        public async Task<string> GetOrder(string id)
        {
            return await outclient.MyGetOrder(id);
        }

        public async Task Set(OrderTransfer orderTransfer)
        {
            await orderTransferDao.Update(orderTransfer);
        }

        public async Task DangerousInsert(OrderTransfer orderTransfer) { 
            /*
             WARNING: THIS METHOD IS NOT MEANT TO BE IMPLEMENTED IN IOrderTransferAccessor.
             This is only added in this class to be used when transitioning from 
             PureWoocommerceOrderTransferSystem to SqlOrderTransferSystem - for the purpose
             of syncing old olders from webshop to sql.
             If you want to use this method with IOrderTransferAccessor THEN YOU MUST CAST IT
             TO SqlOrderTransferAccessor - DO NOT CHANGE THE DESIGN, THIS IS DONE PURPOSEFULLY!

             Again to reiterate why this method should not be used during runtime / normal circumstances:
             THE ONLY SAFE INSERT IS ADDING AN UNACCEPTED ORDER TRANSFER LEST WE RISK HAVING MULTIPLE
             DOUBLES
             */
            await orderTransferDao.Insert(orderTransfer);
        }
    }
}