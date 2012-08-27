﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using THOK.Wms.DbModel;
using THOK.Wms.Bll.Interfaces;
using Microsoft.Practices.Unity;
using THOK.Wms.Dal.Interfaces;
using THOK.Wms.SignalR;
using THOK.Wms.SignalR.Common;
using System.Transactions;
using THOK.Wms.Download.Interfaces;

namespace THOK.Wms.Bll.Service
{
    public class InBillMasterService : ServiceBase<InBillMaster>, IInBillMasterService
    {
        [Dependency]
        public IInBillMasterRepository InBillMasterRepository { get; set; }
        [Dependency]
        public IBillTypeRepository BillTypeRepository { get; set; }
        [Dependency]
        public IWarehouseRepository WarehouseRepository { get; set; }
        [Dependency]
        public IEmployeeRepository EmployeeRepository { get; set; }
        [Dependency]
        public IInBillDetailRepository InBillDetailRepository { get; set; }
        [Dependency]
        public IInBillAllotRepository InBillAllotRepository { get; set; }
        [Dependency]
        public IStorageLocker Locker { get; set; }
        [Dependency]
        public IInBillMasterDownService InBillMasterDownService { get; set; }
        [Dependency]
        public IStorageRepository StorageRepository { get; set; }
        [Dependency]
        public ICellRepository CellRepository { get; set; }

        protected override Type LogPrefix
        {
            get { return this.GetType(); }
        }

        string resultStr = "";//错误提示信息

        #region IInBillMasterService 成员

        public string WhatStatus(string status)
        {
            string statusStr = "";
            switch (status)
            {
                case "1":
                    statusStr = "已录入";
                    break;
                case "2":
                    statusStr = "已审核";
                    break;
                case "3":
                    statusStr = "已分配";
                    break;
                case "4":
                    statusStr = "已确认";
                    break;
                case "5":
                    statusStr = "执行中";
                    break;
                case "6":
                    statusStr = "已结单";
                    break;
            }
            return statusStr;
        }

        public object GetDetails(int page, int rows, string BillNo, string WareHouseCode, string BeginDate, string EndDate, string OperatePersonCode, string CheckPersonCode, string Status, string IsActive)
        {
            IQueryable<InBillMaster> inBillMasterQuery = InBillMasterRepository.GetQueryable();
            var inBillMaster = inBillMasterQuery.Where(i => i.BillNo.Contains(BillNo)
                    && i.Status != "6"
                    && i.WarehouseCode.Contains(WareHouseCode)
                    && i.OperatePerson.EmployeeCode.Contains(OperatePersonCode)
                    && i.Status.Contains(Status))
                    .OrderByDescending(t => t.BillDate)
                    .OrderByDescending(t => t.BillNo)
                    .Select(i => i);

            if (!BeginDate.Equals(string.Empty))
            {
                DateTime begin = Convert.ToDateTime(BeginDate);
                inBillMaster = inBillMaster.Where(i => i.BillDate >= begin);
            }

            if (!EndDate.Equals(string.Empty))
            {
                DateTime end = Convert.ToDateTime(EndDate).AddDays(1);
                inBillMaster = inBillMaster.Where(i => i.BillDate <= end);
            }

            if (!CheckPersonCode.Equals(string.Empty))
            {
                inBillMaster = inBillMaster.Where(i=>i.VerifyPerson.EmployeeCode==CheckPersonCode);
            }
            int total = inBillMaster.Count();
            inBillMaster = inBillMaster.Skip((page - 1) * rows).Take(rows);

            var tmp = inBillMaster.ToArray().AsEnumerable().Select(i => new
            {
                i.BillNo,
                BillDate = i.BillDate.ToString("yyyy-MM-dd HH:mm:ss"),
                i.OperatePersonID,
                i.WarehouseCode,
                i.BillTypeCode,
                i.BillType.BillTypeName,
                i.Warehouse.WarehouseName,
                OperatePersonCode = i.OperatePerson.EmployeeCode,
                OperatePersonName = i.OperatePerson.EmployeeName,
                VerifyPersonID = i.VerifyPersonID == null ? string.Empty : i.VerifyPerson.EmployeeCode,
                VerifyPersonName = i.VerifyPersonID == null ? string.Empty : i.VerifyPerson.EmployeeName,
                VerifyDate = (i.VerifyDate == null ? "" : ((DateTime)i.VerifyDate).ToString("yyyy-MM-dd HH:mm:ss")),
                Status = WhatStatus(i.Status),
                IsActive = i.IsActive == "1" ? "可用" : "不可用",
                Description = i.Description,
                UpdateTime = i.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                i.TargetCellCode
            });
            return new { total, rows = tmp.ToArray() };
        }

