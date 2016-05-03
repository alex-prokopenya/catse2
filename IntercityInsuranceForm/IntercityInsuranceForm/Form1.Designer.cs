namespace IntercityInsuranceForm
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Windows.Forms;
    using Megatec.MasterTour.BusinessRules;

    partial class Form1 : Form
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.findBtn = new System.Windows.Forms.Button();
            this.dateOfTour = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.countriesList = new System.Windows.Forms.CheckedListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupsList = new System.Windows.Forms.CheckedListBox();
            this.dogovorsList = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.blankNum = new System.Windows.Forms.TextBox();
            this.saveBtn = new System.Windows.Forms.Button();
            this.showBtn = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.findBtn);
            this.panel1.Controls.Add(this.dateOfTour);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(915, 52);
            this.panel1.TabIndex = 0;
            // 
            // findBtn
            // 
            this.findBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.findBtn.Location = new System.Drawing.Point(283, 9);
            this.findBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.findBtn.Name = "findBtn";
            this.findBtn.Size = new System.Drawing.Size(176, 34);
            this.findBtn.TabIndex = 5;
            this.findBtn.Text = "Найти путевки";
            this.findBtn.UseVisualStyleBackColor = true;
            this.findBtn.Click += new System.EventHandler(this.findBtn_Click);
            // 
            // dateOfTour
            // 
            this.dateOfTour.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateOfTour.Location = new System.Drawing.Point(117, 14);
            this.dateOfTour.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dateOfTour.Name = "dateOfTour";
            this.dateOfTour.Size = new System.Drawing.Size(143, 22);
            this.dateOfTour.TabIndex = 4;
            this.dateOfTour.Value = new System.DateTime(2015, 6, 9, 0, 0, 0, 0);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Дата тура:";
            // 
            // countriesList
            // 
            this.countriesList.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.countriesList.FormattingEnabled = true;
            this.countriesList.Location = new System.Drawing.Point(12, 111);
            this.countriesList.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.countriesList.Name = "countriesList";
            this.countriesList.Size = new System.Drawing.Size(177, 289);
            this.countriesList.TabIndex = 3;
            this.countriesList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.countriesList_ItemCheck);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(8, 89);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "Страны";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.Location = new System.Drawing.Point(219, 89);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(303, 20);
            this.label3.TabIndex = 6;
            this.label3.Text = "Группы (страна - ночей - N бланка)";
            // 
            // groupsList
            // 
            this.groupsList.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.groupsList.FormattingEnabled = true;
            this.groupsList.Location = new System.Drawing.Point(221, 111);
            this.groupsList.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupsList.Name = "groupsList";
            this.groupsList.Size = new System.Drawing.Size(371, 289);
            this.groupsList.TabIndex = 7;
            this.groupsList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.groupsList_ItemCheck);
            this.groupsList.SelectedIndexChanged += new System.EventHandler(this.groupsList_SelectedIndexChanged);
            // 
            // dogovorsList
            // 
            this.dogovorsList.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.dogovorsList.FormattingEnabled = true;
            this.dogovorsList.ItemHeight = 18;
            this.dogovorsList.Location = new System.Drawing.Point(625, 111);
            this.dogovorsList.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dogovorsList.Name = "dogovorsList";
            this.dogovorsList.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.dogovorsList.Size = new System.Drawing.Size(303, 220);
            this.dogovorsList.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label4.Location = new System.Drawing.Point(621, 89);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(142, 20);
            this.label4.TabIndex = 9;
            this.label4.Text = "Путевки группы";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label5.Location = new System.Drawing.Point(622, 353);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(160, 20);
            this.label5.TabIndex = 10;
            this.label5.Text = "Бланк для группы";
            // 
            // blankNum
            // 
            this.blankNum.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.blankNum.Location = new System.Drawing.Point(626, 382);
            this.blankNum.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.blankNum.Name = "blankNum";
            this.blankNum.Size = new System.Drawing.Size(167, 27);
            this.blankNum.TabIndex = 11;
            // 
            // saveBtn
            // 
            this.saveBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.saveBtn.Location = new System.Drawing.Point(799, 380);
            this.saveBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(127, 31);
            this.saveBtn.TabIndex = 12;
            this.saveBtn.Text = "Сохранить";
            this.saveBtn.UseVisualStyleBackColor = true;
            this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // showBtn
            // 
            this.showBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.showBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.showBtn.Location = new System.Drawing.Point(392, 447);
            this.showBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.showBtn.Name = "showBtn";
            this.showBtn.Size = new System.Drawing.Size(149, 33);
            this.showBtn.TabIndex = 13;
            this.showBtn.Text = "Вывести отчет";
            this.showBtn.UseVisualStyleBackColor = true;
            this.showBtn.Click += new System.EventHandler(this.showBtn_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(937, 494);
            this.Controls.Add(this.showBtn);
            this.Controls.Add(this.saveBtn);
            this.Controls.Add(this.blankNum);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.dogovorsList);
            this.Controls.Add(this.groupsList);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.countriesList);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "Группы по страховкам";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button findBtn;
        private System.Windows.Forms.DateTimePicker dateOfTour;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckedListBox countriesList;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckedListBox groupsList;
        private System.Windows.Forms.ListBox dogovorsList;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox blankNum;
        private System.Windows.Forms.Button saveBtn;
        private System.Windows.Forms.Button showBtn;


        private bool loading = true;

        private Dictionary<int, string> countries = new Dictionary<int, string>();
        private Dictionary<string, string> groups = new Dictionary<string, string>();
        private Dictionary<string, List<Dogovor>> dogovors = new Dictionary<string, List<Dogovor>>();

        public Form1()
        {
            InitializeComponent();

            //  Manager.ConnectionString = "Data Source=online.viziteurope.eu; Initial Catalog=veuro5;User Id=WEB52;Password=web3k5ml";


            countriesList.DisplayMember = "Value";
            countriesList.ValueMember = "Key";

            groupsList.DisplayMember = "Value";
            groupsList.ValueMember = "Key";

            dogovorsList.DisplayMember = "Value";
            dogovorsList.ValueMember = "Key";
        }

        private void findBtn_Click(object sender, EventArgs e)
        {
            countries = new Dictionary<int, string>();
            groups = new Dictionary<string, string>();
            dogovors = new Dictionary<string, List<Dogovor>>();

            string dogList = " dl_svkey = " + Service.Insurance + " and dl_datebeg='" + dateOfTour.Value.ToString("yyyy-MM-dd") + "'";

            //выбрать список стран по путевкам, имеющим визы
            DogovorLists dogs = new DogovorLists(new DataCache());
            dogs.RowFilter = dogList;
            dogs.Fill();

            foreach (DogovorList dl in dogs)
            {
                string comment = dl.Comment.Replace("gr_","").Trim();

                //берем только путевки для групп
                if ((comment != "") && (!dl.Comment.Contains("gr_"))) continue;


                string country = dl.Dogovor.Country.ToString();

                if (!countries.ContainsKey(dl.Dogovor.CountryKey))
                    countries.Add(dl.Dogovor.CountryKey, country);

                string key = country + " - " + (dl.Dogovor.NDays - 1) + " - " + comment;

                if (!groups.ContainsKey(key))
                {
                    groups.Add(key, comment);
                    dogovors.Add(key, new List<Dogovor>());
                }

                dogovors[key].Add(dl.Dogovor);
            }

            countriesList.Items.Clear();
            groupsList.Items.Clear();
            dogovorsList.Items.Clear();

            label4.Text = "Путевки группы ";
            label5.Text = "Бланк для группы ";

            //сортируем страны
            var sortedDict = from entry in countries orderby entry.Value ascending select entry;

            loading = true;
            //добавляем страны в список
            foreach (KeyValuePair<int, string> pair in sortedDict)
                countriesList.Items.Add(pair, true);

            loading = false;

            countriesList_ItemCheck(null, null);
        }

        private void countriesList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (sender != null)
            {
                CheckedListBox clb = (CheckedListBox)sender;
                // Switch off event handler
                clb.ItemCheck -= countriesList_ItemCheck;
                clb.SetItemCheckState(e.Index, e.NewValue);
                // Switch on event handler
                clb.ItemCheck += countriesList_ItemCheck;
            }

            if (!loading)
            {
                //очистить список групп
                groupsList.Items.Clear();

                loading = true;

                List<string> checkedItems = new List<string>();
                foreach (var item in countriesList.CheckedItems)
                    checkedItems.Add(((KeyValuePair<int, string>)item).Value);

                //пройтись по списку стран
                foreach (var chKey in checkedItems)
                {
                    foreach (var key in groups.Keys)
                        if (key.Contains(chKey)) //добавить группы по отмеченным странам
                            groupsList.Items.Add(new KeyValuePair<string, string>(groups[key], key), true);
                }

                loading = false;
            }
        }

        private void groupsList_ItemCheck(object sender, ItemCheckEventArgs e)
        {

        }

        private void groupsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                if (groupsList.SelectedItem == null) return;

                //загрузить список путевок
                string selectedKey = ((KeyValuePair<string, string>)groupsList.SelectedItem).Value;

                dogovorsList.Items.Clear();

                foreach (var dog in dogovors[selectedKey])
                    dogovorsList.Items.Add(new KeyValuePair<int, string>(dog.Key, dog.Code + " - " + dog.NMen + " чел."));

                blankNum.Text = ((KeyValuePair<string, string>)groupsList.SelectedItem).Key;

                label4.Text = "Путевки группы " + selectedKey;
                label5.Text = "Бланк для группы " + selectedKey;
            }
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            //получить текущую группу
            string selectedKey = ((KeyValuePair<string, string>)groupsList.SelectedItem).Value;

            //пройтись по путевкам группы
            foreach (var dog in dogovors[selectedKey])
            {
                dog.DogovorLists.Fill();

                foreach (DogovorList dl in dog.DogovorLists)
                    if (dl.ServiceKey == Service.Visa)
                    {
                        dl.Comment = "gr_" + blankNum.Text;
                        dl.DataContainer.Update();
                        dog.DataContainer.Update();
                    }
            }

            var ind = selectedKey.LastIndexOf(" - ");

            var newKey = selectedKey.Substring(0, ind) + " - " + blankNum.Text;

            dogovors[newKey] = dogovors[selectedKey];

            dogovors.Remove(selectedKey);

            groups.Remove(selectedKey);

            groups[newKey] = blankNum.Text;

            loading = true;

            int index = groupsList.SelectedIndex;

            bool isChecked = groupsList.GetItemChecked(index);

            groupsList.Items.RemoveAt(groupsList.SelectedIndex);

            groupsList.Items.Insert(index, new KeyValuePair<string, string>(groups[newKey], newKey));

            groupsList.SetItemChecked(index, isChecked);

            loading = false;
            groupsList.SelectedIndex = index;
        }

        public DateTime TurDate
        {
            get
            {
                return dateOfTour.Value;
            }
        }

        public Dictionary<string, List<Dogovor>> Dogovors
        {
            get
            {
                return dogovors;
            }
        }

        public List<string> polisNums
        {
            get
            {

                List<string> checkedItems = new List<string>();
                foreach (var item in groupsList.CheckedItems)
                    checkedItems.Add(((KeyValuePair<string, string>)item).Value);

                return checkedItems;
            }
        }
    }
}

