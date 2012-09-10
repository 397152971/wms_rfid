using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using THOK.WES.Dal;
using THOK.WES;
using THOK.WES.Interface;
using THOK.WES.Interface.Model;
using System.Collections;

namespace THOK.WES.View
{
    public partial class BaseTaskForm : THOK.AF.View.ToolbarForm
    {
        private ConfigUtil configUtil = new ConfigUtil();
        private string operateStorageName = "";
        private string targetStorageName = "";
        private string operateName = "";
        private string operateProductName = "";
        private int operatePieceQuantity = 0;
        private int operateBarQuantity = 0;

        private string url = @"http://59.61.87.212:8090/Task";

        /// <summary>
        /// 1����ⵥ��2�����ⵥ��3���ƿⵥ��4���̵㵥
        /// </summary>
        protected string BillTypes = "";

        //ѡ���������
        BillMaster BillMaster = null;

        private string RfidReadProductCode = "";

        /// <summary>
        /// Real: ʵʱ���⣻NoReal: ��ʵʱ���⣻
        /// </summary>
        private string OperateType = "";

        /// <summary>
        /// �������� = 0�����̹�1��N �����ܲ�ţ�
        /// </summary>
        private string OperateAreas = "";

        /// <summary>
        /// ʹ�õ��ӱ�ǩ = 0����ʹ�ã�1��ʹ�ã�
        /// </summary>
        private string UseTag = "";

        /// <summary>
        /// ʹ��Rfid  = 0����ʹ�ã�1��ʹ�ã�
        /// </summary>
        private string UseRfid = "";

        public BaseTaskForm()
        {
            InitializeComponent();

            url = configUtil.GetConfig("URL")["URL"];
            OperateAreas = configUtil.GetConfig("Layers")["Number"];
            UseRfid = configUtil.GetConfig("RFID")["USEDRFID"];

            if (configUtil.GetConfig("DeviceType")["Device"] == "0")
            {
                this.dgvMain.ColumnHeadersHeight = 40;
                this.dgvMain.RowTemplate.Height = 40;
                this.dgvMain.DefaultCellStyle.Font = new Font("����", 16);
                this.dgvMain.ColumnHeadersDefaultCellStyle.Font = new Font("����", 13);
                this.btnBatConfirm.Visible = false;
                UseTag = "0";
            }
            else if (configUtil.GetConfig("DeviceType")["Device"] == "1")
            {
                this.dgvMain.ColumnHeadersHeight = 40;
                this.dgvMain.RowTemplate.Height = 40;
                this.dgvMain.DefaultCellStyle.Font = new Font("����", 16);
                this.dgvMain.ColumnHeadersDefaultCellStyle.Font = new Font("����", 13);
                this.btnBatConfirm.Visible = false;
                UseTag = "1";
            }
            else
            {
                this.dgvMain.ColumnHeadersHeight = 22;
                this.dgvMain.RowTemplate.Height = 22;
                this.dgvMain.DefaultCellStyle.Font = new Font("����", 10);
                this.dgvMain.ColumnHeadersDefaultCellStyle.Font = new Font("����", 10);
                UseTag = "1";
            }
        }