        public bool Add(InBillMaster inBillMaster, string userName)
        {
            bool result = false;
            var ibm = new InBillMaster();
            var employee = EmployeeRepository.GetQueryable().FirstOrDefault(i => i.UserName == userName);
            if (employee != null)
            {
                ibm.BillNo = inBillMaster.BillNo;
                ibm.BillDate = inBillMaster.BillDate;
                ibm.BillTypeCode = inBillMaster.BillTypeCode;
                ibm.WarehouseCode = inBillMaster.WarehouseCode;
                ibm.OperatePersonID = employee.ID;
                ibm.Status = "1";
                ibm.VerifyPersonID = inBillMaster.VerifyPersonID;
                ibm.VerifyDate = inBillMaster.VerifyDate;
                ibm.Description = inBillMaster.Description;
                //ibm.IsActive = inBillMaster.IsActive;
                ibm.IsActive = "1";
                ibm.UpdateTime = DateTime.Now;
                ibm.TargetCellCode = inBillMaster.TargetCellCode;

                InBillMasterRepository.Add(ibm);
                InBillMasterRepository.SaveChanges();
                result = true;
            }
            return result;
        }

        public bool Delete(string BillNo)
        {
            bool result = false;
            var ibm = InBillMasterRepository.GetQueryable().FirstOrDefault(i => i.BillNo == BillNo && i.Status == "1");
            if (ibm != null)
            {
                Del(InBillDetailRepository, ibm.InBillDetails);
                InBillMasterRepository.Delete(ibm);
                InBillMasterRepository.SaveChanges();
                result = true;
            }
            return result;
        }

        public bool Save(InBillMaster inBillMaster)
        {
            bool result = false;
            var ibm = InBillMasterRepository.GetQueryable().FirstOrDefault(i => i.BillNo == inBillMaster.BillNo && i.Status == "1");
            if (ibm != null)
            {
                ibm.BillDate = inBillMaster.BillDate;
                ibm.BillTypeCode = inBillMaster.BillTypeCode;
                ibm.WarehouseCode = inBillMaster.WarehouseCode;
                ibm.OperatePersonID = inBillMaster.OperatePersonID;
                ibm.Status = "1";
                ibm.VerifyPersonID = inBillMaster.VerifyPersonID;
                ibm.VerifyDate = inBillMaster.VerifyDate;
                ibm.Description = inBillMaster.Description;
                //ibm.IsActive = inBillMaster.IsActive;
                ibm.IsActive = "1";
                ibm.UpdateTime = DateTime.Now;
                ibm.TargetCellCode = inBillMaster.TargetCellCode;

                InBillMasterRepository.SaveChanges();
                result = true;
            }
            return result;
        }

        #endregion

        #region IInBillMasterService 成员


        public object GenInBillNo(string userName)
        {
            IQueryable<InBillMaster> inBillMasterQuery = InBillMasterRepository.GetQueryable();
            string sysTime = System.DateTime.Now.ToString("yyMMdd");
            string billNo = "";
            var employee = EmployeeRepository.GetQueryable().FirstOrDefault(i => i.UserName == userName);
            var inBillMaster = inBillMasterQuery.Where(i => i.BillNo.Contains(sysTime)).ToArray().OrderBy(i => i.BillNo).Select(i => new { i.BillNo }.BillNo);
            if (inBillMaster.Count() == 0)
            {
                billNo = System.DateTime.Now.ToString("yyMMdd") + "0001" + "IN";
            }
            else
            {
                string billNoStr = inBillMaster.Last(b => b.Contains(sysTime));
                int i = Convert.ToInt32(billNoStr.ToString().Substring(6, 4));
                i++;
                string newcode = i.ToString();
                for (int j = 0; j < 4 - i.ToString().Length; j++)
                {
                    newcode = "0" + newcode;
                }
                billNo = System.DateTime.Now.ToString("yyMMdd") + newcode + "IN";
            }
            var findBillInfo = new
            {
                BillNo = billNo,
                billNoDate = DateTime.Now.ToString("yyyy-MM-dd"),
                employeeID = employee == null ? "" : employee.ID.ToString(),
                employeeCode = employee == null ? "" : employee.EmployeeCode.ToString(),
                employeeName = employee == null ? "" : employee.EmployeeName.ToString()
            };
            return findBillInfo;
        }

        #endregion

        #region IInBillMasterService 成员


        public bool Audit(string BillNo, string userName,out string strResult)
        {
            bool result = false;
            strResult = string.Empty;
            var ibm = InBillMasterRepository.GetQueryable().FirstOrDefault(i => i.BillNo == BillNo && i.Status == "1");
            var employee = EmployeeRepository.GetQueryable().FirstOrDefault(i => i.UserName == userName);
            if (ibm != null)
            {
                if (string.IsNullOrEmpty(ibm.TargetCellCode))//判断入库主单是否指定货位
                {
                    ibm.Status = "2";
                    ibm.VerifyDate = DateTime.Now;
                    ibm.UpdateTime = DateTime.Now;
                    ibm.VerifyPersonID = employee.ID;
                    InBillMasterRepository.SaveChanges();
                    result = true;
                }
                else//如果入库主单指定了货位那么就进行入库分配
                {
                    result = InAllot(ibm);
                    strResult = resultStr;
                }
            }
            return result;
        }

