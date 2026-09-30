using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace Project_Frontened____Backened
{ 
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }


        private void BtnMenu_Click(object sender, EventArgs e)
        {
            //sidebarTimer.Start();
 
         }

        bool sidebarExpand = false;

        private void SidebarTimer_Tick(object sender, EventArgs e)
        {
            if (sidebarExpand)
            {
                sidebar.Width -= 10;
                if (sidebar.Width <= sidebar.MinimumSize.Width)
                {
                    sidebarExpand = false;
                    sidebarTimer.Stop();
                }
            }
            else
            {
                sidebar.Width += 10;
                if (sidebar.Width >= sidebar.MaximumSize.Width)
                {
                    sidebarExpand = true;
                    sidebarTimer.Stop();
                }
            }
        }

        private void BtnStudent_Click_1(object sender, EventArgs e)
        {
            Student st = new Student();
            st.Show();
        }

        private void BtnTeacher_Click(object sender, EventArgs e)
        {
       
            Teacher obj = new Teacher();
            obj.Show(); 
        }

        private void BtnDashboard_Click(object sender, EventArgs e)
        {
            // If container appear already
            if (pnlDashboardContainer.Visible == true)
            {
                // then hide
                pnlDashboardContainer.Visible = false;
            }
            else
            {
                // If Hide then update and show
                UpdateDashboardCounts();
                pnlDashboardContainer.Visible = true; 
                pnlDashboardContainer.BringToFront();
            }
        }
        private void UpdateDashboardCounts()
        {
            // Connection string 
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""C:\Users\User\source\repos\Project Frontened &  Backened\Project Frontened &  Backened\SmartSchoolDB.mdf"";Integrated Security=True;Connect Timeout=30";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    // 1. Count Total Student
                    SqlCommand cmdStud = new SqlCommand("SELECT COUNT(*) FROM Students", con);
                    int studentCount = Convert.ToInt32(cmdStud.ExecuteScalar());
                    lblTotalStudents.Text = studentCount.ToString();

                    // 2. Count Total Enrollments
                    SqlCommand cmdEnroll = new SqlCommand("SELECT COUNT(*) FROM Enrollments", con);
                    int enrollCount = Convert.ToInt32(cmdEnroll.ExecuteScalar());
                    lblTotalEnrollments.Text = enrollCount.ToString();

                    // 3.Total Teacher Count
                    SqlCommand cmdTeach = new SqlCommand("SELECT COUNT(*) FROM [dbo].[Teachers]", con);
                    int teacherCount = Convert.ToInt32(cmdTeach.ExecuteScalar());
                    lblTotalTeachers.Text = teacherCount.ToString();

                    // 4. Total Attendance
                    string attQuery = "SELECT COUNT(*) FROM Attendance";
                    string presentQuery = "SELECT COUNT(*) FROM Attendance WHERE Status = 'Present'";

                    SqlCommand cmdTotalAtt = new SqlCommand(attQuery, con);
                    SqlCommand cmdPresentAtt = new SqlCommand(presentQuery, con);

                    double totalAttRecords = Convert.ToDouble(cmdTotalAtt.ExecuteScalar());
                    double presentAttRecords = Convert.ToDouble(cmdPresentAtt.ExecuteScalar());

                    if (totalAttRecords > 0)
                    {
                        double percentage = (presentAttRecords / totalAttRecords) * 100;
                        lblTotalAttendance.Text = Math.Round(percentage).ToString() + "%";
                    }
                    else
                    {
                        lblTotalAttendance.Text = "0%";
                    }

                    con.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Dashboard Error: " + ex.Message);
                }
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            // Hide Dashboard in Start 
            pnlDashboardContainer.Visible = false;


        }

        private void btnEnrollment_Click(object sender, EventArgs e)
        {

            Enrollment obj = new Enrollment();
            obj.Show();
        }

        private void btnAttendance_Click_1(object sender, EventArgs e)
        {
            Attendance obj = new Attendance();
            obj.Show();
        }

        private void btnSection_Click(object sender, EventArgs e)
        {
            Result obj = new Result();
            obj.Show();
        }

        private void btnFee_Click(object sender, EventArgs e)
        {
            Fees subForm = new Fees();
            subForm.Show();
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            sidebarTimer.Start();
        }
    }
}

