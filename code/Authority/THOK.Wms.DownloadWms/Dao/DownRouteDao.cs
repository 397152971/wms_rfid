using System;
using System.Collections.Generic;
using System.Text;
using THOK.Util;
using System.Data;

namespace THOK.WMS.DownloadWms.Dao
{
    public class DownRouteDao : BaseDao
    {
        /// <summary>
        /// ��Ӫ�������ͻ���·����Ϣ
        /// </summary>
        public DataTable GetRouteInfo(string routeCodeList)
        {
            string sql = string.Format(@"SELECT * FROM V_DWV_ORD_DIST_BILL A
                                         LEFT JOIN V_WMS_DELIVER_LINE B ON A.DELIVER_LINE_CODE=B.DELIVER_LINE_CODE");
            return this.ExecuteQuery(sql).Tables[0];
        }

        /// <summary>
        /// �ӷּ������ͻ���·����Ϣ
        /// </summary>
        public DataTable GetSortRouteInfo(string routeCodeList)
        {
            string sql = string.Format(@"SELECT DELIVERLINECODE,DELIVERLINENAME FROM WMS_SORT_ORDER WHERE {0}
                                               GROUP BY DELIVERLINECODE,DELIVERLINENAME", routeCodeList);
            return this.ExecuteQuery(sql).Tables[0];
        }

    
        /// <summary>
        /// ��ѯ��·���е���·����
        /// </summary>
        /// <returns></returns>
        public DataTable GetRouteCode()
        {
            string sql = " SELECT deliver_line_code FROM wms_deliver_line";
            return this.ExecuteQuery(sql).Tables[0];
        }

        /// <summary>
        /// ����·��Ϣ�������ݿ�
        /// </summary>
        /// <param name="ds"></param>
        public void Insert(DataSet ds)
        {
            BatchInsert(ds.Tables["DWV_OUT_DELIVER_LINE"], "wms_deliver_line");
        }

      
    }
}
