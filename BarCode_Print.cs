using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Extensions;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarCode
{
    public partial class BarCode_Print : Form
    {
        public BarCode_Print()
        {
            InitializeComponent();
            GridSetting();
        }

        private void btn_ExcelUp_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (gridView1.RowCount > 0)
            {
                MessageBox.Show("이미 업로드한 정보가 있습니다.", "오류");
            }
            else
            {
                OpenFileDialog openFile = new OpenFileDialog();
                openFile.Filter = "엑셀파일(*.xlsx)|*.xlsx|(*.xls)|*.xls";
                openFile.ShowDialog();
                gridControl1.DataSource = GetSheet(openFile.FileName);
            }
        }

        private DataTable GetSheet(string FilePath)
        {
            string oledbConnectionString = string.Empty;
            DataTable dt_Result = null;
            DataTable dt = null;
            string sheetName = string.Empty;
            string sQuery = string.Empty;
            OleDbDataAdapter adt = null;

            try
            {
                if (FilePath.IndexOf(".xlsx") > -1)
                {
                    oledbConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;dATA sOURCE=" + FilePath + ";Extended Properties=\"Excel 12.0;\"";

                    using (OleDbConnection con = new OleDbConnection(oledbConnectionString))
                    {
                        con.Open();

                        dt = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                        sheetName = dt.Rows[0]["TABLE_NAME"].ToString();
                        sQuery = string.Format(" SELECT * FROM [{0}]", sheetName);

                        dt_Result = new DataTable();
                        adt = new OleDbDataAdapter(sQuery, con);
                        adt.Fill(dt_Result);
                    }
                }
                else if (FilePath.IndexOf(".xls") > -1)
                {
                    oledbConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;DataSource=" + FilePath + ";Extended Properties=\"Excel 8.0;\"";

                    using (OleDbConnection con = new OleDbConnection(oledbConnectionString))
                    {
                        con.Open();

                        dt = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                        sheetName = dt.Rows[0]["TABLE_NAME"].ToString();
                        sQuery = string.Format(" SELECT * FROM [{0}]", sheetName);

                        dt_Result = new DataTable();
                        adt = new OleDbDataAdapter(sQuery, con);
                        adt.Fill(dt_Result);
                    }
                }
                else
                {
                    MessageBox.Show("확장자 .xls, .xlsx 만 선택 가능합니다.", "확인");
                }

                return dt_Result;
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message, "확인");
                return dt_Result;
            }
        }

        private void btn_Print_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            CheckEdit check = sender as CheckEdit;

            if(gridView1.SelectedRowsCount == 0)
            {
                MessageBox.Show("목록에서 출력 정보를 선택하세요.");
                return;
            }

            string defaultPrinter = string.Empty;
            string barCode = string.Empty;
            string pt = string.Empty;
            
            PrinterSettings ps = new PrinterSettings();
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                ps.PrinterName = printer;
                if (ps.IsDefaultPrinter)
                {
                    defaultPrinter = printer;
                }
            }

            var rows = gridView1.GetSelectedRows();
            foreach(int row in rows)
            {
                barCode = gridView1.GetRowCellValue(row, "Barcode").ToString();
                pt = gridView1.GetRowCellValue(row, "PT").ToString();

                PrintBarCode(defaultPrinter, barCode, pt);
            }

        }

        private void PrintBarCode(string printer, string barCode, string pt)
        {
            string xLocation = string.Empty;

            if(pt.Length == 1) { xLocation = "120"; }
            else if(pt.Length == 2) { xLocation = "90"; }
            else if(pt.Length == 3) { xLocation = "65"; }

            PrintDialog pd = new PrintDialog();

            string s = "";

            s += "^XA";

            // PT 테두리(원)
            s += "^FO40,100^GC220,3,B^FS";

            // PT
            s += "^FO" + xLocation + ",165^A0N,120,120^FD" + pt + "^FS";

            // 128바코드
            s += "^FO305,100^BY2^BCN,120,N,N^FD" + barCode + "^FS";

            // 바코드 값
            s += "^FO350,270^A0N,40,40^FD" + barCode + "^FS";
        
            s += "^XZ";

            CM_Function.RawPrinterHelper.SendStringToPrinter(printer, s);
        }

        private void btn_Exit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Application.Exit();
        }

        private void btn_Reset_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if(gridView1.RowCount == 0)
            {
                MessageBox.Show("목록이 비어 있습니다.", "오류");
                return;
            }
            else
            {
                gridControl1.DataSource = null;
            }
            
        }

        private void GridSetting()
        {
            for(int i = 0; i < gridView1.Columns.Count; i++)
            {
                gridView1.Columns[i].AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                gridView1.Columns[i].AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            }
        }

        private void gridView1_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            for (int i = 0; i < gridView1.Columns.Count; i++)
            {
                gridView1.Columns[i].AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                gridView1.Columns[i].AppearanceHeader.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            }
            if (e.Column.FieldName == "Barcode")
            {
                e.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            }
            if(e.Column.FieldName == "PT")
            {
                e.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            }
        }
    }
}
