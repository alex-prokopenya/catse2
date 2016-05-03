using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Megatec.MasterTour.BusinessRules;
using Megatec.Common.DataAccess;

namespace RostingReportForm
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.dateFrom = new System.Windows.Forms.DateTimePicker();
            this.dateTo = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.operatorsList = new System.Windows.Forms.ListBox();
            this.bntOk = new System.Windows.Forms.Button();
            this.fontDialog1 = new System.Windows.Forms.FontDialog();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(139, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(143, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Период продаж";
            // 
            // dateFrom
            // 
            this.dateFrom.Location = new System.Drawing.Point(33, 56);
            this.dateFrom.Name = "dateFrom";
            this.dateFrom.Size = new System.Drawing.Size(156, 22);
            this.dateFrom.TabIndex = 1;
            this.dateFrom.Value = new System.DateTime(2015, 5, 31, 0, 0, 0, 0);
            this.dateFrom.ValueChanged += new System.EventHandler(this.dateFrom_ValueChanged);
            // 
            // dateTo
            // 
            this.dateTo.Location = new System.Drawing.Point(239, 56);
            this.dateTo.Name = "dateTo";
            this.dateTo.Size = new System.Drawing.Size(156, 22);
            this.dateTo.TabIndex = 2;
            this.dateTo.Value = new System.DateTime(2015, 5, 31, 0, 0, 0, 0);
            this.dateTo.ValueChanged += new System.EventHandler(this.dateTo_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(15, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "с";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(209, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(24, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "по";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label4.Location = new System.Drawing.Point(139, 115);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(117, 20);
            this.label4.TabIndex = 5;
            this.label4.Text = "Туроператор";
            // 
            // operatorsList
            // 
            this.operatorsList.DisplayMember = "value";
            this.operatorsList.FormattingEnabled = true;
            this.operatorsList.ItemHeight = 16;
            this.operatorsList.Location = new System.Drawing.Point(15, 144);
            this.operatorsList.Name = "operatorsList";
            this.operatorsList.Size = new System.Drawing.Size(380, 164);
            this.operatorsList.TabIndex = 6;
            this.operatorsList.ValueMember = "key";
            // 
            // bntOk
            // 
            this.bntOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.bntOk.Location = new System.Drawing.Point(143, 320);
            this.bntOk.Name = "bntOk";
            this.bntOk.Size = new System.Drawing.Size(128, 35);
            this.bntOk.TabIndex = 7;
            this.bntOk.Text = "Ok";
            this.bntOk.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(417, 367);
            this.Controls.Add(this.bntOk);
            this.Controls.Add(this.operatorsList);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dateTo);
            this.Controls.Add(this.dateFrom);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "Отчет Агента";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dateFrom;
        private System.Windows.Forms.DateTimePicker dateTo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox operatorsList;
        private System.Windows.Forms.Button bntOk;
        private System.Windows.Forms.FontDialog fontDialog1;

        public Form1()
        {
            InitializeComponent();
        }

        private void dateFrom_ValueChanged(object sender, EventArgs e)
        {
            if (dateFrom.Value > dateTo.Value)
                dateTo.Value = dateFrom.Value;
            else
                UpdateList();
        }

        private void dateTo_ValueChanged(object sender, EventArgs e)
        {
            if (dateFrom.Value > dateTo.Value)
                dateFrom.Value = dateTo.Value;
            else
                UpdateList();
        }

        private void UpdateList()
        {
            List<KeyValuePair<Partner, string>> list = new List<KeyValuePair<Partner, string>>();

            Partners partners = new Partners(new DataCache());

            string dogovorsFilter = "(select dg_key from tbl_dogovor where dg_crdate>='{0:yyyy-MM-dd}' and dg_crdate<'{1:yyyy-MM-dd}' and dg_turdate>'2010-10-10')";
            partners.RowFilter= string.Format( "pr_key in (select dl_partnerkey from tbl_dogovorlist where dl_dgkey in "+dogovorsFilter+" and dl_svkey=3)", DateFrom, DateTo);

            partners.Sort = "pr_name asc";

            partners.Fill();

            foreach (Partner pr in partners)
                list.Add( new KeyValuePair<Partner, string>(pr, pr.Name));

            operatorsList.DataSource = list;

            if (list.Count > 0) 
                operatorsList.SelectedIndex = 0;
        }

        public DateTime DateFrom
        {
            get {
                return dateFrom.Value;
            }
        }

        public DateTime DateTo
        {
            get {
                return dateTo.Value;
            }
        }

        public Partner Operator
        {
            get {
                return operatorsList.SelectedValue as Partner;
            }
        }
    }
}
