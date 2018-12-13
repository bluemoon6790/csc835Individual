using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace DesktopApp
{
    /// <summary>
    /// Interaction logic for CommitMessage.xaml
    /// </summary>
    public partial class CommitMessage : Window
    {
        public CommitMessage()
        {
            InitializeComponent();
        }

        private void Click_Textarea(object sender, RoutedEventArgs e)
        {
            if (Commit_Message.Text == "Your message goes here.")
            {
                Commit_Message.Text = "";
            }
        }

        private void Process_Commit(object sender, RoutedEventArgs e)
        {
            string text = Commit_Message.Text;

            ((MainWindow)this.Owner).SendCommit(text);

            this.Close();
        }
    }
}
