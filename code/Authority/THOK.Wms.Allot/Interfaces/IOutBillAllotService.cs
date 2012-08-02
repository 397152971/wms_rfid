﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using THOK.Wms.DbModel;
using THOK.Wms.Allot.Interfaces;

namespace THOK.Wms.Allot.Interfaces
{
    public interface IOutBillAllotService:IService<OutBillAllot>
    {
        object Search(string billNo, int page, int rows);

        bool AllotCancel(string billNo, out string strResult);

        bool AllotConfirm(string billNo, out string strResult);

        bool AllotDelete(string billNo, long id, out string strResult);

        bool AllotEdit(string billNo, long id, string cellCode, int allotQuantity, out string strResult);
    }
}
