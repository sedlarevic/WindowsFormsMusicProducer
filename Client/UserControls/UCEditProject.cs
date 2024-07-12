using Client.Controller;
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
    public partial class UCEditProject : UserControl
    {
        public UCEditProject()
        {
            try
            {
                InitializeComponent();

                loadArtistCMB();
                loadMusicProducerCMB();
                
                loadProjectDgv();

                txtId.Enabled = false;
                txtName.Enabled = false;
                rtxtDescription.Enabled = false;
                cmbArtist.Enabled = false;
                cmbMusicProducer.Enabled = false;
                btnRemove.Enabled = false;
            }
            catch (ServerDisconnectedException ex)
            {
                MessageBox.Show("Server disconnected!");
                LoginController.Instance.frmLogin.Dispose();
                MainController.Instance.frmMain.Dispose();
                this.Dispose();
                return;
            }
            
        }
        private void dgvSongCleanup()
        {
            if (dgvSong.Rows.Count > 0)
            {
                dgvSong.Columns["Id"].Visible = false;
                dgvSong.Columns["Values"].Visible = false;
                dgvSong.Columns["TableName"].Visible = false;
            }
        }
        private void dgvProjectCleanup()
        {
            if (dgvProject.Rows.Count > 0)
            {
                dgvProject.Columns["Id"].Visible = false;
                dgvProject.Columns["Values"].Visible = false;
                dgvProject.Columns["TableName"].Visible = false;
            }
        }
        private void dgvSongsOnProjectCleanup()
        {
            if (dgvSongsOnProject.Rows.Count > 0)
            {
                dgvSongsOnProject.Columns["Id"].Visible = false;
                dgvSongsOnProject.Columns["Values"].Visible = false;
                dgvSongsOnProject.Columns["TableName"].Visible = false;
            }
        }
        private bool Validation()
        {

            bool b1, b2, b3, b4;
            if (!String.IsNullOrEmpty(txtName.Text))
                b1 = true;
            else
            {
                b1 = false;
                MessageBox.Show("Name field is empty!");
            }
            if (!String.IsNullOrEmpty(rtxtDescription.Text))
                b2 = true;
            else
            {
                b2 = false;
                MessageBox.Show("Description field is empty!");
            }
            if (cmbArtist.SelectedIndex != -1)
                b3 = true;
            else
            {
                b3 = false;
                MessageBox.Show("Artist not selected!!");
            }
            if (cmbMusicProducer.SelectedIndex != -1)
                b4 = true;
            else
            {
                b4 = false;
                MessageBox.Show("Music producer not selected!");
            }



            return b1 && b2 && b3 && b4;
        }
        private void loadSongDgv()
        {
            SearchValue sv = new SearchValue();
            sv.Parameter = "Name";
            sv.Value = "";
            sv.Type = typeof(Song).AssemblyQualifiedName;
            dgvSong.DataSource = SearchSongController.Instance.SearchSong(sv);
            dgvSongCleanup();
        }
        private void loadProjectDgv()
        {
            SearchValue sv = new SearchValue();
            sv.Parameter = "Name";
            sv.Value = "";
            sv.Type = typeof(Project).AssemblyQualifiedName;
            dgvProject.DataSource = SearchProjectController.Instance.SearchProject(sv);
            dgvProjectCleanup();     
        }
        private void loadArtistCMB()
        {
            SearchValue sv = new SearchValue();
            sv.Parameter = "stageName";
            sv.Value = "";
            sv.Type = typeof(Artist).AssemblyQualifiedName;
            cmbArtist.DataSource = SearchArtistController.Instance.SearchArtist(sv);
            cmbArtist.DisplayMember = "StageName";
        }
        private void loadMusicProducerCMB()
        {
            SearchValue sv = new SearchValue();
            sv.Parameter = "stageName";
            sv.Value = "";
            sv.Type = typeof(MusicProducer).AssemblyQualifiedName;
            cmbMusicProducer.DataSource = SearchMusicProducerController.Instance.SearchMusicProducer(sv);
            cmbMusicProducer.DisplayMember = "StageName";
        }      
        private void btnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Validation())
                {
                    MessageBox.Show("Try again!");
                    return;
                }

                //editujemo prvo projekat
                if (dgvProject.SelectedRows.Count <= 0)
                {
                    MessageBox.Show("no rows have been selected");
                    return;
                }
                
                DataGridViewRow dgvProjectRow = dgvProject.SelectedRows[0];
                int selectedProjectIndex = dgvProjectRow.Index;
                Project newProject = new Project();
                newProject.Id = Int32.Parse(txtId.Text);
                newProject.Name = txtName.Text;
                newProject.Description = rtxtDescription.Text;
                newProject.Artist = cmbArtist.SelectedItem as Artist;
                newProject.MusicProducer = cmbMusicProducer.SelectedItem as MusicProducer;
                newProject.CreationDate = (DateTime)dgvProjectRow.Cells["CreationDate"].Value;

                Project originalProject = new Project();
                originalProject.Id = Int32.Parse(dgvProjectRow.Cells["Id"].Value.ToString());
                originalProject.Name = dgvProjectRow.Cells["Name"].Value.ToString();
                originalProject.Description = dgvProjectRow.Cells["Description"].Value.ToString();
                originalProject.Artist = dgvProjectRow.Cells["Artist"].Value as Artist;
                originalProject.MusicProducer = dgvProjectRow.Cells["MusicProducer"].Value as MusicProducer;
                originalProject.CreationDate = (DateTime)dgvProjectRow.Cells["CreationDate"].Value;

                EditValue evProject = new EditValue();
                evProject.EditedValue = newProject;
                evProject.OriginalValue = originalProject;
                evProject.Type = typeof(Project).AssemblyQualifiedName;
                //ispitujemo da li je dobro prosao edit
                object resultEdit = EditProjectController.Instance.EditProject(evProject);
                if (resultEdit == null)
                {
                    Console.WriteLine("couldn't edit the project");
                    return;
                }
                //pravimo za editsong editvalue
                List<Song> songsOriginal = new List<Song>();
                List<Song> songsNew = new List<Song>();

                foreach (DataGridViewRow row in dgvSong.SelectedRows)
                {
                    Song s = row.DataBoundItem as Song;
                    songsOriginal.Add(s);
                }
                foreach (Song s in songsOriginal)
                {
                    Song sNew = new Song
                    {
                        Id = s.Id,
                        Name = s.Name,
                        BPM = s.BPM,
                        CreationDate = s.CreationDate,
                        Genre = s.Genre,
                        MusicProducer = s.MusicProducer,
                        Artist = s.Artist,
                        MusicVideo = s.MusicVideo,
                        Project = newProject
                    };
                    songsNew.Add(sNew);
                }
                EditValue evSong = new EditValue();
                evSong.OriginalValue = songsOriginal;
                evSong.EditedValue = songsNew;
                evSong.Type = typeof(Song).AssemblyQualifiedName;

                EditSongController.Instance.EditSong(evSong);
                loadSongDgv();
                
                //sada kada smo editovali i pesme i project, postavljamo sve pesme projekta u dgv
                SearchValue sValueForSongsOnProject = new SearchValue()
                {
                    Parameter = "Project",
                    Type = typeof(Song).AssemblyQualifiedName,
                    Value = newProject.Id
                };
                dgvSongsOnProject.DataSource = SearchSongController.Instance.SearchSong(sValueForSongsOnProject);
                dgvSongsOnProjectCleanup();
                loadSongDgv();
                loadProjectDgv();
                dgvProject.Rows[selectedProjectIndex].Selected = true;
                List<int> selectedIndexes = new List<int>();
                foreach (DataGridViewRow row in dgvSongsOnProject.Rows)
                {
                    selectedIndexes.Add(Int32.Parse(row.Cells["Id"].Value.ToString()));
                }
                foreach (DataGridViewRow row in dgvSong.Rows)
                {
                    if (selectedIndexes.Contains(Int32.Parse(row.Cells["Id"].Value.ToString())))
                        row.Selected = true;
                    else
                        row.Selected = false;
                }
                
            }
            catch (ServerDisconnectedException ex)
            {
                MessageBox.Show("Server disconnected!");
                LoginController.Instance.frmLogin.Dispose();
                MainController.Instance.frmMain.Dispose();
                this.Dispose();
                return;
            }
            

        }
        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                SearchValue sv = new SearchValue();
                sv.Parameter = "Name";
                sv.Value = txtSearch.Text;
                sv.Type = typeof(Project).AssemblyQualifiedName;
                dgvProject.DataSource = SearchProjectController.Instance.SearchProject(sv);
                dgvProjectCleanup();
            }
            catch (ServerDisconnectedException ex)
            {
                MessageBox.Show("Server disconnected!");
                LoginController.Instance.frmLogin.Dispose();
                MainController.Instance.frmMain.Dispose();
                return;
            }
            
        }
        private void btnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvProject.SelectedRows.Count <= 0)
                {
                    MessageBox.Show("no rows have been selected");
                    return;
                }
                DataGridViewRow selectedRow = dgvProject.SelectedRows[0];
                txtId.Text = selectedRow.Cells["Id"].Value.ToString();

                txtName.Text = selectedRow.Cells["Name"].Value.ToString();
                txtName.Enabled = true;

                rtxtDescription.Text = selectedRow.Cells["Description"].Value.ToString();
                rtxtDescription.Enabled = true;


                Artist a = selectedRow.Cells["Artist"].Value as Artist;
                cmbArtist.Enabled = true;
                cmbArtist.SelectedItem = a;
                cmbArtist.DisplayMember = "StageName";

                MusicProducer producer = selectedRow.Cells["MusicProducer"].Value as MusicProducer;
                cmbMusicProducer.Enabled = true;
                cmbMusicProducer.SelectedItem = producer;
                cmbMusicProducer.DisplayMember = "StageName";

                SearchValue svSong = new SearchValue()
                {
                    Value = "",
                    Parameter = "Name",
                    Type = typeof(Song).AssemblyQualifiedName
                };
                dgvSong.DataSource = SearchSongController.Instance.SearchSong(svSong);
                dgvSongCleanup();
                SearchValue projectSongs = new SearchValue()
                {
                    Value = txtId.Text,
                    Parameter = "Project",
                    Type = typeof(Song).AssemblyQualifiedName
                };
                dgvSongsOnProject.DataSource = SearchSongController.Instance.SearchSong(projectSongs);
                dgvSongsOnProjectCleanup();
                List<int> selectedIndexes = new List<int>();
                foreach (DataGridViewRow row in dgvSongsOnProject.Rows)
                {
                    selectedIndexes.Add(Int32.Parse(row.Cells["Id"].Value.ToString()));
                }
                foreach (DataGridViewRow row in dgvSong.Rows)
                {
                    if (selectedIndexes.Contains(Int32.Parse(row.Cells["Id"].Value.ToString())))
                        row.Selected = true;
                    else
                        row.Selected = false;
                }

                btnRemove.Enabled = true;
            }
            catch (ServerDisconnectedException ex)
            {
                MessageBox.Show("Server disconnected!");
                LoginController.Instance.frmLogin.Dispose();
                MainController.Instance.frmMain.Dispose();
                return;
            }
            

        }
        private void btnRemove_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvSongsOnProject.SelectedRows.Count <= 0)
                {
                    MessageBox.Show("no rows have been selected");
                    return;
                }
                DataGridViewRow selected = dgvSongsOnProject.SelectedRows[0];
                Song sOriginal = new Song()
                {
                    Id = Int32.Parse(selected.Cells["Id"].Value.ToString()),
                    Name = selected.Cells["Name"].Value.ToString(),
                    BPM = Int32.Parse(selected.Cells["BPM"].Value.ToString()),
                    CreationDate = (DateTime)selected.Cells["CreationDate"].Value,
                    Genre = (SongGenre)selected.Cells["Genre"].Value,
                    MusicProducer = selected.Cells["MusicProducer"].Value as MusicProducer,
                    Artist = selected.Cells["Artist"].Value as Artist,
                    MusicVideo = selected.Cells["MusicVideo"].Value as MusicVideo,
                    Project = selected.Cells["Project"].Value as Project
                };
                Song sNew = new Song()
                {
                    Id = Int32.Parse(txtId.Text),
                    Name = selected.Cells["Name"].Value.ToString(),
                    BPM = Int32.Parse(selected.Cells["BPM"].Value.ToString()),
                    CreationDate = (DateTime)selected.Cells["CreationDate"].Value,
                    Genre = (SongGenre)selected.Cells["Genre"].Value,
                    MusicProducer = selected.Cells["MusicProducer"].Value as MusicProducer,
                    Artist = selected.Cells["Artist"].Value as Artist,
                    MusicVideo = selected.Cells["MusicVideo"].Value as MusicVideo,
                    Project = null
                };
                EditValue ev = new EditValue()
                {
                    EditedValue = sNew,
                    OriginalValue = sOriginal,
                    Type = typeof(Song).AssemblyQualifiedName
                };

                EditSongController.Instance.EditSong(ev);
                SearchValue evSongsOnProject = new SearchValue()
                {
                    Value = txtId.Text,
                    Parameter = "Project",
                    Type = typeof(Song).AssemblyQualifiedName
                };
                dgvSongsOnProject.DataSource = SearchSongController.Instance.SearchSong(evSongsOnProject);
                dgvSongsOnProjectCleanup();
                loadSongDgv();

                List<int> selectedIndexes = new List<int>();
                foreach (DataGridViewRow row in dgvSongsOnProject.Rows)
                {
                    selectedIndexes.Add(Int32.Parse(row.Cells["Id"].Value.ToString()));
                }
                foreach (DataGridViewRow row in dgvSong.Rows)
                {
                    if (selectedIndexes.Contains(Int32.Parse(row.Cells["Id"].Value.ToString())))
                        row.Selected = true;
                    else
                        row.Selected = false;
                }

                return;
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
