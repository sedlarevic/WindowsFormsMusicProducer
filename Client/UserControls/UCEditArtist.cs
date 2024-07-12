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
    public partial class UCEditArtist : UserControl
    {
        public UCEditArtist()
        {
            try
            {
                InitializeComponent();
                btnEdit.Enabled = false;
                txtFirstName.Enabled = false;
                txtLastName.Enabled = false;
                txtStageName.Enabled = false;
                txtId.Enabled = false;
                DgvLoad();
                
            }catch (ServerDisconnectedException ex)
            {
                MessageBox.Show("Server disconnected!");
                LoginController.Instance.frmLogin.Dispose();
                MainController.Instance.frmMain.Dispose();
                this.Dispose();
                return;
            }
        }

        private bool Validation()
        {

            bool b1, b2, b3;
            if (!String.IsNullOrEmpty(txtFirstName.Text))
                b1 = true;
            else
            {
                b1 = false;
                MessageBox.Show("First name field is empty!");
            }
            if (!String.IsNullOrEmpty(txtLastName.Text))
                b2 = true;
            else
            {
                b2 = false;
                MessageBox.Show("Last name field is empty!");
            }
            if (!String.IsNullOrEmpty(txtStageName.Text))
                b3 = true;
            else
            {
                b3 = false;
                MessageBox.Show("Stage name field is empty!");
            }



            return b1 && b2 && b3;
        }

        private void DgvLoad()
        {
            var ds = SearchArtist();
            if (ds != null)
            {
                
                dgvArtist.DataSource = ds;                
                dgvCleanup();

            }
        }
        private void dgvCleanup()
        {
            if (dgvArtist.Rows.Count>0)
            {
                dgvArtist.Columns["Id"].Visible = false;
                dgvArtist.Columns["Values"].Visible = false;
                dgvArtist.Columns["TableName"].Visible = false;
            }           
        }

        private object SearchArtist(string parameter = "")
        {      
            SearchValue searchValue = new SearchValue()
                {
                    Type = typeof(Artist).AssemblyQualifiedName,
                    Value = parameter,
                    Parameter = "StageName"
                };
            return SearchArtistController.Instance.SearchArtist(searchValue);
        }
           

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                SearchValue sv = new SearchValue()
                {
                Type = typeof(Artist).AssemblyQualifiedName,
                Value = txtSearch.Text,
                Parameter = "StageName"
                };
                dgvArtist.Columns.Clear();
                dgvArtist.DataSource = SearchArtistController.Instance.SearchArtist(sv);
                dgvCleanup();
            
            }
            catch (ServerDisconnectedException ex)
            {
                MessageBox.Show("Server disconnected!");
                LoginController.Instance.frmLogin.Dispose();
                MainController.Instance.frmMain.Dispose();                
                return;
            }
        }

        private void btnLoadArtist_Click(object sender, EventArgs e)
        {
            if (dgvArtist.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dgvArtist.SelectedRows[0];
                txtFirstName.Enabled = true;
                txtFirstName.Text = selectedRow.Cells["FirstName"].Value.ToString();
                txtLastName.Enabled = true;
                txtLastName.Text = selectedRow.Cells["LastName"].Value.ToString();
                txtStageName.Enabled = true;
                txtStageName.Text = selectedRow.Cells["StageName"].Value.ToString();
                txtId.Text = selectedRow.Cells["Id"].Value.ToString();

                btnEdit.Enabled = true;
            }
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
                Artist artistNew = new Artist()
                {
                    Id = Int32.Parse(txtId.Text),
                    FirstName = txtFirstName.Text,
                    LastName = txtLastName.Text,
                    StageName = txtStageName.Text,
                };
                DataGridViewRow selectedRow = dgvArtist.SelectedRows[0];
                //txtFirstName.Text = selectedRow.Cells["FirstName"].Value.ToString();
                //txtLastName.Text = selectedRow.Cells["LastName"].Value.ToString();
                //txtStageName.Text = selectedRow.Cells["StageName"].Value.ToString();
                //txtId.Text = selectedRow.Cells["Id"].Value.ToString();
                Artist artistOriginal = new Artist
                {
                    Id = Int32.Parse(selectedRow.Cells["Id"].Value.ToString()),
                    FirstName = selectedRow.Cells["FirstName"].Value.ToString(),
                    LastName = selectedRow.Cells["LastName"].Value.ToString(),
                    StageName = selectedRow.Cells["StageName"].Value.ToString()
                };
                EditValue ev = new EditValue
                {
                    OriginalValue = artistOriginal,
                    Type = typeof(Artist).AssemblyQualifiedName,
                    EditedValue = artistNew
                };
                EditArtistController.Instance.EditArtist(ev);
                int indexOfSelectedRow = selectedRow.Index;
                DgvLoad();                
                dgvArtist.Rows[indexOfSelectedRow].Selected = true;
            }
            catch(ServerDisconnectedException ex)
            {
                MessageBox.Show("Server disconnected!");
                LoginController.Instance.frmLogin.Dispose();
                MainController.Instance.frmMain.Dispose();
                return;
            }
        }
            
    }
}