        /// <summary>
        /// 入库分配
        /// </summary>
        /// <param name="inBillMaster">入库主单</param>
        /// <returns></returns>
        public bool InAllot(InBillMaster inBillMaster)
        {
            bool result = false;
            var inBillDetails = inBillMaster.InBillDetails.ToArray();
            var cell = CellRepository.GetQueryable().FirstOrDefault(c => c.CellCode == inBillMaster.TargetCellCode);
            //入库单入库
            inBillMaster.InBillDetails.AsParallel().ForAll(
           (Action<InBillDetail>)delegate(InBillDetail i)
           {
               var inStorage = Locker.LockStorage(cell);
               if (inStorage == null)
               {
                   throw new Exception("锁定储位失败，储位其他人正在操作，无法分配请稍候重试！");
               }

               if (i.BillQuantity - i.AllotQuantity > 0)
               {
                   decimal allotQuantity = i.BillQuantity < cell.MaxQuantity * i.Unit.Count ? i.BillQuantity : cell.MaxQuantity * i.Unit.Count;
                   decimal billQuantity = i.BillQuantity - i.AllotQuantity;
                   allotQuantity = allotQuantity < billQuantity ? allotQuantity : billQuantity;
                   i.AllotQuantity += allotQuantity;
                   i.RealQuantity += allotQuantity;
                   inStorage.ProductCode = i.ProductCode;
                   inStorage.Quantity += allotQuantity;

                   var billAllot = new InBillAllot()
                   {
                       BillNo = inBillMaster.BillNo,
                       InBillDetailId = i.ID,
                       ProductCode = i.ProductCode,
                       CellCode = inStorage.CellCode,
                       StorageCode = inStorage.StorageCode,
                       UnitCode = i.UnitCode,
                       AllotQuantity = allotQuantity,
                       RealQuantity = allotQuantity,
                       Status = "2"
                   };

                   lock (inBillMaster.InBillAllots)
                   {
                       inBillMaster.InBillAllots.Add(billAllot);
                   }
                   inStorage.LockTag = string.Empty;
                   result = true;
               }
           });
            //入库结单
            var inMaster = InBillMasterRepository.GetQueryable()
                .FirstOrDefault(i => i.BillNo == inBillMaster.BillNo);
            inMaster.Status = "6";
            inMaster.UpdateTime = DateTime.Now;
            InBillMasterRepository.SaveChanges();
            return result;
        }

        #endregion

        #region IInBillMasterService 成员


        public bool AntiTrial(string BillNo)
        {
            bool result = false;
            var ibm = InBillMasterRepository.GetQueryable().FirstOrDefault(i => i.BillNo == BillNo && i.Status == "2");
            if (ibm != null)
            {
                ibm.Status = "1";
                ibm.VerifyDate = null;
                ibm.UpdateTime = DateTime.Now;
                ibm.VerifyPersonID = null;
                InBillMasterRepository.SaveChanges();
                result = true;
            }
            return result;
        }

        #endregion

        #region IInBillMasterService 成员

        /// <summary>
        /// 根据条件查询订单类型
        /// </summary>
        /// <param name="BillClass">订单类别</param>
        /// <param name="IsActive">是否可用</param>
        /// <returns></returns>
        public object GetBillTypeDetail(string BillClass, string IsActive)
        {
            IQueryable<BillType> billtypeQuery = BillTypeRepository.GetQueryable();
            var billtype = billtypeQuery.Where(b => b.BillClass == BillClass
                && b.IsActive.Contains(IsActive)).ToArray().OrderBy(b => b.BillTypeCode).Select(b => new
            {
                b.BillTypeCode,
                b.BillTypeName,
                b.BillClass,
                b.Description,
                IsActive = b.IsActive == "1" ? "可用" : "不可用",
                UpdateTime = b.UpdateTime.ToString("yyyy-MM-dd hh:mm:ss")
            });
            return billtype.ToArray();
        }

        #endregion

        #region IInBillMasterService 成员

