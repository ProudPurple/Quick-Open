#include <windows.h>
#include <string>
#include <vector>
#include <iostream>

int main() {
    std::string vcvars64 = "";
    std::string projectDir = "";

    std::cout << "What u want twin: ";
    std::string whatuwant;
    std::cin >> whatuwant;

    if (whatuwant == "fbla") {
        vcvars64 = R"(C:\Program Files\Microsoft Visual Studio\2022\Preview\VC\Auxiliary\Build\vcvars64.bat)";
        projectDir = R"(C:\Users\ander\OneDrive\Desktop\Work\Coding\FBLA-Project\Virtual-Pet)";
    } else if (whatuwant == "quickopen") {
        vcvars64 = R"(C:\Program Files\Microsoft Visual Studio\2022\Preview\VC\Auxiliary\Build\vcvars64.bat)";
        projectDir = R"(C:\Users\ander\OneDrive\Desktop\Work\Coding\Actually Useful\Console Shorties)";
    }

    // cmd.exe /k keeps console open
std::string fullCmd = "cmd.exe /c call \"" + vcvars64 + "\" && start /B \"\" code \"" + projectDir + "\" && exit";




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
    return 0;
}




//cd C:\Users\ander\OneDrive\Desktop\Work\Coding\Actually Useful\Console Shorties
//cl /EHsc quickOpen.cpp resources.res