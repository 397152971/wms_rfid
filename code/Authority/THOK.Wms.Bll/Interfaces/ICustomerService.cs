﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using THOK.Wms.DbModel;

namespace THOK.Wms.Bll.Interfaces
{
    public interface ICustomerService : IService<Customer>
    {
        bool DownDeliverLine(out string errorInfo);
    }
}