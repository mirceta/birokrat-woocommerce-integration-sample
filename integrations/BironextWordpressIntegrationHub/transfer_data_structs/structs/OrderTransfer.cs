using ApiClient.utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace validator
{
    public class OrderTransfer {

        const string DATE_FORMAT = "yyyy-MM-ddHH:mm:ss";

        public string OrderId { get; set; }
        public string OrderStatus { get; set; }
        public OrderTransferStatus OrderTransferStatus { get; set; }
        public string Error { get; set; }
        public BirokratDocumentType BirokratDocType { get; set; }
        public string BirokratDocNum { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateLastModified { get; set; }
        public DateTime? DateValidated { get; set; }

        public int IntegrationId { get; set; }
        public int VersionId { get; set; }

        public OrderTransfer() { }
        public OrderTransfer(OrderTransferJson json) {

            
            if (!string.IsNullOrEmpty(json.birokratdoctype) && !Reverse(BironextApiPathHelper.biroDoctTypeMap).ContainsKey(json.birokratdoctype))
                throw new Exception("Unrecognized type of birokrat document");
            if (!OrderTransferStatusMap.ContainsKey(json.ordertransferstatus))
                throw new Exception("Unrecognized type of order transfer status");

            OrderId = json.orderid;
            OrderStatus = json.orderstatus;
            Error = json.error;
            BirokratDocNum = json.birokratdocnum;
            BirokratDocType = string.IsNullOrEmpty(json.birokratdoctype) ? BirokratDocumentType.UNASSIGNED : Reverse(BironextApiPathHelper.biroDoctTypeMap)[json.birokratdoctype];
            OrderTransferStatus = OrderTransferStatusMap[json.ordertransferstatus];
            DateCreated = json.datecreated == null ? DateTime.MinValue : DateTime.ParseExact(json.datecreated, DATE_FORMAT, CultureInfo.InvariantCulture);
            DateLastModified = json.datelastmodified == null ? DateTime.MinValue : DateTime.ParseExact(json.datelastmodified, DATE_FORMAT, CultureInfo.InvariantCulture);
            DateValidated = json.datevalidated == null ? DateTime.MinValue : DateTime.ParseExact(json.datevalidated, DATE_FORMAT, CultureInfo.InvariantCulture);
        }

        public OrderTransferJson ToJson() {
            
            if (!BironextApiPathHelper.biroDoctTypeMap.ContainsKey(BirokratDocType))
                throw new Exception("Unrecognized type of birokrat document");
            if (!Reverse(OrderTransferStatusMap).ContainsKey(OrderTransferStatus))
                throw new Exception("Unrecognized type of order transfer status");

            return new OrderTransferJson() {
                orderid = OrderId,
                orderstatus = OrderStatus,
                error = Error,
                birokratdocnum = BirokratDocNum,
                birokratdoctype = BironextApiPathHelper.biroDoctTypeMap[BirokratDocType],
                ordertransferstatus = Reverse(OrderTransferStatusMap)[OrderTransferStatus],
                datecreated = DateCreated.ToString(DATE_FORMAT),
                datelastmodified = DateLastModified == null || DateLastModified.Value == DateTime.MinValue ?
                                                    "" : DateLastModified.Value.ToString(DATE_FORMAT),
            datevalidated = DateValidated == null || DateValidated.Value == DateTime.MinValue ?
                                                "" : DateValidated.Value.ToString(DATE_FORMAT)
            };
        }

        static Dictionary<string, OrderTransferStatus> OrderTransferStatusMap = new Dictionary<string, OrderTransferStatus>() {
                { "1", OrderTransferStatus.UNACCEPTED},
                { "2", OrderTransferStatus.ACCEPTED},
                { "3", OrderTransferStatus.ERROR},
                { "4", OrderTransferStatus.UNVERIFIED},
                { "5", OrderTransferStatus.VERIFIED},
                { "6", OrderTransferStatus.VERIFICATION_ERROR},
                { "7", OrderTransferStatus.NO_EVENT}
            };

        public static Dictionary<TValue, TKey> Reverse<TKey, TValue>(IDictionary<TKey, TValue> source) {
            var dictionary = new Dictionary<TValue, TKey>();
            foreach (var entry in source) {
                if (!dictionary.ContainsKey(entry.Value))
                    dictionary.Add(entry.Value, entry.Key);
            }
            return dictionary;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Order ID: {OrderId}");
            sb.AppendLine($"Order Status: {OrderStatus}");
            sb.AppendLine($"Order Transfer Status: {Enum.GetName(typeof(OrderTransferStatus), OrderTransferStatus)}");
            sb.AppendLine($"Error: {Error}");
            sb.AppendLine($"Birokrat Document Type: {Enum.GetName(typeof(BirokratDocumentType), BirokratDocType)}");
            sb.AppendLine($"Birokrat Document Number: {BirokratDocNum}");
            sb.AppendLine($"Date Created: {DateCreated.ToString(DATE_FORMAT)}");


            var val = DateLastModified != null ? DateLastModified.Value.ToString(DATE_FORMAT) : "";
            sb.AppendLine($"Date Last Modified: {val}");


            var val1 = DateValidated != null ? DateValidated.Value.ToString(DATE_FORMAT) : "";
            sb.AppendLine($"Date Validated: {val1}");

            return sb.ToString();
        }
    }

    public class OrderTransferJson {
        public string orderid;
        public string orderstatus;
        public string ordertransferstatus;
        public string error;
        public string birokratdoctype;
        public string birokratdocnum;
        public string datecreated;
        public string datelastmodified;
        public string datevalidated;
    }
}