        /// <summary>
        /// 根据条件查询仓库信息
        /// </summary>
        /// <param name="IsActive">是否可用</param>
        /// <returns></returns>
        public object GetWareHouseDetail(string IsActive)
        {
            IQueryable<Warehouse> wareQuery = WarehouseRepository.GetQueryable();
            var warehouse = wareQuery.Where(w => w.IsActive == IsActive).OrderBy(w => w.WarehouseCode).ToArray().Select(w => new
                {
                    w.WarehouseCode,
                    w.WarehouseName,
                    w.WarehouseType,
                    w.Description,
                    w.ShortName,
                    IsActive = w.IsActive == "1" ? "可用" : "不可用",
                    UpdateTime = w.UpdateTime.ToString("yyyy-MM-dd hh:mm:ss")
                });
            return warehouse.ToArray();
        }

        #endregion

        #region IInBillMasterService 成员

        /// <summary>
        /// 入库单结单
        /// </summary>
        /// <param name="BillNo">入库单号</param>
        /// <param name="strResult">提示信息</param>
        /// <returns></returns>
        public bool Settle(string BillNo, out string strResult)
        {
            bool result = false;
            strResult = string.Empty;
            var ibm = InBillMasterRepository.GetQueryable().FirstOrDefault(i => i.BillNo == BillNo);
            if (ibm != null && ibm.Status == "5")
            {
                using (var scope = new TransactionScope())
                {
                    try
                    {
                        //修改分配入库冻结量
                        var inAllot = InBillAllotRepository.GetQueryable()
                                                           .Where(o => o.BillNo == ibm.BillNo
                                                               && o.Status != "2")
                                                           .ToArray();
                        var storages = inAllot.Select(i => i.Storage).ToArray();

                        if (!Locker.Lock(storages))
                        {
                            strResult = "锁定储位失败，储位其他人正在操作，无法结单请稍候重试！";
                            return false;
                        }

                        inAllot.AsParallel().ForAll(
                            (Action<InBillAllot>)delegate(InBillAllot i)
                        {
                            if (i.Storage.ProductCode == i.ProductCode
                                && i.Storage.InFrozenQuantity >= i.AllotQuantity)
                            {
                                i.Storage.InFrozenQuantity -= i.AllotQuantity;
                                i.Storage.LockTag = string.Empty;
                            }
                            else
                            {
                                throw new Exception("储位的卷烟或入库冻结量与当前分配不符，信息可能被异常修改，不能结单！");
                            }
                        }
                        );

                        ibm.Status = "6";
                        ibm.UpdateTime = DateTime.Now;
                        InBillMasterRepository.SaveChanges();
                        scope.Complete();
                        result = true;
                    }
                    catch (Exception e)
                    {
                        strResult = "入库单结单出错！原因：" + e.Message;
                    }
                }
            }
            return result;
        }

        #endregion

        public bool DownInBillMaster(string BeginDate, string EndDate, out string errorInfo)
        {
            errorInfo = string.Empty;
            bool result = false;
            string inBillStr = "";
            string inBillMasterStr = "";
            try
            {
                var inBillNos = InBillMasterRepository.GetQueryable().Where(i => i.BillNo == i.BillNo).Select(i => new { i.BillNo }).ToArray();
                
                for (int i = 0; i < inBillNos.Length; i++)
                {
                    inBillStr += inBillNos[i].BillNo + ",";
                }
                InBillMaster[] inBillMasterList = InBillMasterDownService.GetInBillMaster(inBillStr);
                foreach (var master in inBillMasterList)
                {
                    var inBillMaster = new InBillMaster();
                    inBillMaster.BillNo = master.BillNo;
                    inBillMaster.BillDate = master.BillDate;
                    inBillMaster.BillTypeCode = master.BillTypeCode;
                    inBillMaster.WarehouseCode = master.WarehouseCode;
                    inBillMaster.Status = "1";                
                    inBillMaster.IsActive = master.IsActive;
                    inBillMaster.UpdateTime = DateTime.Now;
                    InBillMasterRepository.Add(inBillMaster);
                    inBillMasterStr += master.BillNo + ",";
                }
                if (inBillMasterStr != string.Empty)
                {
                    InBillDetail[] inBillDetailList = InBillMasterDownService.GetInBillDetail(inBillMasterStr);
                    foreach (var detail in inBillDetailList)
                    {
                        var inBillDetail = new InBillDetail();
                        inBillDetail.BillNo = detail.BillNo;
                        inBillDetail.ProductCode = detail.ProductCode;
                        inBillDetail.UnitCode = detail.UnitCode;
                        inBillDetail.Price = detail.Price;
                        inBillDetail.BillQuantity = detail.BillQuantity;
                        inBillDetail.AllotQuantity = detail.AllotQuantity;
                        inBillDetail.RealQuantity = detail.RealQuantity;
                        inBillDetail.Description = detail.Description;
                        InBillDetailRepository.Add(inBillDetail);
                    }
                }
                InBillMasterRepository.SaveChanges();
                result = true;
            }
            catch (Exception e)
            {
                errorInfo = "出错，原因：" + e.Message;
            }
            return result;
        }
    }
}