        //��ѯ
        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string billNo = string.Empty;
                Task task = new Task(Application.OpenForms[0], url);
                task.GetBillMasterCompleted += new Task.GetBillMasterCompletedEventHandler(delegate(bool isSuccess, string msg, BillMaster[] billMasters)
                {
                    ClosePlWailt();
                    if (billMasters != null)
                    {
                        switch (billMasters.Length)
                        {
                            case 0:
                                billNo = "";
                                break;
                            case 1:
                                billNo = billMasters[0].BillNo;
                                break;
                            default:
                                SelectDialog selectDialog = new SelectDialog(billMasters);
                                if (selectDialog.ShowDialog() == DialogResult.OK)
                                {
                                    billNo = selectDialog.SelectedBillID;
                                }
                                break;
                        }
                        foreach (BillMaster billMaster in billMasters)
                        {
                            if (billNo == billMaster.BillNo)
                            {
                                this.BillMaster = billMaster;
                            }
                        }
                    }
                    RefreshData();
                });
                task.SearchBillMaster(BillTypes);
                DisplayPlWailt();
            }
            catch (Exception ex)
            {
                MessageBox.Show("��ȡ����ʧ�ܣ�ԭ��" + ex.Message, "��Ϣ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        //ˢ������
        private void RefreshData()
        {
            if (BillMaster == null)
            {
                dgvMain.DataSource = null;
                return;
            }
            sslBillID.Text = "���ݺţ�" + BillMaster.BillNo + "                              ";
            sslOperator.Text = "����Ա��" + Environment.MachineName;

            Task task = new Task(Application.OpenForms[0], url);
            task.GetBillDetailCompleted += new Task.GetBillDetailCompletedEventHandler(delegate(bool isSuccess, string msg, BillDetail[] billDetails)
            {
                InTask = false;
                if (billDetails != null && billDetails.Length != 0)
                {
                    dgvMain.AutoGenerateColumns = false;
                    dgvMain.DataSource = billDetails;
                    foreach (BillDetail billDetail in billDetails)
                    {
                        if (billDetail.Status == "1")
                        {
                            InTask = true;
                            btnCancel.Enabled = true;
                            btnConfirm.Enabled = true;
                            btnBatConfirm.Enabled = true;
                        }
                        else
                        {
                            btnApply.Enabled = true;
                        }
                    }
                }
                else
                {
                    dgvMain.DataSource = null;
                }
                ClosePlWailt();
            });
            task.SearchBillDetail(new BillMaster[] { this.BillMaster }, RfidReadProductCode, OperateType, OperateAreas, Environment.MachineName);
            DisplayPlWailt();
        }

        //����
        private void btnApply_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvMain.SelectedRows.Count != 0)
                {
                    DisplayPlWailt();
                    IList<BillDetail> billDetails = new List<BillDetail>();
                    foreach (DataGridViewRow row in dgvMain.SelectedRows)
                    {
                        BillDetail billDetail = new BillDetail();
                        billDetail.BillNo = row.Cells["BillNo"].Value.ToString();
                        billDetail.BillType = row.Cells["@BillType"].Value.ToString();
                        billDetail.DetailID = Convert.ToInt32(row.Cells["DetailID"].Value);
                        billDetail.PieceQuantity = Convert.ToInt32(row.Cells["PieceQuantity"].Value);
                        billDetail.BarQuantity = Convert.ToInt32(row.Cells["BarQuantity"].Value);
                        billDetail.Operator = Environment.MachineName;
                        billDetails.Add(billDetail);
                    }
                    BillDetail[] tmp = new BillDetail[billDetails.Count];
                    billDetails.CopyTo(tmp, 0);

                    Task task = new Task(Application.OpenForms[0], url);
                    task.ApplyCompleted += new Task.ApplyCompletedEventHandler(delegate(bool isSuccess, string msg)
                    {
                        RefreshData();
                    });
                    task.Apply(tmp, UseTag);
                }
                else
                    MessageBox.Show("��ѡ��Ҫִ�еĲֿ���ҵ��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show("����ʧ�ܣ�ԭ��" + ex.Message, "��Ϣ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        //ȡ������
        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvMain.SelectedRows.Count != 0)
                {
                    DisplayPlWailt();
                    IList<BillDetail> billDetails = new List<BillDetail>();
                    foreach (DataGridViewRow row in dgvMain.SelectedRows)
                    {
                        BillDetail billDetail = new BillDetail();
                        billDetail.BillNo = row.Cells["BillNo"].Value.ToString();
                        billDetail.BillType = row.Cells["@BillType"].Value.ToString();
                        billDetail.DetailID = Convert.ToInt32(row.Cells["DetailID"].Value);
                        billDetail.Operator = Environment.MachineName;
                        billDetails.Add(billDetail);
                    }
                    BillDetail[] tmp = new BillDetail[billDetails.Count];
                    billDetails.CopyTo(tmp, 0);

                    Task task = new Task(Application.OpenForms[0], url);
                    task.CancelCompleted += new Task.CancelCompletedEventHandler(delegate(bool isSuccess, string msg)
                    {
                        RefreshData();
                    });
                    task.Cancel(tmp, UseTag);
                }
                else
                    MessageBox.Show("��ѡ��Ҫȡ���Ĳֿ���ҵ��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ȡ��ʧ�ܣ�ԭ��" + ex.Message, "��Ϣ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        //ȷ��
        private void btnConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvMain.SelectedRows.Count > 1)
                {
                    MessageBox.Show("��ǰ����ֻ�������һ������", "��ʾ",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (dgvMain.SelectedRows.Count == 1)
                {
                    IList<BillDetail> billDetails = new List<BillDetail>();
                    BillDetail billDetail = new BillDetail();
                    foreach (DataGridViewRow row in dgvMain.SelectedRows)
                    {
                        if (row.Cells["StatusName"].Value.ToString() == "������")
                        {
                            billDetail.BillNo = row.Cells["BillNo"].Value.ToString();
                            billDetail.BillType = row.Cells["@BillType"].Value.ToString();
                            billDetail.DetailID = Convert.ToInt32(row.Cells["DetailID"].Value);
                            billDetail.Operator = Environment.MachineName;

                            billDetail.OperatePieceQuantity = Convert.ToInt32(row.Cells["OperatePieceQuantity"].Value);
                            billDetail.OperateBarQuantity = Convert.ToInt32(row.Cells["OperateBarQuantity"].Value);

                            operateStorageName = row.Cells["Storage"].Value.ToString();
                            targetStorageName = row.Cells["TargetStorage"].Value.ToString();
                            operateName = row.Cells["BillTypeName"].Value.ToString();
                            operateProductName = row.Cells["ProductName"].Value.ToString();
                            operatePieceQuantity = Convert.ToInt32(row.Cells["OperatePieceQuantity"].Value);
                            operateBarQuantity = Convert.ToInt32(row.Cells["OperateBarQuantity"].Value);
                        }
                    }

                    ConfirmDialog confirmForm = new ConfirmDialog(BillTypes, operateStorageName, targetStorageName, operateName, operateProductName);
                    confirmForm.Piece = operatePieceQuantity;
                    confirmForm.Item = operateBarQuantity;

                    if (confirmForm.ShowDialog() == DialogResult.OK)
                    {
                        DisplayPlWailt();

                        if (BillTypes == "4")
                        {
                            billDetail.OperatePieceQuantity = confirmForm.Piece;
                            billDetail.OperateBarQuantity = confirmForm.Item;
                        }

                        //todo RFID ȷ�ϣ���RFID ��¼��

                        billDetails.Add(billDetail);
                        BillDetail[] tmp = new BillDetail[billDetails.Count];
                        billDetails.CopyTo(tmp, 0);

                        Task task = new Task(Application.OpenForms[0], url);
                        task.ExecuteCompleted += new Task.ExecuteCompletedEventHandler(delegate(bool isSuccess, string msg)
                        {
                            RefreshData();
                        });
                        task.Execute(tmp, UseTag);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ִ��ʧ�ܣ�ԭ��" + ex.Message, "��Ϣ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        //����ȷ��
        private void btnBatConfirm_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("��ǰ����������ȷ��ѡ������������������", "��ʾ",
                MessageBoxButtons.YesNo, MessageBoxIcon.Information) != DialogResult.Yes)
            {
                return;
            }
            try
            {
                if (dgvMain.SelectedRows.Count != 0)
                {
                    DisplayPlWailt();
                    IList<BillDetail> billDetails = new List<BillDetail>();
                    foreach (DataGridViewRow row in dgvMain.SelectedRows)
                    {
                        BillDetail billDetail = new BillDetail();
                        billDetail.BillNo = row.Cells["BillNo"].Value.ToString();
                        billDetail.BillType = row.Cells["@BillType"].Value.ToString();
                        billDetail.DetailID = Convert.ToInt32(row.Cells["DetailID"].Value);
                        billDetail.Operator = Environment.MachineName;

                        billDetail.OperatePieceQuantity = Convert.ToInt32(row.Cells["OperatePieceQuantity"].Value);
                        billDetail.OperateBarQuantity = Convert.ToInt32(row.Cells["OperateBarQuantity"].Value);

                        billDetails.Add(billDetail);
                    }
                    BillDetail[] tmp = new BillDetail[billDetails.Count];
                    billDetails.CopyTo(tmp, 0);

                    Task task = new Task(Application.OpenForms[0], url);
                    task.ExecuteCompleted += new Task.ExecuteCompletedEventHandler(delegate(bool isSuccess, string msg)
                    {
                        RefreshData();
                    });
                    task.Execute(tmp, UseTag);
                }
                else
                    MessageBox.Show("��ѡ��Ҫִ�еĲֿ���ҵ��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ִ��ʧ�ܣ�ԭ��" + ex.Message, "��Ϣ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void DisplayPlWailt()
        {
            this.plWailt.Visible = true;
            this.plWailt.Left = (this.dgvMain.Width - this.plWailt.Width) / 2;
            this.plWailt.Top = (this.dgvMain.Height - this.plWailt.Height) / 2;
            this.btnSearch.Enabled = false;
            this.btnExit.Enabled = false;
            this.btnApply.Enabled = false;
            this.btnCancel.Enabled = false;
            this.btnConfirm.Enabled = false;
            this.btnBatConfirm.Enabled = false;
        }

        public void ClosePlWailt()
        {
            this.plWailt.Visible = false;
            this.btnSearch.Enabled = true;
            this.btnExit.Enabled = true;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Exit();
        }

        private void btnOpType_Click(object sender, EventArgs e)
        {
            if (btnOpType.Text != "����")
            {
                btnOpType.Text = "����";
                OperateType = "NoReal";
            }
            else
            {
                btnOpType.Text = "ʵʱ";
                OperateType = "Real";

                //todo ����ʵʱ��ҵ��Ϣ��������
            }
        }
    }
}

