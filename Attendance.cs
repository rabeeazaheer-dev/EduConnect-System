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
using static System.Collections.Specialized.BitVector32;

namespace Project_Frontened____Backened
{
    public partial class Attendance : Form
    {
        //Database Connection String
        string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""C:\Users\User\source\repos\Project Frontened &  Backened\Project Frontened &  Backened\SmartSchoolDB.mdf"";Integrated Security=True;Connect Timeout=30";
        public Attendance()
        {
            InitializeComponent();
        }
        private void Attendance_Load(object sender, EventArgs e)
        {
            dgvAttendance.ReadOnly = true;
            dgvAttendance.AllowUserToAddRows = false;
            dgvAttendance.AllowUserToDeleteRows = false;
            dgvAttendance.EditMode = DataGridViewEditMode.EditProgrammatically;
            dgvAttendance.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dtpDate.Value = DateTime.Now;
            LoadAttendanceSummary();
            BtnSearch_Click(null, null);
        }
       
        private void BtnSearch_Click(object sender, EventArgs e)
        {
           
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    // Base Query
                    string query = @"SELECT E.StudentID, E.StudentName, E.Class, E.Section, 
                             ISNULL(A.Status, 'Present') as AttendanceStatus
                             FROM Enrollments E
                             LEFT JOIN Attendance A ON E.StudentID = A.StudentID AND A.AttendanceDate = @date";

                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@date", dtpDate.Value.Date);

                    // Agar Student ID likhi hai to sirf wo dikhao, warna Class/Section load karo
                    if (!string.IsNullOrEmpty(txtStudentID.Text.Trim()))
                    {
                        query += " WHERE E.StudentID = @sid";
                        cmd.Parameters.AddWithValue("@sid", txtStudentID.Text.Trim());
                    }
                    else if (!string.IsNullOrEmpty(cmbClass.Text) && !string.IsNullOrEmpty(cmbSection.Text))
                    {
                        query += " WHERE E.Class = @class AND E.Section = @sec";
                        cmd.Parameters.AddWithValue("@class", cmbClass.Text);
                        cmd.Parameters.AddWithValue("@sec", cmbSection.Text);
                    }
                    //else
                    //{
                    //    MessageBox.Show("Please enter Student ID or select Class/Section.");
                    //    return;
                    //}

                    query += " ORDER BY E.StudentID ASC";
                    cmd.CommandText = query;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvAttendance.DataSource = dt;

                    // Coloring logic: Absent bachon ko highlight karne ke liye
                    foreach (DataGridViewRow row in dgvAttendance.Rows)
                    {
                        UpdateRowColor(row);
                    }

                    LoadAttendanceSummary();
                }
                catch (Exception ex) { MessageBox.Show("Search Error: " + ex.Message); }
            }
        }
           
        private void BtnMarkAttendance_Click(object sender, EventArgs e)
        {
         
            string newStatus = rbPresent.Checked ? "Present" : rbAbsent.Checked ? "Absent" : rbLeave.Checked ? "Leave" : "";
            if (string.IsNullOrEmpty(newStatus)) { MessageBox.Show("Select P/A/L first!"); return; }

            // Case 1: Agar koi row select ki hui hai (Specific student)
            if (dgvAttendance.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dgvAttendance.SelectedRows)
                {
                    row.Cells["AttendanceStatus"].Value = newStatus;
                    UpdateRowColor(row);
                }
            }
            // Case 2: Agar kuch select nahi kiya (Sari Class ko Absent ya Leave karna ho)
            else
            {
                DialogResult dr = MessageBox.Show($"Are you wan to sure to{newStatus} ?", "Bulk Update", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    foreach (DataGridViewRow row in dgvAttendance.Rows)
                    {
                        row.Cells["AttendanceStatus"].Value = newStatus;
                        UpdateRowColor(row);
                    }
                }
            }
        }

        // Chota helper function rang badalne ke liye
        private void UpdateRowColor(DataGridViewRow row)
        {
            string status = row.Cells["AttendanceStatus"].Value.ToString();
            if (status == "Absent") row.DefaultCellStyle.BackColor = Color.MistyRose; // Light Red
            else if (status == "Leave") row.DefaultCellStyle.BackColor = Color.LightYellow;
            else row.DefaultCellStyle.BackColor = Color.White;
        }
            
            
        
        
        
        private void txtStudentID_Leave(object sender, EventArgs e)
        {
            // Check if the Student ID field is empty
            if (string.IsNullOrEmpty(lblStudentID.Text)) return;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    // Query to find student in Enrollment table
                    string query = "SELECT StudentName, Class, Section FROM Enrollments WHERE StudentID = @sid";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@sid", lblStudentID.Text.Trim());

                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        // If student is found, auto-fill the Name, Class, and Section
                        txtStudentID.Text = dr["StudentName"].ToString();
                        cmbClass.Text = dr["Class"].ToString();
                        cmbSection.Text = dr["Section"].ToString();
                    }
                    else
                    {
                        // If student is not enrolled, show error and clear fields
                        MessageBox.Show("Student ID not found in Enrollments!", "ID Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtStudentID.Clear();
                        cmbClass.SelectedIndex = -1;
                        cmbSection.SelectedIndex = -1;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Database Error: " + ex.Message);
                }
            }
        }
           
        private void BtnSave_Click(object sender, EventArgs e)
        {
           
            if (dgvAttendance.Rows.Count == 0)
            {
                MessageBox.Show("List Empty!");
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    foreach (DataGridViewRow row in dgvAttendance.Rows)
                    {
                        // Khali row ko ignore karne ke liye
                        if (row.IsNewRow) continue;

                        // Check karein ke StudentID waqai mojood hai
                        if (row.Cells["StudentID"].Value != null)
                        {
                            string sid = row.Cells["StudentID"].Value.ToString();
                            string sname = row.Cells["StudentName"].Value.ToString();
                            string status = row.Cells["AttendanceStatus"].Value.ToString();
                            string cls = row.Cells["Class"].Value.ToString();
                            string sec = row.Cells["Section"].Value.ToString();

                            string upsertQuery = @"IF EXISTS (SELECT 1 FROM Attendance WHERE StudentID=@id AND AttendanceDate=@date)
                                           UPDATE Attendance SET Status=@status WHERE StudentID=@id AND AttendanceDate=@date
                                           ELSE
                                           INSERT INTO Attendance (StudentID, StudentName, AttendanceDate, Status, Class, Section) 
                                           VALUES (@id, @sname, @date, @status, @class, @section)";

                            using (SqlCommand cmd = new SqlCommand(upsertQuery, con))
                            {
                                cmd.Parameters.AddWithValue("@id", sid);
                                cmd.Parameters.AddWithValue("@sname", sname);
                                cmd.Parameters.AddWithValue("@date", dtpDate.Value.Date);
                                cmd.Parameters.AddWithValue("@status", status);
                                cmd.Parameters.AddWithValue("@class", cls);
                                cmd.Parameters.AddWithValue("@section", sec);
                                cmd.ExecuteNonQuery();
                            } // SqlCommand ka using yahan band hua
                        } // StudentID null check yahan band hua
                    } // foreach loop yahan band hua

                    MessageBox.Show("Attendance Saved!", "Success");
                    LoadAttendanceSummary();
                } // try yahan band hua
                catch (Exception ex)
                {
                    MessageBox.Show("Saving Error: " + ex.Message);
                } // catch yahan band hua
            } // SqlConnection ka using yahan band hua
        } // Button click method yahan band hua
            
            
        private void LoadAttendanceSummary()
        {
           
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    string query = @"SELECT 
                COUNT(*) as Total,
                ISNULL(SUM(CASE WHEN Status = 'Present' THEN 1 ELSE 0 END), 0) as Present,
                ISNULL(SUM(CASE WHEN Status = 'Absent' THEN 1 ELSE 0 END), 0) as Absent,
                ISNULL(SUM(CASE WHEN Status = 'Leave' THEN 1 ELSE 0 END), 0) as Leave
                FROM Attendance WHERE AttendanceDate = @date";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@date", dtpDate.Value.Date);

                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        //check value is empty or not
                        lblTotal.Text = dr["Total"].ToString();
                        lblPresentCount.Text = dr["Present"].ToString();
                        lblAbsentCount.Text = dr["Absent"].ToString();
                        lblLeaveCount.Text = dr["Leave"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    // If data not found then display 0
                    lblTotal.Text = "0";
                    lblPresentCount.Text = "0";
                    lblAbsentCount.Text = "0";
                    lblLeaveCount.Text = "0";
                }
            }
        }
           
       
        private void BtnClear_Click(object sender, EventArgs e)
        {
         
            txtStudentID.Clear();
            cmbClass.SelectedIndex = -1;
            cmbSection.SelectedIndex = -1;
            rbPresent.Checked = false;
            rbAbsent.Checked = false;
            rbLeave.Checked = false;
            BtnSearch_Click(null, null); // Refresh the grid
        }

        private void dtpDate_ValueChanged(object sender, EventArgs e)
        {
            // Refresh the grid
            BtnSearch_Click(null, null);
            LoadAttendanceSummary();
        }

        private void Attendance_Load_1(object sender, EventArgs e)
        {
           
            dgvAttendance.AllowUserToAddRows = false; // Ye line add karein

            // Baaki ka aapka purana code...
            dgvAttendance.ReadOnly = true;
            dtpDate.Value = DateTime.Now;
            // ...
        }
    }
}
