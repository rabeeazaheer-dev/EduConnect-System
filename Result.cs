using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;

namespace Project_Frontened____Backened
{
    public partial class Result : Form
    {
        string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""C:\Users\User\source\repos\Project Frontened &  Backened\Project Frontened &  Backened\SmartSchoolDB.mdf"";Integrated Security=True;Connect Timeout=30";
        public Result()
        {
            InitializeComponent();
            txtName.ReadOnly = true;
            txtClass.ReadOnly = true;
        }
       

        // 1. Grid Setup: 4 Columns lazmi hain (Index 0 to 3)
        private void SetupGrid()
        {
            dgvResult.Columns.Clear();
            dgvResult.Columns.Add("Subject", "Subject Name");    // Index 0
            dgvResult.Columns.Add("Obtained", "Obtained Marks"); // Index 1
            dgvResult.Columns.Add("Total", "Total Marks");       // Index 2
            dgvResult.Columns.Add("Term", "Term");               // Index 3

            // Grid UI behtar karne ke liye
            dgvResult.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
        
        private void btnSearch_Click(object sender, EventArgs e)
        {
          
            if (string.IsNullOrEmpty(txtStudentID.Text))
            {
                MessageBox.Show("Please enter a Student ID first!");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // Student details search
                    string query = "SELECT StudentName, Class FROM Enrollments WHERE StudentID = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", txtStudentID.Text);

                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        txtName.Text = dr["StudentName"].ToString();
                        txtClass.Text = dr["Class"].ToString();
                        dr.Close(); // Reader close karna zaroori hai

                        LoadSubjects(txtClass.Text);
                        DisplayFullResult(); // Purana data database se load karein
                    }
                    else
                    {
                        MessageBox.Show("Student not found!");
                        ClearAll();
                    }
                }
                catch (Exception ex) { MessageBox.Show("Search Error: " + ex.Message); }
            }
        }
        private void LoadSubjects(string className)
        {
            // 1. Pehle list ko clear karein
            cmbSubject.Items.Clear();

            // 2. Jo subjects HAR class ke liye common hain, unhe yahan likhein
            // (Aap is list mein mazeed subjects comma de kar add kar sakti hain)
            string[] common = { "English", "Urdu", "Math", "Islamiat", "Pak Studies", "General Knowledge" };
            cmbSubject.Items.AddRange(common);

            // 3. Agar class 9th ya 10th (Nine ya Ten) hai, to ye science subjects bhi add ho jayein
            if (className.ToLower().Contains("nine") || className.ToLower().Contains("ten") ||
                className.Contains("9") || className.Contains("10"))
            {
                cmbSubject.Items.AddRange(new string[] { "Physics", "Chemistry", "Computer Science", "Biology" });
            }
            else
            {
                // Choti classes ke liye automatic General Science add ho jaye
                cmbSubject.Items.Add("General Science");
            }
        }
       

        private void btnAdd_Click_1(object sender, EventArgs e)
        {
          string newSubject = cmbSubject.Text;
            string newTerm = cmbTerm.Text;

            // Grid mein check karein ke ye subject aur term pehle se hai?
            foreach (DataGridViewRow row in dgvResult.Rows)
            {
                if (row.Cells[0].Value?.ToString() == newSubject && row.Cells[3].Value?.ToString() == newTerm)
                {
                    MessageBox.Show("This record already exists!");
                    return; // Yahin se wapis bhej dein
                }
            }

            // Agar duplicate nahi hai to niche wala purana code chale ga
            dgvResult.Rows.Add(newSubject, txtObtainMarks.Text, "100", newTerm);
            UpdateMyChart();
        }
           
        


        
        private void UpdateMyChart()
        {
            Marks.Series["Marks"].Points.Clear();
            foreach (DataGridViewRow row in dgvResult.Rows)
            {
                if (row.Cells[0].Value != null && row.Cells[1].Value != null)
                {
                    Marks.Series["Marks"].Points.AddXY(row.Cells[0].Value.ToString(), Convert.ToDouble(row.Cells[1].Value));
                }
            }
        }
       
        private void btnSave_Click(object sender, EventArgs e)
        {
           
            if (string.IsNullOrEmpty(txtStudentID.Text))
            {
                MessageBox.Show(" Search Student !");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    foreach (DataGridViewRow row in dgvResult.Rows)
                    {
                        if (row.IsNewRow) continue;

                        // --- Ye rahi wo Logic jo aapne puchi ---
                        string query = @"
                    IF EXISTS (SELECT 1 FROM ResultTable WHERE StudentID=@id AND SubjectName=@sub AND TermName=@term)
                    BEGIN
                        UPDATE ResultTable 
                        SET ObtainedMarks=@marks, ExamDate=GETDATE() 
                        WHERE StudentID=@id AND SubjectName=@sub AND TermName=@term
                    END
                    ELSE
                    BEGIN
                        INSERT INTO ResultTable (StudentID, ClassName, SubjectName, TotalMarks, ObtainedMarks, TermName, ExamDate) 
                        VALUES (@id, @class, @sub, @total, @marks, @term, GETDATE())
                    END";

                        SqlCommand cmd = new SqlCommand(query, conn);
                        // Parameters pass karte waqt safe string use karein taake null error na aaye
                        cmd.Parameters.AddWithValue("@id", txtStudentID.Text);
                        cmd.Parameters.AddWithValue("@class", txtClass.Text);
                        cmd.Parameters.AddWithValue("@sub", row.Cells[0].Value?.ToString() ?? "");   // Safe Subject
                        cmd.Parameters.AddWithValue("@marks", row.Cells[1].Value?.ToString() ?? "0"); // Safe Obtained Marks
                        cmd.Parameters.AddWithValue("@total", row.Cells[2].Value?.ToString() ?? "100"); // Safe Total
                        cmd.Parameters.AddWithValue("@term", row.Cells[3].Value?.ToString() ?? "");   // Safe Term Name
                        //// Parameters pass karna zaroori hai safety ke liye
                        //cmd.Parameters.AddWithValue("@id", txtStudentID.Text);
                        //cmd.Parameters.AddWithValue("@class", txtClass.Text);
                        //cmd.Parameters.AddWithValue("@sub", row.Cells[0].Value.ToString());   // Subject
                        //cmd.Parameters.AddWithValue("@marks", row.Cells[1].Value.ToString()); // Obtained Marks
                        //cmd.Parameters.AddWithValue("@total", row.Cells[2].Value.ToString()); // Total (100)
                        //cmd.Parameters.AddWithValue("@term", row.Cells[3].Value.ToString());  // Term Name

                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("Data Saved!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Saving Error: " + ex.Message);
                }
                try
                {
                    // SQL execution ke baad
                    // MessageBox.Show("Data kamyabi se Update ho gaya hai!"); 

                    // CRITICAL STEP: Grid ko database se dubara load karein
                    DisplayFullResult();

                    // Chart ko bhi naye data ke mutabiq update karein
                    UpdateMyChart();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }
         
           
            
            
            
        private void DisplayFullResult()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    string query = "SELECT SubjectName, ObtainedMarks, TotalMarks, TermName FROM ResultTable WHERE StudentID = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", txtStudentID.Text);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvResult.Rows.Clear();
                    foreach (DataRow dr in dt.Rows)
                    {
                        dgvResult.Rows.Add(dr["SubjectName"], dr["ObtainedMarks"], dr["TotalMarks"], dr["TermName"]);
                    }
                    UpdateMyChart();
                }
                catch (Exception ex) { MessageBox.Show("Display Error: " + ex.Message); }
            }
        }
        private void ClearAll()
        {
            txtStudentID.Clear(); txtName.Clear(); txtClass.Clear();
            txtObtainMarks.Clear(); cmbSubject.SelectedIndex = -1;
            cmbTerm.SelectedIndex = -1; dgvResult.Rows.Clear();
            Marks.Series["Marks"].Points.Clear();
        }

       


       
        private void BtnClear_Click(object sender, EventArgs e)
        {

        ClearAll(); }

        private void btnDelete_Click(object sender, EventArgs e)
        {
          
            if (dgvResult.SelectedRows.Count > 0)
            {
                DialogResult dr = MessageBox.Show("Are you want to sure to delete this record ", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (dr == DialogResult.Yes)
                {
                    // Grid se data pakrein
                    string subjectName = dgvResult.SelectedRows[0].Cells[0].Value.ToString();
                    string termName = dgvResult.SelectedRows[0].Cells[3].Value.ToString();
                    string studentID = txtStudentID.Text;

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        try
                        {
                            conn.Open();
                            // Database se delete karne ki query
                            string query = "DELETE FROM ResultTable WHERE StudentID=@id AND SubjectName=@sub AND TermName=@term";

                            SqlCommand cmd = new SqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@id", studentID);
                            cmd.Parameters.AddWithValue("@sub", subjectName);
                            cmd.Parameters.AddWithValue("@term", termName);

                            int result = cmd.ExecuteNonQuery();

                            if (result > 0)
                            {
                                // Agar database se delete ho gaya, to grid se bhi hata dein
                                dgvResult.Rows.RemoveAt(dgvResult.SelectedRows[0].Index);
                                UpdateMyChart();
                                MessageBox.Show("Delete Successfully");
                            }
                        }
                        catch (Exception ex) { MessageBox.Show("Delete Error: " + ex.Message); }
                    }
                }
            }
            else
            {
                MessageBox.Show("Select Row");
            }
        }
           
           
           

        private void Result_Load(object sender, EventArgs e)
        {
            
            // Puri row select karne ke liye
            dgvResult.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // Sirf aik row select ho (professional lagta hai)
            dgvResult.MultiSelect = false;

            // ReadOnly hone ke bawajood selection enable rehti hai
            dgvResult.ReadOnly = true;
        
        // Chart aur ChartArea dono ko Form ke background color se match karein
        Marks.BackColor = this.BackColor;
            Marks.ChartAreas[0].BackColor = this.BackColor;

            // Agar aapne panel use kiya hai to this.BackColor ki jagah panel1.BackColor likhein

            // Bars ka color Maroon set karein
            Marks.Series[0].Color = Color.Maroon;

            // Bars ke border ka color agar change karna ho (optional)
            Marks.Series[0].BorderColor = Color.Maroon;
            // Grid settings - Columns humne designer se add kar liye hain isliye yahan code nahi likhna
        //    dgvResult.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        //    dgvResult.MultiSelect = false;
        //    dgvResult.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
           
            // Pehle check karein ke Term select ki hai ya nahi
            if (cmbTerm.SelectedIndex == -1)
            {
                MessageBox.Show("Please select term");
                return;
            }

            PrintDocument pd = new PrintDocument();
            pd.PrintPage += new PrintPageEventHandler(this.PrintReceiptPage);

            PrintPreviewDialog ppd = new PrintPreviewDialog();
            ppd.Document = pd;
            ((Form)ppd).WindowState = FormWindowState.Maximized;
            ppd.ShowDialog();
        }
        

        private void dgvResult_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
          
            // Check karein ke header par click na hua ho
            if (e.RowIndex >= 0)
            {
                // 1. Data ko wapis input fields mein bhejein
                cmbSubject.Text = dgvResult.Rows[e.RowIndex].Cells[0].Value.ToString();
                txtObtainMarks.Text = dgvResult.Rows[e.RowIndex].Cells[1].Value.ToString();
                cmbTerm.Text = dgvResult.Rows[e.RowIndex].Cells[3].Value.ToString();

                // 2. Grid se wo purani (galat) row remove kar dein
                dgvResult.Rows.RemoveAt(e.RowIndex);

                // 3. Chart update karein
                UpdateMyChart();

                MessageBox.Show("Now you can edit marks and click Add again.");
            }
        // Jab bhi kisi cell mein marks change honge, chart khud update ho jayega
        UpdateMyChart();
        }
        
       
        private void PrintReceiptPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            // --- Colors & Fonts (Premium Maroon Theme) ---
            Color schoolColor = Color.Maroon;
            Color lightShade = Color.FromArgb(255, 248, 248); // Soft pinkish-maroon for row striping
            Color summaryBg = Color.FromArgb(245, 245, 245);  // Sleek grey-white background for summary

            Font fontTitle = new Font("Segoe UI", 22, FontStyle.Bold);
            Font fontHeader = new Font("Segoe UI", 12, FontStyle.Bold);
            Font fontBody = new Font("Segoe UI", 10, FontStyle.Regular);
            Font fontBodyBold = new Font("Segoe UI", 10, FontStyle.Bold);
            Font fontTotal = new Font("Segoe UI", 11, FontStyle.Bold);

            // --- Page Border ---
            g.DrawRectangle(new Pen(schoolColor, 3), 25, 25, e.PageBounds.Width - 50, e.PageBounds.Height - 50);

            // --- 1. HEADER SECTION ---
            int startY = 60;
            try
            {
                Image myLogo = Properties.Resources.logo;
                g.DrawImage(myLogo, 60, startY, 100, 90);

                g.DrawString("SMART SCHOOL SYSTEM", fontTitle, new SolidBrush(schoolColor), 180, startY + 10);
                g.DrawString("Quality Education for a Brighter Future", new Font("Segoe UI", 10, FontStyle.Italic), Brushes.Gray, 180, startY + 48);
                g.DrawString("Faisalabad, Punjab, Pakistan | Contact: 041-111-222", fontBody, Brushes.DimGray, 180, startY + 68);
            }
            catch { }

            startY += 115;
            g.DrawLine(new Pen(schoolColor, 2), 60, startY, e.PageBounds.Width - 60, startY);

            // --- 2. DOCUMENT TYPE ---
            g.DrawString("OFFICIAL RESULT CARD", fontHeader, Brushes.Black, (e.PageBounds.Width / 2) - 80, startY + 15);

            // --- 3. STUDENT INFO SECTION ---
            startY += 65;

            g.DrawString("STUDENT ID:", fontBodyBold, Brushes.Black, 70, startY);
            g.DrawString(txtStudentID.Text, fontBody, Brushes.Black, 200, startY);

            g.DrawString("STUDENT NAME:", fontBodyBold, Brushes.Black, 70, startY + 25);
            g.DrawString(txtName.Text, fontBody, Brushes.Black, 200, startY + 25);

            g.DrawString("CLASS:", fontBodyBold, Brushes.Black, 500, startY);
            g.DrawString(txtClass.Text, fontBody, Brushes.Black, 600, startY);

            g.DrawString("TERM:", fontBodyBold, Brushes.Black, 500, startY + 25);
            g.DrawString(cmbTerm.Text, fontBodyBold, new SolidBrush(schoolColor), 600, startY + 25);

            // --- 4. TABLE SECTION ---
            startY += 75;
            int tableLeft = 60;
            int tableWidth = e.PageBounds.Width - 120; // 730 Width

            // Table Header Background (Maroon)
            g.FillRectangle(new SolidBrush(schoolColor), tableLeft, startY, tableWidth, 35);

            // Term column mukamal khatam, spacing behtar kar di hai
            int colSubject = tableLeft + 15;
            int colTotal = tableLeft + 320;
            int colObtained = tableLeft + 450;
            int colPercent = tableLeft + 580;
            int colGrade = tableLeft + 670;

            g.DrawString("SUBJECT NAME", fontBodyBold, Brushes.White, colSubject, startY + 8);
            g.DrawString("TOTAL MARKS", fontBodyBold, Brushes.White, colTotal, startY + 8);
            g.DrawString("OBTAINED", fontBodyBold, Brushes.White, colObtained, startY + 8);
            g.DrawString("PERCENTAGE", fontBodyBold, Brushes.White, colPercent, startY + 8);
            g.DrawString("GRADE", fontBodyBold, Brushes.White, colGrade, startY + 8);

            int rowY = startY + 35;
            int serialNo = 1;
            string selectedTerm = cmbTerm.Text;

            double grandTotal = 0;
            double grandObtained = 0;

            foreach (DataGridViewRow row in dgvResult.Rows)
            {
                if (row.IsNewRow) continue;

                if (row.Cells[3].Value?.ToString() == selectedTerm)
                {
                    string subjectName = row.Cells[0].Value?.ToString() ?? "";
                    double obtained = Convert.ToDouble(row.Cells[1].Value ?? 0);
                    double total = Convert.ToDouble(row.Cells[2].Value ?? 100);

                    double percentage = (obtained / total) * 100;
                    string grade = GetGrade(percentage);

                    grandTotal += total;
                    grandObtained += obtained;

                    // Zebra striping for premium look
                    if (serialNo % 2 == 0)
                        g.FillRectangle(new SolidBrush(lightShade), tableLeft, rowY, tableWidth, 30);

                    g.DrawString(serialNo + ". " + subjectName, fontBody, Brushes.Black, colSubject, rowY + 5);
                    g.DrawString(total.ToString(), fontBody, Brushes.Black, colTotal, rowY + 5);
                    g.DrawString(obtained.ToString(), fontBody, Brushes.Black, colObtained, rowY + 5);
                    g.DrawString(percentage.ToString("0.0") + "%", fontBody, Brushes.Black, colPercent, rowY + 5);

                    // Fail (F) grade ho to bold red text
                    g.DrawString(grade, fontBodyBold, new SolidBrush(grade == "F" ? Color.Red : Color.Black), colGrade, rowY + 5);

                    g.DrawLine(Pens.LightGray, tableLeft, rowY + 30, tableLeft + tableWidth, rowY + 30);
                    rowY += 30;
                    serialNo++;
                }
            }

            // --- 5. ATTRACTIVE & MODERN SUMMARY BLOCK ---
            // Purana box khatam! Ab ye aik solid, clean aur high-end minimalist design hai.
            rowY += 20;

            double overallPercentage = grandTotal > 0 ? (grandObtained / grandTotal) * 100 : 0;
            string finalGrade = GetGrade(overallPercentage);

            int summaryWidth = 380;
            int summaryX_Start = tableLeft + tableWidth - summaryWidth; // Right align karne ke liye

            // Light grey summary block background
            g.FillRectangle(new SolidBrush(summaryBg), summaryX_Start, rowY, summaryWidth, 135);

            // Left edge bar (Maroon accent strip jo professional industrial applications mein hoti hai)
            g.FillRectangle(new SolidBrush(schoolColor), summaryX_Start, rowY, 5, 135);

            int textX_Label = summaryX_Start + 25;
            int textX_Value = summaryX_Start + 240;

            // Inner details alignment
            g.DrawString("Grand Total Marks:", fontBody, Brushes.DimGray, textX_Label, rowY + 15);
            g.DrawString(grandTotal.ToString(), fontTotal, Brushes.Black, textX_Value, rowY + 15);

            g.DrawString("Total Obtained Marks:", fontBody, Brushes.DimGray, textX_Label, rowY + 42);
            g.DrawString(grandObtained.ToString(), fontTotal, Brushes.Black, textX_Value, rowY + 42);

            g.DrawString("Overall Percentage:", fontBody, Brushes.DimGray, textX_Label, rowY + 69);
            g.DrawString(overallPercentage.ToString("0.00") + "%", fontTotal, Brushes.Black, textX_Value, rowY + 69);

            // Thin line divider inside block before Final Grade
            g.DrawLine(Pens.LightGray, textX_Label, rowY + 98, summaryX_Start + summaryWidth - 20, rowY + 98);

            // Final Grade Row (Highlighted with Maroon)
            g.DrawString("FINAL GRADE:", fontHeader, new SolidBrush(schoolColor), textX_Label, rowY + 106);
            g.DrawString(finalGrade, new Font("Segoe UI", 14, FontStyle.Bold), new SolidBrush(finalGrade == "F" ? Color.Red : Color.Maroon), textX_Value, rowY + 104);


            // --- 6. SIGNATURES ---
            int sigY = rowY + 185;
            g.DrawLine(new Pen(schoolColor, 1), 100, sigY, 250, sigY);
            g.DrawString("Class Teacher", fontBody, Brushes.Black, 130, sigY + 5);

            g.DrawLine(new Pen(schoolColor, 1), 550, sigY, 700, sigY);
            g.DrawString("Principal / Controller", fontBody, Brushes.Black, 560, sigY + 5);

            // --- 7. FOOTER ---
            g.DrawString("System Generated Result Card | Date: " + DateTime.Now.ToString("dd-MMM-yyyy"),
                          new Font("Arial", 8), Brushes.Gray, (e.PageBounds.Width / 2) - 120, e.PageBounds.Height - 80);
        }

        // --- Helper Function for Grading System ---
        private string GetGrade(double percentage)
        {
            if (percentage >= 85) return "A+";
            if (percentage >= 75) return "A";
            if (percentage >= 65) return "B";
            if (percentage >= 50) return "C";
            if (percentage >= 40) return "D";
            return "F";
        }

        
    }
   
}
