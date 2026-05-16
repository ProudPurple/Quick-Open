using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProjectHub;

public class Project : INotifyPropertyChanged
{
    private string _lastCommit = "...";

    public string Name { get; init; } = "";
    public string FolderPath { get; init; } = "";
    public string RelativePath { get; init; } = "";

    public string LastCommit
    {
        get => _lastCommit;
        set { _lastCommit = value; OnPropertyChanged(); }
    }

    public string? GitHubUrl { get; init; }
    public bool HasGitHub => GitHubUrl != null;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
