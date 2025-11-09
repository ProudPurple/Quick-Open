#include <windows.h>
#include <string>
#include <vector>
#include <iostream>

int main() {
    std::string vcvars64 = "";
    std::string projectDir = "";

    std::cout << "What u want twin: ";
    std::string whatuwant, exeFile;
    std::cin >> whatuwant;

    if (whatuwant == "fbla") {
        vcvars64 = R"(C:\Program Files\Microsoft Visual Studio\2022\Preview\VC\Auxiliary\Build\vcvars64.bat)";
        projectDir = R"(C:\Users\ander\OneDrive\Desktop\Work\Coding\FBLA-Project\Virtual-Pet)";
        exeFile = "virtual-pet.exe";
    } else if (whatuwant == "quickopen") {
        exeFile = "quickOpen.exe";
        vcvars64 = R"(C:\Program Files\Microsoft Visual Studio\2022\Preview\VC\Auxiliary\Build\vcvars64.bat)";
        projectDir = R"(C:\Users\ander\OneDrive\Desktop\Work\Coding\Actually Useful\Console Shorties)";
    } else return 0;

    std::cout << "What part: ";
    std::string whatpart;
    std::cin >> whatpart;

    std::string fullCmd ;

    if (whatpart == "app") {
        std::string exePath = projectDir + "\\" + exeFile;
        // create mutable buffer for CreateProcess
        std::vector<char> cmdLine(exePath.begin(), exePath.end());
        cmdLine.push_back('\0');

        STARTUPINFOA si{};
        PROCESS_INFORMATION pi{};
        si.cb = sizeof(si);

        // Provide the working directory as the 8th parameter
        if (!CreateProcessA(nullptr, cmdLine.data(), nullptr, nullptr, FALSE, 0, nullptr,
                        projectDir.c_str(), &si, &pi)) {
            std::cerr << "CreateProcess failed, error: " << GetLastError() << "\n";
            return 1;
        }
    } else if (whatpart == "code") {
        fullCmd = "cmd.exe /c call \"" + vcvars64 + "\" && start /B \"\" code \"" + projectDir + "\" && exit";
        std::vector<char> cmdLine(fullCmd.begin(), fullCmd.end());
        cmdLine.push_back('\0');

        STARTUPINFOA si{};
        PROCESS_INFORMATION pi{};
        si.cb = sizeof(si);

        if (!CreateProcessA(nullptr, cmdLine.data(), nullptr, nullptr, FALSE, CREATE_NO_WINDOW, nullptr, nullptr, &si, &pi)) {
            std::cerr << "CreateProcess failed, error: " << GetLastError() << "\n";
            return 1;
        }

        CloseHandle(pi.hProcess);
        CloseHandle(pi.hThread);
    } else return 0;
    return 0;
}