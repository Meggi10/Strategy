﻿using System;

namespace Strategy
{
    partial class StrategyForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StrategyForm));
            this.PropPanel = new System.Windows.Forms.Panel();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.ResourceView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button1 = new System.Windows.Forms.Button();
            this.CitiesList = new System.Windows.Forms.ListBox();
            this.HerosList = new System.Windows.Forms.ListBox();
            this.MapView = new System.Windows.Forms.PictureBox();
            this.PlayTimer = new System.Windows.Forms.Timer(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.Board = new Strategy.TBoard();
            this.PropPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MapView)).BeginInit();
            this.SuspendLayout();
            // 
            // PropPanel
            // 
            this.PropPanel.BackColor = System.Drawing.Color.Maroon;
            this.PropPanel.Controls.Add(this.comboBox1);
            this.PropPanel.Controls.Add(this.ResourceView);
            this.PropPanel.Controls.Add(this.button1);
            this.PropPanel.Controls.Add(this.CitiesList);
            this.PropPanel.Controls.Add(this.HerosList);
            this.PropPanel.Controls.Add(this.MapView);
            this.PropPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.PropPanel.Location = new System.Drawing.Point(680, 0);
            this.PropPanel.Margin = new System.Windows.Forms.Padding(2);
            this.PropPanel.Name = "PropPanel";
            this.PropPanel.Size = new System.Drawing.Size(209, 650);
            this.PropPanel.TabIndex = 2;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(14, 596);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(183, 21);
            this.comboBox1.TabIndex = 5;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.ComboBox1_SelectedIndexChanged);
            // 
            // ResourceView
            // 
            this.ResourceView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.ResourceView.HideSelection = false;
            this.ResourceView.Location = new System.Drawing.Point(14, 417);
            this.ResourceView.Margin = new System.Windows.Forms.Padding(2);
            this.ResourceView.Name = "ResourceView";
            this.ResourceView.Size = new System.Drawing.Size(183, 173);
            this.ResourceView.TabIndex = 4;
            this.ResourceView.UseCompatibleStateImageBehavior = false;
            this.ResourceView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 123;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Width = 113;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(44, 372);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(124, 26);
            this.button1.TabIndex = 3;
            this.button1.Text = "End Turn";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // CitiesList
            // 
            this.CitiesList.FormattingEnabled = true;
            this.CitiesList.Location = new System.Drawing.Point(113, 202);
            this.CitiesList.Margin = new System.Windows.Forms.Padding(2);
            this.CitiesList.Name = "CitiesList";
            this.CitiesList.Size = new System.Drawing.Size(84, 147);
            this.CitiesList.TabIndex = 2;
            // 
            // HerosList
            // 
            this.HerosList.FormattingEnabled = true;
            this.HerosList.Location = new System.Drawing.Point(14, 202);
            this.HerosList.Margin = new System.Windows.Forms.Padding(2);
            this.HerosList.Name = "HerosList";
            this.HerosList.Size = new System.Drawing.Size(93, 147);
            this.HerosList.TabIndex = 1;
            this.HerosList.SelectedIndexChanged += new System.EventHandler(this.HerosList_SelectedIndexChanged);
            // 
            // MapView
            // 
            this.MapView.Location = new System.Drawing.Point(14, 18);
            this.MapView.Margin = new System.Windows.Forms.Padding(2);
            this.MapView.Name = "MapView";
            this.MapView.Size = new System.Drawing.Size(183, 171);
            this.MapView.TabIndex = 0;
            this.MapView.TabStop = false;
            this.MapView.Paint += new System.Windows.Forms.PaintEventHandler(this.MapView_Paint);
            this.MapView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MapView_MouseMove);
            // 
            // PlayTimer
            // 
            this.PlayTimer.Enabled = true;
            this.PlayTimer.Interval = 33;
            this.PlayTimer.Tick += new System.EventHandler(this.PlayTimer_Tick);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog1_FileOk);
            // 
            // Board
            // 
            this.Board.BackColor = System.Drawing.Color.White;
            this.Board.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Board.Location = new System.Drawing.Point(0, 0);
            this.Board.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Board.Name = "Board";
            this.Board.ScrollPos = ((System.Drawing.PointF)(resources.GetObject("Board.ScrollPos")));
            this.Board.Size = new System.Drawing.Size(680, 650);
            this.Board.TabIndex = 1;
            this.Board.Zoom = 0.5F;
            this.Board.Load += new System.EventHandler(this.Board_Load);
            // 
            // StrategyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(889, 650);
            this.Controls.Add(this.Board);
            this.Controls.Add(this.PropPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "StrategyForm";
            this.Text = "Form1";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.StrategyForm_KeyDown);
            this.PropPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MapView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private TBoard Board;
        private System.Windows.Forms.Panel PropPanel;
        private System.Windows.Forms.PictureBox MapView;
        private System.Windows.Forms.Timer PlayTimer;
        private System.Windows.Forms.ListBox CitiesList;
        private System.Windows.Forms.ListBox HerosList;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListView ResourceView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}

