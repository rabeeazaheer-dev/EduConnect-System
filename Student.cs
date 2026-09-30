using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace Project_Frontened____Backened
{
    public partial class Student : Form
    {
        public Student()
        {
            InitializeComponent();
           
            cbGender.Items.Clear(); // Delete old data
            cbGender.Items.Add("Male");
            cbGender.Items.Add("Female");
            cbGender.SelectedIndex = -1; // Male
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!txtEmail.Text.Contains("@"))
                {
                    MessageBox.Show("Please enter a valid Email address(Format: @gmail.com)", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return; // Ye line database save hone se rok degi
                }
                string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""C:\Users\User\source\repos\Project Frontened &  Backened\Project Frontened &  Backened\SmartSchoolDB.mdf"";Integrated Security=True;Connect Timeout=30";

               
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO Students ([StudentID], StudentName, [Father Name], CNIC, [Date of Birth], Gender, Phone, Email, Address) " +
               "VALUES (@id, @name, @fname, @cnic, @dob, @gender, @phone, @email, @address)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // For Numbers
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = Convert.ToInt32(txtStudent.Text);

                        // For Textboxes
                        cmd.Parameters.Add("@name", SqlDbType.NVarChar).Value = txtName.Text;
                        cmd.Parameters.Add("@fname", SqlDbType.NVarChar).Value = txtFatherName.Text;
                        cmd.Parameters.Add("@cnic", SqlDbType.NVarChar).Value = txtCNIC.Text;
                        cmd.Parameters.Add("@dob", SqlDbType.NVarChar).Value = dtpDOB.Text; // DatePicker
                        cmd.Parameters.Add("@gender", SqlDbType.NVarChar).Value = cbGender.Text;
                        cmd.Parameters.Add("@phone", SqlDbType.NVarChar).Value = txtPhone.Text;
                        cmd.Parameters.Add("@email", SqlDbType.NVarChar).Value = txtEmail.Text;
                        cmd.Parameters.Add("@address", SqlDbType.NVarChar).Value = txtAddress.Text;

                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();

                        MessageBox.Show("All Data Saved Successfully!");
                        btnDisplay_Click(sender, e);
                    }


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

            
           

        private void btnDisplay_Click(object sender, EventArgs e)
        {
                   
            try
            {
               
                string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""C:\Users\User\source\repos\Project Frontened &  Backened\Project Frontened &  Backened\SmartSchoolDB.mdf"";Integrated Security=True;Connect Timeout=30";

                
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    // Data read query
                    string query = "SELECT * FROM Students";

                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();

                    // Fill data table
                    da.Fill(dt);

                    // Data Display in Grid
                    dataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }      

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                // Email Format Validation (Update ke liye)
                if (!System.Text.RegularExpressions.Regex.IsMatch(txtEmail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    MessageBox.Show("Invalid Email Format!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    txtEmail.Focus();
                    return; // Yahan se code wapis chala jayega aur SQL query run nahi hogi
                }
                
                string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""C:\Users\User\source\repos\Project Frontened &  Backened\Project Frontened &  Backened\SmartSchoolDB.mdf"";Integrated Security=True;Connect Timeout=30";

                
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    //  This Query Update all columns
                    string query = "UPDATE Students SET StudentName=@name, [Father Name]=@fname, CNIC=@cnic, [Date of Birth]=@dob, " +
                                   "Gender=@gender, Phone=@phone, Email=@email, Address=@address " +
                                   "WHERE [StudentID]=@id";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Check ID is write or not
                        if (string.IsNullOrWhiteSpace(txtStudent.Text))
                        {
                            MessageBox.Show("Please enter the Student ID to update!");
                            return;
                        }

                        // Set Parameters
                        cmd.Parameters.AddWithValue("@id", txtStudent.Text);
                        cmd.Parameters.AddWithValue("@name", txtName.Text);
                        cmd.Parameters.AddWithValue("@fname", txtFatherName.Text);
                        cmd.Parameters.AddWithValue("@cnic", txtCNIC.Text);
                        cmd.Parameters.AddWithValue("@dob", dtpDOB.Text);
                        cmd.Parameters.AddWithValue("@gender", cbGender.Text);
                        cmd.Parameters.AddWithValue("@phone", txtPhone.Text);
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                        cmd.Parameters.AddWithValue("@address", txtAddress.Text);

                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        con.Close();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("All Data Updated Successfully!");
                            // Refresh Grid
                            btnDisplay_Click(sender, e);
                        }
                        else
                        {
                            MessageBox.Show("No student found with this ID.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

           
           
        private void btnSearch_Click(object sender, EventArgs e)
        {
                   
            try
            {
                
                string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""C:\Users\User\source\repos\Project Frontened &  Backened\Project Frontened &  Backened\SmartSchoolDB.mdf"";Integrated Security=True;Connect Timeout=30";

               
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM Students WHERE [StudentID] = @id";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", txtStudent.Text);
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            txtName.Text = reader["StudentName"].ToString();
                            txtFatherName.Text = reader["Father Name"].ToString();
                            txtCNIC.Text = reader["CNIC"].ToString();
                            dtpDOB.Text = reader["Date of Birth"].ToString();


                            if (reader["Gender"] != DBNull.Value)
                            {
                                string genderValue = reader["Gender"].ToString();


                                cbGender.SelectedIndex = cbGender.FindStringExact(genderValue);

                                if (cbGender.SelectedIndex == -1)
                                {
                                    cbGender.Text = genderValue;
                                }
                            }


                            txtPhone.Text = reader["Phone"].ToString();
                            txtEmail.Text = reader["Email"].ToString();
                            txtAddress.Text = reader["Address"].ToString();

                            MessageBox.Show("Record Found!");
                        }
                        else
                        {
                            MessageBox.Show("No record found with this ID.");
                            ClearFields();
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // Clear form
        private void ClearFields()
        {
            txtName.Clear();
            txtFatherName.Clear();
            txtCNIC.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtAddress.Clear();
        }


        private void btndelete_Click(object sender, EventArgs e)
        {

            // delete Confirmation
            DialogResult result = MessageBox.Show("Are you sure you want to delete this record?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {

                    string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""C:\Users\User\source\repos\Project Frontened &  Backened\Project Frontened &  Backened\SmartSchoolDB.mdf"";Integrated Security=True;Connect Timeout=30";


                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        // Remove ID whose write In textbox
                        string query = "DELETE FROM Students WHERE [StudentID] = @id";

                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            if (string.IsNullOrWhiteSpace(txtStudent.Text))
                            {
                                MessageBox.Show("Please enter the Student ID to delete!");
                                return;
                            }

                            cmd.Parameters.AddWithValue("@id", txtStudent.Text);

                            con.Open();
                            int rowsAffected = cmd.ExecuteNonQuery();
                            con.Close();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Record Deleted Successfully!");

                                // Delete and update List
                                ClearFields();
                                btnDisplay_Click(sender, e);
                            }
                            else
                            {
                                MessageBox.Show("No record found with this ID.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
          
            // Form Empty
            txtStudent.Clear();
            txtName.Clear();
            txtFatherName.Clear();
            txtCNIC.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtAddress.Clear();

            // Set default gender and date
            cbGender.SelectedIndex = -1;
            dtpDOB.Value = DateTime.Now;

            // Curser move to Id again
            txtStudent.Focus();
        }


       

       



        private void txtCNIC_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnlyNumbers(sender, e);
        }

        private void txtPhone_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnlyNumbers(sender, e);
        }

        private void txtStudentID_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnlyNumbers(sender, e);
        }
        private void OnlyNumbers(object sender, KeyPressEventArgs e)
        {
            // Agar input digit nahi hai aur backspace bhi nahi hai
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true; // Yeh typing ko rok dega
                MessageBox.Show("Only Numbers are allowed", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }

}