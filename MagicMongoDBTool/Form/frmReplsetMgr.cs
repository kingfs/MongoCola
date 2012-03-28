﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MagicMongoDBTool.Module;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Bson;

namespace MagicMongoDBTool
{
    public partial class frmReplsetMgr : Form
    {
        ConfigHelper.MongoConnectionConfig _config;
        public frmReplsetMgr(ref ConfigHelper.MongoConnectionConfig config)
        {
            InitializeComponent();
            _config = config;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdAddHost_Click(object sender, EventArgs e)
        {
            CommandResult Result = MongoDBHelper.AddToReplsetServer(SystemManager.GetCurrentService(),
                          txtReplHost.Text.ToString() + ":" + NumReplPort.Value.ToString(), (int)NumPriority.Value, chkArbiterOnly.Checked);
            if (!Result.Response.ToBsonDocument().GetElement("retval").Value.IsBsonDocument)
            {
                _config.ReplsetList.Add(txtReplHost.Text.ToString() + ":" + NumReplPort.Value.ToString());
                MyMessageBox.ShowMessage("Add Memeber", "Result:OK");
            }
            else
            {
                if (Result.Response.ToBsonDocument().GetElement("retval").Value.AsBsonDocument.GetElement("ok").Value.ToString() == "1")
                {
                    _config.ReplsetList.Add(txtReplHost.Text.ToString() + ":" + NumReplPort.Value.ToString());
                    MyMessageBox.ShowMessage("Add Memeber", "Result:OK");
                }
                else
                {
                    MyMessageBox.ShowMessage("Add Memeber", "Result:Fail", Result.Response.ToString());
                }
            }
        }
        /// <summary>
        /// 移除主机
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdRemoveHost_Click(object sender, EventArgs e)
        {
            //使用修改系统数据集和repleSetReconfig
            MongoCollection replsetCol = SystemManager.GetCurrentService().
                                      GetDatabase(MongoDBHelper.DATABASE_NAME_LOCAL).GetCollection("system.replset");
            BsonDocument ReplsetDoc = replsetCol.FindOneAs<BsonDocument>();
            BsonArray memberlist = ReplsetDoc.GetElement("members").Value.AsBsonArray;
            String strHost = lstHost.SelectedItem.ToString();
            for (int i = 0; i < memberlist.Count; i++)
            {
                if (memberlist[i].AsBsonDocument.GetElement("host").Value.ToString() == strHost)
                {
                    memberlist.RemoveAt(i);
                    break;
                }
            }
            List<CommandResult> Resultlst = new List<CommandResult>();
            CommandResult Result = MongoDBHelper.ReconfigReplsetServer(SystemManager.GetCurrentService(), ReplsetDoc);
            ///由于这个命令会触发异常，所以没有Result可以获得
            _config.ReplsetList.Remove(strHost);
            lstHost.Items.Remove(lstHost.SelectedItem);
            MyMessageBox.ShowMessage("Remove Memeber", "Please wait one minute and check the server list");

        }

        private void frmReplsetMgr_Load(object sender, EventArgs e)
        {
            if (!SystemManager.IsUseDefaultLanguage())
            {
                cmdClose.Text = SystemManager.mStringResource.GetText(StringResource.TextType.Common_Close);
                cmdAddHost.Text = SystemManager.mStringResource.GetText(StringResource.TextType.AddConnection_Region_AddHost);
                cmdRemoveHost.Text = SystemManager.mStringResource.GetText(StringResource.TextType.AddConnection_Region_RemoveHost);
                lblpriority.Text = SystemManager.mStringResource.GetText(StringResource.TextType.AddConnection_Priority);
                lblReplHost.Text = SystemManager.mStringResource.GetText(StringResource.TextType.Common_Host);
                lblReplPort.Text = SystemManager.mStringResource.GetText(StringResource.TextType.Common_Port);
            }

            MongoServer server = SystemManager.GetCurrentService();
            foreach (var item in server.Instances)
            {
                lstHost.Items.Add(item.Address.ToString());
            }
        }
        private void cmdClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}