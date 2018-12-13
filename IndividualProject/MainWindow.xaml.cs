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
using System.Windows.Navigation;
using System.Windows.Shapes;
using DesktopApp;
using System.IO;
using System.Diagnostics;

namespace DesktopApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string remote;
        string branch;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string gitPath = GitPath.Text;
            remote = RemotePath.Text;

            // Deactivate those text boxes so the user cannot change them anymore.
            GitPath.IsReadOnly = true;
            RemotePath.IsReadOnly = true;
            SetDir.Visibility = Visibility.Hidden;
            GitPath.Foreground = new SolidColorBrush(Colors.DarkGray);
            GitPath.Background = new SolidColorBrush(Colors.LightGray);
            RemotePath.Foreground = new SolidColorBrush(Colors.DarkGray);
            RemotePath.Background = new SolidColorBrush(Colors.LightGray);

            // Make sure the local repo is tied to something.
            if ("" == gitCommand("remote -v", gitPath))
            {
                gitCommand("remote add origin " + this.remote, gitPath);
            }

            // Update status
            Output.Text = gitCommand("status", gitPath);
            Directory.Text = gitCommand("ls-files -t", gitPath) + gitCommand("ls-files --other", gitPath);

            UpdateBranches();

        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            RunUpdate();
        }

        private void RunUpdate()
        {
            string gitPath = GitPath.Text;
            Output.Text = gitCommand("status", gitPath);
            CommitBtn.Visibility = Visibility.Hidden;

            // Check for untracked files.
            string line;
            string status = gitCommand("status -s", gitPath);
            StringReader read = new StringReader(status);

            // Want to compose a list of files w/ changes.
            string toAdd = "", toCommit = "";

            bool hasAdd = false;
            bool hasCommit = false;
            while (true)
            {
                line = read.ReadLine();
                if (line != null)
                {
                    if (line.Substring(1, 1) == "M" || line.Substring(0, 2) == "??" || line.Substring(1, 1) == "D") // Check for modified files, deleted files, and new files
                    {
                        AddAll.Visibility = Visibility.Visible;
                        hasAdd = true;
                        switch (line.Substring(0, 2))
                        {
                            case " M":
                                toAdd += "[M]";
                                break;
                            case " D":
                                toAdd += "[-]";
                                break;
                            case "??":
                                toAdd += "[+]";
                                break;
                            default:
                                break;
                        }
                        toAdd += " " + line.Substring(2) + "\r\n";
                    }
                    if (line.Substring(0, 1) == "M")
                    {
                        hasCommit = true;
                        toCommit += "[C]" + line.Substring(2) + "\r\n";
                    }
                    if (line.Substring(0, 1) == "A")
                    {
                        hasCommit = true;
                        toCommit += "[A]" + line.Substring(2) + "\r\n";
                    }
                    if (line.Substring(0, 1) == "D")
                    {
                        hasCommit = true;
                        toCommit += "[D]" + line.Substring(2) + "\r\n";
                    }
                }
                else
                {
                    break;
                }
            }

            if (hasCommit && !hasAdd)
            {
                CommitBtn.Visibility = Visibility.Visible;
            }

            if (hasAdd)
            {
                Directory.Text = toAdd;
            }
            else if (hasCommit)
            {
                Directory.Text = toCommit;
            }
            else
            {
                Directory.Text = gitCommand("ls-files", gitPath);
            }

        }

        private void UpdateBranches()
        {
            string gitPath = GitPath.Text;
            // Make sure branches are up to date
            Branches.Items.Clear();

            string line;

            string branches = gitCommand("branch", gitPath);
            StringReader readBranch = new StringReader(branches);
            Branches.Items.Add("<branches>");
            while (true)
            {
                line = readBranch.ReadLine();
                if (line != null)
                {
                    Branches.Items.Add(line.Substring(2));
                    if (line.Substring(0, 1) == "*")
                    {
                        Branches.SelectedItem = line.Substring(2);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void GitPath_GotFocus(object sender, RoutedEventArgs e)
        {
            if (GitPath.IsReadOnly == false)
            {
                GitPath.Text = "";
            }
        }

        private void RemotePath_GotFocus(object sender, RoutedEventArgs e)
        {
            if (RemotePath.IsReadOnly == false)
            {
                RemotePath.Text = "";
            }
        }

        private void AddAll_Click(object sender, RoutedEventArgs e)
        {
            string gitPath = GitPath.Text;
            gitCommand("add .", gitPath);
            AddAll.Visibility = Visibility.Hidden;
            RunUpdate();
        }

        private void Commit_Click(object sender, RoutedEventArgs e)
        {
            // Create a new window to accept commit message input

            CommitMessage cm = new CommitMessage();
            cm.Owner = this;
            cm.Show();
        }

        public void SendCommit(string message)
        {
            string gitPath = GitPath.Text;
            // Read that input and append to git command
            gitCommand("commit -m \"" + message + "\"", gitPath);
            RunUpdate();
        }

        private void Push(object sender, RoutedEventArgs e)
        {
            string gitPath = GitPath.Text;
            this.branch = Branches.SelectedItem.ToString(); // Remove once branch handling is put in place.
            Output.Text = gitCommand("push --set-upstream origin " + this.branch, gitPath);

        }

        private void Pull(object sender, RoutedEventArgs e)
        {
            string gitPath = GitPath.Text;
            this.branch = "master"; // Remove once branch handling is put in place.
            Output.Text = gitCommand("pull", gitPath);

        }

        private void Branches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string gitPath = GitPath.Text;
            if (!(Branches.SelectedItem is null))
            {
                gitCommand("checkout " + Branches.SelectedItem.ToString(), gitPath);
                RunUpdate();
            }
        }

        public string gitCommand(string command, string gitPath)
        {
            string fn = @"C:\Program Files\Git\bin\git.exe";

            ProcessStartInfo gitInfo = new ProcessStartInfo();
            gitInfo.CreateNoWindow = true;
            gitInfo.RedirectStandardError = true;
            gitInfo.RedirectStandardOutput = true;
            gitInfo.UseShellExecute = false;

            //file directory of local git install directory
            gitInfo.FileName = fn;
            //directory of local repository
            gitInfo.WorkingDirectory = gitPath;

            gitInfo.Arguments = @command; // such as "log" or "status"

            //Then create a Process to actually run the command.
            Process gitProcess = new Process();
            gitProcess.StartInfo = gitInfo;
            gitProcess.Start();

            string stderr_str = gitProcess.StandardError.ReadToEnd();  // pick up STDERR
            string stdout_str = gitProcess.StandardOutput.ReadToEnd(); // pick up STDOUT

            Console.WriteLine(stderr_str);
            return stdout_str;
            //Console.ReadKey();

        }
    }
}
