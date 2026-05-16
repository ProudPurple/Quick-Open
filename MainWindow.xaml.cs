using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace ProjectHub;

public partial class MainWindow : Window
{
    private const string CodingRoot = @"C:\Users\ander\OneDrive\Desktop\Work\Coding";
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ProjectHub", "settings.json");

    private List<Project> _allProjects = [];
    private AppSettings _settings = new();

    public MainWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadProjectsAsync();
    }

    private void LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsPath))
                _settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(SettingsPath)) ?? new();
            else
                _settings = new();
        }
        catch { _settings = new(); }
    }

    private void SaveSettings()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }

    private async Task LoadProjectsAsync()
    {
        LoadSettings();
        LoadingText.Visibility = Visibility.Visible;
        EmptyText.Visibility = Visibility.Collapsed;
        ProjectsScroller.Visibility = Visibility.Collapsed;
        RefreshBtn.IsEnabled = false;
        AddBtn.IsEnabled = false;

        var scannedPaths = await Task.Run(() => FindGitRepos(CodingRoot).ToList());
        var allPaths = scannedPaths
            .Concat(_settings.ManualRepos.Where(Directory.Exists))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(p => !_settings.ExcludedPaths.Any(e => e.Equals(p, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var tasks = allPaths.Select(LoadProject).ToArray();
        _allProjects = (await Task.WhenAll(tasks))
            .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        RefreshBtn.IsEnabled = true;
        AddBtn.IsEnabled = true;
        LoadingText.Visibility = Visibility.Collapsed;
        StatusText.Text = $"{_allProjects.Count} project{(_allProjects.Count == 1 ? "" : "s")}";
        ApplyFilter(SearchBox.Text);
    }

    private static async Task<Project> LoadProject(string path)
    {
        var name = Path.GetFileName(path);
        var relative = Path.GetRelativePath(CodingRoot, path);

        string parent;
        if (Path.IsPathFullyQualified(relative) || relative.StartsWith(".."))
            parent = Path.GetFileName(Path.GetDirectoryName(path) ?? "") + "/";
        else
            parent = Path.GetDirectoryName(relative) is { } p && p != "." ? p + "/" : "";

        var lastCommit = await RunGit(path, "log -1 --format=%s");
        var remoteUrl = await RunGit(path, "remote get-url origin");
        var gitHubUrl = ToGitHubUrl(remoteUrl);

        return new Project
        {
            Name = name,
            FolderPath = path,
            RelativePath = parent,
            LastCommit = string.IsNullOrWhiteSpace(lastCommit) ? "no commits" : lastCommit,
            GitHubUrl = gitHubUrl,
        };
    }

    // Recursively finds git repos, stopping at each one (doesn't walk inside found repos).
    private static IEnumerable<string> FindGitRepos(string root, int depth = 0)
    {
        if (depth > 3) yield break;

        if (Directory.Exists(Path.Combine(root, ".git")))
        {
            yield return root;
            yield break;
        }

        IEnumerable<string> subdirs;
        try { subdirs = Directory.GetDirectories(root); }
        catch { yield break; }

        foreach (var dir in subdirs)
        {
            var name = Path.GetFileName(dir);
            if (name.StartsWith('.') || name is "node_modules" or "vendor") continue;
            foreach (var repo in FindGitRepos(dir, depth + 1))
                yield return repo;
        }
    }

    private static async Task<string> RunGit(string workDir, string args)
    {
        try
        {
            var psi = new ProcessStartInfo("git", args)
            {
                WorkingDirectory = workDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var proc = Process.Start(psi)!;
            var output = await proc.StandardOutput.ReadToEndAsync();
            await proc.WaitForExitAsync();
            return output.Trim();
        }
        catch { return ""; }
    }

    private static string? ToGitHubUrl(string remote)
    {
        if (string.IsNullOrEmpty(remote)) return null;

        var url = remote;
        if (url.StartsWith("git@github.com:"))
            url = "https://github.com/" + url["git@github.com:".Length..];

        if (!url.Contains("github.com")) return null;
        if (url.EndsWith(".git")) url = url[..^4];
        return url;
    }

    private void ApplyFilter(string search)
    {
        var list = string.IsNullOrWhiteSpace(search)
            ? _allProjects
            : _allProjects
                .Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();

        ProjectsControl.ItemsSource = list;

        bool empty = list.Count == 0 && _allProjects.Count > 0;
        EmptyText.Visibility = empty ? Visibility.Visible : Visibility.Collapsed;
        ProjectsScroller.Visibility = list.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        => ApplyFilter(SearchBox.Text);

    private async void Refresh_Click(object sender, RoutedEventArgs e)
        => await LoadProjectsAsync();

    private async void AddProject_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Select a git repository folder"
        };

        if (dialog.ShowDialog() != true) return;

        var folder = dialog.FolderName;
        if (!Directory.Exists(Path.Combine(folder, ".git")))
        {
            MessageBox.Show("The selected folder is not a git repository.", "Not a Git Repo",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_allProjects.Any(p => p.FolderPath.Equals(folder, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show("This repository is already in your list.", "Already Added",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _settings.ExcludedPaths.RemoveAll(p => p.Equals(folder, StringComparison.OrdinalIgnoreCase));
        if (!_settings.ManualRepos.Any(p => p.Equals(folder, StringComparison.OrdinalIgnoreCase)))
            _settings.ManualRepos.Add(folder);
        SaveSettings();

        var project = await LoadProject(folder);
        _allProjects = [.. _allProjects.Append(project).OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)];
        StatusText.Text = $"{_allProjects.Count} project{(_allProjects.Count == 1 ? "" : "s")}";
        ApplyFilter(SearchBox.Text);
    }

    private void RemoveProject_Click(object sender, RoutedEventArgs e)
    {
        var path = (string)((Button)sender).Tag;

        if (!_settings.ExcludedPaths.Any(p => p.Equals(path, StringComparison.OrdinalIgnoreCase)))
            _settings.ExcludedPaths.Add(path);
        _settings.ManualRepos.RemoveAll(p => p.Equals(path, StringComparison.OrdinalIgnoreCase));
        SaveSettings();

        _allProjects = _allProjects
            .Where(p => !p.FolderPath.Equals(path, StringComparison.OrdinalIgnoreCase))
            .ToList();
        StatusText.Text = $"{_allProjects.Count} project{(_allProjects.Count == 1 ? "" : "s")}";
        ApplyFilter(SearchBox.Text);
    }

    private void OpenVSCode_Click(object sender, RoutedEventArgs e)
    {
        var path = (string)((Button)sender).Tag;
        try
        {
            Process.Start(new ProcessStartInfo("code", $"\"{path}\"") { UseShellExecute = true });
        }
        catch
        {
            OpenFolderPath(path);
        }
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
        => OpenFolderPath((string)((Button)sender).Tag);

    private static void OpenFolderPath(string path)
        => Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"") { UseShellExecute = true });

    private void OpenGitHub_Click(object sender, RoutedEventArgs e)
    {
        var url = (string)((Button)sender).Tag;
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
}

public class AppSettings
{
    public List<string> ManualRepos { get; set; } = [];
    public List<string> ExcludedPaths { get; set; } = [];
}
