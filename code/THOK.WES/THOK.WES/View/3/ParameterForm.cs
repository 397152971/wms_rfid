using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using THOK.Util;
using THOK.ParamUtil;
using THOK.WES.Dal;

namespace THOK.WES.View
{
    public partial class ParameterForm : THOK.AF.View.ToolbarForm
    {
        private Parameter parameter = new Parameter();
        private ConfigUtil configUtil = new ConfigUtil();
        private Dictionary<string, string> url = null;
        private Dictionary<string, string> udp = null;
        private Dictionary<string, string> rfid = null;
        private Dictionary<string, string> layers = null;
        private Dictionary<string, string> deviceType = null;

        public ParameterForm()
        {
            InitializeComponent();
            ReadParameter();        
        }

        private void ReadParameter()
        {
            url = configUtil.GetConfig("URL");
            parameter.Url = url["URL"];

            udp = configUtil.GetConfig("UDP");
            parameter.UdpIP = udp["IP"];
            parameter.UdpPort = udp["PORT"];
            
            rfid = configUtil.GetConfig("RFID");
            if (rfid["USEDRFID"] == "0")
                parameter.UsedRFID = false;
            else
                parameter.UsedRFID = true;
            parameter.RfidPort = rfid["PORT"];

            layers = configUtil.GetConfig("Layers");
            parameter.LayersNumber = layers["Number"];          

            deviceType = configUtil.GetConfig("DeviceType");
            parameter.SelectItem = Convert.ToInt32( deviceType["Device"]);
            propertyGrid.SelectedObject = parameter;            
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                url["URL"] = parameter.Url;
                configUtil.SaveConfig("URL", url);

                udp["IP"] = parameter.UdpIP;
                udp["PORT"] = parameter.UdpPort;
                configUtil.SaveConfig("UDP", udp);

                rfid["PORT"] = parameter.RfidPort;
                rfid["USEDRFID"] = parameter.UsedRFID ? "1" : "0";
                configUtil.SaveConfig("RFID", rfid);

                layers["Number"] = parameter.LayersNumber;
                configUtil.SaveConfig("Layers", layers);

                deviceType["Device"] = parameter.SelectItem.ToString();
                configUtil.SaveConfig("DeviceType", deviceType);        

                MessageBox.Show("ϵͳ��������ɹ���������������ϵͳ��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exp)
            {
                MessageBox.Show("����ϵͳ���������г����쳣��ԭ��" + exp.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }   

        private void btnExit_Click(object sender, EventArgs e)
        {
            Exit();
        }
    }
}
