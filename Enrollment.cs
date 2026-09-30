using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project_Frontened____Backened
{
    public partial class Enrollment : Form
    {
        
        string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""C:\Users\User\source\repos\Project Frontened &  Backened\Project Frontened &  Backened\SmartSchoolDB.mdf"";Integrated Security=True;Connect Timeout=30";
        public Enrollment()
        {
            InitializeComponent();
           
        }
        private void LoadAllRecords()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Enrollments", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvEnrollment.DataSource = dt;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtEnrollmentID.Text))
            {
                MessageBox.Show("Please enter Enrollment ID.");
                return;
            }


            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    SqlCommand checkEnroll = new SqlCommand("SELECT COUNT(*) FROM Enrollments WHERE EnrollmentID = @eid", con);
                    checkEnroll.Parameters.AddWithValue("@eid", txtEnrollmentID.Text.Trim());
                    int enrollExists = (int)checkEnroll.ExecuteScalar();

                    string query = enrollExists > 0
                        ? "UPDATE Enrollments SET StudentID=@sid, StudentName=@sname, Class=@class, Section=@sec, EnrollDate=@date, Year=@year, Status=@status, MonthlyFee=@fee WHERE EnrollmentID=@eid"
                        : "INSERT INTO Enrollments (EnrollmentID, StudentID, StudentName, Class, Section, EnrollDate, Year, Status, MonthlyFee) VALUES (@eid, @sid, @sname, @class, @sec, @date, @year, @status, @fee)";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@eid", txtEnrollmentID.Text.Trim());
                    cmd.Parameters.AddWithValue("@sid", txtStudentID.Text.Trim());
                    cmd.Parameters.AddWithValue("@sname", txtStudentName.Text.Trim());
                    cmd.Parameters.AddWithValue("@class", cmbClass.Text);
                    cmd.Parameters.AddWithValue("@sec", cmbSection.Text);
                    cmd.Parameters.AddWithValue("@date", dtpEnrollDate.Value);
                    cmd.Parameters.AddWithValue("@year", cmbYear.Text);
                    cmd.Parameters.AddWithValue("@status", cmbStatus.Text);
                    cmd.Parameters.AddWithValue("@fee", (string.IsNullOrEmpty(txtMonthlyFee.Text) || txtMonthlyFee.Text == "0") ? "0" : txtMonthlyFee.Text.Trim());

                    cmd.ExecuteNonQuery();
                    MessageBox.Show(enrollExists > 0 ? "Record Updated!" : "Record Saved!");
                    LoadAllRecords();
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
        }
           
            
          

        private void btnSearch_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    bool recordFound = false;

                    // 1. Pehle check karein ke kya ye bacha Enrollments table mein hai?
                    string enrollQuery = "SELECT * FROM Enrollments WHERE EnrollmentID = @id OR StudentID = @id";
                    SqlCommand cmdEnroll = new SqlCommand(enrollQuery, con);
                    cmdEnroll.Parameters.AddWithValue("@id", txtStudentID.Text.Trim() != "" ? txtStudentID.Text.Trim() : txtEnrollmentID.Text.Trim());

                    SqlDataReader drEnroll = cmdEnroll.ExecuteReader();

                    if (drEnroll.Read())
                    {
                        // Agar Enrollment mil gayi
                        txtEnrollmentID.Text = drEnroll["EnrollmentID"].ToString();
                        txtStudentID.Text = drEnroll["StudentID"].ToString();
                        txtStudentName.Text = drEnroll["StudentName"].ToString();
                        cmbClass.Text = drEnroll["Class"].ToString();
                        cmbSection.Text = drEnroll["Section"].ToString();
                        dtpEnrollDate.Value = Convert.ToDateTime(drEnroll["EnrollDate"]);
                        cmbYear.Text = drEnroll["Year"].ToString();
                        cmbStatus.Text = drEnroll["Status"].ToString();
                        txtMonthlyFee.Text = drEnroll["MonthlyFee"].ToString();

                        txtStudentName.ReadOnly = true; // Name ko lock kar dein
                        recordFound = true;
                        drEnroll.Close();
                    }
                    else
                    {
                        drEnroll.Close();
                        // 2. Agar Enrollment nahi mili, to check karein Student ke main table mein (assuming table name is 'Students')
                        if (!string.IsNullOrEmpty(txtStudentID.Text))
                        {
                            SqlCommand cmdStd = new SqlCommand("SELECT StudentName FROM Students WHERE StudentID = @sid", con);
                            cmdStd.Parameters.AddWithValue("@sid", txtStudentID.Text.Trim());
                            object result = cmdStd.ExecuteScalar();

                            if (result != null)
                            {
                                txtStudentName.Text = result.ToString();
                                txtStudentName.ReadOnly = true;

                                // Baqi form khali rakhein naye enrollment ke liye
                                txtEnrollmentID.Clear();
                                cmbClass.SelectedIndex = -1;
                                txtMonthlyFee.Clear();
                                MessageBox.Show("Student found in records. Please complete the enrollment.");
                                recordFound = true;
                            }
                        }
                    }

                    if (!recordFound)
                    {
                        MessageBox.Show("No record found for this ID.");
                        btnNew_Click(sender, e); // Form reset kar dein
                    }

                    // Grid update karne ke liye (Optional)
                    LoadAllRecords();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }



        private void dgvEnrollment_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Grid se row select karne par data textboxes mein bhar jayega
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvEnrollment.Rows[e.RowIndex];
                txtEnrollmentID.Text = row.Cells["EnrollmentID"].Value.ToString();
                txtStudentID.Text = row.Cells["StudentID"].Value.ToString();
                txtStudentName.Text = row.Cells["StudentName"].Value.ToString();
                cmbClass.Text = row.Cells["Class"].Value.ToString();
                cmbSection.Text = row.Cells["Section"].Value.ToString();
                dtpEnrollDate.Value = Convert.ToDateTime(row.Cells["EnrollDate"].Value);
                cmbYear.Text = row.Cells["Year"].Value.ToString();
                cmbStatus.Text = row.Cells["Status"].Value.ToString();
                txtMonthlyFee.Text = row.Cells["MonthlyFee"].Value.ToString();
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            // Sab se asaan aur confirm tareeqa
            txtEnrollmentID.Clear();
            txtStudentID.Clear();
            txtStudentName.Clear();
            txtMonthlyFee.Clear();

            // ComboBoxes ko reset karein
            cmbClass.SelectedIndex = -1;
            cmbSection.SelectedIndex = -1;
            cmbYear.SelectedIndex = -1;
            cmbStatus.SelectedIndex = -1;

            // Name ko wapis editable kar dein agar search ne lock kiya tha
            txtStudentName.ReadOnly = false;

            dtpEnrollDate.Value = DateTime.Now;
        }

        private void txtMonthlyFee_TextChanged(object sender, EventArgs e)
        {
            txtMonthlyFee.ReadOnly = true;
        }

        private void cmbClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbClass.SelectedIndex == -1) return;

            string cls = cmbClass.Text;
        int fee = 0;

            // Behtar logic .Contains use karke
            if ( cls == "Play" || cls == "Nursery" || cls == "Prep")
                fee = 2000;
            else if (cls.Contains("One") || cls.Contains("Two") || cls.Contains("Three"))
                fee = 2500;
            else if (cls.Contains("Four") || cls.Contains("Five"))
                fee = 3000;
            else if (cls.Contains("Six") || cls.Contains("Seven") || cls.Contains("Eight"))
                fee = 4000;
            else if (cls.Contains("Nine") || cls.Contains("Ten"))
                fee = 5000;

            // Pehle ReadOnly false karein taake value likhi ja sakay
            txtMonthlyFee.ReadOnly = false;
            txtMonthlyFee.Text = fee.ToString();
            txtMonthlyFee.ReadOnly = true; // Wapis lock kar dein
        }
       
        private void Enrollment_Load(object sender, EventArgs e)
        {
            LoadAllRecords(); // Form load hotay hi saara data nazar aaye
        }

       
          
        private void btnDelete_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtEnrollmentID.Text))
            {
                MessageBox.Show("Please select a record from the grid or enter Enrollment ID to delete.");
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to delete this enrollment?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    try
                    {
                        con.Open();
                        SqlCommand cmd = new SqlCommand("DELETE FROM Enrollments WHERE EnrollmentID = @eid", con);
                        cmd.Parameters.AddWithValue("@eid", txtEnrollmentID.Text.Trim());
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Record Deleted Successfully!");
                        btnNew_Click(sender, e); // Clear fields
                        LoadAllRecords(); // Refresh Grid
                    }
                    catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
                }
            }
        }
        public void OnlyNumbers(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Typing rok dega
            }
        }
        private void txtEnrollID_KeyPres(object sender, KeyPressEventArgs e)
        {
            OnlyNumbers(sender, e);
        }

        private void txtStudentID_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnlyNumbers(sender, e);
        }

       
    }
}
