using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(){InitializeComponent();

            
        }
        
        private void directory_tree_view_Loaded(object sender, RoutedEventArgs e){MyLogics.directory_tree_view_Loaded(directory_tree_view);}
        private void directory_tree_view_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e){ MyLogics.directory_tree_view_SelectedItemChanged((TreeView)sender, location_bar); }
        private void location_bar_TextChanged(object sender, TextChangedEventArgs e){MyLogics.location_bar_TextChanged(location_bar.Text, data_grid);}
        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e){MyLogics.location_bar_TextChanged(location_bar.Text, sender as DataGridRow, location_bar);}
        private void Back_Button_Click(object sender, RoutedEventArgs e) { MyLogics.Back_Button_Click(location_bar); }
        private void Forward_Button_Click(object sender, RoutedEventArgs e) { MyLogics.Forward_Button_Click(location_bar); }
    }


}




class MyLogics
{
    static object dummynode = null;

    public static List<MyClasses.MyDataEntry> data = new List<MyClasses.MyDataEntry>();
    public static List<string> history = new List<string>();
    public static int history_index = -1;
    public static bool add_to_history = true;

    public static void directory_tree_view_Loaded(TreeView root)
    {
        foreach (string s in Directory.GetLogicalDrives())
        {
            TreeViewItem item = new TreeViewItem();
            item.Header = s;
            item.Tag = s;
            item.Items.Add(dummynode);
            item.Expanded += new RoutedEventHandler(folder_expanded);

            root.Items.Add(item);
        }
    }
    public static void folder_expanded(object sender, RoutedEventArgs e)
    {
        TreeViewItem root = (TreeViewItem)sender;
        if (root.Items.Count == 1 && root.Items[0] == dummynode)
        {
            root.Items.Clear();
            try
            {
                foreach (string s in Directory.GetDirectories(root.Tag.ToString()))
                {
                    TreeViewItem item = new TreeViewItem();
                    item.Header = s.Substring(s.LastIndexOf("\\") + 1);
                    item.Tag = s;
                    item.Margin = new Thickness(5);
                    item.Items.Add(dummynode);
                    item.Expanded += new RoutedEventHandler(folder_expanded);

                    root.Items.Add(item);
                }
            }
            catch (Exception) { }
        }
    }
    public static void directory_tree_view_SelectedItemChanged(TreeView root, TextBox box)
    {
        TreeViewItem item = (TreeViewItem)root.SelectedItem;
        if (item == null) return;
        box.Text = item.Tag.ToString();
    }
    public static void location_bar_TextChanged(string uri, DataGrid dataGrid)
    {
        data = new List<MyClasses.MyDataEntry>();
        
        try
        {
            foreach (string s in Directory.GetFileSystemEntries(uri))
            {
                try
                {
                    data.Add(new MyClasses.MyDataEntry()
                    {
                        Checked = false,
                        Name = s.Substring(s.LastIndexOf("\\") + 1),
                        Date_Modified = Directory.GetLastAccessTime(s),
                        Type = (Directory.Exists(s)) ? "Folder" : "File",
                        Size = (File.Exists(s)) ? get_size(new FileInfo(s).Length) : Directory.GetFileSystemEntries(s).Length + " files",
                        SizeInt = (File.Exists(s)) ? new FileInfo(s).Length : Directory.GetFileSystemEntries(s).Length
                    });
                }
                catch (Exception) { }
            }

            if (add_to_history)
            {
                if (history.Count > 100)
                {
                    history.RemoveRange(0, 50);
                }

                history_index = history.Count;
                history.Add(uri);
            }
            else add_to_history = true;

            
            dataGrid.ItemsSource = data;
        }
        catch (Exception) { }
    }
    public static void location_bar_TextChanged(string uri, DataGridRow row, TextBox textbox)
    {
        
        if (!(uri.EndsWith("\\") || uri.EndsWith("/"))) uri += "\\";
        uri += (row.Item as MyClasses.MyDataEntry).Name;
        //location_bar_TextChanged(uri, grid);
        if(Directory.Exists(uri))textbox.Text = uri;
        else if(File.Exists(uri))
        {
            try
            {
                System.Diagnostics.Process.Start(uri);
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot open ... No default application found");
            }   
        }

    }
    public static void Back_Button_Click(TextBox box)
    {
        if(history_index > 0)
        {
            add_to_history = false;
            --history_index;
            box.Text = history.ElementAt(history_index);
        }
        
    }
    public static void Forward_Button_Click(TextBox box)
    {
        if (history_index < history.Count - 1)
        {
            add_to_history = false;
            ++history_index;
            box.Text = history.ElementAt(history_index);
        }
        
    }
    


    public static void show_history()
    {
        string r = "";
        foreach(string s in history)
        {
            r += s+"\n";
        }
        MessageBox.Show(r);
    }
    public static string get_size(long size)
    {
        if (size < 1024) return size + " Bytes";
        else if (size < 1048576) return (size / 1024) + "." + ((size % 1024) * 10) / 1024 + " KB";
        else if (size < 1073741824) return (size / 1048576) + "." + ((size % 1048576) * 10) / 1048576 + " MB";
        else return (size / 1073741824) + "." + ((size % 1073741824) * 10) / 1073741824 + " GB";
    }

}



class MyClasses
{
    public class MyDataEntry
    {
        public bool Checked { get; set; }
        public string Name { get; set; }
        public DateTime Date_Modified { get; set; }
        public string Type { get; set; }
        public string Size { get; set; }
        public long SizeInt { get; set; }
    }
}




