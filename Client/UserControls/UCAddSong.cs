﻿using Client.Controller;
using Common.Domain;
using Common.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client.UserControls
{
    public partial class UCAddSong : UserControl
    {
        public UCAddSong()
        {
            try
            {
                InitializeComponent();
                loadArtistCMB();
                cmbArtist.DisplayMember = "StageName";
                loadGenresCMB();
                loadMusicVideoCMB();
                cmbMusicVideo.DisplayMember = "Name";
                cmbMusicVideo.SelectedIndex = -1;
                loadMusicProducerCMB();
                cmbMusicProducer.DisplayMember = "StageName";
                loadProjectCMB();
                cmbProject.DisplayMember = "Name";
                cmbProject.SelectedIndex = -1;
            }
            catch (ServerDisconnectedException ex)
            {
                MessageBox.Show("Server disconnected!");
                LoginController.Instance.frmLogin.Dispose();
                MainController.Instance.frmMain.Dispose();
                return;
            }
        }

        private bool Validation()
        {
            bool b1, b2, b3, b4, b5;
            if (!String.IsNullOrEmpty(txtSongName.Text))
                b1 = true;
            else
            {
                b1 = false;
                MessageBox.Show("Name field empty!");
            }
            if (String.IsNullOrEmpty(txtBPM.Text) || !int.TryParse(txtBPM.Text, out int number))
            {
                b2 = false;
                MessageBox.Show("BPM field either empty or not a number!");
            }
            else
                b2 = true;
            if (cmbGenre.SelectedIndex != -1)
                b3 = true;
            else
            {
                b3 = false;
                MessageBox.Show("Genre not selected!");
            }
            if (cmbArtist.SelectedIndex != -1)
                b4 = true;
            else
            {
                b4 = false;
                MessageBox.Show("Artist not selected!");
            }
            if (cmbMusicProducer.SelectedIndex != -1)
                b5 = true;
            else
            {
                b5 = false;
                MessageBox.Show("Music Producer not selected!");
            }
           

            return b1 && b2 && b3 && b4 && b5;

        }
        private void loadArtistCMB()
        {
            SearchValue sv = new SearchValue()
            {
                Type = typeof(Artist).AssemblyQualifiedName,
                Parameter = "StageName",
                Value = ""
            };

            cmbArtist.DataSource = SearchArtistController.Instance.SearchArtist(sv);
        }
        private void loadMusicVideoCMB()
        {
            SearchValue sv = new SearchValue()
            {
                Type = typeof(MusicVideo).AssemblyQualifiedName,
                Parameter = "Name",
                Value = ""
            };
            cmbMusicVideo.DataSource = SearchMusicVideoController.Instance.SearchMusicVideo(sv);
        }
        private void loadGenresCMB()
        {
            cmbGenre.DataSource = Enum.GetValues(typeof(SongGenre));
        }
        private void loadMusicProducerCMB()
        {
            SearchValue sv = new SearchValue()
            {
                Parameter = "stageName",
                Value = "",
                Type = typeof(MusicProducer).AssemblyQualifiedName
            };
            cmbMusicProducer.DataSource = SearchMusicProducerController.Instance.SearchMusicProducer(sv);
        }
        private void loadProjectCMB()
        {
            SearchValue sv = new SearchValue();
            sv.Parameter="Name";
            sv.Value = "";
            sv.Type=typeof(Project).AssemblyQualifiedName;
            cmbProject.DataSource = SearchProjectController.Instance.SearchProject(sv);
        }
        private void btnAddSong_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Validation())
                {
                    MessageBox.Show("Try again!");
                    return;
                }

                Song s = new Song();
                s.Name = txtSongName.Text;
                s.BPM = Int32.Parse(txtBPM.Text);
                s.CreationDate = DateTime.Now;
                s.MusicProducer = (MusicProducer)cmbMusicProducer.SelectedItem;
                if (cmbMusicVideo.SelectedItem != null)
                    s.MusicVideo = (MusicVideo)cmbMusicVideo.SelectedItem;
                s.Artist = (Artist)cmbArtist.SelectedItem;
                if (cmbProject.SelectedItem != null)
                    s.Project = (Project)cmbProject.SelectedItem;
                AddSongController.Instance.AddSong(s);
            }
            catch (ServerDisconnectedException ex)
            {
                MessageBox.Show("Server disconnected!");
                LoginController.Instance.frmLogin.Dispose();
                MainController.Instance.frmMain.Dispose();
                return;
            }
            
        }
    }
}
