﻿using System;
using System.Linq;
using THOK.Wms.Bll.Interfaces;
using Microsoft.Practices.Unity;
using THOK.Wms.Dal.Interfaces;

namespace THOK.Wms.Bll.Service
{
    public class StockledgerService : IStockledgerService
    {
        [Dependency]
        public IDailyBalanceRepository StockledgerRepository { get; set; }

        [Dependency]
        public IInBillDetailRepository InBillDetailRepository { get; set; }
        [Dependency]
        public IOutBillDetailRepository OutBillDetailRepository { get; set; }
        [Dependency]
        public IProfitLossBillDetailRepository ProfitLossBillDetailRepository { get; set; }

        //protected override Type LogPrefix
        //{
        //    get { return this.GetType(); }
        //}

        #region IStockledgerService 成员

        public object GetDetails(int page, int rows, string warehouseCode, string productCode, string beginDate, string endDate)
        {
            var ledgerQuery = StockledgerRepository.GetQueryable().AsEnumerable();
            var query = ledgerQuery.Where(i => i.ProductCode.Contains(productCode)
                                         && i.WarehouseCode.Contains(warehouseCode)
                                       ).OrderBy(i => i.SettleDate).OrderBy(i => i.Product.ProductName
                                       ).AsEnumerable().Select(i => new
                                       {
                                           SettleDate = i.SettleDate.ToString("yyyy-MM-dd"),
                                           i.WarehouseCode,
                                           i.Warehouse.WarehouseName,
                                           i.ProductCode,
                                           i.Product.ProductName,
                                           i.UnitCode,
                                           i.Unit.UnitName,
                                           i.Beginning,
                                           Item_Beginning=Convert.ToDouble(i.Beginning/50),
                                           i.EntryAmount,
                                           Item_EntryAmount = Convert.ToDouble(i.EntryAmount / 50),
                                           i.DeliveryAmount,
                                           Item_DeliveryAmount = Convert.ToDouble(i.DeliveryAmount / 50),
                                           i.ProfitAmount,
                                           Item_ProfitAmount = Convert.ToDouble(i.ProfitAmount / 50),
                                           i.LossAmount,
                                           Item_LossAmount = Convert.ToDouble(i.LossAmount / 50),
                                           ProfitLossAmount = i.ProfitAmount - i.LossAmount,
                                           Item_ProfitLossAmount = Convert.ToDouble((i.ProfitAmount-i.LossAmount) / 50),
                                           i.Ending
            });
            if (!beginDate.Equals(string.Empty))
            {
                DateTime begin = Convert.ToDateTime(beginDate);
                query = query.Where(i => Convert.ToDateTime(i.SettleDate) >= begin);
            }

            if (!endDate.Equals(string.Empty))
            {
                DateTime end = Convert.ToDateTime(endDate);
                query = query.Where(i => Convert.ToDateTime(i.SettleDate) <= end);
            }
            int total = query.Count();
            query = query.Skip((page - 1) * rows).Take(rows);
            return new { total, rows = query.ToArray() };
        }
        
        public object GetInfoDetails(int page, int rows, string warehouseCode, string productCode, string settleDate)
        {
            var inQuery = InBillDetailRepository.GetQueryable();
            var outQuery = OutBillDetailRepository.GetQueryable();
            var differQuery = ProfitLossBillDetailRepository.GetQueryable();
            var Allquery = inQuery.Select(a => new
            {
                BillDate = a.InBillMaster.BillDate,
                a.InBillMaster.Warehouse.WarehouseCode,
                a.InBillMaster.Warehouse.WarehouseName,
                a.BillNo,
                a.InBillMaster.BillType.BillTypeCode,
                a.InBillMaster.BillType.BillTypeName,
                a.ProductCode,
                a.Product.ProductName,
                a.RealQuantity,
                a.Unit.UnitName
            }).Union(outQuery.Select(a => new
            {
                BillDate = a.OutBillMaster.BillDate,
                a.OutBillMaster.Warehouse.WarehouseCode,
                a.OutBillMaster.Warehouse.WarehouseName,
                a.BillNo,
                a.OutBillMaster.BillType.BillTypeCode,
                a.OutBillMaster.BillType.BillTypeName,
                a.ProductCode,
                a.Product.ProductName,
                a.RealQuantity,
                a.Unit.UnitName
            })).Union(differQuery.Select(a => new
            {
                BillDate = a.ProfitLossBillMaster.BillDate,
                a.ProfitLossBillMaster.Warehouse.WarehouseCode,
                a.ProfitLossBillMaster.Warehouse.WarehouseName,
                a.BillNo,
                a.ProfitLossBillMaster.BillType.BillTypeCode,
                a.ProfitLossBillMaster.BillType.BillTypeName,
                a.ProductCode,
                a.Product.ProductName,
                RealQuantity = a.Quantity,
                a.Unit.UnitName
            }));
            if (!settleDate.Equals(string.Empty))
            {
                DateTime date = Convert.ToDateTime(settleDate);
                Allquery = Allquery.Where(i => i.BillDate == date);
            }
            Allquery = Allquery.OrderBy(a => a.BillDate).OrderBy(a => a.WarehouseName);
            int total = Allquery.Count();
            Allquery = Allquery.Skip((page - 1) * rows).Take(rows);
            var query = Allquery.Where(i => i.ProductCode.Contains(productCode) && i.WarehouseCode.Contains(warehouseCode)).ToArray().Select(i => new
            {
                BillDate = i.BillDate.ToString("yyyy-MM-dd"),
                i.WarehouseCode,
                i.WarehouseName,
                i.BillNo,
                i.BillTypeCode,
                i.BillTypeName,
                i.ProductCode,
                i.ProductName,
                i.RealQuantity,
                JQuantity = Convert.ToDouble(i.RealQuantity / 50),
                TQuantity = i.RealQuantity,
                i.UnitName

            });
            return new { total, rows = query.ToArray() };
        }

        #endregion

    }
}
