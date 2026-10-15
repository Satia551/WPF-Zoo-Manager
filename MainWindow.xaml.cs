using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WPF_Zoo_Manager;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    SqlConnection sqlConnection;

    public MainWindow()
    {
        InitializeComponent();
        string connectionString =
            @"Server=localhost\PANJUTORIALSSQL;Database=PanjutorialsDB;User Id=sa;Password=12345654321;TrustServerCertificate=True;";
        // The next line of the code is for old version, and it doesn't work with the version that I'm currently using.
        //string connectionString = ConfigurationManager.ConnectionStrings["WPF_ZooManager.Properties.Settings.PanjutorialsDBConnectionString"].ConnectionString;
        sqlConnection = new SqlConnection(connectionString);
        ShowZoos();
        ShowAnimals();
    }

    private void ShowZoos()
    {
        try
        {
            string query = "select * from Zoo";
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query, sqlConnection);
            using (sqlDataAdapter)
            {
                DataTable zooTable = new DataTable();

                sqlDataAdapter.Fill(zooTable);

                //Which Information of the Table in DataTable should be shown in our ListBox?
                listZoos.DisplayMemberPath = "Location";
                //Which Value should be delivered, when an Item from our ListBox is selected?
                listZoos.SelectedValuePath = "Id";
                //The Reference to the Data the ListBox should populate
                listZoos.ItemsSource = zooTable.DefaultView;
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString());
        }
    }

    private void ShowAssociatedAnimals()
    {
        try
        {

            if (listZoos.SelectedItem is DataRowView selectedRow)
            {
                // Wyciągamy 'Id' bezpośrednio z tabeli
                int selectedZooId = Convert.ToInt32(selectedRow["Id"]);

                string query =
                    "select za.Id, a.Name from Animal a inner join ZooAnimal za on a.Id = za.AnimalId where za.ZooId = @ZooId";

                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);

                using (sqlDataAdapter)
                {
                    sqlCommand.Parameters.AddWithValue("@ZooId", selectedZooId);

                    DataTable animalTable = new DataTable();
                    sqlDataAdapter.Fill(animalTable);

                    listAssociatedAnimals.DisplayMemberPath = "Name";
                    listAssociatedAnimals.SelectedValuePath = "Id";
                    listAssociatedAnimals.ItemsSource = animalTable.DefaultView;
                }
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString());
        }
    }

    private void listZoos_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ShowAssociatedAnimals();
        ShowSelectedZooInTextBox();
    }
    private void listAllAnimals_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ShowSelectedAnimalInTextBox();
    }

    private void ShowAnimals()
    {
        try
        {
            string query = "select * from Animal";
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query, sqlConnection);
            using (sqlDataAdapter)
            {

                DataTable animalTable = new DataTable();

                sqlDataAdapter.Fill(animalTable);

                //Which Information of the Table in DataTable should be shown in our ListBox?
                listAllAnimals.DisplayMemberPath = "NAME";
                //Which Value should be delivered, when an Item from our ListBox is selected?
                listAllAnimals.SelectedValuePath = "Id";
                //The Reference to the Data the ListBox should populate
                listAllAnimals.ItemsSource = animalTable.DefaultView;
            }

        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString());
        }
    }

    private void DeleteZoo_Click(object sender, RoutedEventArgs e)
    {
        if (listZoos.SelectedItem is DataRowView selectedRow)
        {
            int selectedZooId = Convert.ToInt32(selectedRow["Id"]);

            try
            {
                string query = "delete from Zoo where id = @ZooId";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);

                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("@ZooId", selectedZooId);
                sqlCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            ShowZoos();
        }
        else
        {
            MessageBox.Show("You have to select Zoo!");
        }
    }

    private void AddZoo_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(myTextBox.Text))
        {
            MessageBox.Show("Please enter a zoo location!");
            return;
        }
        try
        {
            string query = "insert into Zoo values (@Location)";
            SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);

            sqlConnection.Open();
            sqlCommand.Parameters.AddWithValue("@Location", myTextBox.Text);
            sqlCommand.ExecuteScalar();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            sqlConnection.Close();
        }

        ShowZoos();
    }

    private void addAnimalToZoo_Click(object sender, RoutedEventArgs e)
    {
        if (listAllAnimals.SelectedItem is DataRowView selectedAnimal &&
            listZoos.SelectedItem is DataRowView selectedZoo)
        {
            int zooId = Convert.ToInt32(selectedZoo["Id"]);
            int animalId = Convert.ToInt32(selectedAnimal["Id"]);

            try
            {
                string query = "insert into ZooAnimal values (@ZooId, @AnimalId)";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);

                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("@ZooId", zooId);
                sqlCommand.Parameters.AddWithValue("@AnimalId", animalId);
                sqlCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            ShowAssociatedAnimals();
        }
    }

    private void RemoveAnimal_Click(object sender, RoutedEventArgs e)
    {
        if (listAssociatedAnimals.SelectedItem is DataRowView selectedAnimal)
        {
            int animalId = Convert.ToInt32(selectedAnimal["Id"]);
            try
            {
                string query = "delete from ZooAnimal where Id = @Id";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);

                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("@Id", animalId);
                sqlCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            ShowAssociatedAnimals();
        }
    }

    private void AddAnimal_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(myTextBox.Text))
        {
            MessageBox.Show("Please enter an animal name!");
            return;
        }
        try
        {
            sqlConnection.Open();

            string checkQuery = "select count(*) from Animal where lower(Name) = lower(@NAME)";
            SqlCommand checkCommand = new SqlCommand(checkQuery, sqlConnection);
            checkCommand.Parameters.AddWithValue("@NAME", myTextBox.Text.Trim());

            int existingCount = Convert.ToInt32(checkCommand.ExecuteScalar());

            if (existingCount > 0)
            {
                MessageBox.Show("An animal with this name already exists in the database!");
                return;
            }
            
            
            string query = "insert into Animal (Name) values (@NAME)";
            SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);

            
            sqlCommand.Parameters.AddWithValue("@NAME", myTextBox.Text.Trim());
            sqlCommand.ExecuteScalar();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            sqlConnection.Close();
        }

        ShowAnimals();
    }

    private void DeleteAnimal_Click(object sender, RoutedEventArgs e)
    {
        if (listAllAnimals.SelectedItem is DataRowView selectedRow)
        {
            int selectedAnimalId = Convert.ToInt32(selectedRow["Id"]);

            try
            {
                string query =
                    "delete from ZooAnimal where AnimalId = @AnimalId; delete from Animal where id = @AnimalId";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);

                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("@AnimalId", selectedAnimalId);
                sqlCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            ShowAnimals();
            ShowAssociatedAnimals();
        }
        else
        {
            MessageBox.Show("You have to select Animal!");
        }
    }

    private void ShowSelectedInTextBox(ListBox listBox, string columnName)
    {
        if (listBox.SelectedItem is DataRowView selectedRow)
        {
            myTextBox.Text = selectedRow[columnName].ToString();
        }
    }
    private void ShowSelectedZooInTextBox()
    {
        ShowSelectedInTextBox(listZoos, "Location");
    }

    private void ShowSelectedAnimalInTextBox()
    {
        ShowSelectedInTextBox(listAllAnimals, "NAME");
    }
    
    
private void UpdateZoo_Click(object sender, RoutedEventArgs e)
    {
        if (listZoos.SelectedItem is DataRowView selectedZoo)
        {
            int zooId = Convert.ToInt32(selectedZoo["Id"]);
            try
            {
                string query = "update Zoo Set Location = @Location where Id = @ZooId";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);

                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("@ZooId", zooId);
                sqlCommand.Parameters.AddWithValue("@Location", myTextBox.Text);
                sqlCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            ShowZoos();
        }
        else
        {
            MessageBox.Show("You have to select Zoo!");
        }
    }

    private void UpdateAnimal_Click(object sender, RoutedEventArgs e)
    {
        if (listAllAnimals.SelectedItem is DataRowView selectedAnimal)
        {
            int AnimalId = Convert.ToInt32(selectedAnimal["Id"]);
            try
            {
                string query = "update Animal Set NAME = @NAME where Id = @AnimalId";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);

                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("@AnimalId", AnimalId);
                sqlCommand.Parameters.AddWithValue("@NAME", myTextBox.Text);
                sqlCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                sqlConnection.Close();
            }

            ShowAnimals();
        }
        else
        {
            MessageBox.Show("You have to select Animal!");
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            this.DragMove();
        }
    }
}



