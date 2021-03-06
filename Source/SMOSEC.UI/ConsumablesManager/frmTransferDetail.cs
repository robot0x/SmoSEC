using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Smobiler.Core;
using Smobiler.Core.Controls;
using SMOSEC.DTOs.Enum;
using SMOSEC.DTOs.InputDTO;
using SMOSEC.Domain.Entity;
using System.Data;

namespace SMOSEC.UI.ConsumablesManager
{
    partial class frmTransferDetail : Smobiler.Core.Controls.MobileForm
    {
        #region "definition"
        AutofacConfig autofacConfig = new AutofacConfig();     //调用配置类
        public String TOID;     //调拨单编号
        #endregion
        /// <summary>
        /// 调拨确认
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSure_Press(object sender, EventArgs e)
        {
            frmTransferDeal frm = new frmTransferDeal();
            frm.TOID = TOID;
            frm.Type = PROCESSMODE.调拨确认;
            Show(frm, (MobileForm sender1, object args) =>
            {
                if (frm.ShowResult == ShowResult.Yes)
                    Bind();   //刷新数据显示
            });
        }
        /// <summary>
        /// 调拨取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Press(object sender, EventArgs e)
        {
            frmTransferDeal frm = new frmTransferDeal();
            frm.TOID = TOID;
            frm.Type = PROCESSMODE.调拨取消;
            Show(frm, (MobileForm sender1, object args) =>
            {
                if (frm.ShowResult == ShowResult.Yes)
                    Bind();   //刷新数据显示
            });
        }
        /// <summary>
        /// 页面初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmTransferDetail_Load(object sender, EventArgs e)
        {
            Bind();
        }
        /// <summary>
        /// 数据加载
        /// </summary>
        public void Bind()
        {
            try
            {
                TOInputDto TOData = autofacConfig.assTransferOrderService.GetByID(TOID);
                coreUser DeanInUser = autofacConfig.coreUserService.GetUserByID(TOData.MANAGER);
                coreUser DealUser = autofacConfig.coreUserService.GetUserByID(TOData.HANDLEMAN);
                AssLocation assLocation = autofacConfig.assLocationService.GetByID(TOData.DESLOCATIONID);
                lblTDInMan.Text = DeanInUser.USER_NAME;
                lblDealMan.Text = DealUser.USER_NAME;
                lblLocation.Text = assLocation.NAME;
                DatePicker.Value = TOData.TRANSFERDATE;
                if (String.IsNullOrEmpty(TOData.NOTE)==false) lblNote.Text = TOData.NOTE;

                DataTable tableAssets = new DataTable();      //未开启SN的资产列表
                tableAssets.Columns.Add("ASSID");             //资产编号
                tableAssets.Columns.Add("NAME");              //资产名称
                tableAssets.Columns.Add("IMAGE");             //资产图片
                tableAssets.Columns.Add("WAITREPAIRQTY");     //待确认数量
                tableAssets.Columns.Add("STATUS");            //行项状态
                foreach (AssTransferOrderRow Row in TOData.Rows)
                {
                    Assets assets = autofacConfig.orderCommonService.GetAssetsByID(Row.ASSID);
                    if (Row.STATUS == 0)
                    {
                        tableAssets.Rows.Add(Row.ASSID, assets.NAME , assets.IMAGE , Row.INTRANSFERQTY, "调拨中");
                    }
                    else if(Row.STATUS == 1)
                    {
                        tableAssets.Rows.Add(Row.ASSID, assets.NAME, assets.IMAGE, Row.INTRANSFERQTY, "已完成");
                    }
                    else
                    {
                        tableAssets.Rows.Add(Row.ASSID, assets.NAME, assets.IMAGE, Row.INTRANSFERQTY, "已取消");
                    }
                }
                if (tableAssets.Rows.Count > 0)
                {
                    ListAssets.DataSource = tableAssets;
                    ListAssets.DataBind();
                }
                if (Client.Session["Role"].ToString() == "SMOSECUser") plButton.Visible = false;
                //如果维修单已完成，则隐藏维修单处理按钮
                if (TOData.STATUS == 1 || TOData.STATUS==2) plButton.Visible = false;
            }
            catch (Exception ex)
            {
                Toast(ex.Message);
            }
        }
    }
}