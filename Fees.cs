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
    public partial class Fees : Form
    {
        // Use the exact same connection string format as your Enrollment form to avoid path conflicts
        string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""C:\Users\User\source\repos\Project Frontened &  Backened\Project Frontened &  Backened\SmartSchoolDB.mdf"";Integrated Security=True;Connect Timeout=30";

        public Fees()
        {
            InitializeComponent();
            // Adding events for real-time balance calculation
            txtPaidAmount.TextChanged += CalculateBalance;
            txtTotalFee.TextChanged += CalculateBalance;
        }
        private void CalculateBalance(object sender, EventArgs e)
        {
            double total = 0, paid = 0;
            double.TryParse(txtTotalFee.Text, out total);
            double.TryParse(txtPaidAmount.Text, out paid);

            txtBalance.Text = (total - paid).ToString();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    // 1. Student ki details fetch karna
                    SqlCommand cmdFetch = new SqlCommand("SELECT StudentName, MonthlyFee FROM Enrollments WHERE StudentID = @id", con);
                    cmdFetch.Parameters.AddWithValue("@id", txtStudentID.Text.Trim());
                    SqlDataReader dr = cmdFetch.ExecuteReader();

                    if (dr.Read())
                    {
                        txtStudentName.Text = dr["StudentName"].ToString();
                        txtTotalFee.Text = dr["MonthlyFee"].ToString();
                        dr.Close();

                        // 2. Month-wise sorting ke liye query
                        // Hum CASE use kar rahe hain taake months order mein aayein (Jan=1, Feb=2, etc.)
                        string gridQuery = @"SELECT FeeMonth, TotalAmount, PaidAmount, Balance, PaymentDate, Status 
                                   FROM Fees 
                                   WHERE StudentID = @id 
                                   ORDER BY CASE 
                                        WHEN FeeMonth = 'January' THEN 1
                                        WHEN FeeMonth = 'February' THEN 2
                                        WHEN FeeMonth = 'March' THEN 3
                                        WHEN FeeMonth = 'April' THEN 4
                                        WHEN FeeMonth = 'May' THEN 5
                                        WHEN FeeMonth = 'June' THEN 6
                                        WHEN FeeMonth = 'July' THEN 7
                                        WHEN FeeMonth = 'August' THEN 8
                                        WHEN FeeMonth = 'September' THEN 9
                                        WHEN FeeMonth = 'October' THEN 10
                                        WHEN FeeMonth = 'November' THEN 11
                                        WHEN FeeMonth = 'December' THEN 12
                                        ELSE 13 END";

                        SqlDataAdapter da = new SqlDataAdapter(gridQuery, con);
                        da.SelectCommand.Parameters.AddWithValue("@id", txtStudentID.Text.Trim());
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvFeesHistory.DataSource = dt;
                    }
                    else
                    {
                        dr.Close();
                        MessageBox.Show("Student not found!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }






        private void btnPayFee_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(txtStudentID.Text) || string.IsNullOrEmpty(cmbFeeMonth.Text))
            {
                MessageBox.Show("Please enter Student ID and select Fee Month.");
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    //  Check if the fee for this month is already paid/exists
                    string checkQuery = "SELECT COUNT(*) FROM Fees WHERE StudentID = @id AND FeeMonth = @month";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                    checkCmd.Parameters.AddWithValue("@id", txtStudentID.Text.Trim());
                    checkCmd.Parameters.AddWithValue("@month", cmbFeeMonth.Text);

                    int count = (int)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        // Stop the process if duplicate is found
                        MessageBox.Show("Fee for " + cmbFeeMonth.Text + " has already been submitted for this student!");
                        return;
                    }

                    //  Proceed with Insert if no duplicate found
                    string query = "INSERT INTO Fees (StudentID, StudentName, FeeMonth, TotalAmount, PaidAmount, Balance, Status, PaymentDate) " +
                                   "VALUES (@id, @name, @month, @total, @paid, @bal, @status, GETDATE())";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", txtStudentID.Text.Trim());
                    cmd.Parameters.AddWithValue("@name", txtStudentName.Text.Trim());
                    cmd.Parameters.AddWithValue("@month", cmbFeeMonth.Text);
                    cmd.Parameters.AddWithValue("@total", string.IsNullOrEmpty(txtTotalFee.Text) ? "0" : txtTotalFee.Text);
                    cmd.Parameters.AddWithValue("@paid", string.IsNullOrEmpty(txtPaidAmount.Text) ? "0" : txtPaidAmount.Text);
                    cmd.Parameters.AddWithValue("@bal", string.IsNullOrEmpty(txtBalance.Text) ? "0" : txtBalance.Text);

                    double balance = 0;
                    double.TryParse(txtBalance.Text, out balance);
                    string status = (balance <= 0) ? "Paid" : "Pending";
                    cmd.Parameters.AddWithValue("@status", status);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Fee Transaction Saved Successfully!");

                    //Refresh Dashboard and Grid
                    UpdateDashboard();
                    btnSearch_Click(sender, e);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Database Error: " + ex.Message);
                }
            }
        }


        private void UpdateDashboard()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    // Get Total Received from database
                    SqlCommand cmdReceived = new SqlCommand("SELECT SUM(PaidAmount) FROM Fees", con);
                    object received = cmdReceived.ExecuteScalar();
                    string receivedVal = received != DBNull.Value ? received.ToString() : "0";
                    lblTotalReceived.Text = receivedVal;

                    //  Get Total Pending from database
                    SqlCommand cmdPending = new SqlCommand("SELECT SUM(Balance) FROM Fees", con);
                    object pending = cmdPending.ExecuteScalar();
                    string pendingVal = pending != DBNull.Value ? pending.ToString() : "0";
                    lblTotalPending.Text = pendingVal;

                    //  Calculate Total Expected
                    double r = 0, p = 0;
                    double.TryParse(receivedVal, out r);
                    double.TryParse(pendingVal, out p);
                    lblTotalExpected.Text = (r + p).ToString();
                }
                catch (Exception ex)
                {
                    //  This helps you see if there is an error during update
                    Console.WriteLine("Dashboard Error: " + ex.Message);
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtStudentID.Clear();
            txtStudentName.Clear();
            txtTotalFee.Clear();
            txtPaidAmount.Clear();
            txtBalance.Clear();
            cmbFeeMonth.SelectedIndex = -1;
        }

        private void Fees_Load(object sender, EventArgs e)
        {

            //  Load dashboard values when form starts
            UpdateDashboard();
            dgvFeesHistory.ReadOnly = true;
        }

        private void btnPrintReceipt_Click_1(object sender, EventArgs e)
        {
            // Check if data is present before printing
            if (string.IsNullOrEmpty(txtStudentName.Text))
            {
                MessageBox.Show("Please search for a student and process payment first.");
                return;
            }

            //  Show the Print Preview dialog
            // Is se print preview hamesha poori screen par khulay ga
            ((Form)printPreviewDialog1).WindowState = FormWindowState.Maximized;
            printPreviewDialog1.ShowDialog();
            
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            // --- FONTS ---
            Font titleFont = new Font("Arial", 22, FontStyle.Bold);
            Font subTitleFont = new Font("Arial", 12, FontStyle.Bold);
            Font headerFont = new Font("Arial", 10, FontStyle.Bold);
            Font bodyFont = new Font("Arial", 10, FontStyle.Regular);
            Font footerFont = new Font("Arial", 8, FontStyle.Italic);

            Color maroonTheme = Color.Maroon;
            Brush maroonBrush = new SolidBrush(maroonTheme);

            float pageWidth = e.PageBounds.Width;
            float startX = 50;
            float currentY = 40;

            // --- 1. HEADER SECTION (Logo + New Heading) ---
            try
            {
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                Image myLogo = Properties.Resources.logo;

                // Logo Draw (Width barha di hai taake niche wala EduConnect parha jaye)
                e.Graphics.DrawImage(myLogo, startX, currentY, 130, 100);

                // Sirf "SMART SCHOOLING SYSTEM" heading mein
                string headingName = "SMART SCHOOL SYSTEM";
                e.Graphics.DrawString(headingName, titleFont, maroonBrush, startX + 150, currentY + 35);
            }
            catch { }

            currentY += 120;

            // --- 2. OFFICIAL RECEIPT BAR ---
            e.Graphics.FillRectangle(maroonBrush, startX, currentY, pageWidth - 100, 30);
            string receiptText = "OFFICIAL FEE RECEIPT";
            float textWidth = e.Graphics.MeasureString(receiptText, subTitleFont).Width;
            e.Graphics.DrawString(receiptText, subTitleFont, Brushes.White, (pageWidth - textWidth) / 2, currentY + 5);

            currentY += 50;

            // --- 3. STUDENT INFO ---
            e.Graphics.DrawRectangle(new Pen(Color.LightGray, 1), startX, currentY, pageWidth - 100, 90);
            e.Graphics.DrawString($"Student ID:   {txtStudentID.Text}", bodyFont, Brushes.Black, startX + 15, currentY + 15);
            e.Graphics.DrawString($"Student Name: {txtStudentName.Text}", bodyFont, Brushes.Black, startX + 15, currentY + 40);
            e.Graphics.DrawString($"Fee Month:    {cmbFeeMonth.Text}", bodyFont, Brushes.Black, startX + 15, currentY + 65);
            e.Graphics.DrawString($"Date: {DateTime.Now:dd-MMM-yyyy}", bodyFont, Brushes.Black, pageWidth - 230, currentY + 15);

            currentY += 130;

            // --- 4. FEE DETAILS ---
            double monthlyFee = double.TryParse(txtTotalFee.Text, out double mFee) ? mFee : 0;
            double paid = double.TryParse(txtPaidAmount.Text, out double pAmt) ? pAmt : 0;
            double bal = monthlyFee - paid;

            e.Graphics.DrawString("Monthly Tuition Fee", bodyFont, Brushes.Black, startX, currentY);
            e.Graphics.DrawString(monthlyFee.ToString("N2"), bodyFont, Brushes.Black, pageWidth - 180, currentY);

            currentY += 30;
            e.Graphics.DrawString("TOTAL BALANCE DUE", headerFont, maroonBrush, startX, currentY);
            e.Graphics.DrawString("Rs. " + bal.ToString("N2"), headerFont, maroonBrush, pageWidth - 180, currentY);

            currentY += 80;

            // --- 5. LATE FEE SCHEDULE (7 & 15 Days) ---
            e.Graphics.DrawString("LATE FEE SCHEDULE", headerFont, maroonBrush, startX, currentY);
            currentY += 25;
            e.Graphics.DrawRectangle(Pens.LightGray, startX, currentY, pageWidth - 100, 60);

            DateTime date7 = DateTime.Now.AddDays(7);
            DateTime date15 = DateTime.Now.AddDays(15);

            e.Graphics.DrawString($"Payable After {date7:dd-MMM-yyyy}:", bodyFont, Brushes.Black, startX + 10, currentY + 10);
            e.Graphics.DrawString("Rs. " + (monthlyFee * 1.05).ToString("N2"), headerFont, Brushes.Black, pageWidth - 180, currentY + 10);

            e.Graphics.DrawString($"Payable After {date15:dd-MMM-yyyy}:", bodyFont, Brushes.Black, startX + 10, currentY + 35);
            e.Graphics.DrawString("Rs. " + (monthlyFee * 1.10).ToString("N2"), headerFont, Brushes.Black, pageWidth - 180, currentY + 35);

            // --- 6. SIGNATURE ---
            currentY += 110;
            e.Graphics.DrawLine(Pens.Black, pageWidth - 250, currentY, pageWidth - 50, currentY);
            e.Graphics.DrawString("Authorized Signature", footerFont, Brushes.Black, pageWidth - 200, currentY + 5);
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
           
            //  Validation: Check karein ke ID aur Month entered hain
            if (string.IsNullOrEmpty(txtStudentID.Text) || string.IsNullOrEmpty(cmbFeeMonth.Text))
            {
                MessageBox.Show("Please enter Student ID and select Fee Month.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //  Confirmation: Galti se bachne ke liye confirmation mangna
            DialogResult dr = MessageBox.Show($"Are you sure you want to delete {cmbFeeMonth.Text} fee record?",
                                             "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dr == DialogResult.Yes)
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    try
                    {
                        con.Open();
                        //  Delete Query: ID aur Month dono match hona zaroori hain
                        string deleteQuery = "DELETE FROM Fees WHERE StudentID = @id AND FeeMonth = @month";

                        SqlCommand cmd = new SqlCommand(deleteQuery, con);
                        cmd.Parameters.AddWithValue("@id", txtStudentID.Text.Trim());
                        cmd.Parameters.AddWithValue("@month", cmbFeeMonth.Text);

                        int rowsEffected = cmd.ExecuteNonQuery();

                        if (rowsEffected > 0)
                        {
                            MessageBox.Show("Fee record deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            //  GUI Refresh: Dashboard aur Grid ko update karna
                            UpdateDashboard();
                            btnSearch_Click(sender, e); // Ye grid ko refresh kar dega

                            // Fields saaf karna (Optionally)
                            txtPaidAmount.Clear();
                            txtBalance.Clear();
                        }
                        else
                        {
                            MessageBox.Show("No record found to delete.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Database Error: " + ex.Message);
                    }
                }
            }
        }
    }
}
