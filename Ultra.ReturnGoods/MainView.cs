using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ultra.Surface.Form;
using Ultra.Surface.Lanuch;
using Ultra.Surface.Extend;
using Ultra.Surface.Common;
using DbEntity;
using PetaPoco;

namespace Ultra.ReturnGoods {
    public partial class MainView : BaseForm, Ultra.Surface.Interfaces.ISurfacePermission {
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
                new Ultra.Surface.Interfaces.PermitGridView(this.gvUnAudit,"待审核"),
                new Ultra.Surface.Interfaces.PermitGridView(this.gvAudit,"已审核"),
                new Ultra.Surface.Interfaces.PermitGridView(this.gvInvalid,"已作废"),
                new Ultra.Surface.Interfaces.PermitGridView(this.gvOrder,"商品信息")
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
                    myBar.btnCreate,
                    myBar.btnRefresh,
                    myBar.btnExport,
                    myBar.btnReView,
                    myBar.btnInvalid,
                };
            }
        }

        #endregion

        public MainView() {
            InitializeComponent();
        }

        private void MainView_Load(object sender, EventArgs e) {
            myBar.btnRefresh.ItemClick += btnRefresh_ItemClick;
            myBar.btnReView.ItemClick += btnReView_ItemClick;
            myBar.btnCreate.ItemClick += btnCreate_ItemClick;
            myBar.btnExport.ItemClick += btnExport_ItemClick;
            myBar.btnInvalid.ItemClick += btnInvalid_ItemClick;

            gvUnAudit.FocusedRowChanged += gv_FocusedRowChanged;
            gvAudit.FocusedRowChanged += gv_FocusedRowChanged;
            gvInvalid.FocusedRowChanged += gv_FocusedRowChanged;
        }

        void gv_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e) {
            t_rettrade trd = null;
            switch (tabMain.SelectedTabPage.Text) {
                case "待审核":
                    trd=gcUnAudit.GetFocusedDataSource<t_rettrade>();
                    break;
                case "已审核":
                    trd = gcAudit.GetFocusedDataSource<t_rettrade>();
                    break;
                case "已作废":
                    trd = gcInvalid.GetFocusedDataSource<t_rettrade>();
                    break;
                default:
                    break;
            }
            if (trd == null) {
                gcOrder.DataSource = null;
                return;
            }
            using (var db = new Database()) {
                gcOrder.DataSource = db.Fetch<t_retorder>(" where tradeguid=@0",trd.Guid);
            }
        }

        void btnInvalid_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            var trd = gcUnAudit.GetFocusedDataSource<t_rettrade>();
            if (trd == null)
                return;
            using (var db = new Database()) {
                db.Execute("update t_rettrade set isinvalid=1 where guid=@0", trd.Guid);
            }
            gcUnAudit.RemoveSelected();
        }

        void btnExport_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            switch (tabMain.SelectedTabPage.Text) {
                case "待审核":
                    gcUnAudit.GridExportXls();
                    break;
                case "已审核":
                    using (var db = new Database()) {
                        gcAudit.GridExportXls();
                    }
                    break;
                case "已作废":
                    using (var db = new Database()) {
                        gcInvalid.GridExportXls();
                    }
                    break;
                default:
                    break;
            }
        }


        void btnCreate_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            var vw = new NewView();
            vw.EditMode = Web.Core.Enums.EnViewEditMode.New;
            if (Lanucher.InitView(vw).ShowDialog() == System.Windows.Forms.DialogResult.OK) {

            }
        }

        void btnReView_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            var trd = gcUnAudit.GetFocusedDataSource<t_rettrade>();
            if (trd == null)
                return;
            using (var db = new Database()) {
                try {
                    db.BeginTransaction();
                    //入库更新库存
                    db.Execute("exec p_retgoodsupdateinvt @0", trd.Guid);
                    db.Execute("update t_rettrade set isaudit=1 where guid=@0", trd.Guid);
                    db.CompleteTransaction();
                    gcUnAudit.RemoveSelected();
                } catch (Exception) {
                    db.AbortTransaction();
                    throw;
                }
            }
        }

        void btnRefresh_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            switch (tabMain.SelectedTabPage.Text) {
                case "待审核":
                    using (var db = new Database()) {
                        gcUnAudit.DataSource = db.Fetch<t_rettrade>(" where isaudit=0 and isnull(isinvalid,0)=0");
                    }
                    break;
                case "已审核":
                    using (var db = new Database()) {
                        gcAudit.DataSource = db.Fetch<t_rettrade>(" where isaudit=1 and isnull(isinvalid,0)=0");
                    }
                    break;
                case "已作废":
                    using (var db = new Database()) {
                        gcInvalid.DataSource = db.Fetch<t_rettrade>(" where isinvalid=1");
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
