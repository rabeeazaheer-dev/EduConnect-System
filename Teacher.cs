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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace Project_Frontened____Backened
{
    public partial class Teacher : Form
    {
        ErrorProvider errorProvider1 = new ErrorProvider();
        public Teacher()
        {
            InitializeComponent();
        }
        
        string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""C:\Users\User\source\repos\Project Frontened &  Backened\Project Frontened &  Backened\SmartSchoolDB.mdf"";Integrated Security=True;Connect Timeout=30";
       
        private void btnSave_Click(object sender, EventArgs e)
        {
           
            // Pehle check karo validations sahi hain
            if (IsValid())
            {
                // 1. Email Format Validation yahan add karein
                if (!System.Text.RegularExpressions.Regex.IsMatch(txtEmail.Text.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    MessageBox.Show("Please enter a valid Teacher Email (Format: @gmail.com)", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return; // Code yahan se wapis chala jayega
                }
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    try
                    {
                        string query = "INSERT INTO Teachers (TeacherID, TeacherName, FatherName, CNIC, DOB, Gender, Phone, Email, Address) " +
                                       "VALUES (@id, @name, @fname, @cnic, @dob, @gender, @phone, @email, @address)";

                        SqlCommand cmd = new SqlCommand(query, con);
                        cmd.Parameters.AddWithValue("@id", txtTeacherID.Text.Trim());
                        cmd.Parameters.AddWithValue("@name", txtTeacherName.Text.Trim());
                        cmd.Parameters.AddWithValue("@fname", txtFatherName.Text.Trim());
                        cmd.Parameters.AddWithValue("@cnic", txtCNIC.Text.Trim());
                        cmd.Parameters.AddWithValue("@dob", dtpDOB.Value.Date);
                        cmd.Parameters.AddWithValue("@gender", cmbGender.Text);
                        cmd.Parameters.AddWithValue("@phone", txtPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@address", txtAddress.Text.Trim());

                        con.Open();
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Teacher Record Saved Successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        btnCreate_Click(sender, e);
                    }
                    catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
                }
            }
        }
        private bool IsValid()
        {
            bool isValid = true;
            errorProvider1.Clear(); // Pehle saare purane errors saaf karein

            // CNIC Check
            if (txtCNIC.Text.Length != 13)
            {
                errorProvider1.SetError(txtCNIC, "CNIC poora 13 digits ka hona chahiye!");
                isValid = false;
            }

            // Phone Check
            if (txtPhone.Text.Length != 11)
            {
                errorProvider1.SetError(txtPhone, "Phone number poora 11 digits ka hona chahiye!");
                isValid = false;
            }

            // Email Check
            if (!txtEmail.Text.ToLower().EndsWith("@gmail.com"))
            {
                errorProvider1.SetError(txtEmail, "Sirf @gmail.com allow hai!");
                isValid = false;
            }

            if (!isValid)
            {
                MessageBox.Show("Something went wrong", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return isValid;
        }
       
        
        private void btnSearch_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    string query = "SELECT * FROM Teachers WHERE TeacherID = @id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", txtTeacherID.Text.Trim());

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        txtTeacherName.Text = dr["TeacherName"].ToString();
                        txtFatherName.Text = dr["FatherName"].ToString();
                        txtCNIC.Text = dr["CNIC"].ToString();
                        dtpDOB.Value = Convert.ToDateTime(dr["DOB"]);
                        cmbGender.Text = dr["Gender"].ToString();
                        txtPhone.Text = dr["Phone"].ToString();
                        txtEmail.Text = dr["Email"].ToString();
                        txtAddress.Text = dr["Address"].ToString();
                    }
                    else
                    {
                        MessageBox.Show("No teacher found with this ID.");
                    }
                    con.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Search Error: " + ex.Message);
                }
            }
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            txtTeacherID.Clear();
            txtTeacherName.Clear();
            txtFatherName.Clear();
            txtCNIC.Clear();
            dtpDOB.Value = DateTime.Now;
            cmbGender.SelectedIndex = -1;
            txtPhone.Clear();
            txtEmail.Clear();
            txtAddress.Clear();
            txtTeacherID.Focus();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // Pehle check karein ke saari limitations (CNIC, Phone, Email) poori hain
            if (IsValid())
            {
                // Email Format Validation (Update ke liye)
                if (!System.Text.RegularExpressions.Regex.IsMatch(txtEmail.Text.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    MessageBox.Show("Invalid Email Format", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    txtEmail.Focus();
                    return; // Ye line aage database query (Line 161) ko chalne se rok degi
                }
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    try
                    {
                        // Query jo ID ki bunyaad par record update karegi
                        string query = "UPDATE Teachers SET TeacherName=@name, FatherName=@fname, CNIC=@cnic, DOB=@dob, " +
                                       "Gender=@gender, Phone=@phone, Email=@email, Address=@address WHERE TeacherID=@id";

                        SqlCommand cmd = new SqlCommand(query, con);

                        // Parameters ko textboxes se link karna
                        cmd.Parameters.AddWithValue("@id", txtTeacherID.Text.Trim());
                        cmd.Parameters.AddWithValue("@name", txtTeacherName.Text.Trim());
                        cmd.Parameters.AddWithValue("@fname", txtFatherName.Text.Trim());
                        cmd.Parameters.AddWithValue("@cnic", txtCNIC.Text.Trim());
                        cmd.Parameters.AddWithValue("@dob", dtpDOB.Value.Date);
                        cmd.Parameters.AddWithValue("@gender", cmbGender.Text);
                        cmd.Parameters.AddWithValue("@phone", txtPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@address", txtAddress.Text.Trim());

                        con.Open();
                        int rows = cmd.ExecuteNonQuery();
                        con.Close();

                        if (rows > 0)
                        {
                            MessageBox.Show("Teacher Record Updated Successfully!", "Update Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Grid aur list ko foran refresh karein
                            btnDisplay_Click(sender, e);
                        }
                        else
                        {
                            MessageBox.Show("Record nahi mila! Pehle sahi Teacher ID likh kar Search karein.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Update Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to delete this teacher?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    try
                    {
                        string query = "DELETE FROM Teachers WHERE TeacherID = @id";
                        SqlCommand cmd = new SqlCommand(query, con);
                        cmd.Parameters.AddWithValue("@id", txtTeacherID.Text.Trim());

                        con.Open();
                        int rows = cmd.ExecuteNonQuery();
                        con.Close();

                        if (rows > 0)
                        {
                            MessageBox.Show("Teacher Deleted!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            btnCreate_Click(sender, e); // Form Empty
                            btnDisplay_Click(sender, e); // Refresh
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("Delete Error: " + ex.Message); }
                }
            }
        }

        private void btnDisplay_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Teachers", con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    
                    dataGridView1.DataSource = dt;
                }
                catch (Exception ex) { MessageBox.Show("Display Error: " + ex.Message); }
            }
        }

        private void Teacher_Load(object sender, EventArgs e)
        {
            errorProvider1.Clear();
            txtPhone.MaxLength = 11;  // 11 se zyada digit nahi likhe jayenge
            txtCNIC.MaxLength = 13;   // 13 se zyada digit nahi likhe jayenge
            txtTeacherID.MaxLength = 5;
        }
        
      
        // 1. Common function jo sirf numbers allow karega aur galat input par Message Box dikhayega
        private void OnlyNumbers(object sender, KeyPressEventArgs e)
        {
            // Agar input digit nahi hai aur backspace bhi nahi hai
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true; // Yeh typing ko rok dega
                MessageBox.Show("Only Numbers are allowed", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

       



        private void txtTeacherID_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnlyNumbers(sender, e);

        }

        private void txtCNIC_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnlyNumbers(sender, e);
        }

        private void txtPhone_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnlyNumbers(sender, e);
        }
    }
}
