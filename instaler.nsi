Unicode true

!define APP_NAME     "MagicLittleBox"
!define VERSION      "0.2.2"
!define INSTALL_DIR  "$PROGRAMFILES\${APP_NAME}"
!define SOURCE_DIR   "W:\Cisdi\01ZJ\EgmTest\MagicLittleBox\bin\x64\Debug Copy"
!define SOURCE_EXE   "MagicLittleBox.exe"
!define TARGET_EXE   "MagicLittleBox.exe"

!include "LogicLib.nsh"
!include "FileFunc.nsh"
!include "WordFunc.nsh"
!insertmacro GetFileVersion

Icon "${SOURCE_DIR}\Trayicon.ico"
UninstallIcon "${SOURCE_DIR}\Trayicon.ico"

VIProductVersion "${VERSION}.0"
VIAddVersionKey "ProductName"      "${APP_NAME}"
VIAddVersionKey "FileDescription"  "${APP_NAME} 安装程序"
VIAddVersionKey "CompanyName"      "Cisdi"
VIAddVersionKey "FileVersion"      "${VERSION}"
VIAddVersionKey "LegalCopyright"   "© 2025 Cisdi HuJunHan. 版权所有。"

OutFile "${APP_NAME}_v${VERSION}.exe"

InstallDir "${INSTALL_DIR}"
InstallDirRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "InstallLocation"

Name "${APP_NAME}"

RequestExecutionLevel admin
SetCompressor /SOLID lzma

Page directory
Page instfiles

UninstPage uninstConfirm
UninstPage instfiles

;---------------- 安装前：只做进程检测 + 旧目录指向 ----------------
Function .onInit
  ; 检查程序是否在运行（用 CSV 格式，避免中文系统问题）
  nsExec::ExecToStack 'tasklist /FI "IMAGENAME eq ${TARGET_EXE}" /FO CSV /NH'
  Pop $0
  Pop $1

  StrCpy $R0 $1 1
  ${If} $R0 == '"'
    MessageBox MB_ICONSTOP|MB_OK "${APP_NAME} 正在运行，请先关闭程序后再重新安装。"
    Abort
  ${EndIf}

  ; 旧版本目录
  ReadRegStr $2 HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "InstallLocation"
  ${If} $2 != ""
    StrCpy $INSTDIR $2
  ${EndIf}
FunctionEnd

;---------------- 安装区：把 Debug Copy 整包搬过去 ----------------
Section "Install"

  SetOutPath "$INSTDIR"
  SetOverwrite on

  ; 关键：完全复制 Debug Copy 的所有内容
  File /r "${SOURCE_DIR}\*.*"

  ; 桌面快捷方式，强制用 Trayicon.ico
  CreateShortCut "$DESKTOP\${APP_NAME}.lnk" "$INSTDIR\${TARGET_EXE}" "" "$INSTDIR\Trayicon.ico" 0

  ; 以管理员运行（可选）
  WriteRegStr HKCU "Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers" "$INSTDIR\${TARGET_EXE}" "RUNASADMIN"

  ; 卸载程序
  WriteUninstaller "$INSTDIR\Uninstall.exe"

  ; 卸载信息
  WriteRegStr   HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "DisplayName"     "${APP_NAME}"
  WriteRegStr   HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "UninstallString" "$INSTDIR\Uninstall.exe"
  WriteRegStr   HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "DisplayVersion"  "${VERSION}"
  WriteRegStr   HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "Publisher"       "HuJunHan"
  WriteRegStr   HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "InstallLocation" "$INSTDIR"
  WriteRegStr   HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "DisplayIcon"     "$INSTDIR\Trayicon.ico"
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "NoRepair" 1

SectionEnd

;---------------- 卸载区 ----------------
Section "Uninstall"

  nsExec::Exec 'taskkill /IM "${TARGET_EXE}" /F'
  Sleep 500

  Delete "$DESKTOP\${APP_NAME}.lnk"
  DeleteRegValue HKCU "Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers" "$INSTDIR\${TARGET_EXE}"
  RMDir /r "$INSTDIR"
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}"

  MessageBox MB_OK "${APP_NAME} 已卸载完成。"

SectionEnd

;---------------- 安装成功后自动运行 ----------------
Function .onInstSuccess
  Exec "$INSTDIR\${TARGET_EXE}"
FunctionEnd
