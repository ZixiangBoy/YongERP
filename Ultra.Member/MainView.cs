﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ultra.Surface.Form;
using Ultra.Surface.Lanuch;
using Ultra.Surface.Extend;
using Ultra.Surface.Common;
using Ultra.Web.Core.Common;
using DbEntity;
using System.Data.SqlClient;
using PetaPoco;
using DevExpress.XtraBars;

namespace Ultra.Member
{
    public partial class MainView : BaseForm, Ultra.Surface.Interfaces.ISurfacePermission
    {
        #region ISurfacePermission 成员

        public List<Control> ButtonItems {
            get;
            set;
        }

        public List<BaseSurface> DialogForms {
            get;
            set;
        }

        public List<Ultra.Surface.Interfaces.PermitGridView> Grids {
            get {
                return new List<Ultra.Surface.Interfaces.PermitGridView> { 
                    new Ultra.Surface.Interfaces.PermitGridView(this.gridView1,"客户信息")
                };
            }
        }

        public List<Control> MenuItems {
            get;
            set;
        }

        public List<DevExpress.XtraBars.BarButtonItem> ToolBarItems {
            get {
                return new List<DevExpress.XtraBars.BarButtonItem> { 
                    this.myBar.btnCreate,
                    this.myBar.btnModify,
                    this.myBar.btnRefresh,
                    this.myBar.btnExport,
                    myBar.btnUsing,
                    myBar.btnUnUsing
                };
            }
        }

        #endregion

        public MainView()
        {
            InitializeComponent();
        }

        private void MainView_Load(object sender, EventArgs e)
        {
            myBar.btnCreate.ItemClick += barBtnNew_ItemClick;
            myBar.btnRefresh.ItemClick += barBtnRefresh_ItemClick;
            myBar.btnModify.ItemClick += barBtnEdt_memberClick;
            myBar.btnExport.ItemClick += barBtnExport_memberClick;
            myBar.btnUsing.ItemClick += btnUsing_ItemClick;
            myBar.btnUnUsing.ItemClick += btnUnUsing_ItemClick;
        }

        void btnUnUsing_ItemClick(object sender, ItemClickEventArgs e) {
            var et = gridControlEx1.GetFocusedDataSource<t_member>();
            if (null == et) return;
            using (var db = new Database()) {
                db.Update<t_member>(" set isusing=0 where guid=@0",et.Guid);
            }
            et.IsUsing=false;
            gridControlEx1.RefreshDataSource();
        }

        void btnUsing_ItemClick(object sender, ItemClickEventArgs e) {
            var et = gridControlEx1.GetFocusedDataSource<t_member>();
            if (null == et) return;
            using (var db = new Database()) {
                db.Update<t_member>(" set isusing=1 where guid=@0", et.Guid);
            }
            et.IsUsing = true;
            gridControlEx1.RefreshDataSource();
        }

        void barBtnExport_memberClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.gridControlEx1.GridExportXls();
        }

        void barBtnEdt_memberClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            gridControlEx1_RowCellDoubleClick(sender, null);
        }

        void barBtnRefresh_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (var db = new Database())
            {
                this.gridControlEx1.DataSource = db.Fetch<t_member>("select * from t_member");
            }
        }

        void barBtnNew_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var vw = new NewView();
            Lanucher.InitView(vw);
            vw.EditMode = Ultra.Web.Core.Enums.EnViewEditMode.New;
            if (vw.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                barBtnRefresh_ItemClick(sender, e);
            }
        }

        private void gridControlEx1_RowCellDoubleClick(object sender, MouseEventArgs e)
        {
            var et = gridControlEx1.GetFocusedDataSource<t_member>();
            if (null == et) return;
            var vw = new NewView();
            Lanucher.InitView(vw);
            vw.EditMode = Ultra.Web.Core.Enums.EnViewEditMode.Edit;
            vw.GuidKey = et.Guid;
            if (vw.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                barBtnRefresh_ItemClick(sender, null);
            }
            
        }
    }
}
