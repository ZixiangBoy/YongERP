using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PetaPoco;
using Ultra.Surface.Common;
using Ultra.Surface.Form;
using Ultra.Web.Core.Common;
using DbEntity;
using Ultra.Surface.Extend;
using DevExpress.XtraEditors;

namespace Ultra.ReturnGoods {
    public partial class NewView : DialogView {

        private t_rettrade Trade;

        public NewView() {
            InitializeComponent();
        }

        private void NewView_Load(object sender, EventArgs e) {

        }

        private void btnOK_Click(object sender, EventArgs e) {
            if (!dxValidationProvider1.Validate())
                return;
            var odrs = gcOrder.GetDataSource<t_retorder>();
            if (odrs == null || odrs.Count < 1) {
                MsgBox.ShowMessage("提示", "没有退货商品不能保存退货单!");
                return;
            }
            if (odrs.Any(k=>string.IsNullOrEmpty(k.LocName))) {
                MsgBox.ShowMessage("提示", "所有商品都必须选择库位!");
                return;
            }
            if (EditMode == Ultra.Web.Core.Enums.EnViewEditMode.New) {
                var rettrd = this.Trade;
                rettrd.Guid = Guid.NewGuid();
                rettrd.Id = 0;
                rettrd.IsAudit = false;
                using (var db = new Database()) {
                    try {
                        db.BeginTransaction();
                        db.Save(rettrd);
                        odrs.ForEach(k => { 
                            k.Guid = Guid.NewGuid();
                            k.Id = 0; 
                            k.TradeGuid = rettrd.Guid; 
                            db.Save(k); 
                        });
                        db.CompleteTransaction();
                    } catch (Exception) {
                        db.AbortTransaction();
                        throw;
                    }
                }
            } else if (EditMode == Ultra.Web.Core.Enums.EnViewEditMode.Edit) {
                using (var db = new Database()) {
                    var trd = db.FirstOrDefault<t_rettrade>(" where guid=@0", GuidKey);
                    trd.ReceiverName = Trade.ReceiverName;
                    trd.ReceiverMobile = txtMobile.Text;
                    trd.ReceiverAddress = txtReceiverAddress.Text;
                    trd.MemberGuid = Trade.Guid;
                    trd.DeliveryDate = TimeSync.Default.CurrentSyncTime;
                    trd.DeliveryDate = dateDeliveryDate.DateTime;
                    try {
                        db.BeginTransaction();
                        db.Save(trd);
                        db.Execute("delete t_retorder where tradeguid=@0", trd.Guid);
                        odrs.ForEach(k => {
                            k.Id = 0;
                            db.Save(k);
                        });
                        db.CompleteTransaction();
                    } catch (Exception) {
                        db.AbortTransaction();
                        throw;
                    }
                }
            }
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void btnClose_Click(object sender, EventArgs e) {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }

        private void rspNum_EditValueChanged(object sender, EventArgs e) {
            var odr = gcOrder.GetFocusedDataSource<t_retorder>();
            if (odr == null)
                return;
            var spn = sender as DevExpress.XtraEditors.SpinEdit;
            odr.RetNum = (int)spn.Value;
            odr.OrderPrice = odr.Price * odr.RetNum;
            gcOrder.RefreshDataSource();
        }

        private void txtTradeNo_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e) {
            var vw = new SendedTradeView();
            if (vw.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                this.Trade = vw.Trade;

                txtReceiverName.Text = this.Trade.ReceiverName;
                dateDeliveryDate.DateTime = this.Trade.DeliveryDate ?? DateTime.Now;
                txtMobile.Text = this.Trade.ReceiverMobile;
                txtReceiverAddress.Text = this.Trade.ReceiverAddress;
                txtTradeNo.Text = this.Trade.TradeNo;

                using (var db = new Database()) {
                    var retodrs = db.Fetch<t_retorder>("select * from v_retorder where tradeno=@0", vw.Trade.TradeNo);
                    retodrs.ForEach(k=>k.RetNum=k.Num);
                    gcOrder.DataSource = retodrs;
                }
            }
        }

        private void gvOrder_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e) {
            var odr = gcOrder.GetFocusedDataSource<t_retorder>();
            if (odr == null)
                return;

            if (odr.RetNum > odr.Num) {
                e.ErrorText = "退货数量不能大于销售单的数量";
                e.Valid = false;
            }
        }

        private void gvOrder_ShownEditor(object sender, EventArgs e) {
            var gv = sender as DevExpress.XtraGrid.Views.Grid.GridView;
            if (gv == null)
                return;
            var gc = gv.ActiveEditor as GridLookUpEdit;
            if (gc == null)
                return;
            using (var db = new Database()) {
                gc.Properties.DataSource = db.Fetch<t_location>(" where isusing=1");
            }
        }

        private void rspLoc_EditValueChanged(object sender, EventArgs e) {
            var odr = gcOrder.GetFocusedDataSource<t_retorder>();
            if (odr == null)
                return;
            var gc = sender as GridLookUpEdit;
            if (gc == null)
                return;
            var loc = gc.Properties.View.GetFocusedDataSource<t_location>();
            if (loc == null)
                return;
            odr.WareName = loc.WareName;
            odr.AreaName = loc.AreaName;
            odr.LocName = loc.LocName;
            gcOrder.RefreshDataSource();
        }
    }
}
