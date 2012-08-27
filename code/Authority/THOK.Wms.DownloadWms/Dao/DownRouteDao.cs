using System;
using System.Collections.Generic;
using System.Text;
using THOK.Util;
using System.Data;

namespace THOK.WMS.DownloadWms.Dao
{
    public class DownRouteDao : BaseDao
    {
        #region �����ͻ���·��Ϣ

        /// <summary>
        /// �����ͻ���·����Ϣ
        /// </summary>
        public DataTable GetRouteInfo(string routeCodeList)
        {
            string sql = string.Format("SELECT * FROM V_WMS_DELIVER_LINE WHERE {0}", routeCodeList);
            return this.ExecuteQuery(sql).Tables[0];
        }

        /// <summary>
        /// �����ͻ���·����Ϣ
        /// </summary>
        public DataTable GetRouteInfo()
        {
            string sql = "SELECT * FROM V_WMS_DELIVER_LINE ";
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

        /// <summary>
        /// ����ͻ���·��
        /// </summary>
        public void Delete()
        {
            string sql = "DELETE wms_deliver_line";
            this.ExecuteNonQuery(sql);
        }

        #endregion
    }
}
